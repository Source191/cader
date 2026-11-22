using Microsoft.Extensions.Time.Testing;
using Source191.Cader.Adapters.Git;
using Source191.Cader.Application;
using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;
using System.Globalization;

namespace Source191.Cader.Core.Tests.Application;

/// <summary>The branch and origin rows over a scripted git: the branch listing, the counts and the pull request lookups.</summary>
public sealed class WorktreeInventoryBranchesShould : IDisposable
{
    private const string Root = @"C:\Users\me\source\repos";
    private const string Repository = @"C:\Users\me\source\repos\Northwind.Orders";
    private const string Worktree = @"C:\Users\me\source\repos\Northwind.Orders-fix-123";
    private const string Email = "me@northwind.example";
    private const string TipAt = "2026-11-10T09:00:00+00:00";

    private const string MainOnMainAndFix123InItsWorktree = "worktree C:/Users/me/source/repos/Northwind.Orders\nHEAD a\nbranch refs/heads/main\n\nworktree C:/Users/me/source/repos/Northwind.Orders-fix-123\nHEAD b\nbranch refs/heads/fix-123\n";

    private readonly FakeFolders folders = new FakeFolders().WithDirectory(Path.Combine(Repository, ".git", "objects")).WithDirectory(Worktree);
    private readonly FakeProcessRunner processes = new FakeProcessRunner()
        .AnswerAll(new ProcessResult(0, string.Empty, string.Empty))
        .Answer(request => request.Arguments.Contains("--porcelain"), new ProcessResult(0, MainOnMainAndFix123InItsWorktree, string.Empty))
        .Answer(request => request.Arguments.SequenceEqual(["config", "--get", "user.email"]), new ProcessResult(0, Email + "\n", string.Empty))
        .Answer(request => request.Arguments.Contains("symbolic-ref"), new ProcessResult(0, "origin/main\n", string.Empty))
        .Answer(request => request.Arguments.Contains("rev-list") && request.Arguments.Contains("--count"), new ProcessResult(0, "0\n", string.Empty));

    private static readonly RemoteRepository.GitHub Orders = new("northwind", "Northwind.Orders");
    private static readonly CommitId MergedHead = CommitId.From("afdacd8b1c2e3f4a5b6c7d8e9f0a1b2c3d4e5f60");

    private readonly FakePullRequests hosts = new();
    private readonly PullRequestCatalogue catalogue;
    private readonly OriginPruner pruner;
    private readonly List<ProcessRequest> started = [];

    public WorktreeInventoryBranchesShould()
    {
        FakeProcessRunner origin = new FakeProcessRunner().AnswerAll(new ProcessResult(0, string.Empty, string.Empty)).Answer(request => request.Arguments.SequenceEqual(["remote", "get-url", "origin"]), new ProcessResult(0, "git@github.com:northwind/Northwind.Orders.git\n", string.Empty));
        catalogue = new(new GitCli(origin, "git"), hosts, new FakeTimeProvider(Fixtures.Noon), new RefreshCounts(new FakeTimeProvider(Fixtures.Noon), new DiagnosticLog(new FakeTimeProvider(Fixtures.Noon), [])));
        pruner = new(new GitCli(processes, "git"), new FakeTimeProvider(Fixtures.Noon), new DiagnosticLog(new FakeTimeProvider(Fixtures.Noon), []));
        // Recorded as each run starts, on the caller's path, so a prune the scan starts and never awaits is seen as the scan returns.
        processes.WhileRunning = request =>
        {
            lock (started)
            {
                started.Add(request);
            }

            return Task.CompletedTask;
        };
    }

    private WorktreeInventory Inventory => new(new GitCli(processes, "git"), folders, TestSettings.Current(Root), catalogue, pruner, new DiagnosticLog(new FakeTimeProvider(Fixtures.Noon), []));

