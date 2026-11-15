using Source191.Cader.Application;
using Source191.Cader.Domain;

namespace Source191.Cader.TestDoubles;

/// <summary>Settings for tests: the given roots with every other setting at its default, held in memory.</summary>
public static class TestSettings
{
    public static FolderPath Home { get; } = FolderPath.From(@"C:\Users\me");

    /// <summary>Where the diagnostic log is written in the tests' made-up state folder.</summary>
    public static string DiagnosticLogFile { get; } = Home.Combine(Path.Combine("AppData", "Local", "Cader", "cader.log")).Value;
    public static ClaudeFolder DefaultClaudeFolder { get; } = new ClaudeFolder.Unset(Home.Combine(".claude"));

    public static Settings For(params string[] roots) => Settings.Resolve([.. roots.Select(root => FolderPath.From(root))], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.VsCode, WorktreeBase.RemoteDefault, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset);

    public static Settings For(string[] roots, string worktreeRoot, WorktreeBase defaultBase = WorktreeBase.RemoteDefault) => Settings.Resolve([.. roots.Select(root => FolderPath.From(root))], new WorktreeRootChoice.At(FolderPath.From(worktreeRoot)), EditorChoice.VsCode, defaultBase, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset);

    public static CurrentSettings Current(params string[] roots) => new(For(roots), new InMemoryState());

    public static CurrentSettings Current(Settings settings) => new(settings, new InMemoryState());
}