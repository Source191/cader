using Microsoft.Extensions.Time.Testing;
using Source191.Cader.Application;
using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Application.Reporting;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application.Reporting;

public sealed class ProblemReportsShould
{
    private static readonly Started Start = new(new ToolVersion("0.17.0-preview.6"), new Version(2, 1, 312), new Version(10, 0, 26100), new Version(10, 0, 14), 120, 36);

    private readonly FakeTimeProvider clock = new(new DateTimeOffset(2026, 10, 29, 12, 0, 0, TimeSpan.Zero));
    private readonly ProcessCounts counts = new();
    private readonly FakeCrashFile crashFile = new();
    private readonly DiagnosticLog log;

    public ProblemReportsShould()
    {
        log = new DiagnosticLog(clock, []);
    }

    [Fact]
    public async Task BuildTheEnvironmentEventsCountsAndCrash()
    {
        string crashLine = CrashLine.At(new DateTimeOffset(2026, 10, 28, 9, 0, 1, TimeSpan.Zero));
        crashFile.Write($"2026-10-28T09:00:00.000Z view Sessions\n{crashLine}\n");
        log.Record(new ViewShown(ViewKind.Worktrees));
        counts.Add(ChildCommand.For(new ProcessRequest("git", ["status"], WorkingFolder.Inherit)), 0, TimeSpan.FromMilliseconds(12));
        CurrentSettings settings = TestSettings.Current(@"C:\Users\me\source\repos", @"D:\work");

        ProblemReport report = await new ProblemReports(log, counts, crashFile, Start, settings, clock).BuildAsync();

        report.HasCrash.ShouldBeTrue();
        report.Text.ShouldBe(string.Join('\n',
            "Environment",
            "Cader 0.17.0-preview.6, Claude Code 2.1.312, Windows 10.0.26100, .NET 10.0.14, terminal 120x36",
            "Settings: editor VsCode, update checks on, 2 repository roots",
            string.Empty,
            "Last crash",
            "2026-10-28T09:00:00.000Z view Sessions",
            crashLine,
            string.Empty,
            "Recent events",
            $"2026-10-29T12:00:00.000Z [{Environment.ProcessId}] view Worktrees",
            string.Empty,
            "Child program counts",
            $"2026-10-29T12:00:00.000Z [{Environment.ProcessId}] counts git status exit 0: 1 run, median 12 ms, p95 12 ms, longest 12 ms"));
    }

    [Fact]
    public async Task LeaveOutTheCrashHeadingWithoutACrash()
    {
        ProblemReport report = await new ProblemReports(log, counts, crashFile, Start, TestSettings.Current(@"C:\Users\me\source\repos"), clock).BuildAsync();

        report.HasCrash.ShouldBeFalse();
        report.Text.ShouldNotContain("Last crash");
        report.Text.ShouldContain("Settings: editor VsCode, update checks on, 1 repository root");
    }

    [Fact]
    public async Task SayWhenUpdateChecksAreOffAndASingleRootIsOne()
    {
        Settings off = Settings.Resolve([FolderPath.From(@"C:\Users\me\source\repos")], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.VsCode, WorktreeBase.RemoteDefault, TestSettings.Home, checkForUpdates: false, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset);

        ProblemReport report = await new ProblemReports(log, counts, crashFile, Start, TestSettings.Current(off), clock).BuildAsync();

        report.Text.Split('\n')[2].ShouldBe("Settings: editor VsCode, update checks off, 1 repository root");
    }

    [Fact]
    public async Task SayManyRootsInThePlural()
    {
        ProblemReport report = await new ProblemReports(log, counts, crashFile, Start, TestSettings.Current(@"C:\Users\me\source\repos", @"D:\work", @"E:\more"), clock).BuildAsync();

        report.Text.Split('\n')[2].ShouldBe("Settings: editor VsCode, update checks on, 3 repository roots");
    }

    [Fact]
    public async Task LeaveLinesOutInTheOrderOfTheirBands()
    {
        string crashLine = CrashLine.At(new DateTimeOffset(2026, 10, 28, 9, 0, 1, TimeSpan.Zero));
        crashFile.Write(string.Join('\n', [.. Enumerable.Range(0, 3).Select(index => $"earlier crash line {index} {new string('c', 100)}"), crashLine]) + "\n");
        log.Record(new ViewShown(ViewKind.Sessions));
        clock.Advance(TimeSpan.FromSeconds(1));
        log.Record(new Failed(FailureSite.Load, null, ExceptionSummary.From(new GitException("x", inUse: false))));
        clock.Advance(TimeSpan.FromSeconds(1));
        log.Record(new ViewShown(ViewKind.Worktrees));
        clock.Advance(TimeSpan.FromSeconds(1));
        log.Record(new Failed(FailureSite.Effect, null, ExceptionSummary.From(new GitException("y", inUse: false))));
        clock.Advance(TimeSpan.FromSeconds(1));
        log.Record(new ViewShown(ViewKind.Config));
        counts.Add(ChildCommand.For(new ProcessRequest("git", ["status"], WorkingFolder.Inherit)), 0, TimeSpan.FromMilliseconds(5));
        counts.Add(ChildCommand.For(new ProcessRequest("git", ["status"], WorkingFolder.Inherit)), 128, TimeSpan.FromMilliseconds(5));
        counts.Add(ChildCommand.For(new ProcessRequest("git", ["log"], WorkingFolder.Inherit)), 0, TimeSpan.FromMilliseconds(5));
        counts.Add(ChildCommand.For(new ProcessRequest("git", ["log"], WorkingFolder.Inherit)), 128, TimeSpan.FromMilliseconds(5));
        ReportTitle title = ReportTitle.From("Attach froze");
        ProblemReport report = await new ProblemReports(log, counts, crashFile, Start, TestSettings.Current(@"C:\Users\me\source\repos"), clock).BuildAsync();
        string[] all = report.Text.Split('\n');
        string[] inOrder =
        [
            .. all.Where(line => line.StartsWith("earlier crash line", StringComparison.Ordinal)),
            .. all.Where(line => line.Contains(" view ", StringComparison.Ordinal)),
            .. all.Where(line => line.Contains(" counts ", StringComparison.Ordinal) && line.Contains("exit 0", StringComparison.Ordinal)),
            .. all.Where(line => line.Contains(" failed ", StringComparison.Ordinal) && line != crashLine),
            .. all.Where(line => line.Contains(" counts ", StringComparison.Ordinal) && line.Contains("exit 128", StringComparison.Ordinal)),
        ];

        inOrder.Length.ShouldBe(12);
        foreach (int dropped in Enumerable.Range(1, inOrder.Length))
        {
            string expected = string.Join('\n', all.Where(line => !inOrder.Take(dropped).Contains(line))) + (dropped == 1 ? "\n(1 line left out to fit the page address)" : $"\n({dropped} lines left out to fit the page address)");

            report.FitWithin(title, ProblemReportAddress.Length(title, expected)).ShouldBe(expected, $"after {dropped} lines are left out");
        }
    }
}