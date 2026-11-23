using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Domain;

public sealed class BranchFlagsShould
{
    private static readonly HeadCommit MergedHead = new HeadCommit.Known(CommitId.From("afdacd8b1c2e3f4a5b6c7d8e9f0a1b2c3d4e5f60"));

    private static readonly Upstream Tracking = new Upstream.Tracking(RemoteBranch.From("origin/c-new-bug"));

    [Fact]
    public void PutLeftBehindBeforeGone()
    {
        BranchFlags.Of(Shown(PullRequestState.Merged, MergedHead), new Upstream.Gone(), commitsAfterMergedHead: 2).ShouldBe(BranchFlagKind.LeftBehind);
    }

    [Theory]
    [InlineData(PullRequestState.Merged)]
    [InlineData(PullRequestState.Completed)]
    public void SayLeftBehindForCommitsAfterTheMergedHead(PullRequestState merged)
    {
        BranchFlags.Of(Shown(merged, MergedHead), Tracking, commitsAfterMergedHead: 1).ShouldBe(BranchFlagKind.LeftBehind);
    }

    [Fact]
    public void SayGoneAfterAMergeWithNothingBehind()
    {
        BranchFlags.Of(Shown(PullRequestState.Merged, MergedHead), new Upstream.Gone(), commitsAfterMergedHead: 0).ShouldBe(BranchFlagKind.Gone);
    }

    [Theory]
    [InlineData(PullRequestState.Closed)]
    [InlineData(PullRequestState.Abandoned)]
    public void SayClosedWhileTheUpstreamStillTracks(PullRequestState closed)
    {
        BranchFlags.Of(Shown(closed, new HeadCommit.NotRecorded()), Tracking, commitsAfterMergedHead: null).ShouldBe(BranchFlagKind.Closed);
    }

    [Fact]
    public void SayNoPullRequestWhenTheHostListedNone()
    {
        BranchFlags.Of(new PullRequestLookup.Missing(PullRequestGap.NoPullRequest), new Upstream.None(), commitsAfterMergedHead: null).ShouldBe(BranchFlagKind.NoPullRequest);
    }

    [Theory]
    [InlineData(PullRequestState.Open)]
    [InlineData(PullRequestState.Draft)]
    [InlineData(PullRequestState.Active)]
    public void SayNothingForAnOpenPullRequest(PullRequestState open)
    {
        BranchFlags.Of(Shown(open, MergedHead), Tracking, commitsAfterMergedHead: null).ShouldBe(BranchFlagKind.None);
    }

    [Theory]
    [InlineData(PullRequestGap.NoBranch)]
    [InlineData(PullRequestGap.NoRemote)]
    [InlineData(PullRequestGap.UnknownHost)]
    [InlineData(PullRequestGap.CliMissing)]
    [InlineData(PullRequestGap.NotSignedIn)]
    [InlineData(PullRequestGap.Looking)]
    [InlineData(PullRequestGap.Failed)]
    [InlineData(PullRequestGap.BranchNotGivenToAz)]
    public void SayNothingWhenTheLookupCouldNotAnswerButStillSayGone(PullRequestGap gap)
    {
        PullRequestLookup missing = new PullRequestLookup.Missing(gap);

        BranchFlags.Of(missing, Tracking, commitsAfterMergedHead: null).ShouldBe(BranchFlagKind.None);
        BranchFlags.Of(missing, new Upstream.None(), commitsAfterMergedHead: null).ShouldBe(BranchFlagKind.None);
        BranchFlags.Of(missing, new Upstream.Gone(), commitsAfterMergedHead: null).ShouldBe(BranchFlagKind.Gone);
    }

    [Fact]
    public void JudgeNothingLeftBehindWhenGitCouldNotCount()
    {
        BranchFlags.Of(Shown(PullRequestState.Merged, MergedHead), Tracking, commitsAfterMergedHead: null).ShouldBe(BranchFlagKind.None);
    }

    [Fact]
    public void JudgeNothingLeftBehindWithoutARecordedHead()
    {
        BranchFlags.Of(Shown(PullRequestState.Merged, new HeadCommit.NotRecorded()), Tracking, commitsAfterMergedHead: 3).ShouldBe(BranchFlagKind.None);
    }

    [Fact]
    public void SayNothingForAMergedBranchWithNothingBehind()
    {
        BranchFlags.Of(Shown(PullRequestState.Merged, MergedHead), Tracking, commitsAfterMergedHead: 0).ShouldBe(BranchFlagKind.None);
    }

    private static PullRequestLookup.Found Shown(PullRequestState state, HeadCommit head) =>
        new(new PullRequest(PullRequestHost.GitHub, 298, new Uri("https://github.com/northwind/Northwind.Orders/pull/298"), state, Fixtures.Noon, head, new RecordedTime.NotRecorded()));
}