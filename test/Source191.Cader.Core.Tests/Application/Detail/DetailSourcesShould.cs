using Microsoft.Extensions.Time.Testing;
using Source191.Cader.Adapters.Git;
using Source191.Cader.Application;
using Source191.Cader.Application.Detail;
using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application.Detail;

public sealed class DetailSourcesShould
{
    private static readonly DateTimeOffset SummerNoon = new(2026, 7, 15, 12, 0, 0, TimeSpan.Zero);
    private static readonly TimeZoneInfo UnitedKingdom = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
    private static readonly FolderPath Worktree = FolderPath.From(@"C:\Users\me\source\worktrees\Northwind.Billing-invoice-pdf");
    private static readonly FolderPath Repository = FolderPath.From(@"C:\Users\me\source\repos\Northwind.Billing");

    private readonly FakeProcessRunner processes = new FakeProcessRunner().AnswerAll(new ProcessResult(0, string.Empty, string.Empty));
    private readonly FakeTimeProvider clock = new(Fixtures.Noon);

    [Fact]
    public async Task WordASessionsDetailKeyedByItsIdInItsFactRows()
    {
        SessionDetails source = new(Loader(), clock);
        Session.Background session = new(SessionId.From("81cdb0f9-0000-4000-8000-000000000001"), JobId.From("81cdb0f9"), Listed.As("invoice-pdf"), Worktree, Fixtures.Noon.AddMinutes(-5), DisplayState.Stopped, new JobProcess.None());
        SessionRow row = new(session, new SessionLocation.Outside("Northwind.Billing"), new Head.OnBranch(BranchName.From("invoice-pdf")), new LiveTab.None(), HasTranscript: true);

        DetailContent content = await source.ReadAsync(row, lines: 6, CancellationToken.None);

        source.MostRows.ShouldBe(Wording.SessionFactRows);
        source.KeyOf(row).ShouldBe(session.SessionId);
        content.Title.ShouldBe("invoice-pdf");
        content.Facts[0][1].Text.ShouldBe("11:55 (5m ago)");
    }

    [Fact]
    public async Task WordAWorktreesDetailKeyedByItsFolderWithRoomForItsListing()
    {
        WorktreeDetails source = new(Loader(), clock);
        WorktreeRow row = new(Repository, Worktree, new Head.OnBranch(BranchName.From("invoice-pdf")), new WorktreeHealth(WorktreeHealthKind.Ok, new HealthReason.None()), 0, 12, new LastCommit.Made(Fixtures.Noon), new WorktreeSession.None(), LeftoverFileCount: 0, OwnerExists: false, FolderExists: true);

        DetailContent content = await source.ReadAsync(row, lines: 10, CancellationToken.None);

        source.MostRows.ShouldBe(200);
        source.KeyOf(row).ShouldBe(Worktree.Key);
        content.Title.ShouldBe("Northwind.Billing-invoice-pdf");
        content.Facts[0][0].Text.ShouldBe("ok");
        processes.CapturedRequests.ShouldContain(request => request.Arguments.Count > 2 && request.Arguments[0] == "log" && request.Arguments[2] == "5");
    }

