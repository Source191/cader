using Source191.Cader.Ports;

namespace Source191.Cader.TestDoubles;

/// <summary>Reads a claude request as the words a test cares about: a session's start and resume carry the settings that quiet its title, which most facts do not.</summary>
public static class ClaudeRequestWords
{
    public static IReadOnlyList<string> WithoutTitleSettings(this ProcessRequest request)
    {
        int at = request.Arguments.ToList().IndexOf("--settings");

        return at < 0 ? request.Arguments : [.. request.Arguments.Take(at), .. request.Arguments.Skip(at + 2)];
    }
}