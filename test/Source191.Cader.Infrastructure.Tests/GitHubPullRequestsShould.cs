using Source191.Cader.Adapters.PullRequests;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;
using System.ComponentModel;

namespace Source191.Cader.Infrastructure.Tests;

public sealed class GitHubPullRequestsShould
{
    /// <summary>Recorded from gh 2.x's pr list --json, with the repository and branches made up.</summary>
    private const string Recorded = """
        [
          {"createdAt":"2026-10-20T09:12:00Z","headRefName":"fix-123","headRefOid":"afdacd8b1c2e3f4a5b6c7d8e9f0a1b2c3d4e5f60","isDraft":false,"number":123,"state":"OPEN","url":"https://github.com/northwind/Northwind.Orders/pull/123"},
          {"createdAt":"2026-10-19T15:40:21Z","headRefName":"refund-retry","isDraft":true,"number":121,"state":"OPEN","url":"https://github.com/northwind/Northwind.Orders/pull/121"},
          {"createdAt":"2026-10-12T08:01:44Z","headRefName":"tidy-logging","headRefOid":"d6ed49cf0a1b2c3d4e5f60718293a4b5c6d7e8f9","isDraft":false,"number":88,"state":"MERGED","url":"https://github.com/northwind/Northwind.Orders/pull/88"},
          {"createdAt":"2026-10-02T11:30:00Z","headRefName":"spike-cache","isDraft":false,"number":80,"state":"CLOSED","url":"https://github.com/northwind/Northwind.Orders/pull/80"}
        ]
        """;

    private const string Gh = @"C:\Program Files\GitHub CLI\gh.exe";

    private static readonly RemoteRepository.GitHub Orders = new("northwind", "Northwind.Orders");
    private static readonly FolderPath Checkout = FolderPath.From(@"C:\Users\me\source\repos\Northwind.Orders");

    private readonly FakeProcessRunner processes = new FakeProcessRunner().AnswerAll(new ProcessResult(0, Recorded, string.Empty));

    private GitHubPullRequests Hosts => new(processes, new ProgramLocation.Found(Gh));

    [Fact]
    public async Task AnswerCliMissingWhenGhIsMissing()
    {
        GitHubPullRequests hosts = new(processes, new ProgramLocation.Missing());

        (await hosts.ListAsync(Orders, Checkout, BranchName.From("fix-123"), CancellationToken.None)).ShouldBe(new PullRequestListing.Unavailable(PullRequestGap.CliMissing));
        processes.CapturedRequests.ShouldBeEmpty();
    }

    [Fact]
    public async Task AskForTheBranchOnly()
    {
        PullRequestListing listing = await Hosts.ListAsync(Orders, Checkout, BranchName.From("fix-123"), CancellationToken.None);

        processes.CapturedRequests.Single().ShouldSatisfyAllConditions(
            request => request.FileName.ShouldBe(Gh),
            request => request.WorkingDirectory.ShouldBe(new WorkingFolder.At(Checkout.Value)),
            request => request.Arguments.ShouldBe(["pr", "list", "--repo", "northwind/Northwind.Orders", "--head", "fix-123", "--state", "all", "--limit", "20", "--json", "number,url,state,isDraft,headRefName,headRefOid,createdAt,closedAt"]));
        listing.ShouldBeOfType<PullRequestListing.Listed>().PullRequests.ShouldBe(
        [
            Row("fix-123", 123, PullRequestState.Open, "2026-10-20T09:12:00Z", Head("afdacd8b1c2e3f4a5b6c7d8e9f0a1b2c3d4e5f60")),
            Row("refund-retry", 121, PullRequestState.Draft, "2026-10-19T15:40:21Z"),
            Row("tidy-logging", 88, PullRequestState.Merged, "2026-10-12T08:01:44Z", Head("d6ed49cf0a1b2c3d4e5f60718293a4b5c6d7e8f9")),
            Row("spike-cache", 80, PullRequestState.Closed, "2026-10-02T11:30:00Z"),
        ]);
    }

    [Fact]
    public async Task KeepABranchWithASlash()
    {
        BranchName republisher = BranchName.From("feature/republisher");
        processes.AnswerAll(new ProcessResult(0, """
            [{"createdAt":"2026-10-21T10:00:00Z","headRefName":"feature/republisher","isDraft":true,"number":130,"state":"OPEN","url":"https://github.com/northwind/Northwind.Orders/pull/130"}]
            """, string.Empty));

        PullRequestListing listing = await Hosts.ListAsync(Orders, Checkout, republisher, CancellationToken.None);

        processes.CapturedRequests.Single().Arguments.ShouldContain("feature/republisher");
        listing.ShouldBeOfType<PullRequestListing.Listed>().PullRequests.Single().Branch.ShouldBe(republisher);
    }

