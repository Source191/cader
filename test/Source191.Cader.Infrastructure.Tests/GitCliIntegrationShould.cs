using Source191.Cader.Adapters.Git;
using Source191.Cader.Adapters.Processes;
using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.Infrastructure.Tests;

/// <summary>
/// Drives the real git against throwaway repositories under the temp folder.
/// </summary>
[Trait("Category", "Integration")]
public sealed class GitCliIntegrationShould : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "test-git-" + Guid.NewGuid().ToString("N"));
    private readonly GitCli git = new(new ProcessRunner(TimeSpan.FromMinutes(1)), "git");

    private string Repository => Path.Combine(root, "Northwind.Orders");

    private string Worktree => Path.Combine(root, "Northwind.Orders-fix-123");

    private string Origin => Path.Combine(root, "Northwind.Orders.git");

    private string Elsewhere => Path.Combine(root, "elsewhere", "Northwind.Orders");

    [Fact]
    public async Task ListOnlyTheMainCheckoutOfAFreshRepository()
    {
        await CreateRepositoryWithOneCommitAsync();

        IReadOnlyList<GitWorktreeEntry> entries = await git.ListWorktreesAsync(Repository, TestContext.Current.CancellationToken);

        entries.Single().ShouldSatisfyAllConditions(
            entry => entry.IsMain.ShouldBeTrue(),
            entry => entry.Head.ShouldBe(new Head.OnBranch(BranchName.From("main"))),
            entry => entry.Path.ShouldBe(Repository, StringCompareShould.IgnoreCase));
    }

    [Fact]
    public async Task AddListInspectAndRemoveASiblingWorktree()
    {
        await CreateRepositoryWithOneCommitAsync();

        await git.AddWorktreeAsync(Repository, Worktree, "fix-123", "main", TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(Path.Combine(Worktree, "new.txt"), "untracked", TestContext.Current.CancellationToken);

        IReadOnlyList<GitWorktreeEntry> entries = await git.ListWorktreesAsync(Repository, TestContext.Current.CancellationToken);
        entries.Count.ShouldBe(2);
        entries[1].Head.ShouldBe(new Head.OnBranch(BranchName.From("fix-123")));
        (await git.CountChangedFilesAsync(Worktree, TestContext.Current.CancellationToken)).ShouldBe(1);
        (await git.CountUnpushedCommitsAsync(Worktree, TestContext.Current.CancellationToken)).ShouldBe(1);
        (await git.LastCommitAtAsync(Worktree, TestContext.Current.CancellationToken)).ShouldNotBeNull();
        (await git.CurrentBranchAsync(Worktree, TestContext.Current.CancellationToken)).ShouldBe("fix-123");
        (await git.MainCheckoutOfAsync(Worktree, TestContext.Current.CancellationToken)).ShouldBe(Repository, StringCompareShould.IgnoreCase);
        (await git.IsMergedAsync(Repository, "fix-123", "main", TestContext.Current.CancellationToken)).ShouldBeTrue();

        await git.RemoveWorktreeAsync(Repository, Worktree, force: true, TestContext.Current.CancellationToken);
        await git.DeleteBranchAsync(Repository, "fix-123", force: false, TestContext.Current.CancellationToken);

        Directory.Exists(Worktree).ShouldBeFalse();
        (await git.ListWorktreesAsync(Repository, TestContext.Current.CancellationToken)).Count.ShouldBe(1);
    }

    [Fact]
    public async Task ListTheUnpushedCommitsNewestFirstUpToTheLimit()
    {
        await CreateRepositoryWithOneCommitAsync();
        foreach (string subject in new[] { "Add the refund rule", "Retry the refund twice" })
        {
            await File.AppendAllTextAsync(Path.Combine(Repository, "README.md"), subject, TestContext.Current.CancellationToken);
            await RunGitAsync("commit", "-q", "-am", subject);
        }

        IReadOnlyList<UnpushedCommit> commits = await git.UnpushedCommitsAsync(Repository, 2, TestContext.Current.CancellationToken);

        commits.Select(commit => commit.Subject).ShouldBe(["Retry the refund twice", "Add the refund rule"]);
        commits.ShouldAllBe(commit => commit.Hash.Length >= 7);
        commits[0].At.ShouldBeGreaterThan(DateTimeOffset.UnixEpoch);
    }

    [Fact]
    public async Task ListTheChangedAndUntrackedFilesUpToTheLimit()
    {
        await CreateRepositoryWithOneCommitAsync();
        await File.AppendAllTextAsync(Path.Combine(Repository, "README.md"), "changed", TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(Path.Combine(Repository, "Refunds.cs"), "new", TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(Path.Combine(Repository, "Retry.cs"), "new", TestContext.Current.CancellationToken);

        IReadOnlyList<string> files = await git.ChangedFilesAsync(Repository, 2, TestContext.Current.CancellationToken);

        files.ShouldBe(["README.md", "Refunds.cs"]);
    }

    [Fact]
    public async Task ListAChangedFileByItsOwnName()
    {
        await CreateRepositoryWithOneCommitAsync();
        await File.WriteAllTextAsync(Path.Combine(Repository, "naïve.txt"), "new", TestContext.Current.CancellationToken);

        IReadOnlyList<string> files = await git.ChangedFilesAsync(Repository, 5, TestContext.Current.CancellationToken);

        files.ShouldBe(["naïve.txt"]);
    }

    [Fact]
    public async Task PruneARegistrationWhoseFolderWasDeletedByHand()
    {
        await CreateRepositoryWithOneCommitAsync();
        await git.AddWorktreeAsync(Repository, Worktree, "fix-123", "main", TestContext.Current.CancellationToken);
        Directory.Delete(Worktree, recursive: true);

        int pruned = await git.PruneWorktreesAsync(Repository, TestContext.Current.CancellationToken);

        pruned.ShouldBe(1);
        (await git.ListWorktreesAsync(Repository, TestContext.Current.CancellationToken)).Count.ShouldBe(1);
    }

    [Fact]
    public async Task AnswerTheOriginUrlOnlyOnceARemoteIsAdded()
    {
        await CreateRepositoryWithOneCommitAsync();

        (await git.OriginUrlAsync(Repository, TestContext.Current.CancellationToken)).ShouldBeNull();
        await RunGitAsync("remote", "add", "origin", "https://github.com/northwind/Northwind.Orders.git");
        (await git.OriginUrlAsync(Repository, TestContext.Current.CancellationToken)).ShouldBe("https://github.com/northwind/Northwind.Orders.git");
    }

    [Fact]
    public async Task AnswerRepositoryQuestionsOutsideAndInsideARepository()
    {
        await CreateRepositoryWithOneCommitAsync();
        string nested = Path.Combine(Repository, "src");
        Directory.CreateDirectory(nested);

        (await git.RepositoryRootAsync(nested, TestContext.Current.CancellationToken)).ShouldBe(Repository, StringCompareShould.IgnoreCase);
        (await git.RepositoryRootAsync(root, TestContext.Current.CancellationToken)).ShouldBeNull();
        (await git.DefaultRemoteBranchAsync(Repository, TestContext.Current.CancellationToken)).ShouldBeNull();
    }

    [Fact]
    public async Task AddAWorktreeUnderAFolderThatDoesNotExistYet()
    {
        await CreateRepositoryWithOneCommitAsync();
        string worktree = Path.Combine(root, "worktrees", "Northwind.Orders-fix-123");

        await git.AddWorktreeAsync(Repository, worktree, "fix-123", "main", TestContext.Current.CancellationToken);

        File.Exists(Path.Combine(worktree, "README.md")).ShouldBeTrue("git creates the missing worktree root");
    }

    [Fact]
    public async Task AnswerNothingForAFolderThatWasDeleted()
    {
        await CreateRepositoryWithOneCommitAsync();
        await git.AddWorktreeAsync(Repository, Worktree, "fix-123", "main", TestContext.Current.CancellationToken);
        Directory.Delete(Worktree, recursive: true);

        (await git.RepositoryRootAsync(Worktree, TestContext.Current.CancellationToken)).ShouldBeNull();
        (await git.MainCheckoutOfAsync(Worktree, TestContext.Current.CancellationToken)).ShouldBeNull();
        (await git.CurrentBranchAsync(Worktree, TestContext.Current.CancellationToken)).ShouldBeNull();
        (await git.OriginUrlAsync(Worktree, TestContext.Current.CancellationToken)).ShouldBeNull();
        (await git.DefaultRemoteBranchAsync(Worktree, TestContext.Current.CancellationToken)).ShouldBeNull();
        (await git.LastCommitAtAsync(Worktree, TestContext.Current.CancellationToken)).ShouldBeNull();
        (await git.IsMergedAsync(Worktree, "fix-123", "main", TestContext.Current.CancellationToken)).ShouldBeFalse();
    }

    [Fact]
    public async Task ThrowAGitExceptionForACommandInAFolderThatWasDeleted()
    {
        await CreateRepositoryWithOneCommitAsync();
        await git.AddWorktreeAsync(Repository, Worktree, "fix-123", "main", TestContext.Current.CancellationToken);
        Directory.Delete(Worktree, recursive: true);

        await Should.ThrowAsync<GitException>(() => git.CountChangedFilesAsync(Worktree, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task FetchOriginsNewCommitIntoTheRemoteTrackingBranch()
    {
        await CloneABareOriginAsync();
        string before = await OutputOfGitAsync(Repository, "rev-parse", "refs/remotes/origin/main");
        string pushed = await PushFromElsewhereAsync("Orders.cs", "class Orders;");

        await git.FetchBranchAsync(FolderPath.From(Repository), RemoteBranch.From("origin/main"), TestContext.Current.CancellationToken);

        (await OutputOfGitAsync(Repository, "rev-parse", "refs/remotes/origin/main")).ShouldBe(pushed);
        pushed.ShouldNotBe(before);
        (await OutputOfGitAsync(Repository, "rev-parse", "refs/heads/main")).ShouldBe(before, "a fetch moves no local branch");
    }

    [Fact]
    public async Task FastForwardTheCheckedOutMainAndKeepAChangeTheCommitsDoNotTouch()
    {
        await CloneABareOriginAsync();
        string pushed = await PushFromElsewhereAsync("Orders.cs", "class Orders;");
        await File.WriteAllTextAsync(Path.Combine(Repository, "README.md"), "# changed here", TestContext.Current.CancellationToken);
        await git.FetchBranchAsync(FolderPath.From(Repository), RemoteBranch.From("origin/main"), TestContext.Current.CancellationToken);

        await git.FastForwardCheckoutAsync(FolderPath.From(Repository), RemoteBranch.From("origin/main"), TestContext.Current.CancellationToken);

        (await OutputOfGitAsync(Repository, "rev-parse", "HEAD")).ShouldBe(pushed);
        (await File.ReadAllTextAsync(Path.Combine(Repository, "Orders.cs"), TestContext.Current.CancellationToken)).ShouldBe("class Orders;");
        (await File.ReadAllTextAsync(Path.Combine(Repository, "README.md"), TestContext.Current.CancellationToken)).ShouldBe("# changed here");
    }

    [Fact]
    public async Task RefuseToFastForwardOverAChangeTheCommitsTouch()
    {
        await CloneABareOriginAsync();
        string before = await OutputOfGitAsync(Repository, "rev-parse", "HEAD");
        await PushFromElsewhereAsync("README.md", "# changed on origin");
        await File.WriteAllTextAsync(Path.Combine(Repository, "README.md"), "# changed here", TestContext.Current.CancellationToken);
        await git.FetchBranchAsync(FolderPath.From(Repository), RemoteBranch.From("origin/main"), TestContext.Current.CancellationToken);

        await Should.ThrowAsync<GitException>(() => git.FastForwardCheckoutAsync(FolderPath.From(Repository), RemoteBranch.From("origin/main"), TestContext.Current.CancellationToken));

        (await OutputOfGitAsync(Repository, "rev-parse", "HEAD")).ShouldBe(before);
        (await File.ReadAllTextAsync(Path.Combine(Repository, "README.md"), TestContext.Current.CancellationToken)).ShouldBe("# changed here");
    }

    [Fact]
    public async Task RefuseToFastForwardAMainWithACommitOfItsOwn()
    {
        await CloneABareOriginAsync();
        await CommitInAsync(Repository, "Local.cs", "class Local;");
        string local = await OutputOfGitAsync(Repository, "rev-parse", "HEAD");
        await PushFromElsewhereAsync("Orders.cs", "class Orders;");
        await git.FetchBranchAsync(FolderPath.From(Repository), RemoteBranch.From("origin/main"), TestContext.Current.CancellationToken);

        await Should.ThrowAsync<GitException>(() => git.FastForwardCheckoutAsync(FolderPath.From(Repository), RemoteBranch.From("origin/main"), TestContext.Current.CancellationToken));

        (await OutputOfGitAsync(Repository, "rev-parse", "HEAD")).ShouldBe(local);
    }

    [Fact]
    public async Task MoveAMainThatIsNotCheckedOutAndLeaveTheCheckoutAlone()
    {
        await CloneABareOriginAsync();
        await RunGitAsync("switch", "-q", "-c", "fix-99");
        string pushed = await PushFromElsewhereAsync("Orders.cs", "class Orders;");
        await git.FetchBranchAsync(FolderPath.From(Repository), RemoteBranch.From("origin/main"), TestContext.Current.CancellationToken);

        await git.FastForwardBranchAsync(FolderPath.From(Repository), BranchName.From("main"), RemoteBranch.From("origin/main"), TestContext.Current.CancellationToken);

        (await OutputOfGitAsync(Repository, "rev-parse", "refs/heads/main")).ShouldBe(pushed);
        (await git.CurrentBranchAsync(Repository, TestContext.Current.CancellationToken)).ShouldBe("fix-99");
        File.Exists(Path.Combine(Repository, "Orders.cs")).ShouldBeFalse("no worktree's files change");
    }

    [Fact]
    public async Task RefuseToMoveABranchThatIsNotAFastForwardOrIsCheckedOut()
    {
        await CloneABareOriginAsync();
        await PushFromElsewhereAsync("Orders.cs", "class Orders;");
        await git.FetchBranchAsync(FolderPath.From(Repository), RemoteBranch.From("origin/main"), TestContext.Current.CancellationToken);

        await Should.ThrowAsync<GitException>(() => git.FastForwardBranchAsync(FolderPath.From(Repository), BranchName.From("main"), RemoteBranch.From("origin/main"), TestContext.Current.CancellationToken));

        await CommitInAsync(Repository, "Local.cs", "class Local;");
        string local = await OutputOfGitAsync(Repository, "rev-parse", "HEAD");
        await RunGitAsync("switch", "-q", "-c", "fix-99");
        await Should.ThrowAsync<GitException>(() => git.FastForwardBranchAsync(FolderPath.From(Repository), BranchName.From("main"), RemoteBranch.From("origin/main"), TestContext.Current.CancellationToken));
        (await OutputOfGitAsync(Repository, "rev-parse", "refs/heads/main")).ShouldBe(local);
    }

    /// <summary>
    /// A main ahead of origin's, with nothing new on origin: merge --ff-only answers "Already up to date" in the checkout,
    /// while fetch . refuses to move a branch elsewhere backwards, so Pull warns only when main is not checked out.
    /// </summary>
    [Fact]
    public async Task LeaveAMainAheadOfOriginWhereItIsEitherWay()
    {
        await CloneABareOriginAsync();
        await CommitInAsync(Repository, "Local.cs", "class Local;");
        string local = await OutputOfGitAsync(Repository, "rev-parse", "HEAD");
        await git.FetchBranchAsync(FolderPath.From(Repository), RemoteBranch.From("origin/main"), TestContext.Current.CancellationToken);

        await git.FastForwardCheckoutAsync(FolderPath.From(Repository), RemoteBranch.From("origin/main"), TestContext.Current.CancellationToken);
        (await OutputOfGitAsync(Repository, "rev-parse", "HEAD")).ShouldBe(local);

        await RunGitAsync("switch", "-q", "-c", "fix-99");
        await Should.ThrowAsync<GitException>(() => git.FastForwardBranchAsync(FolderPath.From(Repository), BranchName.From("main"), RemoteBranch.From("origin/main"), TestContext.Current.CancellationToken));
        (await OutputOfGitAsync(Repository, "rev-parse", "refs/heads/main")).ShouldBe(local);
    }

    [Fact]
    public async Task SayWhetherALocalBranchExists()
    {
        await CloneABareOriginAsync();

        (await git.BranchExistsAsync(FolderPath.From(Repository), BranchName.From("main"), TestContext.Current.CancellationToken)).ShouldBeTrue();
        (await git.BranchExistsAsync(FolderPath.From(Repository), BranchName.From("release"), TestContext.Current.CancellationToken)).ShouldBeFalse();
        (await git.DefaultRemoteBranchAsync(Repository, TestContext.Current.CancellationToken)).ShouldBe("origin/main");
    }

    [Fact]
    public async Task ListEveryLocalBranchWithItsUpstreamAndEachBranchOnlyOnOrigin()
    {
        await CloneABareOriginAsync();
        await RunGitAsync("switch", "-q", "-c", "tracked");
        await CommitInAsync(Repository, "Tracked.cs", "class Tracked;");
        await RunGitAsync("push", "-q", "-u", "origin", "tracked");
        await RunGitAsync("switch", "-q", "-c", "gone");
        await RunGitAsync("push", "-q", "-u", "origin", "gone");
        await RunGitAsync("push", "-q", "origin", "--delete", "gone");
        await RunGitAsync("switch", "-q", "-c", "unpushed");
        await CommitInAsync(Repository, "Unpushed.cs", "class Unpushed;");
        await RunGitAsync("switch", "-q", "main");
        await RunGitAsync("update-ref", "refs/heads/-x", "HEAD");
        await PushABranchFromElsewhereAsync("c-new-bug", "TESTS@example.test");
        await PushABranchFromElsewhereAsync("dependabot-bump", "bot@example.test");
        await RunGitAsync("fetch", "-q", "--prune");

        IReadOnlyList<GitBranch> branches = await git.ListBranchesAsync(FolderPath.From(Repository), TestContext.Current.CancellationToken);

        branches.ShouldAllBe(branch => branch.TipAt > DateTimeOffset.UnixEpoch);
        branches.Select(branch => branch with { TipAt = DateTimeOffset.UnixEpoch }).ShouldBe(
        [
            Local("-x", new Upstream.None()),
            Local("gone", new Upstream.Gone()),
            Local("main", new Upstream.Tracking(RemoteBranch.From("origin/main"))),
            Local("tracked", new Upstream.Tracking(RemoteBranch.From("origin/tracked"))),
            Local("unpushed", new Upstream.None()),
            new GitBranch.OriginOnly(BranchName.From("c-new-bug"), DateTimeOffset.UnixEpoch, Mine: true),
            new GitBranch.OriginOnly(BranchName.From("dependabot-bump"), DateTimeOffset.UnixEpoch, Mine: false),
        ],
        ignoreOrder: true);
    }

    [Fact]
    public async Task PruneOnlyTheBranchesOriginHasDeleted()
    {
        await CloneABareOriginAsync();
        await PushABranchFromElsewhereAsync("kept", "me@example.test");
        await PushABranchFromElsewhereAsync("old-fix", "me@example.test");
        await RunGitAsync("fetch", "-q");
        await RunGitAsync("branch", "-q", "spike", "main");
        await OutputOfGitAsync(Elsewhere, "push", "-q", "origin", "--delete", "old-fix");
        FolderPath repository = FolderPath.From(Repository);
        IReadOnlyList<GitBranch> before = await git.ListBranchesAsync(repository, TestContext.Current.CancellationToken);

        await git.PruneOriginAsync(repository, TestContext.Current.CancellationToken);

        IReadOnlyList<GitBranch> after = await git.ListBranchesAsync(repository, TestContext.Current.CancellationToken);
        before.OfType<GitBranch.OriginOnly>().Select(branch => branch.Name.Value).ShouldBe(["kept", "old-fix"], ignoreOrder: true);
        after.OfType<GitBranch.OriginOnly>().Select(branch => branch.Name.Value).ShouldBe(["kept"]);
        after.OfType<GitBranch.Local>().ShouldBe(before.OfType<GitBranch.Local>(), ignoreOrder: true);
    }

    [Fact]
    public async Task ReadAnUpstreamThatIsALocalBranchAsNoneEvenOnceItIsDeleted()
    {
        await CloneABareOriginAsync();
        await RunGitAsync("branch", "-q", "feature/x", "main");
        await RunGitAsync("branch", "-q", "local-up", "main");
        await RunGitAsync("branch", "-q", "--set-upstream-to=feature/x", "local-up");
        FolderPath repository = FolderPath.From(Repository);

        (await git.ListBranchesAsync(repository, TestContext.Current.CancellationToken)).OfType<GitBranch.Local>().Single(branch => branch.Name == BranchName.From("local-up")).Upstream.ShouldBe(new Upstream.None());

        await RunGitAsync("branch", "-q", "-D", "feature/x");

        (await git.ListBranchesAsync(repository, TestContext.Current.CancellationToken)).OfType<GitBranch.Local>().Single(branch => branch.Name == BranchName.From("local-up")).Upstream.ShouldBe(new Upstream.None());
    }

    [Fact]
    public async Task CountTheCommitsABranchHasAfterAPullRequestsHead()
    {
        await CloneABareOriginAsync();
        await RunGitAsync("switch", "-q", "-c", "fix-88");
        await CommitInAsync(Repository, "Fix.cs", "class Fix;");
        CommitId head = CommitId.From(await OutputOfGitAsync(Repository, "rev-parse", "HEAD"));
        FolderPath repository = FolderPath.From(Repository);

        (await git.CountCommitsAfterAsync(repository, "refs/heads/fix-88", head, TestContext.Current.CancellationToken)).ShouldBe(0);

        await CommitInAsync(Repository, "After.cs", "class After;");
        await RunGitAsync("switch", "-q", "main");
        await RunGitAsync("update-ref", "refs/heads/-x", "refs/heads/fix-88");

        (await git.CountCommitsAfterAsync(repository, "refs/heads/fix-88", head, TestContext.Current.CancellationToken)).ShouldBe(1);
        (await git.CountCommitsAfterAsync(repository, "refs/heads/-x", head, TestContext.Current.CancellationToken)).ShouldBe(1);
        (await git.CountCommitsAfterAsync(repository, "refs/remotes/origin/main", head, TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task CountNoCommitsRebasedOntoMainBeforeTheMerge()
    {
        await CloneABareOriginAsync();
        await RunGitAsync("switch", "-q", "-c", "fix-90");
        await CommitInAsync(Repository, "Refund.cs", "class Refund;");
        await CommitInAsync(Repository, "Retry.cs", "class Retry;");
        await PushFromElsewhereAsync("Tax.cs", "class Tax;");
        await RunGitAsync("switch", "-q", "main");
        await RunGitAsync("pull", "-q", "--ff-only");
        await RunGitAsync("cherry-pick", "main..fix-90");
        await RunGitAsync("push", "-q", "origin", "main");
        CommitId mergedHead = CommitId.From(await OutputOfGitAsync(Repository, "rev-parse", "HEAD"));

        int? count = await git.CountCommitsAfterAsync(FolderPath.From(Repository), "refs/heads/fix-90", mergedHead, TestContext.Current.CancellationToken);

        count.ShouldBe(0);
    }

    [Fact]
    public async Task CountNoCommitsOfABranchMovedForwardToMainAfterItsMerge()
    {
        await CloneABareOriginAsync();
        await RunGitAsync("switch", "-q", "-c", "fix-91");
        await CommitInAsync(Repository, "Refund.cs", "class Refund;");
        CommitId mergedHead = CommitId.From(await OutputOfGitAsync(Repository, "rev-parse", "HEAD"));
        await RunGitAsync("switch", "-q", "main");
        await RunGitAsync("merge", "-q", "--ff-only", "fix-91");
        await CommitInAsync(Repository, "Tax.cs", "class Tax;");
        await RunGitAsync("push", "-q", "origin", "main");
        await RunGitAsync("branch", "-f", "fix-91", "origin/main");

        int? count = await git.CountCommitsAfterAsync(FolderPath.From(Repository), "refs/heads/fix-91", mergedHead, TestContext.Current.CancellationToken);

        count.ShouldBe(0);
    }

    [Fact]
    public async Task CountOnlyTheWorkAfterTheMergeThatMainLacks()
    {
        CommitId mergedHead = await BranchWithMainMergedInAndOneCommitAfterItsHeadAsync();

        int? count = await git.CountCommitsAfterAsync(FolderPath.From(Repository), "refs/heads/fix-92", mergedHead, TestContext.Current.CancellationToken);

        count.ShouldBe(1);
    }

    [Fact]
    public async Task CountEveryCommitAfterTheHeadWhenOriginsDefaultIsNotRecorded()
    {
        CommitId mergedHead = await BranchWithMainMergedInAndOneCommitAfterItsHeadAsync();
        await RunGitAsync("remote", "set-head", "origin", "-d");

        int? count = await git.CountCommitsAfterAsync(FolderPath.From(Repository), "refs/heads/fix-92", mergedHead, TestContext.Current.CancellationToken);

        count.ShouldBe(3);
    }

    [Fact]
    public async Task AnswerNoCountForAHeadGitHasNotGotOrARevisionThatIsNotARef()
    {
        await CloneABareOriginAsync();
        FolderPath repository = FolderPath.From(Repository);
        CommitId head = CommitId.From(await OutputOfGitAsync(Repository, "rev-parse", "HEAD"));
        CommitId unknown = CommitId.From("0123456789abcdef0123456789abcdef01234567");

        (await git.CountCommitsAfterAsync(repository, "refs/heads/main", unknown, TestContext.Current.CancellationToken)).ShouldBeNull();
        (await git.CountCommitsAfterAsync(repository, "main", head, TestContext.Current.CancellationToken)).ShouldBeNull();
        (await git.CountCommitsAfterAsync(repository, "--all", head, TestContext.Current.CancellationToken)).ShouldBeNull();
    }

    [Fact]
    public async Task CountTheCommitsOfABranchCheckedOutNowhereThatAreOnNoRemote()
    {
        await CloneABareOriginAsync();
        await RunGitAsync("switch", "-q", "-c", "spike-refunds");
        await CommitInAsync(Repository, "Refunds.cs", "class Refunds;");
        await CommitInAsync(Repository, "Retry.cs", "class Retry;");
        await RunGitAsync("switch", "-q", "main");
        await RunGitAsync("update-ref", "refs/heads/-x", "refs/heads/spike-refunds");
        FolderPath repository = FolderPath.From(Repository);

        (await git.CountCommitsOnNoRemoteAsync(repository, BranchName.From("spike-refunds"), TestContext.Current.CancellationToken)).ShouldBe(2);
        (await git.CountCommitsOnNoRemoteAsync(repository, BranchName.From("-x"), TestContext.Current.CancellationToken)).ShouldBe(2);
        (await git.CountCommitsOnNoRemoteAsync(repository, BranchName.From("main"), TestContext.Current.CancellationToken)).ShouldBe(0);
        await Should.ThrowAsync<GitException>(() => git.CountCommitsOnNoRemoteAsync(repository, BranchName.From("no-such-branch"), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DeleteABranchWhoseNameStartsWithAHyphen()
    {
        await CreateRepositoryWithOneCommitAsync();
        await RunGitAsync("update-ref", "refs/heads/-x", "HEAD");

        await git.DeleteBranchAsync(Repository, "-x", force: true, TestContext.Current.CancellationToken);

        (await git.BranchExistsAsync(FolderPath.From(Repository), BranchName.From("-x"), TestContext.Current.CancellationToken)).ShouldBeFalse();
    }

    public void Dispose()
    {
        if (!Directory.Exists(root))
        {
            return;
        }

        foreach (string file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            File.SetAttributes(file, FileAttributes.Normal);
        }

        Directory.Delete(root, recursive: true);
    }

    [Fact]
    public async Task ReadWhenABranchWasCreated()
    {
        DateTimeOffset began = TimeProvider.System.GetUtcNow().AddSeconds(-5);
        await CreateRepositoryWithOneCommitAsync();
        await RunGitAsync("branch", "tidy-logging");
        await git.AddWorktreeAsync(Repository, Worktree, "fix-123", "main", TestContext.Current.CancellationToken);
        await RunGitAsync("branch", "spike-cache");
        await RunGitAsync("branch", "-m", "spike-cache", "refund-retry");
        DateTimeOffset ended = TimeProvider.System.GetUtcNow().AddSeconds(5);
        FolderPath repository = FolderPath.From(Repository);

        foreach (string created in new[] { "tidy-logging", "fix-123", "refund-retry" })
        {
            DateTimeOffset? at = await git.BranchCreatedAtAsync(repository, BranchName.From(created), TestContext.Current.CancellationToken);
            at.ShouldNotBeNull(created);
            at.Value.ShouldBeInRange(began, ended);
        }

        (await git.BranchCreatedAtAsync(repository, BranchName.From("main"), TestContext.Current.CancellationToken)).ShouldBeNull();
    }
    private async Task CreateRepositoryWithOneCommitAsync()
    {
        Directory.CreateDirectory(Repository);
        await RunGitAsync("init", "-q", "-b", "main");
        await RunGitAsync("config", "user.email", "tests@example.test");
        await RunGitAsync("config", "user.name", "Cader Tests");
        await File.WriteAllTextAsync(Path.Combine(Repository, "README.md"), "# test");
        await RunGitAsync("add", "-A");
        await RunGitAsync("commit", "-q", "-m", "init");
    }

    private async Task RunGitAsync(params string[] arguments)
    {
        await OutputOfGitAsync(Repository, arguments);
    }

    /// <summary>
    /// A bare repository in the temp folder stands for origin, never the network: a first checkout makes the first commit
    /// and pushes it, and the repository under test is a clone of origin, so it has origin/HEAD as a real clone does.
    /// </summary>
    private async Task CloneABareOriginAsync()
    {
        Directory.CreateDirectory(Elsewhere);
        await OutputOfGitAsync(root, "init", "-q", "--bare", "-b", "main", Origin);
        await OutputOfGitAsync(Elsewhere, "init", "-q", "-b", "main");
        await IdentifyAsync(Elsewhere);
        await CommitInAsync(Elsewhere, "README.md", "# test");
        await OutputOfGitAsync(Elsewhere, "remote", "add", "origin", Origin);
        await OutputOfGitAsync(Elsewhere, "push", "-q", "origin", "main");
        await OutputOfGitAsync(root, "clone", "-q", Origin, Repository);
        await IdentifyAsync(Repository);
    }

    /// <summary>
    /// fix-92 with one commit at its merged head, then main's newer commit merged into it and one more commit of its own:
    /// three commits after the head, of which only the last is work main lacks. Answers the merged head.
    /// </summary>
    private async Task<CommitId> BranchWithMainMergedInAndOneCommitAfterItsHeadAsync()
    {
        await CloneABareOriginAsync();
        await RunGitAsync("switch", "-q", "-c", "fix-92");
        await CommitInAsync(Repository, "Refund.cs", "class Refund;");
        CommitId mergedHead = CommitId.From(await OutputOfGitAsync(Repository, "rev-parse", "HEAD"));
        await PushFromElsewhereAsync("Tax.cs", "class Tax;");
        await RunGitAsync("fetch", "-q");
        await RunGitAsync("merge", "-q", "--no-edit", "origin/main");
        await CommitInAsync(Repository, "Retry.cs", "class Retry;");

        return mergedHead;
    }

    /// <summary>Commits one file in the other checkout and pushes it to origin; answers the new commit.</summary>
    private async Task<string> PushFromElsewhereAsync(string file, string content)
    {
        await CommitInAsync(Elsewhere, file, content);
        await OutputOfGitAsync(Elsewhere, "push", "-q", "origin", "main");

        return await OutputOfGitAsync(Elsewhere, "rev-parse", "HEAD");
    }

    /// <summary>Pushes a new branch to origin from the other checkout, its one commit authored with the email given.</summary>
    private async Task PushABranchFromElsewhereAsync(string branch, string email)
    {
        await OutputOfGitAsync(Elsewhere, "switch", "-q", "-c", branch, "main");
        await File.WriteAllTextAsync(Path.Combine(Elsewhere, $"{branch}.txt"), branch, TestContext.Current.CancellationToken);
        await OutputOfGitAsync(Elsewhere, "add", "-A");
        await OutputOfGitAsync(Elsewhere, "-c", $"user.email={email}", "commit", "-q", "-m", $"Add {branch}");
        await OutputOfGitAsync(Elsewhere, "push", "-q", "origin", branch);
    }

    private static GitBranch.Local Local(string branch, Upstream upstream) => new(BranchName.From(branch), upstream, DateTimeOffset.UnixEpoch, Mine: true);

    private static async Task CommitInAsync(string folder, string file, string content)
    {
        await File.WriteAllTextAsync(Path.Combine(folder, file), content, TestContext.Current.CancellationToken);
        await OutputOfGitAsync(folder, "add", "-A");
        await OutputOfGitAsync(folder, "commit", "-q", "-m", $"Add {file}");
    }

    private static async Task IdentifyAsync(string folder)
    {
        await OutputOfGitAsync(folder, "config", "user.email", "tests@example.test");
        await OutputOfGitAsync(folder, "config", "user.name", "Cader Tests");
    }

    private static async Task<string> OutputOfGitAsync(string folder, params string[] arguments)
    {
        ProcessResult result = await new ProcessRunner(TimeSpan.FromMinutes(1)).RunAsync(new ProcessRequest("git", arguments, new WorkingFolder.At(folder)), TestContext.Current.CancellationToken);
        result.ExitCode.ShouldBe(0, result.StandardError);

        return result.StandardOutput.Trim();
    }
}