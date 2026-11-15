using Microsoft.Extensions.Time.Testing;
using Source191.Cader.Adapters.Git;
using Source191.Cader.Application;
using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

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
        .Answer(request => request.Arguments.Contains("rev-list"), new ProcessResult(0, "0\n", string.Empty));

    private readonly PullRequestCatalogue catalogue = new(new GitCli(new FakeProcessRunner().AnswerAll(new ProcessResult(2, string.Empty, string.Empty)), "git"), new FakePullRequests(), new FakeTimeProvider(Fixtures.Noon), new RefreshCounts(new FakeTimeProvider(Fixtures.Noon), new DiagnosticLog(new FakeTimeProvider(Fixtures.Noon), [])));

    private WorktreeInventory Inventory => new(new GitCli(processes, "git"), folders, TestSettings.Current(Root), catalogue, new DiagnosticLog(new FakeTimeProvider(Fixtures.Noon), []));

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

    public void Dispose() => catalogue.Dispose();

    private void Branches(params string[] refs) =>
        processes.Answer(request => request.Arguments.Count > 0 && request.Arguments[0] == "for-each-ref", new ProcessResult(0, string.Concat(refs), string.Empty));

    private static string Local(string name, string upstream, string track = "", string email = Email) => $"refs/heads/{name}\0{upstream}\0{track}\0{TipAt}\0<{email}>\0\n";
}