    [Fact]
    public async Task ListABranchCheckedOutNowhereAsARowOfItsOwnWithNoFolder()
    {
        Branches(Local("main", "refs/remotes/origin/main"), Local("fix-123", "refs/remotes/origin/fix-123"), Local("spike-refunds", string.Empty));

        IReadOnlyList<WorktreeRow> rows = await Inventory.ScanAsync([], CancellationToken.None);

        rows.Count.ShouldBe(2, "the fix-123 worktree and the spike-refunds branch; main and fix-123 are checked out");
        WorktreeRow branch = rows.Single(row => row.Health.Kind == WorktreeHealthKind.Branch);
        branch.Health.ShouldBe(WorktreeHealth.CheckedOutNowhere);
        branch.Placement.ShouldBe(Placement.NoFolder);
        branch.Head.ShouldBe(new Head.OnBranch(BranchName.From("spike-refunds")));
        branch.Path.ShouldBe(FolderPath.From(Repository));
        branch.Repository.ShouldBe(FolderPath.From(Repository));
        branch.OwnerExists.ShouldBeTrue();
        branch.FolderExists.ShouldBeTrue();
        branch.Key.ShouldBe($@"{FolderPath.From(Repository).Key}|branch|spike-refunds");
        branch.LastCommitAt.ShouldBe(new LastCommit.Made(DateTimeOffset.Parse(TipAt, System.Globalization.CultureInfo.InvariantCulture)));
        branch.Session.ShouldBe(new WorktreeSession.None());
        rows.Single(row => row.Health.Kind == WorktreeHealthKind.Ok).Placement.ShouldBe(Placement.Folder);
        rows.Single(row => row.Health.Kind == WorktreeHealthKind.Ok).Key.ShouldBe(FolderPath.From(Worktree).Key);
    }

    [Fact]
    public async Task LeaveOutTheBranchThatTracksOriginsDefaultAndTheOneTheMainCheckoutHas()
    {
        processes.Answer(request => request.Arguments.Contains("--porcelain"), new ProcessResult(0, "worktree C:/Users/me/source/repos/Northwind.Orders\nHEAD a\nbranch refs/heads/fix-123\n", string.Empty));
        Branches(Local("main", "refs/remotes/origin/main"), Local("fix-123", "refs/remotes/origin/fix-123"));

        IReadOnlyList<WorktreeRow> rows = await Inventory.ScanAsync([], CancellationToken.None);

        rows.ShouldBeEmpty();
    }

    [Fact]
    public async Task NotLendASessionInTheMainCheckoutToABranchRow()
    {
        Branches(Local("spike-refunds", string.Empty));
        Session session = new Session.Interactive(SessionId.From("81cdb0f9-0000-4000-8000-000000000001"), Listed.As("orders"), FolderPath.From(Repository), Fixtures.Noon, DisplayState.Stopped, ProcessId: 4242);

        IReadOnlyList<WorktreeRow> rows = await Inventory.ScanAsync([session], CancellationToken.None);

        rows.Single(row => row.Health.Kind == WorktreeHealthKind.Branch).Session.ShouldBe(new WorktreeSession.None());
    }

    [Fact]
    public async Task ListNoBranchRowsWhenGitCannotSayWhichBranchesAreCheckedOut()
    {
        processes.Answer(request => request.Arguments.Contains("--porcelain"), new ProcessResult(128, string.Empty, "fatal: not a git repository"));
        Branches(Local("main", "refs/remotes/origin/main"), Local("spike-refunds", string.Empty));

        IReadOnlyList<WorktreeRow> rows = await Inventory.ScanAsync([], CancellationToken.None);

        rows.ShouldBeEmpty();
    }

    [Fact]
    public async Task LookUpABranchOfYoursOnlyOnOriginButListNoRowForItUntilItsAnswerLands()
    {
        hosts.Answer(Orders, Listing(("c-new-bug", PullRequestState.Closed, new HeadCommit.NotRecorded())));
        MainCheckoutOnly();
        Branches(Origin("c-new-bug"));
        // Held, so the lookup the scan starts and never awaits cannot land before the scan reads its rows.
        hosts.Hold();

        IReadOnlyList<WorktreeRow> rows = await Inventory.ScanAsync([], CancellationToken.None);
        await hosts.WhenCalledAsync(1).WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        hosts.Release();

        rows.ShouldBeEmpty();
        hosts.Requests.ShouldBe([(Orders, BranchName.From("c-new-bug"))]);
    }

