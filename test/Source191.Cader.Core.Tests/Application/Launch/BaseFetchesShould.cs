using Microsoft.Extensions.Time.Testing;
using Source191.Cader.Adapters.Git;
using Source191.Cader.Application.Launch;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application.Launch;

public sealed class BaseFetchesShould
{
    private const string Unreachable = "fatal: unable to access 'https://github.com/northwind/Northwind.Orders.git/': Could not resolve host: github.com";

    private static readonly FolderPath Orders = FolderPath.From(@"C:\Users\me\source\repos\Northwind.Orders");
    private static readonly RemoteBranch OriginMain = RemoteBranch.From("origin/main");

    private readonly FakeTimeProvider clock = new(Fixtures.Noon);
    private readonly FakeProcessRunner processes = new FakeProcessRunner()
        .AnswerAll(new ProcessResult(0, string.Empty, string.Empty))
        .Answer(request => request.Arguments.Contains("symbolic-ref"), new ProcessResult(0, "origin/main\n", string.Empty));

    private readonly BaseFetches fetches;
    private int fetchesStarted;

    public BaseFetchesShould()
    {
        fetches = new BaseFetches(new GitCli(processes, "git"), clock);
        processes.OnRun = request => Interlocked.Add(ref fetchesStarted, IsFetch(request) ? 1 : 0);
    }

    [Fact]
    public async Task FetchOriginsDefaultBranchWhenBegun()
    {
        await fetches.Begin(Orders);

        ProcessRequest fetch = processes.CapturedRequests.Single(IsFetch);
        fetch.Arguments.ShouldBe(["fetch", "origin", "+refs/heads/main:refs/remotes/origin/main"]);
        fetch.WorkingDirectory.ShouldBe(new WorkingFolder.At(Orders.Value));
    }

    [Fact]
    public async Task StartNoSecondFetchWhileOneRuns()
    {
        TaskCompletionSource release = HoldTheFetchOpen();

        Task first = fetches.Begin(Orders);
        Task second = fetches.Begin(Orders);
        release.SetResult();
        await Task.WhenAll(first, second);

        processes.CapturedRequests.Count(IsFetch).ShouldBe(1);
    }

    [Fact]
    public async Task JoinARunningFetch()
    {
        TaskCompletionSource release = HoldTheFetchOpen();
        Task early = fetches.Begin(Orders);

        Task joined = fetches.FetchAsync(Orders, OriginMain, CancellationToken.None);
        await Task.Delay(50, TestContext.Current.CancellationToken);
        joined.IsCompleted.ShouldBeFalse("the launch waits for the early fetch to end");
        release.SetResult();
        await Task.WhenAll(early, joined);

        processes.CapturedRequests.Count(IsFetch).ShouldBe(1);
    }

    [Fact]
    public async Task JoinAFetchThatSucceededWithinAMinute()
    {
        await fetches.Begin(Orders);
        clock.Advance(TimeSpan.FromSeconds(59));

        await fetches.FetchAsync(Orders, OriginMain, CancellationToken.None);

        processes.CapturedRequests.Count(IsFetch).ShouldBe(1);
    }

    [Fact]
    public async Task FetchAgainAfterAMinute()
    {
        await fetches.Begin(Orders);
        clock.Advance(TimeSpan.FromMinutes(1));

        await fetches.FetchAsync(Orders, OriginMain, CancellationToken.None);

        processes.CapturedRequests.Count(IsFetch).ShouldBe(2);
    }

    [Fact]
    public async Task FetchAgainAfterAFailedEarlyFetch()
    {
        processes.AnswerInTurn(IsFetch, new ProcessResult(128, string.Empty, Unreachable), new ProcessResult(0, string.Empty, string.Empty));
        await fetches.Begin(Orders);

        await fetches.FetchAsync(Orders, OriginMain, CancellationToken.None);

        processes.CapturedRequests.Count(IsFetch).ShouldBe(2);
    }

    [Fact]
    public async Task LetTheLaunchsOwnFetchFailWithGitsWords()
    {
        processes.Answer(IsFetch, new ProcessResult(128, string.Empty, Unreachable));
        await fetches.Begin(Orders);

        GitException failure = await Should.ThrowAsync<GitException>(() => fetches.FetchAsync(Orders, OriginMain, CancellationToken.None));

        failure.Message.ShouldContain("Could not resolve host: github.com");
    }

    [Fact]
    public async Task FetchAgainForAnotherRemoteBranch()
    {
        await fetches.Begin(Orders);

        await fetches.FetchAsync(Orders, RemoteBranch.From("origin/develop"), CancellationToken.None);

        processes.CapturedRequests.Where(IsFetch).Select(request => request.Arguments[^1]).ShouldBe(["+refs/heads/main:refs/remotes/origin/main", "+refs/heads/develop:refs/remotes/origin/develop"]);
    }

    [Fact]
    public async Task FetchItselfWhenNothingWasBegun()
    {
        await fetches.FetchAsync(Orders, OriginMain, CancellationToken.None);

        processes.CapturedRequests.Count(IsFetch).ShouldBe(1);
        processes.CapturedRequests.ShouldNotContain(request => request.Arguments.Contains("symbolic-ref"));
    }

    [Fact]
    public async Task FetchNothingWithoutOriginHead()
    {
        processes.Answer(request => request.Arguments.Contains("symbolic-ref"), new ProcessResult(128, string.Empty, "fatal: ref refs/remotes/origin/HEAD is not a symbolic ref"));

        await fetches.Begin(Orders);

        processes.CapturedRequests.ShouldNotContain(request => IsFetch(request));
    }

    [Fact]
    public async Task NeverFaultWhenGitFails()
    {
        processes.Throw(IsFetch, new ProcessTimedOutException("git.exe", TimeSpan.FromSeconds(60)));
        Task timedOut = fetches.Begin(Orders);
        await timedOut;
        clock.Advance(TimeSpan.FromMinutes(2));

        Task again = fetches.Begin(Orders);
        await again;

        timedOut.IsCompletedSuccessfully.ShouldBeTrue();
        again.IsCompletedSuccessfully.ShouldBeTrue();
        fetchesStarted.ShouldBe(2, "a finished early fetch, failed or not, lets the next one start");
    }

    private TaskCompletionSource HoldTheFetchOpen()
    {
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        processes.WhileRunning = request => IsFetch(request) ? release.Task : Task.CompletedTask;

        return release;
    }

    private static bool IsFetch(ProcessRequest request) => request.Arguments is ["fetch", "origin", ..];
}