    [Fact]
    public async Task LeaveOutAPullRequestWhoseLinkIsNotHttps()
    {
        processes.AnswerAll(new ProcessResult(0, """
            [{"createdAt":"2026-10-20T09:12:00Z","headRefName":"fix-123","isDraft":false,"number":123,"state":"OPEN","url":"file:///C:/Users/me/notes.html"},
             {"createdAt":"2026-10-19T15:40:21Z","headRefName":"fix-123","isDraft":false,"number":121,"state":"MERGED","url":"https://github.com/northwind/Northwind.Orders/pull/121"}]
            """, string.Empty));

        PullRequestListing listing = await Hosts.ListAsync(Orders, Checkout, BranchName.From("fix-123"), CancellationToken.None);

        listing.ShouldBeOfType<PullRequestListing.Listed>().PullRequests.Select(row => row.PullRequest.Number).ShouldBe([121]);
    }

    [Fact]
    public async Task SkipARowMissingAProperty()
    {
        processes.AnswerAll(new ProcessResult(0, """
            [{"createdAt":"2026-10-20T09:12:00Z","headRefName":"fix-123","isDraft":false,"number":123,"url":"https://github.com/northwind/Northwind.Orders/pull/123"},
             {"createdAt":"2026-10-19T15:40:21Z","headRefName":"fix-123","isDraft":false,"number":121,"state":"MERGED","url":"https://github.com/northwind/Northwind.Orders/pull/121"}]
            """, string.Empty));

        PullRequestListing listing = await Hosts.ListAsync(Orders, Checkout, BranchName.From("fix-123"), CancellationToken.None);

        listing.ShouldBeOfType<PullRequestListing.Listed>().PullRequests.Select(row => row.PullRequest.Number).ShouldBe([121]);
    }

    [Theory]
    [InlineData("")]
    [InlineData(",\"headRefOid\":null")]
    [InlineData(",\"headRefOid\":\"\"")]
    [InlineData(",\"headRefOid\":\"d6ed49cf\"")]
    [InlineData(",\"headRefOid\":42")]
    public async Task ListAPullRequestWhoseHeadCommitIsMissingOrUnreadableWithNoHeadCommit(string headRefOid)
    {
        processes.AnswerAll(new ProcessResult(0, $$"""
            [{"createdAt":"2026-10-12T08:01:44Z","headRefName":"fix-123","isDraft":false,"number":88,"state":"MERGED","url":"https://github.com/northwind/Northwind.Orders/pull/88"{{headRefOid}}}]
            """, string.Empty));

        PullRequestListing listing = await Hosts.ListAsync(Orders, Checkout, BranchName.From("fix-123"), CancellationToken.None);

        listing.ShouldBeOfType<PullRequestListing.Listed>().PullRequests.Single().PullRequest.Head.ShouldBe(new HeadCommit.NotRecorded());
    }

    [Fact]
    public async Task AskForClosedAt()
    {
        await Hosts.ListAsync(Orders, Checkout, BranchName.From("fix-123"), CancellationToken.None);

        string fields = processes.CapturedRequests.Single().Arguments.SkipWhile(argument => argument != "--json").ElementAt(1);
        fields.Split(',').ShouldContain("closedAt");
    }

    [Theory]
    [InlineData("MERGED")]
    [InlineData("CLOSED")]
    public async Task ReadWhenAMergedPullRequestClosed(string state)
    {
        processes.AnswerAll(new ProcessResult(0, $$"""
            [{"closedAt":"2026-11-21T14:28:23Z","createdAt":"2026-11-20T10:00:00Z","headRefName":"c-status","isDraft":false,"number":371,"state":"{{state}}","url":"https://github.com/northwind/Northwind.Orders/pull/371"}]
            """, string.Empty));

        PullRequestListing listing = await Hosts.ListAsync(Orders, Checkout, BranchName.From("c-status"), CancellationToken.None);

        RecordedTime.At finished = listing.ShouldBeOfType<PullRequestListing.Listed>().PullRequests.Single().PullRequest.Finished.ShouldBeOfType<RecordedTime.At>();
        finished.When.ShouldBe(new DateTimeOffset(2026, 11, 21, 14, 28, 23, TimeSpan.Zero));
        finished.When.Offset.ShouldBe(TimeSpan.Zero);
    }