    [Fact]
    public async Task ListABranchOfYoursOnlyOnOriginLeftBehindByItsMergedPullRequest()
    {
        hosts.Answer(Orders, Listing(("c-new-bug", PullRequestState.Merged, new HeadCommit.Known(MergedHead))));
        MainCheckoutOnly();
        Branches(Origin("c-new-bug"));
        AnswerWorkAfterTheMergedHead("refs/remotes/origin/c-new-bug", commits: 1);
        await Landed("c-new-bug");

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single();

        row.Health.ShouldBe(WorktreeHealth.OnlyOnOrigin);
        row.Placement.ShouldBe(Placement.NoFolder);
        row.Head.ShouldBe(new Head.OnBranch(BranchName.From("c-new-bug")));
        row.Key.ShouldBe($"{FolderPath.From(Repository).Key}|branch|c-new-bug");
        row.Flag.ShouldBe(BranchFlagKind.LeftBehind);
        row.CommitsAfterMerge.ShouldBe(1);
        row.UnpushedCommits.ShouldBe(0);
        row.OwnerExists.ShouldBeTrue();
        row.FolderExists.ShouldBeTrue();
    }

    [Fact]
    public async Task ListABranchOfYoursOnlyOnOriginWhosePullRequestWasClosed()
    {
        hosts.Answer(Orders, Listing(("old-spike", PullRequestState.Closed, new HeadCommit.NotRecorded())));
        MainCheckoutOnly();
        Branches(Origin("old-spike"));
        await Landed("old-spike");

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single();

        row.Health.ShouldBe(WorktreeHealth.OnlyOnOrigin);
        row.Flag.ShouldBe(BranchFlagKind.Closed);
    }

    [Fact]
    public async Task ListNoRowForABranchOnlyOnOriginMergedWithNothingBehind()
    {
        hosts.Answer(Orders, Listing(("fix-88", PullRequestState.Merged, new HeadCommit.Known(MergedHead))));
        MainCheckoutOnly();
        Branches(Origin("fix-88"));
        await Landed("fix-88");

        (await Inventory.ScanAsync([], CancellationToken.None)).ShouldBeEmpty();
    }

    [Fact]
    public async Task ListNoRowForSomeoneElsesBranchOnlyOnOrigin()
    {
        hosts.Answer(Orders, Listing(("dependabot/nuget/bump", PullRequestState.Closed, new HeadCommit.NotRecorded())));
        MainCheckoutOnly();
        Branches(Origin("dependabot/nuget/bump", email: "bot@example.invalid"));
        await Landed("dependabot/nuget/bump");

        (await Inventory.ScanAsync([], CancellationToken.None)).ShouldBeEmpty();
    }

    [Fact]
    public async Task ListNoRowForOriginsDefaultBranch()
    {
        hosts.Answer(Orders, Listing(("main", PullRequestState.Closed, new HeadCommit.NotRecorded())));
        processes.Answer(request => request.Arguments.Contains("--porcelain"), new ProcessResult(0, "worktree C:/Users/me/source/repos/Northwind.Orders\nHEAD a\nbranch refs/heads/fix-123\n", string.Empty));
        Branches(Local("fix-123", "refs/remotes/origin/fix-123"), Origin("main"));
        await Landed("main");

        (await Inventory.ScanAsync([], CancellationToken.None)).ShouldBeEmpty();
    }

    [Fact]
    public async Task FlagEveryRowThatNamesABranch()
    {
        hosts.Answer(Orders, new PullRequestListing.Listed([]));
        Branches(Local("fix-123", "refs/remotes/origin/fix-123", track: "[gone]"), Local("spike-refunds", string.Empty));
        await Landed("fix-123", "spike-refunds");

        IReadOnlyList<WorktreeRow> rows = await Inventory.ScanAsync([], CancellationToken.None);

        rows.Single(row => row.Health.Kind == WorktreeHealthKind.Ok).Flag.ShouldBe(BranchFlagKind.Gone);
        rows.Single(row => row.Health.Kind == WorktreeHealthKind.Branch).Flag.ShouldBe(BranchFlagKind.NoPullRequest);
    }

    [Fact]
    public async Task CountCommitsAfterOnlyAMergedPullRequestsKnownHead()
    {
        hosts.Answer(Orders, Listing(("fix-123", PullRequestState.Open, new HeadCommit.Known(MergedHead)), ("spike-refunds", PullRequestState.Merged, new HeadCommit.NotRecorded())));
        Branches(Local("fix-123", "refs/remotes/origin/fix-123"), Local("spike-refunds", "refs/remotes/origin/spike-refunds"));
        await Landed("fix-123", "spike-refunds");

        await Inventory.ScanAsync([], CancellationToken.None);

        processes.CapturedRequests.ShouldNotContain(request => request.Arguments.Contains(MergedHead.Value));
    }

