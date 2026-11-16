using Source191.Cader.Domain;
using Source191.Cader.Ports;
using System.Text.Json;
namespace Source191.Cader.Adapters.Claude;

/// <summary>
/// Looks for <c>~\.claude\projects\&lt;project&gt;\&lt;sessionId&gt;.jsonl</c>. The project folder name encodes the
/// working directory, so every project folder is checked rather than reproducing that encoding.
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

    private static bool HoldsAMessage(string transcript) => File.ReadLines(transcript).Any(IsMessage);

    private static bool IsMessage(string line)
    {
        using JsonDocument document = JsonDocument.Parse(line);

        return document.RootElement.GetProperty("type").GetString() is "user" or "assistant";
    }
}
