using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Domain;

public sealed class PullRequestsShould
{
    private static readonly DateTimeOffset Monday = new(2026, 10, 19, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset MergedOnSaturday = new(2026, 11, 21, 14, 28, 23, TimeSpan.Zero);
    private static readonly DateTimeOffset CreatedOnSunday = new(2026, 11, 22, 9, 12, 51, TimeSpan.Zero);

    [Theory]
    [InlineData(PullRequestState.Open)]
    [InlineData(PullRequestState.Draft)]
    [InlineData(PullRequestState.Active)]
    public void PreferTheOpenOneCountingDraftsAndActive(PullRequestState open)
    {
        PullRequest older = GitHub(7, open, Monday);
        PullRequest newer = GitHub(9, PullRequestState.Merged, Monday.AddDays(2));

        PullRequests.ForBranch([newer, older], new RecordedTime.NotRecorded()).ShouldBe(new PullRequestLookup.Found(older));
    }

    [Fact]
    public void OtherwiseTakeTheMostRecent()
    {
        PullRequest closed = GitHub(7, PullRequestState.Closed, Monday);
        PullRequest merged = GitHub(9, PullRequestState.Merged, Monday.AddDays(2));
        PullRequest abandoned = GitHub(8, PullRequestState.Abandoned, Monday.AddDays(1));

        PullRequests.ForBranch([closed, merged, abandoned], new RecordedTime.NotRecorded()).ShouldBe(new PullRequestLookup.Found(merged));
    }

    [Fact]
    public void AnswerNoPullRequestForAnEmptyList()
    {
        PullRequests.ForBranch([], new RecordedTime.NotRecorded()).ShouldBe(new PullRequestLookup.Missing(PullRequestGap.NoPullRequest));
    }

    [Theory]
    [InlineData(PullRequestState.Open, true)]
    [InlineData(PullRequestState.Draft, true)]
    [InlineData(PullRequestState.Active, true)]
    [InlineData(PullRequestState.Merged, false)]
    [InlineData(PullRequestState.Closed, false)]
    [InlineData(PullRequestState.Completed, false)]
    [InlineData(PullRequestState.Abandoned, false)]
    public void KnowWhichStatesAreOpen(PullRequestState state, bool open)
    {
        state.IsOpen().ShouldBe(open);
    }

    [Fact]
    public void CarryTheHeadCommitTheHostRecordedOrSayItRecordedNone()
    {
        CommitId head = CommitId.From("afdacd8b1c2e3f4a5b6c7d8e9f0a1b2c3d4e5f60");

        GitHub(298, PullRequestState.Merged, Monday, new HeadCommit.Known(head)).Head.ShouldBe(new HeadCommit.Known(head));
        GitHub(299, PullRequestState.Open, Monday, new HeadCommit.NotRecorded()).Head.ShouldBe(new HeadCommit.NotRecorded());
    }

    [Fact]
    public void LeaveOutAPullRequestThatFinishedBeforeTheBranchWasCreated()
    {
        PullRequest merged = Finished(371, PullRequestState.Merged, MergedOnSaturday);

        PullRequests.ForBranch([merged], new RecordedTime.At(CreatedOnSunday)).ShouldBe(new PullRequestLookup.Missing(PullRequestGap.NoPullRequest));
    }

    [Fact]
    public void KeepAPullRequestThatFinishedAfterTheBranchWasCreated()
    {
        PullRequest merged = Finished(123, PullRequestState.Merged, CreatedOnSunday.AddHours(7));

        PullRequests.ForBranch([merged], new RecordedTime.At(CreatedOnSunday)).ShouldBe(new PullRequestLookup.Found(merged));
    }

    [Fact]
    public void ShowTheNewerPullRequestOfAReusedName()
    {
        PullRequest earlier = Finished(371, PullRequestState.Merged, MergedOnSaturday);
        PullRequest later = Finished(380, PullRequestState.Merged, CreatedOnSunday.AddHours(5), CreatedOnSunday.AddHours(1));

        PullRequests.ForBranch([earlier, later], new RecordedTime.At(CreatedOnSunday)).ShouldBe(new PullRequestLookup.Found(later));
    }

    [Theory]
    [InlineData(PullRequestState.Open)]
    [InlineData(PullRequestState.Draft)]
    public void NeverLeaveOutAnOpenPullRequest(PullRequestState open)
    {
        PullRequest colleagues = GitHub(123, open, CreatedOnSunday.AddDays(-1), new HeadCommit.NotRecorded(), new RecordedTime.At(CreatedOnSunday.AddDays(-1)));

        PullRequests.ForBranch([colleagues], new RecordedTime.At(CreatedOnSunday)).ShouldBe(new PullRequestLookup.Found(colleagues));
    }

    [Fact]
    public void LeaveOutAnAbandonedPullRequestThatFinishedBeforeTheBranch()
    {
        PullRequest abandoned = Finished(4400, PullRequestState.Abandoned, MergedOnSaturday);

        PullRequests.ForBranch([abandoned], new RecordedTime.At(CreatedOnSunday)).ShouldBe(new PullRequestLookup.Missing(PullRequestGap.NoPullRequest));
    }

    [Fact]
    public void KeepEverythingWhenTheBranchsCreationIsNotRecorded()
    {
        PullRequest merged = Finished(371, PullRequestState.Merged, MergedOnSaturday);

        PullRequests.ForBranch([merged], new RecordedTime.NotRecorded()).ShouldBe(new PullRequestLookup.Found(merged));
    }

    [Fact]
    public void KeepAFinishedPullRequestWithNoRecordedFinish()
    {
        PullRequest merged = GitHub(371, PullRequestState.Merged, MergedOnSaturday.AddDays(-1));

        PullRequests.ForBranch([merged], new RecordedTime.At(CreatedOnSunday)).ShouldBe(new PullRequestLookup.Found(merged));
    }

    [Fact]
    public void KeepAPullRequestThatFinishedInTheSameSecond()
    {
        PullRequest merged = Finished(371, PullRequestState.Merged, CreatedOnSunday);

        PullRequests.ForBranch([merged], new RecordedTime.At(CreatedOnSunday)).ShouldBe(new PullRequestLookup.Found(merged));
    }

    private static PullRequest Finished(int number, PullRequestState state, DateTimeOffset finished) => Finished(number, state, finished, finished.AddDays(-1));

    private static PullRequest Finished(int number, PullRequestState state, DateTimeOffset finished, DateTimeOffset createdAt) =>
        GitHub(number, state, createdAt, new HeadCommit.NotRecorded(), new RecordedTime.At(finished));

    private static PullRequest GitHub(int number, PullRequestState state, DateTimeOffset createdAt) => GitHub(number, state, createdAt, new HeadCommit.NotRecorded());

    private static PullRequest GitHub(int number, PullRequestState state, DateTimeOffset createdAt, HeadCommit head) => GitHub(number, state, createdAt, head, new RecordedTime.NotRecorded());

    private static PullRequest GitHub(int number, PullRequestState state, DateTimeOffset createdAt, HeadCommit head, RecordedTime finished) =>
        new(PullRequestHost.GitHub, number, new Uri($"https://github.com/northwind/Northwind.Orders/pull/{number}"), state, createdAt, head, finished);
}