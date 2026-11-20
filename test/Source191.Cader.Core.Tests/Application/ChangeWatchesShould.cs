using Microsoft.Extensions.Time.Testing;
using Source191.Cader.Application;
using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application;

public sealed class ChangeWatchesShould : IDisposable
{
    private const string Repos = @"C:\Users\me\source\repos";

    private static readonly FolderPath Sessions = FolderPath.From(@"C:\Users\me\.claude\sessions");
    private static readonly FolderPath Jobs = FolderPath.From(@"C:\Users\me\.claude\jobs");
    private static readonly FolderPath Orders = FolderPath.From(@$"{Repos}\Northwind.Orders");
    private static readonly FolderPath Billing = FolderPath.From(@$"{Repos}\Northwind.Billing");

    private readonly FakeTimeProvider clock = new(Fixtures.Noon);
    private readonly FakeFolderChanges changes = new();
    private readonly RecordingSink sink = new();
    private readonly RefreshSchedule sessions;
    private readonly RefreshSchedule worktrees;
    private readonly ChangeWatches watches;

    public ChangeWatchesShould()
    {
        sessions = new RefreshSchedule(() => RefreshInterval.Off, clock, new LaunchHold());
        worktrees = new RefreshSchedule(() => RefreshInterval.Off, clock, new LaunchHold());
        watches = new ChangeWatches(changes, sessions, worktrees, new DiagnosticLog(clock, [sink]));
    }

    [Fact]
    public void StartAWatchForEachPlannedFolder()
    {
        watches.Follow(Plan(Orders));

        changes.Live.ShouldBe([(Sessions, false), (Jobs, true), (Orders.Combine(".git"), true), (FolderPath.From(Repos), false)], ignoreOrder: true);
    }

    [Fact]
    public void StopWatchingAFolderNoLongerPlanned()
    {
        watches.Follow(Plan(Orders, Billing));

        watches.Follow(Plan(Orders));

        changes.Live.Select(watch => watch.Folder).ShouldNotContain(Billing.Combine(".git"));
        changes.Live.Count.ShouldBe(4);
    }

    [Fact]
    public void LeaveAWatchStillPlannedAlone()
    {
        watches.Follow(Plan(Orders));

        watches.Follow(Plan(Orders, Billing));

        changes.Started.Count.ShouldBe(5);
        changes.Live.Count.ShouldBe(5);
    }

    [Fact]
    public void TellTheSessionsScheduleOfAChangeThatCounts()
    {
        watches.Follow(Plan(Orders));

        changes.Raise(Jobs, new FolderChange(@"aba4b38e\state.json", FolderChangeKind.Renamed));
        clock.Advance(TimeSpan.FromSeconds(2));

        sessions.IsDueForChange().ShouldBeTrue();
        worktrees.IsDueForChange().ShouldBeFalse();
    }

    [Fact]
    public void TellTheWorktreesScheduleOfAChangeThatCounts()
    {
        watches.Follow(Plan(Orders));

        changes.Raise(Orders.Combine(".git"), new FolderChange(@"worktrees\feature", FolderChangeKind.Created));
        clock.Advance(TimeSpan.FromSeconds(2));

        worktrees.IsDueForChange().ShouldBeTrue();
        sessions.IsDueForChange().ShouldBeFalse();
    }

    [Fact]
    public void TellNoScheduleOfAChangeThatDoesNotCount()
    {
        watches.Follow(Plan(Orders));

        changes.Raise(Orders.Combine(".git"), new FolderChange("index", FolderChangeKind.Changed));
        changes.Raise(Sessions, new FolderChange("4242.1f9c03.key", FolderChangeKind.Created));
        clock.Advance(TimeSpan.FromSeconds(2));

        worktrees.IsDueForChange().ShouldBeFalse();
        sessions.IsDueForChange().ShouldBeFalse();
    }

    [Fact]
    public void RecordAWatchWindowsRefusesAndStartTheRest()
    {
        changes.Refused.Add(Orders.Combine(".git"));

        watches.Follow(Plan(Orders, Billing));

        changes.Live.Count.ShouldBe(4);
        sink.Lines.ShouldHaveSingleItem().ShouldEndWith("] watch Repository CouldNotStart");
    }

    [Fact]
    public void TryARefusedWatchAgainOnlyOnceItHasLeftThePlan()
    {
        changes.Refused.Add(Orders.Combine(".git"));
        watches.Follow(Plan(Orders));
        watches.Follow(Plan(Orders));
        changes.Refused.Clear();

        watches.Follow(Plan());
        watches.Follow(Plan(Orders));

        sink.Lines.Count.ShouldBe(1);
        changes.Live.Select(watch => watch.Folder).ShouldContain(Orders.Combine(".git"));
    }

    [Fact]
    public void RecordAnOverflowAndRefreshForIt()
    {
        watches.Follow(Plan(Orders));

        changes.Raise(Jobs, new FolderChange(string.Empty, FolderChangeKind.Overflowed));
        clock.Advance(TimeSpan.FromSeconds(2));

        sessions.IsDueForChange().ShouldBeTrue();
        sink.Lines.ShouldHaveSingleItem().ShouldEndWith("] watch ClaudeJobs Overflowed");
    }

    [Fact]
    public void StopEveryWatchWhenDisposed()
    {
        watches.Follow(Plan(Orders));

        watches.Dispose();

        changes.Live.ShouldBeEmpty();
    }

    public void Dispose()
    {
        watches.Dispose();
    }

    private static IReadOnlyList<PlannedWatch> Plan(params FolderPath[] repositories)
    {
        return WatchPlan.For(TestSettings.For(Repos), TestSettings.DefaultClaudeFolder, repositories);
    }
}