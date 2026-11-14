using Source191.Cader.Adapters.Claude;
using Source191.Cader.Adapters.Files;
using Source191.Cader.Adapters.Terminal;
using Source191.Cader.Application;
using Source191.Cader.Application.Actions;
using Source191.Cader.Application.Launch;
using Source191.Cader.Domain;

namespace Source191.Cader.Composition;

/// <summary><c>cader here</c>: starts a session in the current folder and attaches this terminal to it.</summary>
internal static class HereComposition
{
    public static async Task<int> RunAsync(SharedParts parts, LaunchParts launch, HereName name, string prompt, CancellationToken cancellationToken)
    {
        ClaudeCli claude = launch.Claude;
        LaunchService launchService = launch.Launches;
        // The settings are read once, so the Diagnostic log setting decides whether this run is written to the log file; a change made in a console
        // meanwhile applies to the next run, as the two processes share no memory.
        CurrentSettings settings = await CurrentSettings.LoadAsync(parts.Store, FolderPath.From(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)));
        parts.LogSwitch.PointAt(() => settings.Value.DiagnosticLog);
        // here does not ask Claude Code for its version, so as not to slow the attach down.
        parts.RecordStart(new ClaudeVersionReport.NotRead());
        TerminalConsole hereConsole = new(Console.Out, Wording.TabTitle);
        HereCommandRunner runner = new(new Folders(), launchService, new TerminalEffects(claude, hereConsole), hereConsole, Console.Out, Console.Error);

        return await runner.RunAsync(Directory.GetCurrentDirectory(), name, prompt, cancellationToken);
    }
}