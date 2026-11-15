using Source191.Cader.Domain;
using Source191.Cader.Ports;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Source191.Cader.Adapters.Terminal;

/// <summary>
/// Windows Terminal's <c>settings.json</c> and <c>state.json</c> in the Store package's <c>LocalState</c> folder, both
/// Terminal's own. Read allowing comments and trailing commas, as a file the user has annotated may hold them. The
/// profile list is <c>profiles.list</c>, or <c>profiles</c> itself when it is a bare array.
/// </summary>
public sealed class TerminalSettings(string localStateFolder, TimeProvider time) : ITerminalSettings
{
    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private static readonly JsonReaderOptions ReaderOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private const string NoProfileList = "the settings file has no profile list";

    public static TerminalSettings ForCurrentUser(TimeProvider time) => new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", "Microsoft.WindowsTerminal_8wekyb3d8bbwe", "LocalState"), time);

    public string Location => Path.Combine(localStateFolder, "settings.json");

    private string StateFile => Path.Combine(localStateFolder, "state.json");

    public async Task<TerminalProfileRecords> ReadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(StateFile) || !File.Exists(Location))
        {
            return new TerminalProfileRecords.Missing();
        }

        try
        {
            using JsonDocument state = Parse(await File.ReadAllBytesAsync(StateFile, cancellationToken));
            using JsonDocument settings = Parse(await File.ReadAllBytesAsync(Location, cancellationToken));

            JsonElement list = ProfileList(settings.RootElement);

            return new TerminalProfileRecords.Read(Generated(state.RootElement), Listed(list), Added(list));
        }
        catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
        {
            // The system's own words report what failed; they are shown as the reason and never acted on.
            return new TerminalProfileRecords.Unreadable(exception.Message);
        }
    }

    /// <summary>
    /// Inserts the entries as text after the list's last element, or after its <c>[</c> when it is empty, each on a line of
    /// its own, indented like the first element, with the file's own line ending; nothing else in the file is touched, a
    /// byte order mark included.
    /// </summary>
    public Task<TerminalSettingsEdit> AddProfileEntriesAsync(IReadOnlyList<TerminalProfileEntry> entries, CancellationToken cancellationToken) => EditAsync(bytes => Inserted(bytes, entries), cancellationToken);

    /// <summary>
    /// Takes out each element that is exactly one of the entries as setup adds it, one at a time, with one comma: the one
    /// before it, or after it when it is the first. An element alone on its line goes with the whole line; one that starts
    /// its line with the line break before it; any other with the spaces and tabs before it. Every other byte stays.
    /// </summary>
    public Task<TerminalSettingsEdit> RemoveProfileEntriesAsync(IReadOnlyList<TerminalProfileEntry> entries, CancellationToken cancellationToken) => EditAsync(bytes => Removed(bytes, entries), cancellationToken);

    /// <summary>
    /// The one guarded write: refuses a link or a read-only file, checks the new bytes hold the profile list expected, and
    /// writes nothing if the file changed since it was read. Written beside the file and put in its place with File.Replace,
    /// so Terminal, which reloads the file when it changes, never reads half of it, and the file keeps its creation time and
    /// access rules; the file as it was is kept beside it, named after Terminal's own backups with the UTC time.
    /// </summary>
    private async Task<TerminalSettingsEdit> EditAsync(Func<byte[], Change> change, CancellationToken cancellationToken)
    {
        if (IsALink())
        {
            return new TerminalSettingsEdit.IsALink();
        }

        if (File.GetAttributes(Location).HasFlag(FileAttributes.ReadOnly))
        {
            return new TerminalSettingsEdit.IsReadOnly();
        }

        byte[] bytes = await File.ReadAllBytesAsync(Location, cancellationToken);
        Change changed = change(bytes);
        if (!Holds(bytes, changed.Before))
        {
            return new TerminalSettingsEdit.NotVerified();
        }

        if (changed.Written.AsSpan().SequenceEqual(bytes))
        {
            return new TerminalSettingsEdit.Unchanged();
        }

        if (!Holds(changed.Written, changed.After))
        {
            return new TerminalSettingsEdit.NotVerified();
        }

        string backup = Location + time.GetUtcNow().ToString(".yyyy-MM-ddTHH-mm-ss'.cader.backup'", CultureInfo.InvariantCulture);
        string temporary = Location + ".cader-tmp";
        try
        {
            await File.WriteAllBytesAsync(temporary, changed.Written, cancellationToken);
            byte[] now = await File.ReadAllBytesAsync(Location, cancellationToken);
            if (!bytes.AsSpan().SequenceEqual(now))
            {
                return new TerminalSettingsEdit.ChangedMeanwhile();
            }

            File.Replace(temporary, Location, backup);
        }
        finally
        {
            File.Delete(temporary);
        }

        return new TerminalSettingsEdit.Written(backup);
    }

    private Change Inserted(byte[] bytes, IReadOnlyList<TerminalProfileEntry> entries)
    {
        int start = ByteOrderMarkLength(bytes);
        ListPlace place = PlaceOfTheList(bytes, start);
        string indent = place.Empty ? new string(' ', 12) : LineIndent(bytes, start + place.First);
        byte[] insertion = Encoding.UTF8.GetBytes(Insertion(entries, place, indent, NewLine(bytes)));
        JsonElement[] before = ElementsOf(bytes, start, place);

        return new Change([.. bytes.AsSpan(0, start + place.InsertAt), .. insertion, .. bytes.AsSpan(start + place.InsertAt)], before, [.. before, .. entries.Select(entry => JsonElement.Parse(entry.Json))]);
    }

    private Change Removed(byte[] bytes, IReadOnlyList<TerminalProfileEntry> entries)
    {
        int start = ByteOrderMarkLength(bytes);
        JsonElement[] before = ElementsOf(bytes, start, PlaceOfTheList(bytes, start));
        byte[] written = bytes;
        try
        {
            while (FirstToRemove(written, start, entries) is { Index: >= 0 } found)
            {
                written = WithoutElement(written, start, found.Place, found.Index);
            }
        }
        catch (JsonException)
        {
            // A cut that left the file unreadable is kept as it is, for the check to refuse.
        }

        return new Change(written, before, [.. before.Where(element => !entries.Any(entry => IsAsSetupAdds(element, entry)))]);
    }

    /// <summary>The list as the text walker found it, and the index of its first element to take out, -1 when none.</summary>
    private static (ListPlace Place, int Index) FirstToRemove(byte[] bytes, int start, IReadOnlyList<TerminalProfileEntry> entries)
    {
        ListPlace place = FindList(bytes.AsSpan(start));
        JsonElement[] elements = ElementsOf(bytes, start, place);

        return (place, Array.FindIndex(elements, element => entries.Any(entry => IsAsSetupAdds(element, entry))));
    }

    private static byte[] WithoutElement(byte[] bytes, int start, ListPlace place, int index)
    {
        int first = start + place.Items[index].Start;
        int end = start + place.Items[index].End;
        int comma = index > 0 ? CommaAt(bytes, start + place.Items[index - 1].End) : CommaAt(bytes, end);
        int lineStart = Array.LastIndexOf(bytes, (byte)'\n', first - 1) + 1;
        bool startsLine = bytes.AsSpan(lineStart, first - lineStart).IndexOfAnyExcept((byte)' ', (byte)'\t') < 0;
        int lineEnd = LineEndAfter(bytes, end, comma);
        (int From, int To) cut = (startsLine, lineEnd >= 0) switch
        {
            (true, true) => (lineStart, lineEnd),
            (true, false) => (bytes[lineStart - 2] == '\r' ? lineStart - 2 : lineStart - 1, end),
            _ => (bytes.AsSpan(0, first).LastIndexOfAnyExcept((byte)' ', (byte)'\t') + 1, end),
        };

        return comma < 0 || (comma >= cut.From && comma < cut.To)
            ? [.. bytes.AsSpan(0, cut.From), .. bytes.AsSpan(cut.To)]
            : Cut(bytes, cut, comma);
    }

    private static byte[] Cut(byte[] bytes, (int From, int To) cut, int comma)
    {
        return comma < cut.From
            ? [.. bytes.AsSpan(0, comma), .. bytes.AsSpan(comma + 1, cut.From - comma - 1), .. bytes.AsSpan(cut.To)]
            : [.. bytes.AsSpan(0, cut.From), .. bytes.AsSpan(cut.To, comma - cut.To), .. bytes.AsSpan(comma + 1)];
    }

    /// <summary>The comma that is the next thing after <paramref name="from"/>, past white space and comments, or -1.</summary>
    private static int CommaAt(byte[] bytes, int from)
    {
        int at = PastSpaceAndComments(bytes, from);

        return at < bytes.Length && bytes[at] == ',' ? at : -1;
    }

    private static int PastSpaceAndComments(byte[] bytes, int from)
    {
        int at = from;
        int next = Past(bytes, at);
        while (next != at)
        {
            at = next;
            next = Past(bytes, at);
        }

        return at;
    }

    /// <summary>Past the white space character or comment at <paramref name="at"/>, or <paramref name="at"/> itself.</summary>
    private static int Past(byte[] bytes, int at)
    {
        ReadOnlySpan<byte> rest = bytes.AsSpan(at);

        return rest switch
        {
            [(byte)' ' or (byte)'\t' or (byte)'\r' or (byte)'\n', ..] => at + 1,
            [(byte)'/', (byte)'/', ..] => LineCommentEnd(bytes, at),
            [(byte)'/', (byte)'*', ..] => at + 2 + rest[2..].IndexOf("*/"u8) + 2,
            _ => at,
        };
    }

    private static int LineCommentEnd(byte[] bytes, int at)
    {
        int newLine = Array.IndexOf(bytes, (byte)'\n', at);

        return newLine < 0 ? bytes.Length : newLine;
    }

    /// <summary>Just past the line break that ends the element's line when nothing but spaces, tabs and its comma follow it there, else -1.</summary>
    private static int LineEndAfter(byte[] bytes, int end, int comma)
    {
        int at = end;
        while (at < bytes.Length && (bytes[at] == ' ' || bytes[at] == '\t' || at == comma))
        {
            at++;
        }

        return bytes.AsSpan(at) switch
        {
            [(byte)'\r', (byte)'\n', ..] => at + 2,
            [(byte)'\n', ..] => at + 1,
            _ => -1,
        };
    }

    /// <summary>Whether the new bytes, read as Terminal reads them, hold exactly the expected profile list, element for element.</summary>
    private static bool Holds(byte[] written, JsonElement[] expected)
    {
        try
        {
            JsonElement[] list = ListOf(written);

            return list.Length == expected.Length && list.Zip(expected).All(pair => JsonElement.DeepEquals(pair.First, pair.Second));
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>The profile list's elements, kept once the document is gone.</summary>
    private static JsonElement[] ListOf(byte[] bytes)
    {
        using JsonDocument document = Parse(bytes);

        return [.. ProfileList(document.RootElement).EnumerateArray().Select(element => element.Clone())];
    }

    /// <summary>A symbolic link or other reparse point, as a dotfiles set-up makes it: moving a file over it would replace the link.</summary>
    private bool IsALink()
    {
        FileInfo file = new(Location);

        return file.Exists && (file.LinkTarget is not null || file.Attributes.HasFlag(FileAttributes.ReparsePoint));
    }
    /// <summary>Where the profile list is, once the whole file is known to be JSON with one; refused as an IOException otherwise.</summary>
    private ListPlace PlaceOfTheList(byte[] bytes, int start)
    {
        try
        {
            using JsonDocument document = Parse(bytes);
            _ = ProfileList(document.RootElement);

            return FindList(bytes.AsSpan(start));
        }
        catch (JsonException exception)
        {
            throw new IOException($"{Location} could not be read ({exception.Message}), so it was left alone.", exception);
        }
    }

    private static string Insertion(IReadOnlyList<TerminalProfileEntry> entries, ListPlace place, string indent, string newLine) => string.Concat(entries.Select((entry, index) => $"{(index == 0 && place.Empty ? string.Empty : ",")}{newLine}{indent}{entry.Json}"));

    /// <summary>The spaces and tabs that start the line the first element is on.</summary>
    private static string LineIndent(byte[] bytes, int first)
    {
        int lineStart = Array.LastIndexOf(bytes, (byte)'\n', first) + 1;
        int length = bytes.AsSpan(lineStart).IndexOfAnyExcept((byte)' ', (byte)'\t');

        return Encoding.UTF8.GetString(bytes, lineStart, length);
    }

    private static string NewLine(byte[] bytes) => bytes.AsSpan().IndexOf("\r\n"u8) >= 0 ? "\r\n" : "\n";

    /// <summary>Where the profile list is: <c>profiles.list</c>, or <c>profiles</c> itself, at the top level only.</summary>
    private static ListPlace FindList(ReadOnlySpan<byte> json)
    {
        Utf8JsonReader reader = new(json, ReaderOptions);
        reader.Read();
        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            bool profiles = reader.ValueTextEquals("profiles"u8);
            reader.Read();
            if (profiles && reader.TokenType == JsonTokenType.StartArray)
            {
                return Elements(ref reader);
            }

            if (profiles && reader.TokenType == JsonTokenType.StartObject)
            {
                return ListIn(ref reader);
            }

            reader.Skip();
        }

        throw new JsonException(NoProfileList);
    }

    private static ListPlace ListIn(ref Utf8JsonReader reader)
    {
        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            bool list = reader.ValueTextEquals("list"u8);
            reader.Read();
            if (list && reader.TokenType == JsonTokenType.StartArray)
            {
                return Elements(ref reader);
            }

            reader.Skip();
        }

        throw new JsonException(NoProfileList);
    }

    /// <summary>From the list's <c>[</c>: just after it, and where each element starts and ends.</summary>
    private static ListPlace Elements(ref Utf8JsonReader reader)
    {
        int open = (int)reader.BytesConsumed;
        List<ElementPlace> elements = [];
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            int first = (int)reader.TokenStartIndex;
            reader.Skip();
            elements.Add(new ElementPlace(first, (int)reader.BytesConsumed));
        }

        return new ListPlace(open, elements);
    }

    /// <summary>Each element as the text walker found it, parsed on its own as the whole file is.</summary>
    private static JsonElement[] ElementsOf(byte[] bytes, int start, ListPlace place)
    {
        return [.. place.Items.Select(element =>
        {
            using JsonDocument document = JsonDocument.Parse(bytes.AsMemory(start + element.Start, element.End - element.Start), DocumentOptions);

            return document.RootElement.Clone();
        })];
    }

    /// <summary>
    /// The bytes to write, the profile list the file must hold as read for the text walker to have found it, and the list
    /// the new bytes must hold once read back.
    /// </summary>
    private sealed record Change(byte[] Written, JsonElement[] Before, JsonElement[] After);

    /// <summary>Byte offsets after any byte order mark.</summary>
    private sealed record ListPlace(int Open, IReadOnlyList<ElementPlace> Items)
    {
        public bool Empty => Items.Count == 0;

        public int First => Items[0].Start;

        public int InsertAt => Empty ? Open : Items[^1].End;
    }

    private sealed record ElementPlace(int Start, int End);

    /// <summary>A byte order mark, which the parser does not skip, is left out.</summary>
    private static JsonDocument Parse(byte[] bytes) => JsonDocument.Parse(bytes.AsMemory(ByteOrderMarkLength(bytes)), DocumentOptions);

    private static int ByteOrderMarkLength(byte[] bytes) => bytes.AsSpan().StartsWith(Encoding.UTF8.Preamble) ? Encoding.UTF8.Preamble.Length : 0;

    private static HashSet<TerminalProfileId> Generated(JsonElement state)
    {
        if (state.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("the state file is not a JSON object");
        }

        return state.TryGetProperty("generatedProfiles", out JsonElement generated) && generated.ValueKind == JsonValueKind.Array
            ? Ids(generated.EnumerateArray())
            : [];
    }

    private static HashSet<TerminalProfileId> Listed(JsonElement list) => Ids(list.EnumerateArray().Where(entry => entry.ValueKind == JsonValueKind.Object).Select(entry => entry.TryGetProperty("guid", out JsonElement guid) ? guid : default));

    private static HashSet<TerminalProfileId> Added(JsonElement list) => [.. TerminalProfiles.Entries.Where(entry => list.EnumerateArray().Any(element => IsAsSetupAdds(element, entry))).Select(entry => entry.Id)];

    /// <summary>Exactly the four properties setup writes for the entry, with its values, the id in any letter case.</summary>
    private static bool IsAsSetupAdds(JsonElement element, TerminalProfileEntry entry)
    {
        return element.ValueKind == JsonValueKind.Object
            && element.EnumerateObject().Count() == 4
            && Ids([element.TryGetProperty("guid", out JsonElement guid) ? guid : default]).Contains(entry.Id)
            && element.TryGetProperty("hidden", out JsonElement hidden) && hidden.ValueKind == JsonValueKind.False
            && HoldsText(element, "name", entry.Name)
            && HoldsText(element, "source", entry.Source);
    }

    private static bool HoldsText(JsonElement element, string property, string text) => element.TryGetProperty(property, out JsonElement value) && value.ValueKind == JsonValueKind.String && value.ValueEquals(text);

    /// <summary>Every element that is a string holding a guid; anything else is left out.</summary>
    private static HashSet<TerminalProfileId> Ids(IEnumerable<JsonElement> elements)
    {
        return [.. elements
            .Where(element => element.ValueKind == JsonValueKind.String)
            .Select(element => Guid.TryParse(element.GetString(), out Guid id) ? new TerminalProfileId(id) : null)
            .OfType<TerminalProfileId>()];
    }

    private static JsonElement ProfileList(JsonElement settings)
    {
        JsonElement profiles = settings.ValueKind == JsonValueKind.Object && settings.TryGetProperty("profiles", out JsonElement found) ? found : default;

        return profiles.ValueKind switch
        {
            JsonValueKind.Array => profiles,
            JsonValueKind.Object when profiles.TryGetProperty("list", out JsonElement list) && list.ValueKind == JsonValueKind.Array => list,
            _ => throw new JsonException(NoProfileList),
        };
    }
}