    [Theory]
    [InlineData(12, 10, "… 2 more")]
    [InlineData(8, 8, null)]
    public async Task ListAsManyItemsAsTheLinesHold(int unpushed, int listed, string? more)
    {
        processes.Answer(request => request.Arguments.Count > 2 && request.Arguments[0] == "log" && request.Arguments[1] == "-n", new ProcessResult(0, string.Join('\n', Enumerable.Range(1, listed).Select(commit => $"abc{commit:D4}\t2026-11-20T09:00:00+00:00\tCommit {commit}")), string.Empty));
        WorktreeDetails source = new(Loader(), clock);
        WorktreeRow row = new(Repository, Worktree, new Head.OnBranch(BranchName.From("invoice-pdf")), new WorktreeHealth(WorktreeHealthKind.Ok, new HealthReason.None()), 0, unpushed, new LastCommit.Made(Fixtures.Noon), new WorktreeSession.None(), LeftoverFileCount: 0, OwnerExists: false, FolderExists: true);

        DetailContent content = await source.ReadAsync(row, lines: 15, CancellationToken.None);

        processes.CapturedRequests.ShouldContain(request => request.Arguments.Count > 2 && request.Arguments[0] == "log" && request.Arguments[2] == "10");
        content.Facts.Count.ShouldBe(Wording.WorktreeFactRows);
        content.Below.Count(line => line.Text.Contains("Commit ", StringComparison.Ordinal)).ShouldBe(listed);
        content.Below.LastOrDefault(line => line.Text.Contains("more", StringComparison.Ordinal))?.Text.ShouldBe(more);
    }

    [Fact]
    public void KeyTwoBranchRowsOfOneRepositoryApart()
    {
        WorktreeDetails source = new(Loader(), clock);
        WorktreeRow spike = new(Repository, Repository, new Head.OnBranch(BranchName.From("spike-refunds")), WorktreeHealth.CheckedOutNowhere, 0, 0, new LastCommit.Made(Fixtures.Noon), new WorktreeSession.None(), LeftoverFileCount: 0, OwnerExists: true, FolderExists: true);
        WorktreeRow oldIdea = spike with { Head = new Head.OnBranch(BranchName.From("old-idea")) };

        source.KeyOf(oldIdea).ShouldBe(oldIdea.Key);
        source.KeyOf(oldIdea).ShouldNotBe(source.KeyOf(spike));
    }

    [Fact]
    public async Task WordASessionsStartInTheClocksLocalZone()
    {
        FakeTimeProvider summer = new(SummerNoon);
        summer.SetLocalTimeZone(UnitedKingdom);
        SessionDetails source = new(Loader(new InMemoryState(), summer), summer);
        Session.Background session = new(SessionId.From("81cdb0f9-0000-4000-8000-000000000001"), JobId.From("81cdb0f9"), Listed.As("invoice-pdf"), Worktree, SummerNoon.AddMinutes(-5), DisplayState.Stopped, new JobProcess.None());
        SessionRow row = new(session, new SessionLocation.Outside("Northwind.Billing"), new Head.OnBranch(BranchName.From("invoice-pdf")), new LiveTab.None(), HasTranscript: true);

        DetailContent content = await source.ReadAsync(row, lines: 6, CancellationToken.None);

        content.Facts[0][1].Text.ShouldBe("12:55 (5m ago)");
    }

    [Fact]
    public async Task WordAWorktreesCreationInTheClocksLocalZone()
    {
        FakeTimeProvider summer = new(SummerNoon);
        summer.SetLocalTimeZone(UnitedKingdom);
        InMemoryState state = new();
        await state.RecordCreatedWorktreeAsync(new CreatedWorktree(Worktree, BranchName.From("invoice-pdf"), Repository, new DateTimeOffset(2026, 7, 15, 8, 14, 0, TimeSpan.Zero)));
        WorktreeDetails source = new(Loader(state, summer), summer);
        WorktreeRow row = new(Repository, Worktree, new Head.OnBranch(BranchName.From("invoice-pdf")), new WorktreeHealth(WorktreeHealthKind.Ok, new HealthReason.None()), 0, 0, new LastCommit.Made(SummerNoon), new WorktreeSession.None(), LeftoverFileCount: 0, OwnerExists: true, FolderExists: true);

        DetailContent content = await source.ReadAsync(row, lines: 10, CancellationToken.None);

        content.Facts[2][1].Text.ShouldBe("by Cader, 09:14");
    }

    private DetailLoader Loader() => Loader(new InMemoryState(), clock);

    private DetailLoader Loader(InMemoryState state, TimeProvider time) => new(new GitCli(processes, "git"), state, new SessionActivity(time), new DiagnosticLog(time, []));
}