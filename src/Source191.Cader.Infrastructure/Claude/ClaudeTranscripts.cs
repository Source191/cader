using Source191.Cader.Domain;
using Source191.Cader.Ports;
using System.Collections.Concurrent;
using System.Text.Json;
namespace Source191.Cader.Adapters.Claude;

/// <summary>
/// Looks for <c>~\.claude\projects\&lt;project&gt;\&lt;sessionId&gt;.jsonl</c>. The project folder name encodes the
/// working directory, so every project folder is checked rather than reproducing that encoding. A transcript has
/// messages once it holds a user or assistant line: after <c>/bg</c> Claude writes a new session's transcript with only
/// its title and name (2.1.325), which <c>--resume</c> cannot find. Of each line only the top-level type is read, and
/// of the first <c>session-owner</c> line (2.1.325 on) its <c>organizationUuid</c>.
/// </summary>
public sealed class ClaudeTranscripts(string claudeHome) : ITranscripts
{
    // An owner never changes, so one found is kept for good; a transcript without one is read again only once it has
    // changed, so a long one from an older Claude Code is not scanned on every refresh. Refreshes can overlap.
    private readonly ConcurrentDictionary<SessionId, OwnerRead> owners = new();

    public bool HasMessages(SessionId sessionId) => TranscriptsOf(sessionId).Any(HoldsAMessage);

    public SessionOwner OwnerOf(SessionId sessionId)
    {
        bool known = owners.TryGetValue(sessionId, out OwnerRead? kept);
        if (known && kept is { Owner: SessionOwner.Recorded })
        {
            return kept.Owner;
        }

        if (TranscriptsOf(sessionId).FirstOrDefault() is not string transcript)
        {
            return new SessionOwner.NotRecorded();
        }

        try
        {
            FileInfo file = new(transcript);
            Stamp stamp = new(file.Length, file.LastWriteTimeUtc);
            if (known && kept is not null && kept.Stamp == stamp)
            {
                return kept.Owner;
            }

            SessionOwner owner = ReadOwner(transcript);
            owners[sessionId] = new OwnerRead(owner, stamp);

            return owner;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // Not kept, so the next refresh tries again once Claude lets the file go.
            return new SessionOwner.NotRecorded();
        }
    }

    private IEnumerable<string> TranscriptsOf(SessionId sessionId)
    {
        string projects = Path.Combine(claudeHome, "projects");
        if (!Directory.Exists(projects))
        {
            return [];
        }

        return Directory.EnumerateDirectories(projects)
            .Select(project => Path.Combine(project, $"{sessionId.Value}.jsonl"))
            .Where(File.Exists);
    }

    /// <summary>A transcript that exists but cannot be read keeps the answer its existence gave before lines were read.</summary>
    private static bool HoldsAMessage(string transcript)
    {
        try
        {
            using FileStream stream = Open(transcript);
            using StreamReader reader = new(stream);

            return Lines(reader).Any(IsMessage);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return true;
        }
    }

    private static SessionOwner ReadOwner(string transcript)
    {
        using FileStream stream = Open(transcript);
        using StreamReader reader = new(stream);

        // Only a line naming the type is parsed: message lines can be large, and most are messages.
        string? line = Lines(reader).FirstOrDefault(text => text.Contains("\"session-owner\"", StringComparison.Ordinal) && TypeOf(text) is "session-owner");

        return line is not null && OrganisationOf(line) is OrganisationId organisation ? new SessionOwner.Recorded(organisation) : new SessionOwner.NotRecorded();
    }

    private static FileStream Open(string transcript) => new(transcript, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

    private static IEnumerable<string> Lines(StreamReader reader)
    {
        while (reader.ReadLine() is string line)
        {
            yield return line;
        }
    }

    private static bool IsMessage(string line) => TypeOf(line) is "user" or "assistant";

    private static string? TypeOf(string line) => StringProperty(line, "type");

    private static OrganisationId? OrganisationOf(string line) => OrganisationId.TryParse(StringProperty(line, "organizationUuid"), out OrganisationId? organisation) ? organisation : null;

    private static string? StringProperty(string line, string name)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(line);

            return document.RootElement.ValueKind is JsonValueKind.Object
                && document.RootElement.TryGetProperty(name, out JsonElement value)
                && value.ValueKind is JsonValueKind.String
                ? value.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed record Stamp(long Length, DateTime WrittenUtc);

    private sealed record OwnerRead(SessionOwner Owner, Stamp Stamp);
}