    [Theory]
    [InlineData(false, ",\"closedAt\":\"2026-11-21T14:28:23Z\"")]
    [InlineData(true, ",\"closedAt\":\"2026-11-21T14:28:23Z\"")]
    [InlineData(false, ",\"closedAt\":null")]
    public async Task RecordNoFinishForAnOpenPullRequest(bool isDraft, string closedAt)
    {
        processes.AnswerAll(new ProcessResult(0, $$"""
            [{"createdAt":"2026-11-20T10:00:00Z","headRefName":"fix-123","isDraft":{{(isDraft ? "true" : "false")}},"number":123,"state":"OPEN","url":"https://github.com/northwind/Northwind.Orders/pull/123"{{closedAt}}}]
            """, string.Empty));

        PullRequestListing listing = await Hosts.ListAsync(Orders, Checkout, BranchName.From("fix-123"), CancellationToken.None);

        listing.ShouldBeOfType<PullRequestListing.Listed>().PullRequests.Single().PullRequest.Finished.ShouldBe(new RecordedTime.NotRecorded());
    }

    [Theory]
    [InlineData("")]
    [InlineData(",\"closedAt\":null")]
    [InlineData(",\"closedAt\":\"\"")]
    [InlineData(",\"closedAt\":\"yesterday\"")]
    [InlineData(",\"closedAt\":42")]
    public async Task RecordNoFinishWhenClosedAtIsMissingOrBlank(string closedAt)
    {
        processes.AnswerAll(new ProcessResult(0, $$"""
            [{"createdAt":"2026-11-20T10:00:00Z","headRefName":"c-status","isDraft":false,"number":371,"state":"MERGED","url":"https://github.com/northwind/Northwind.Orders/pull/371"{{closedAt}}}]
            """, string.Empty));

        PullRequestListing listing = await Hosts.ListAsync(Orders, Checkout, BranchName.From("c-status"), CancellationToken.None);

        listing.ShouldBeOfType<PullRequestListing.Listed>().PullRequests.Single().PullRequest.Finished.ShouldBe(new RecordedTime.NotRecorded());
    }

    [Fact]
    public async Task ReportCliMissingWhenGhCannotStart()
    {
        processes.Fail(new Win32Exception(2, "The system cannot find the file specified."));

        (await Hosts.ListAsync(Orders, Checkout, BranchName.From("fix-123"), CancellationToken.None)).ShouldBe(new PullRequestListing.Unavailable(PullRequestGap.CliMissing));
    }

    [Fact]
    public async Task ReportNotSignedInOnExitCodeFour()
    {
        processes.AnswerAll(new ProcessResult(4, string.Empty, "To get started with GitHub CLI, please run:  gh auth login"));

        (await Hosts.ListAsync(Orders, Checkout, BranchName.From("fix-123"), CancellationToken.None)).ShouldBe(new PullRequestListing.Unavailable(PullRequestGap.NotSignedIn));
    }

    [Theory]
    [InlineData(1, "GraphQL: Could not resolve to a Repository with the name 'northwind/Northwind.Orders'.")]
    [InlineData(0, "not json")]
    public async Task ReportFailedOnAnythingElse(int exitCode, string words)
    {
        processes.AnswerAll(new ProcessResult(exitCode, exitCode == 0 ? words : string.Empty, words));

        (await Hosts.ListAsync(Orders, Checkout, BranchName.From("fix-123"), CancellationToken.None)).ShouldBe(new PullRequestListing.Unavailable(PullRequestGap.Failed));
    }

    private static BranchPullRequest Row(string branch, int number, PullRequestState state, string createdAt) => Row(branch, number, state, createdAt, new HeadCommit.NotRecorded());

    private static BranchPullRequest Row(string branch, int number, PullRequestState state, string createdAt, HeadCommit head) =>
        new(BranchName.From(branch), new PullRequest(PullRequestHost.GitHub, number, new Uri($"https://github.com/northwind/Northwind.Orders/pull/{number}"), state, DateTimeOffset.Parse(createdAt, System.Globalization.CultureInfo.InvariantCulture), head, new RecordedTime.NotRecorded()));

    private static HeadCommit.Known Head(string commit) => new(CommitId.From(commit));

}