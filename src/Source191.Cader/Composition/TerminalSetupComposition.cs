using Source191.Cader.Adapters.Claude;
using Source191.Cader.Adapters.Files;
using Source191.Cader.Application.TerminalSetup;

namespace Source191.Cader.Composition;

/// <summary><c>cader setup-terminal</c>: needs neither Claude Code nor a terminal tab, so Claude Code is not located.</summary>
internal static class TerminalSetupComposition
{
    public static async Task<int> RunAsync(SharedParts parts, TerminalSetupRequest request, CancellationToken cancellationToken)
    {
        string windowsMedia = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Media");
        TerminalSetupRunner setupRunner = new(new Folders(), parts.TerminalFragment, ClaudeSettings.ForCurrentUser(), parts.ConsoleExecutable, windowsMedia, Console.In, Console.Out, Console.Error);

        return await setupRunner.RunAsync(request, cancellationToken);
    }
}