    [Fact]
    public async Task CountCommitsAfterAMergedHeadOnTheLocalBranch()
    {
        hosts.Answer(Orders, Listing(("fix-123", PullRequestState.Merged, new HeadCommit.Known(MergedHead))));
        Branches(Local("fix-123", "refs/remotes/origin/fix-123"));
        AnswerWorkAfterTheMergedHead("refs/heads/fix-123", commits: 2);
        await Landed("fix-123");

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single();

        row.Flag.ShouldBe(BranchFlagKind.LeftBehind);
    }

    [Fact]
    public async Task CarryABranchRowsCommitsOnNoRemoteAsItsUnpushedCommits()
    {
        Branches(Local("spike-refunds", string.Empty));
        processes.Answer(request => request.Arguments.SequenceEqual(["rev-list", "--count", "refs/heads/spike-refunds", "--not", "--remotes"]), new ProcessResult(0, "2\n", string.Empty));

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single(row => row.Health.Kind == WorktreeHealthKind.Branch);

        row.UnpushedCommits.ShouldBe(2);
    }

    [Fact]
    public async Task CountNoCommitsAtRiskOnABranchRowWhenTheyAreInsideItsMergedHead()
    {
        hosts.Answer(Orders, Listing(("spike-refunds", PullRequestState.Merged, new HeadCommit.Known(MergedHead))));
        Branches(Local("spike-refunds", "refs/remotes/origin/spike-refunds", track: "[gone]"));
        processes.Answer(request => request.Arguments.SequenceEqual(["rev-list", "--count", "refs/heads/spike-refunds", "--not", "--remotes"]), new ProcessResult(0, "3\n", string.Empty));
        await Landed("spike-refunds");

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single(row => row.Health.Kind == WorktreeHealthKind.Branch);

        row.UnpushedCommits.ShouldBe(0);
        row.Flag.ShouldBe(BranchFlagKind.Gone);
    }

    [Fact]
    public async Task KeepABranchRowsCommitsAtRiskWhenTheMergedHeadCannotBeCounted()
    {
        hosts.Answer(Orders, Listing(("spike-refunds", PullRequestState.Merged, new HeadCommit.Known(MergedHead))));
        Branches(Local("spike-refunds", "refs/remotes/origin/spike-refunds", track: "[gone]"));
        processes.Answer(request => request.Arguments.SequenceEqual(["rev-list", "--count", "refs/heads/spike-refunds", "--not", "--remotes"]), new ProcessResult(0, "3\n", string.Empty));
        processes.Answer(request => request.Arguments.Contains(MergedHead.Value), new ProcessResult(128, string.Empty, "fatal: bad revision"));
        await Landed("spike-refunds");

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single(row => row.Health.Kind == WorktreeHealthKind.Branch);

        row.UnpushedCommits.ShouldBe(3, "a head git has not got says nothing about what is safe");
    }

    [Fact]
    public async Task LeaveOutABranchRowWhoseCommitsAtRiskCannotBeCounted()
    {
        Branches(Local("spike-refunds", string.Empty), Local("old-idea", string.Empty));
        processes.Answer(request => request.Arguments.SequenceEqual(["rev-list", "--count", "refs/heads/spike-refunds", "--not", "--remotes"]), new ProcessResult(128, string.Empty, "fatal: bad revision"));

        IReadOnlyList<WorktreeRow> rows = await Inventory.ScanAsync([], CancellationToken.None);

        rows.Where(row => row.Health.Kind == WorktreeHealthKind.Branch).Select(row => row.Head).ShouldBe([new Head.OnBranch(BranchName.From("old-idea"))], "a count of nothing at risk is never guessed");
    }

    [Fact]
    public async Task ListBranchesOncePerRepositoryAndNeverFetch()
    {
        Branches(Local("spike-refunds", string.Empty), Origin("c-new-bug"));

        await Inventory.ScanAsync([], CancellationToken.None);

        processes.CapturedRequests.Count(request => request.Arguments[0] == "for-each-ref").ShouldBe(1);
        processes.CapturedRequests.ShouldNotContain(request => request.Arguments.Contains("fetch") || request.Arguments.Contains("pull") || request.Arguments.Contains("ls-remote"));
    }

