using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Domain;

public sealed class PullRequestsShould
{
    private static readonly DateTimeOffset Monday = new(2026, 10, 19, 9, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(PullRequestState.Open)]
    [InlineData(PullRequestState.Draft)]
    [InlineData(PullRequestState.Active)]
    public void PreferTheOpenOneCountingDraftsAndActive(PullRequestState open)
    {
        PullRequest older = GitHub(7, open, Monday);
        PullRequest newer = GitHub(9, PullRequestState.Merged, Monday.AddDays(2));

        PullRequests.ForBranch([newer, older]).ShouldBe(new PullRequestLookup.Found(older));
    }

    [Fact]
    public void OtherwiseTakeTheMostRecent()
    {
        PullRequest closed = GitHub(7, PullRequestState.Closed, Monday);
        PullRequest merged = GitHub(9, PullRequestState.Merged, Monday.AddDays(2));
        PullRequest abandoned = GitHub(8, PullRequestState.Abandoned, Monday.AddDays(1));

        PullRequests.ForBranch([closed, merged, abandoned]).ShouldBe(new PullRequestLookup.Found(merged));
    }

    [Fact]
    public void AnswerNoPullRequestForAnEmptyList()
    {
        PullRequests.ForBranch([]).ShouldBe(new PullRequestLookup.Missing(PullRequestGap.NoPullRequest));
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

    private static PullRequest GitHub(int number, PullRequestState state, DateTimeOffset createdAt) => GitHub(number, state, createdAt, new HeadCommit.NotRecorded());

    private static PullRequest GitHub(int number, PullRequestState state, DateTimeOffset createdAt, HeadCommit head) =>
        new(PullRequestHost.GitHub, number, new Uri($"https://github.com/northwind/Northwind.Orders/pull/{number}"), state, createdAt, head, new RecordedTime.NotRecorded());
}