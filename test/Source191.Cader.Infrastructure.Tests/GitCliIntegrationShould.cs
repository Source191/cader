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

    /// <summary>Commits one file in the other checkout and pushes it to origin; answers the new commit.</summary>
    private async Task<string> PushFromElsewhereAsync(string file, string content)
    {
        await CommitInAsync(Elsewhere, file, content);
        await OutputOfGitAsync(Elsewhere, "push", "-q", "origin", "main");

        return await OutputOfGitAsync(Elsewhere, "rev-parse", "HEAD");
    }

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