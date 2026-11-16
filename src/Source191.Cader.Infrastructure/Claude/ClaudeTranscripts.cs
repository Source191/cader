using Source191.Cader.Domain;
using Source191.Cader.Ports;
using System.Text.Json;
namespace Source191.Cader.Adapters.Claude;

/// <summary>
/// Looks for <c>~\.claude\projects\&lt;project&gt;\&lt;sessionId&gt;.jsonl</c>. The project folder name encodes the
/// working directory, so every project folder is checked rather than reproducing that encoding. A transcript has
/// messages once it holds a user or assistant line: after <c>/bg</c> Claude writes a new session's transcript with only
/// its title and name (2.1.325), which <c>--resume</c> cannot find. Only each line's top-level type is read.
/// </summary>
public sealed class ClaudeTranscripts(string claudeHome) : ITranscripts
{
    public bool HasMessages(SessionId sessionId)
    {
        string projects = Path.Combine(claudeHome, "projects");
        if (!Directory.Exists(projects))
        {
            return false;
        }

        return Directory.EnumerateDirectories(projects)
            .Select(project => Path.Combine(project, $"{sessionId.Value}.jsonl"))
            .Where(File.Exists)
            .Any(HoldsAMessage);
    }

    /// <summary>A transcript that exists but cannot be read keeps the answer its existence gave before lines were read.</summary>
    private static bool HoldsAMessage(string transcript)
    {
        try
        {
            using FileStream stream = new(transcript, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using StreamReader reader = new(stream);

            return Lines(reader).Any(IsMessage);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return true;
        }
    }

    private static IEnumerable<string> Lines(StreamReader reader)
    {
        while (reader.ReadLine() is string line)
        {
            yield return line;
        }
    }

    private static bool IsMessage(string line)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(line);

            return document.RootElement.ValueKind is JsonValueKind.Object
                && document.RootElement.TryGetProperty("type", out JsonElement type)
                && type.ValueKind is JsonValueKind.String
                && type.GetString() is "user" or "assistant";
        }
        catch (JsonException)
        {
            return false;
        }
    }
}