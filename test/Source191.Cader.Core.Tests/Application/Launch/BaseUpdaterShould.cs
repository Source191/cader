using Source191.Cader.Adapters.Git;
using Source191.Cader.Application.Launch;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application.Launch;

public sealed class BaseUpdaterShould
{
    private const string Repository = @"C:\Users\me\source\repos\Northwind.Orders";
    private const string FixWorktree = @"C:\Users\me\source\worktrees\Northwind.Orders-fix-99";
    private const string MainWorktree = @"C:\Users\me\source\worktrees\Northwind.Orders-main";
    private const string Unreachable = "fatal: unable to access 'https://github.com/northwind/Northwind.Orders.git/': Could not resolve host: github.com";

    private static readonly RemoteBranch OriginMain = RemoteBranch.From("origin/main");

    private readonly FakeProcessRunner processes = new FakeProcessRunner().AnswerAll(new ProcessResult(0, string.Empty, string.Empty));

    private BaseUpdater Updater => new(new GitCli(processes, "git"));

    [Fact]
    public async Task RunNoGitWhenAskedForNothing()
    {
        BaseUpdate update = await Updater.UpdateAsync(FolderPath.From(Repository), OriginMain, BeforeBranching.None, CancellationToken.None);

        update.ShouldBe(new BaseUpdate.Current());
        processes.CapturedRequests.ShouldBeEmpty();
    }

    [Fact]
    public async Task FetchTheBaseAndAnswerCurrent()
    {
        BaseUpdate update = await Updater.UpdateAsync(FolderPath.From(Repository), OriginMain, BeforeBranching.Fetch, CancellationToken.None);

        update.ShouldBe(new BaseUpdate.Current());
        ProcessRequest fetch = processes.CapturedRequests.ShouldHaveSingleItem();
        fetch.Arguments.ShouldBe(["fetch", "origin", "+refs/heads/main:refs/remotes/origin/main"]);
        fetch.WorkingDirectory.ShouldBe(new WorkingFolder.At(Repository));
    }

    [Fact]
    public async Task AnswerNotFetchedWithGitsWordsWhenTheFetchFails()
    {
        processes.Answer(IsFetchFromOrigin, new ProcessResult(128, string.Empty, Unreachable));

        BaseUpdate update = await Updater.UpdateAsync(FolderPath.From(Repository), OriginMain, BeforeBranching.Fetch, CancellationToken.None);

        BaseUpdate.NotFetched notFetched = update.ShouldBeOfType<BaseUpdate.NotFetched>();
        notFetched.Base.ShouldBe(OriginMain);
        notFetched.Reason.ShouldContain("Could not resolve host: github.com");
    }

    [Fact]
    public async Task AnswerNotFetchedWithTheLimitsWordsWhenTheFetchRunsOutOfTime()
    {
        processes.Throw(IsFetchFromOrigin, new ProcessTimedOutException("git.exe", TimeSpan.FromSeconds(60)));

        BaseUpdate update = await Updater.UpdateAsync(FolderPath.From(Repository), OriginMain, BeforeBranching.Fetch, CancellationToken.None);

        update.ShouldBe(new BaseUpdate.NotFetched(OriginMain, "git.exe did not finish within 60 seconds and was ended"));
    }

    [Fact]
    public async Task LetACancellationThrough()
    {
        processes.Throw(IsFetchFromOrigin, new OperationCanceledException());

        await Should.ThrowAsync<OperationCanceledException>(() => Updater.UpdateAsync(FolderPath.From(Repository), OriginMain, BeforeBranching.Fetch, CancellationToken.None));
    }

    [Fact]
    public async Task FastForwardTheMainCheckoutAfterTheFetchForPull()
    {
        ListWorktrees(Block(Repository, "main"), Block(FixWorktree, "fix-99"));

        BaseUpdate update = await Updater.UpdateAsync(FolderPath.From(Repository), OriginMain, BeforeBranching.Pull, CancellationToken.None);

        update.ShouldBe(new BaseUpdate.Current());
        processes.CapturedRequests.Select(request => request.Arguments[0]).ShouldBe(["fetch", "rev-parse", "worktree", "merge"]);
        ProcessRequest merge = processes.CapturedRequests[^1];
        merge.Arguments.ShouldBe(["merge", "--ff-only", "refs/remotes/origin/main"]);
        merge.WorkingDirectory.ShouldBe(new WorkingFolder.At(Repository));
    }

