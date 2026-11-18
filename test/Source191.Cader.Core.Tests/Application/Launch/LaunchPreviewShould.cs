using Source191.Cader.Application;
using Source191.Cader.Application.Launch;
using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Application.Launch;

public sealed class LaunchPreviewShould
{
    private static readonly RepositoryChoice Repository = new("Northwind.Orders", FolderPath.From(@"C:\repos\Northwind.Orders"), IsRepository: true);
    private static readonly FolderPath WorktreeRoot = FolderPath.From(@"C:\Users\me\source\worktrees");
    private static readonly RepositoryChoice NoRepository = new(@"C:\repos", FolderPath.From(@"C:\repos"), IsRepository: false);

    [Fact]
    public void SayTheSessionRunsOnTheCurrentBranchWhenNoWorktreeIsWanted()
    {
        LaunchPreview.Describe(Repository, "Fix #123: flaky tests!", LaunchMode.CurrentBranch, WorktreeRoot)
            .ShouldBe("Runs in Northwind.Orders on its current branch; the name is only the session's label.");
    }

    [Fact]
    public void NameTheFolderUnderTheWorktreeRoot()
    {
        LaunchPreview.Describe(Repository, "Fix #123: flaky tests!", LaunchMode.NewWorktree, WorktreeRoot)
            .ShouldBe(@"Worktree Northwind.Orders-fix-123-flaky-tests under C:\Users\me\source\worktrees, on branch fix-123-flaky-tests.");
    }

    [Fact]
    public void ExplainWhatTheNameWillBecomeBeforeOneIsTyped()
    {
        LaunchPreview.Describe(Repository, "  ", LaunchMode.NewWorktree, WorktreeRoot)
            .ShouldBe(@"The name becomes the worktree folder Northwind.Orders-<name> under C:\Users\me\source\worktrees and its branch.");
    }

    [Theory]
    [InlineData(LaunchMode.NewWorktree)]
    [InlineData(LaunchMode.CurrentBranch)]
    public void SayAFolderChoiceIsNotARepositoryWhateverTheMode(LaunchMode mode)
    {
        LaunchPreview.Describe(NoRepository, "anything", mode, WorktreeRoot).ShouldBe(Wording.PreviewFolder(@"C:\repos"));
    }
}