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
            _ => throw new JsonException("the settings file has no profile list"),
        };
    }
}
