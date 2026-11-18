using Source191.Cader.Domain;
using System.Diagnostics;
using System.Globalization;

namespace Source191.Cader.Application.Diagnostics;

/// <summary>
/// The one text form of a diagnostic record, shared by the in-memory log, the log file and the crash
/// file. These lines are a maintainer's format rather than interface text, so they live here and not in Wording.
/// </summary>
public static class DiagnosticLine
{
    private const string FailedWord = "failed";
    private const string UnknownWord = "unknown";

    // Stryker disable once String : Accepted: a static initialiser runs once per test process and Stryker's MTP runner switches mutants without restarting it (stryker-mutator/stryker-net#3742), so the mutant cannot be attributed; DiagnosticLineShould.RecogniseACrashLineWhoseTextStartsAtTheCrashMarkerAndOneThatEndsAtIt pins the marker
    private static readonly string CrashMarker = $" {FailedWord} {FailureSite.Crash}";

    public static string Render(TimedEvent timed) => $"{Stamp(timed.At)} {Body(timed.Event)}";

    /// <summary>The child program counts, one line per program, subcommand and exit code, each with the time it was written.</summary>
    public static IReadOnlyList<string> Render(ProcessCounts counts, DateTimeOffset at) => [.. counts.Snapshot().Select(count => $"{Stamp(at)} counts since start {count.Command} exit {count.ExitCode}: {Figures(count.Durations, "run", "runs")}")];

    /// <summary>Every count, each line with the time it was written: the child programs, each refresh part that has run, and the UI thread's stalls even with none, so a file shows the heartbeat ran, then what Cader holds now.</summary>
    public static IReadOnlyList<string> Render(PerformanceCounts counts, DateTimeOffset at)
    {
        IEnumerable<string> refreshes = counts.Refreshes.Snapshot().Select(count => $"{Stamp(at)} refreshes since start {Name(count.Part)}: {Figures(count.Durations, "run", "runs")}");

        string stalls = $"{Stamp(at)} stalls since start ui thread: {Figures(counts.Stalls.Figures(), "stall", "stalls")}";

        string holding = $"{Stamp(at)} process now: {Holding(counts.Own.Read())}";

        return [.. Render(counts.Processes, at), .. refreshes, stalls, holding];
    }

    /// <summary>The figures of the process now line: thousands separated, memory as the settings row writes it, and unknown for what Windows refused.</summary>
    private static string Holding(OwnHolding holding) => $"{Handles(holding.Handles)} handles, {PrivateMemory(holding.Memory)} private memory";

    private static string Handles(Reading<int> reading) => reading switch
    {
        Reading<int>.Read read => read.Value.ToString("N0", CultureInfo.InvariantCulture),
        Reading<int>.Unknown => UnknownWord,
        _ => throw new UnreachableException(),
    };

    private static string PrivateMemory(Reading<MemorySize> reading) => reading switch
    {
        Reading<MemorySize>.Read read => Wording.Memory(read.Value),
        Reading<MemorySize>.Unknown => UnknownWord,
        _ => throw new UnreachableException(),
    };

    /// <summary>Whether a line is a crash: a failure at <see cref="FailureSite.Crash"/>, with or without a What after the site.</summary>
    public static bool IsCrashLine(string line)
    {
        int start = line.IndexOf(CrashMarker, StringComparison.Ordinal);
        if (start < 0)
        {
            return false;
        }

        int end = start + CrashMarker.Length;

        return end == line.Length || line[end] is ':' or ' ';
    }

    /// <summary>The start facts without the time, as a problem report's environment line shows them.</summary>
    public static string Environment(Started started) => $"Cader {started.Tool.Text}, Claude Code {started.ClaudeCode?.ToString() ?? "unknown"}, Windows {started.Windows}, .NET {started.Runtime}, terminal {started.Columns}x{started.Rows}";

    private static string Body(DiagnosticEvent diagnostic) => diagnostic switch
    {
        Started started => $"started {Environment(started)}",
        ViewShown view => $"view {view.View}",
        DialogShown dialog => $"dialog {dialog.Dialog}",
        PackagedNotesUnreadable unreadable => $"no readable release notes in the package of {unreadable.Running.Text}",
        EffectRan ran => $"effect {ran.Effect} -> {ran.Outcome} in {Milliseconds(ran.Duration)}",
        WizardOpened opened => $"wizard opened offering {Count(opened.Repositories, "repository", "repositories")}",
        DiagnosticLogCleared cleared => $"diagnostic log cleared: {Count(cleared.FilesRemoved, "file", "files")}, {cleared.BytesRemoved.ToString(CultureInfo.InvariantCulture)} bytes, {cleared.FilesKept.ToString(CultureInfo.InvariantCulture)} kept",
        WizardFilterStarted filter => $"wizard filter first changed after {Milliseconds(filter.After)}: {filter.FirstCharacter}, {Count(filter.Characters, "character", "characters")}, {(filter.ListFilled ? "list filled" : "list not filled")}",

        ProcessSlow slow => $"slow {slow.Command} exit {slow.ExitCode} in {Milliseconds(slow.Duration)}",
        RefreshSlow slow => $"slow refresh {Name(slow.Part)} in {Milliseconds(slow.Duration)}",
        WatchTrouble => "watch",
        UiThreadStalled stalled => $"stalled ui thread for {Milliseconds(stalled.Lateness)}, no marked work",
        UiThreadStalledRunning stalled => $"stalled ui thread for {Milliseconds(stalled.Lateness)}, longest marked work {stalled.Work} {Milliseconds(stalled.Took)}",
        ProcessTimedOut timedOut => $"timed out {timedOut.Command} after {Milliseconds(timedOut.Limit)}",
        ProcessFailedToStart failed => $"not started {failed.Command}: {failed.Failure}",
        Failed failed => failed.What is null ? $"{FailedWord} {failed.Site}: {failed.Failure}" : $"{FailedWord} {failed.Site} {failed.What}: {failed.Failure}",
        TerminalGuiReported reported => $"terminal.gui {reported.Level}{(reported.Raiser is null ? string.Empty : $" from {reported.Raiser}")}{(reported.Failure is null ? string.Empty : $": {reported.Failure}")}",
        _ => throw new UnreachableException(),
    };

    private static string Name(RefreshPart part) => part switch
    {
        RefreshPart.SessionsListing => "sessions listing",
        RefreshPart.SessionsEnrichment => "sessions enrichment",
        RefreshPart.WorktreesScan => "worktrees scan",
        RefreshPart.PullRequestRefresh => "pull-request refresh",
    };

    /// <summary>The UTC time and Cader's own process id, so the lines of several processes sharing one file can be told apart.</summary>
    private static string Stamp(DateTimeOffset at) => $"{at.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture)} [{ProcessId}]";

    private static string ProcessId => System.Environment.ProcessId.ToString(CultureInfo.InvariantCulture);

    /// <summary>The number of runs, then the typical and longest durations when there were any.</summary>
    private static string Figures(DurationFigures figures, string one, string many)
    {
        string runs = Count(figures.Runs, one, many);

        return figures.Runs == 0
            ? runs
            : $"{runs}, median {Milliseconds(figures.Median)}, p95 {Milliseconds(figures.NinetyFifth)}, longest {Milliseconds(figures.Longest)}";
    }

    private static string Count(int count, string one, string many) => count == 1 ? $"1 {one}" : $"{count.ToString(CultureInfo.InvariantCulture)} {many}";

    private static string Milliseconds(TimeSpan duration) => $"{((long)duration.TotalMilliseconds).ToString(CultureInfo.InvariantCulture)} ms";
}