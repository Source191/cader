using Source191.Cader.Adapters.Git;
using Source191.Cader.Adapters.Processes;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Infrastructure.Tests;

/// <summary>
/// Pins what the real git does with a merged branch whose pull request origin has merged and deleted: Cader's own check
/// against origin's default branch says merged, but <c>git branch -d</c> checks HEAD once the upstream is pruned, so it
/// refuses while the local default branch is behind. <c>WorktreeRemover</c> relies on the forced delete.
/// </summary>
[Trait("Category", "Integration")]
public sealed class GitBranchDeletionShould : IDisposable
{
    private readonly TemporaryGitRoot root = new();

    private static GitCli Git => new(new ProcessRunner(TimeSpan.FromMinutes(1)), "git");

    [Fact]
    public async Task RefuseDashDForAMergedBranchWhoseUpstreamWasPruned()
    {
        string clone = await MergedAndPrunedBranchAsync();

        (await Git.IsMergedAsync(clone, "feat", "origin/main", TestContext.Current.CancellationToken)).ShouldBeTrue();
        GitException exception = await Should.ThrowAsync<GitException>(() => Git.DeleteBranchAsync(clone, "feat", force: false, TestContext.Current.CancellationToken));

        exception.Message.ShouldContain("not fully merged");
    }

    [Fact]
    public async Task ForceDeleteThatBranch()
    {
        string clone = await MergedAndPrunedBranchAsync();

        await Git.DeleteBranchAsync(clone, "feat", force: true, TestContext.Current.CancellationToken);

        (await Git.IsMergedAsync(clone, "feat", "origin/main", TestContext.Current.CancellationToken)).ShouldBeFalse();
    }

    /// <summary>A clone whose branch feat was pushed with -u, merged into origin's main, deleted on origin and pruned, with its own main behind.</summary>
    private async Task<string> MergedAndPrunedBranchAsync()
    {
        string origin = Path.Combine(root.Path, "origin.git");
        string clone = Path.Combine(root.Path, "Northwind.Orders");
        await root.GitAsync(root.Path, "init", "-q", "--bare", "-b", "main", origin);
        await root.GitAsync(root.Path, "clone", "-q", origin, clone);
        await root.GitAsync(clone, "config", "user.email", "tests@example.test");
        await root.GitAsync(clone, "config", "user.name", "Cader Tests");
        await root.CommitAFileAsync(clone, "init.txt");
        await root.GitAsync(clone, "push", "-q", "-u", "origin", "main");
        await root.GitAsync(clone, "switch", "-q", "-c", "feat");
        await root.CommitAFileAsync(clone, "feat.txt");
        await root.GitAsync(clone, "push", "-q", "-u", "origin", "feat");
        await root.GitAsync(clone, "switch", "-q", "main");
        await root.GitAsync(clone, "push", "-q", "origin", "feat:main");
        await root.GitAsync(clone, "push", "-q", "origin", "--delete", "feat");
        await root.GitAsync(clone, "fetch", "-q", "--prune", "origin");

        return clone;
    }

    public void Dispose()
    {
        root.Dispose();
    }
}