using Source191.Cader.Domain;
using Source191.Cader.Ports;
using System.Text;
using System.Text.Json;

namespace Source191.Cader.Adapters.Terminal;

/// <summary>
/// Windows Terminal's <c>settings.json</c> and <c>state.json</c> in the Store package's <c>LocalState</c> folder, both
/// Terminal's own. Read allowing comments and trailing commas, as a file the user has annotated may hold them. The
/// profile list is <c>profiles.list</c>, or <c>profiles</c> itself when it is a bare array.
/// </summary>
public sealed class TerminalSettings(string localStateFolder) : ITerminalSettings
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

    public static TerminalSettings ForCurrentUser() => new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", "Microsoft.WindowsTerminal_8wekyb3d8bbwe", "LocalState"));

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

            return new TerminalProfileRecords.Read(Generated(state.RootElement), Listed(ProfileList(settings.RootElement)));
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
    /// byte order mark included. Written beside the file and moved over it, so Terminal, which reloads the file when it
    /// changes, never reads half of it.
    /// </summary>
    public async Task AddProfileEntriesAsync(IReadOnlyList<TerminalProfileEntry> entries, CancellationToken cancellationToken)
    {
        byte[] bytes = await File.ReadAllBytesAsync(Location, cancellationToken);
        int start = ByteOrderMarkLength(bytes);
        ListPlace place = PlaceOfTheList(bytes, start);
        string indent = place.Empty ? new string(' ', 12) : LineIndent(bytes, start + place.First);
        byte[] insertion = Encoding.UTF8.GetBytes(Insertion(entries, place, indent, NewLine(bytes)));
        byte[] written = [.. bytes.AsSpan(0, start + place.InsertAt), .. insertion, .. bytes.AsSpan(start + place.InsertAt)];

        string temporary = Location + ".cader-tmp";
        try
        {
            await File.WriteAllBytesAsync(temporary, written, cancellationToken);
            File.Move(temporary, Location, overwrite: true);
        }
        finally
        {
            File.Delete(temporary);
        }
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

    /// <summary>From the list's <c>[</c>: where its first element starts (-1 when empty) and where its last one ends.</summary>
    private static ListPlace Elements(ref Utf8JsonReader reader)
    {
        int first = -1;
        int end = (int)reader.BytesConsumed;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            first = first < 0 ? (int)reader.TokenStartIndex : first;
            reader.Skip();
            end = (int)reader.BytesConsumed;
        }

        return new ListPlace(first, end);
    }

    /// <summary>Byte offsets after any byte order mark.</summary>
    private sealed record ListPlace(int First, int InsertAt)
    {
        public bool Empty => First < 0;
    }

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