    [Fact]
    public async Task PruneTheRepositoryOfAnOriginRowAfterTheScan()
    {
        hosts.Answer(Orders, Listing(("c-new-bug", PullRequestState.Closed, new HeadCommit.NotRecorded())));
        MainCheckoutOnly();
        Branches(Origin("c-new-bug"));
        await Landed("c-new-bug");

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single();

        row.Health.Kind.ShouldBe(WorktreeHealthKind.Origin);
        Prunes().ShouldBe([Repository]);
    }

    [Fact]
    public async Task PruneNothingForAnOriginCandidateThatIsNotShown()
    {
        hosts.Answer(Orders, Listing(("c-new-bug", PullRequestState.Open, new HeadCommit.NotRecorded())));
        MainCheckoutOnly();
        Branches(Origin("c-new-bug"));
        await Landed("c-new-bug");

        (await Inventory.ScanAsync([], CancellationToken.None)).ShouldBeEmpty();

        Prunes().ShouldBeEmpty();
    }

    [Fact]
    public async Task PruneNothingWhenTheScanShowsOnlyWorktreeAndBranchRows()
    {
        Branches(Local("main", "refs/remotes/origin/main"), Local("fix-123", "refs/remotes/origin/fix-123"), Local("spike-refunds", string.Empty));

        (await Inventory.ScanAsync([], CancellationToken.None)).Count.ShouldBe(2);

        Prunes().ShouldBeEmpty();
    }

    public void Dispose() => catalogue.Dispose();

    private IReadOnlyList<string> Prunes()
    {
        lock (started)
        {
            return [.. started.Where(request => request.Arguments.SequenceEqual(["remote", "prune", "origin"])).Select(request => request.WorkingDirectory.ShouldBeOfType<WorkingFolder.At>().Path)];
        }
    }
    private void MainCheckoutOnly() =>
        processes.Answer(request => request.Arguments.Contains("--porcelain"), new ProcessResult(0, "worktree C:/Users/me/source/repos/Northwind.Orders\nHEAD a\nbranch refs/heads/main\n", string.Empty));

    private void Branches(params string[] refs) =>
        processes.Answer(request => request.Arguments.Count > 0 && request.Arguments[0] == "for-each-ref", new ProcessResult(0, string.Concat(refs), string.Empty));

    private static string Local(string name, string upstream, string track = "", string email = Email) => $"refs/heads/{name}\0{upstream}\0{track}\0{TipAt}\0<{email}>\0\n";

    private static string Origin(string name, string email = Email) => $"refs/remotes/origin/{name}\0\0\0{TipAt}\0<{email}>\0\n";

    private static PullRequestListing.Listed Listing(params (string Branch, PullRequestState State, HeadCommit Head)[] pullRequests) =>
        new([.. pullRequests.Select((pullRequest, index) => new BranchPullRequest(BranchName.From(pullRequest.Branch), new PullRequest(PullRequestHost.GitHub, 290 + index, new Uri($"https://github.com/northwind/Northwind.Orders/pull/{290 + index}"), pullRequest.State, Fixtures.Noon.AddDays(-index - 1), pullRequest.Head, new RecordedTime.NotRecorded())))]);

    private Task Landed(params string[] branches) => catalogue.RefreshStaleAsync([.. branches.Select(branch => (FolderPath.From(Repository), BranchName.From(branch)))]);

    /// <summary>Has git list that many commits after the merged head, each of them work origin's default branch lacks.</summary>
    private void AnswerWorkAfterTheMergedHead(string revision, int commits)
    {
        string listed = string.Concat(Enumerable.Range(1, commits).Select(commit => commit.ToString("x40", CultureInfo.InvariantCulture) + "\n"));
        processes.Answer(request => request.Arguments.SequenceEqual(["rev-list", revision, "--not", MergedHead.Value]), new ProcessResult(0, listed, string.Empty));
        processes.Answer(request => request.Arguments.SequenceEqual(["rev-list", "--no-merges", "--cherry-pick", "--right-only", $"refs/remotes/origin/HEAD...{revision}"]), new ProcessResult(0, listed, string.Empty));
    }
}