using Source191.Cader.Application;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Application;

public sealed class WordingOfBranchesShould
{
    private static readonly FolderPath Repository = FolderPath.From(@"C:\Users\me\source\repos\Northwind.Orders");

    [Theory]
    [InlineData(BranchFlagKind.LeftBehind, "▲ left behind", TextTone.Red)]
    [InlineData(BranchFlagKind.Gone, "✗ gone", TextTone.Amber)]
    [InlineData(BranchFlagKind.Closed, "✗ closed", TextTone.Amber)]
    [InlineData(BranchFlagKind.NoPullRequest, "· no PR", TextTone.Grey)]
    [InlineData(BranchFlagKind.None, "", TextTone.Plain)]
    public void ShowEachFlagAsItsGlyphAndWordInItsTone(BranchFlagKind flag, string cell, TextTone tone)
    {
        Wording.BranchCell(flag).ShouldBe(cell);
        Wording.ToneOf(flag).ShouldBe(tone);
    }

    [Fact]
    public void CountTheCommitsAfterTheMergedPullRequest()
    {
        Wording.BranchReason(Row(BranchFlagKind.LeftBehind, Shown(PullRequestHost.GitHub, 298, PullRequestState.Merged)) with { CommitsAfterMerge = 1 }).ShouldBe("1 commit after #298 merged");
        Wording.BranchReason(Row(BranchFlagKind.LeftBehind, Shown(PullRequestHost.AzureDevOps, 12, PullRequestState.Completed)) with { CommitsAfterMerge = 2 }).ShouldBe("2 commits after !12 completed");
    }

    [Fact]
    public void SayAGoneUpstreamIsAsOfTheLastFetch()
    {
        Wording.BranchReason(Row(BranchFlagKind.Gone, new PullRequestLookup.Missing(PullRequestGap.NotSignedIn))).ShouldBe("origin no longer has it, as of the last fetch");
    }

    [Fact]
    public void NameTheClosedPullRequest()
    {
        Wording.BranchReason(Row(BranchFlagKind.Closed, Shown(PullRequestHost.GitHub, 301, PullRequestState.Closed))).ShouldBe("#301 closed without merging");
        Wording.BranchReason(Row(BranchFlagKind.Closed, Shown(PullRequestHost.AzureDevOps, 14, PullRequestState.Abandoned))).ShouldBe("!14 abandoned without merging");
    }

    [Fact]
    public void SayThereIsNoPullRequest()
    {
        Wording.BranchReason(Row(BranchFlagKind.NoPullRequest, new PullRequestLookup.Missing(PullRequestGap.NoPullRequest))).ShouldBe("no pull request on its host");
    }

    [Fact]
    public void GiveNoReasonWhenNothingIsFlagged()
    {
        Wording.BranchReason(Row(BranchFlagKind.None, Shown(PullRequestHost.GitHub, 302, PullRequestState.Open))).ShouldBeEmpty();
    }

    [Fact]
    public void ListEveryFlagInTheBranchHelp()
    {
        string branch = Wording.WorktreesColumns.Single(column => column.Name == "BRANCH").Meaning;

        Enum.GetValues<BranchFlagKind>().Where(flag => flag is not BranchFlagKind.None).ShouldAllBe(flag => branch.Contains(Wording.BranchCell(flag), StringComparison.Ordinal));
    }

    private static PullRequestLookup.Found Shown(PullRequestHost host, int number, PullRequestState state) =>
        new(new PullRequest(host, number, new Uri($"https://github.com/northwind/Northwind.Orders/pull/{number}"), state, Fixtures.Noon, new HeadCommit.NotRecorded()));

    private static WorktreeRow Row(BranchFlagKind flag, PullRequestLookup lookup) =>
        new(Repository, Repository, new Head.OnBranch(BranchName.From("c-new-bug")), WorktreeHealth.OnlyOnOrigin, 0, 0, new LastCommit.Unknown(), new WorktreeSession.None(), LeftoverFileCount: 0, OwnerExists: true, FolderExists: true)
        {
            PullRequest = lookup,
            Flag = flag,
        };
}