    [Fact]
    public async Task MoveTheBranchItselfWhenNoWorktreeHasItCheckedOut()
    {
        ListWorktrees(Block(Repository, "fix-99"));

        BaseUpdate update = await Updater.UpdateAsync(FolderPath.From(Repository), OriginMain, BeforeBranching.Pull, CancellationToken.None);

        update.ShouldBe(new BaseUpdate.Current());
        processes.CapturedRequests[^1].Arguments.ShouldBe(["fetch", ".", "refs/remotes/origin/main:refs/heads/main"]);
        processes.CapturedRequests.ShouldNotContain(request => request.Arguments.Contains("merge"));
    }

    [Fact]
    public async Task LeaveABranchCheckedOutInALinkedWorktreeAndSayWhere()
    {
        ListWorktrees(Block(Repository, "fix-99"), Block(MainWorktree, "main"));

        BaseUpdate update = await Updater.UpdateAsync(FolderPath.From(Repository), OriginMain, BeforeBranching.Pull, CancellationToken.None);

        update.ShouldBe(new BaseUpdate.NotFastForwarded(BranchName.From("main"), new FastForwardRefusal.CheckedOutElsewhere(FolderPath.From(MainWorktree))));
        processes.CapturedRequests.ShouldNotContain(request => IsFastForward(request));
    }

    [Fact]
    public async Task CreateNoBranchWhenTheRepositoryHasNoLocalOne()
    {
        processes.Answer(request => request.Arguments is ["rev-parse", "--verify", ..], new ProcessResult(1, string.Empty, string.Empty));
        ListWorktrees(Block(Repository, "fix-99"));

        BaseUpdate update = await Updater.UpdateAsync(FolderPath.From(Repository), OriginMain, BeforeBranching.Pull, CancellationToken.None);

        update.ShouldBe(new BaseUpdate.Current());
        processes.CapturedRequests.ShouldNotContain(request => IsFastForward(request));
    }

    [Fact]
    public async Task AnswerNotFastForwardedWithGitsWordsWhenGitRefuses()
    {
        const string refusal = "error: Your local changes to the following files would be overwritten by merge:\n\tREADME.md\nPlease commit your changes or stash them before you merge.\nAborting";
        ListWorktrees(Block(Repository, "main"));
        processes.Answer(request => request.Arguments.Contains("merge"), new ProcessResult(1, string.Empty, refusal));

        BaseUpdate update = await Updater.UpdateAsync(FolderPath.From(Repository), OriginMain, BeforeBranching.Pull, CancellationToken.None);

        BaseUpdate.NotFastForwarded refused = update.ShouldBeOfType<BaseUpdate.NotFastForwarded>();
        refused.Branch.ShouldBe(BranchName.From("main"));
        refused.Refusal.ShouldBeOfType<FastForwardRefusal.Refused>().Reason.ShouldContain("would be overwritten by merge");
    }

    [Fact]
    public async Task AnswerNotFastForwardedWhenTheWorktreesCannotBeListed()
    {
        processes.Answer(request => request.Arguments is ["worktree", "list", ..], new ProcessResult(128, string.Empty, "fatal: not a git repository"));

        BaseUpdate update = await Updater.UpdateAsync(FolderPath.From(Repository), OriginMain, BeforeBranching.Pull, CancellationToken.None);

        update.ShouldBeOfType<BaseUpdate.NotFastForwarded>().Refusal.ShouldBeOfType<FastForwardRefusal.Refused>().Reason.ShouldContain("not a git repository");
        processes.CapturedRequests.ShouldNotContain(request => IsFastForward(request));
    }

    [Fact]
    public async Task FastForwardNothingWhenTheFetchFails()
    {
        processes.Answer(IsFetchFromOrigin, new ProcessResult(128, string.Empty, Unreachable));
        ListWorktrees(Block(Repository, "main"));

        BaseUpdate update = await Updater.UpdateAsync(FolderPath.From(Repository), OriginMain, BeforeBranching.Pull, CancellationToken.None);

        update.ShouldBeOfType<BaseUpdate.NotFetched>();
        processes.CapturedRequests.ShouldHaveSingleItem().Arguments[0].ShouldBe("fetch");
    }

    private void ListWorktrees(params string[] blocks) => processes.Answer(request => request.Arguments is ["worktree", "list", ..], new ProcessResult(0, string.Join("\n", blocks), string.Empty));

    private static string Block(string path, string branch) => $"worktree {path.Replace('\\', '/')}\nHEAD 0f3c2a1\nbranch refs/heads/{branch}\n";

    private static bool IsFastForward(ProcessRequest request) => request.Arguments is ["merge", ..] or ["fetch", ".", ..];

    private static bool IsFetchFromOrigin(ProcessRequest request) => request.Arguments is ["fetch", "origin", ..];
}
