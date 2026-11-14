using Source191.Cader.Application.Launch;
using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.Core.Tests.Application.Launch;

public sealed class FastForwardPlaceShould
{
    private const string Repository = @"C:\Users\me\source\repos\Northwind.Orders";
    private const string MainWorktree = @"C:\Users\me\source\worktrees\Northwind.Orders-main";

    private static readonly BranchName Main = BranchName.From("main");

    [Fact]
    public void FastForwardInTheMainCheckoutWhenItHasTheBranch()
    {
        IReadOnlyList<GitWorktreeEntry> entries = [Entry(Repository, OnBranch("main"), isMain: true), Entry(MainWorktree, OnBranch("fix-99"), isMain: false)];

        FastForwardPlace.Of(entries, Main).ShouldBe(new FastForwardPlace.MainCheckout());
    }

    [Fact]
    public void NameTheLinkedWorktreeThatHasTheBranch()
    {
        IReadOnlyList<GitWorktreeEntry> entries = [Entry(Repository, OnBranch("fix-99"), isMain: true), Entry(MainWorktree, OnBranch("main"), isMain: false)];

        FastForwardPlace.Of(entries, Main).ShouldBe(new FastForwardPlace.ElsewhereAt(FolderPath.From(MainWorktree)));
    }

    [Fact]
    public void MoveTheBranchItselfWhenNoWorktreeHasIt()
    {
        IReadOnlyList<GitWorktreeEntry> others = [Entry(Repository, OnBranch("fix-99"), isMain: true), Entry(MainWorktree, OnBranch("mainline"), isMain: false)];
        IReadOnlyList<GitWorktreeEntry> detached = [Entry(Repository, new Head.Detached(), isMain: true)];
        IReadOnlyList<GitWorktreeEntry> unknown = [Entry(Repository, new Head.Unknown(), isMain: true)];

        FastForwardPlace.Of(others, Main).ShouldBe(new FastForwardPlace.NotCheckedOut());
        FastForwardPlace.Of(detached, Main).ShouldBe(new FastForwardPlace.NotCheckedOut());
        FastForwardPlace.Of(unknown, Main).ShouldBe(new FastForwardPlace.NotCheckedOut());
        FastForwardPlace.Of([], Main).ShouldBe(new FastForwardPlace.NotCheckedOut());
    }

    private static Head.OnBranch OnBranch(string branch) => new(BranchName.From(branch));

    private static GitWorktreeEntry Entry(string path, Head head, bool isMain) => new(path, "0f3c2a1", head, isMain, new WorktreeLock.Unlocked(), IsPrunable: false);
}