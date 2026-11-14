using Source191.Cader.Adapters.Git;
using Source191.Cader.Application.Launch;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application.Launch;

public sealed class BaseUpdaterShould
{
    private const string Repository = @"C:\Users\me\source\repos\Northwind.Orders";
    private const string Unreachable = "fatal: unable to access 'https://github.com/northwind/Northwind.Orders.git/': Could not resolve host: github.com";

    private static readonly RemoteBranch OriginMain = RemoteBranch.From("origin/main");

    private readonly FakeProcessRunner processes = new FakeProcessRunner().AnswerAll(new ProcessResult(0, string.Empty, string.Empty));

    private BaseUpdater Updater => new(new GitCli(processes, "git"));

    [Fact]
    public async Task RunNoGitWhenAskedForNothing()
    {
        BaseUpdate update = await Updater.UpdateAsync(FolderPath.From(Repository), OriginMain, BeforeBranching.None, CancellationToken.None);

        update.ShouldBe(new BaseUpdate.Current());
        processes.CapturedRequests.ShouldBeEmpty();
    }

    [Fact]
    public async Task FetchTheBaseAndAnswerCurrent()
    {
        BaseUpdate update = await Updater.UpdateAsync(FolderPath.From(Repository), OriginMain, BeforeBranching.Fetch, CancellationToken.None);

        update.ShouldBe(new BaseUpdate.Current());
        ProcessRequest fetch = processes.CapturedRequests.ShouldHaveSingleItem();
        fetch.Arguments.ShouldBe(["fetch", "origin", "+refs/heads/main:refs/remotes/origin/main"]);
        fetch.WorkingDirectory.ShouldBe(new WorkingFolder.At(Repository));
    }

    [Fact]
    public async Task AnswerNotFetchedWithGitsWordsWhenTheFetchFails()
    {
        processes.Answer(IsFetchFromOrigin, new ProcessResult(128, string.Empty, Unreachable));

        BaseUpdate update = await Updater.UpdateAsync(FolderPath.From(Repository), OriginMain, BeforeBranching.Fetch, CancellationToken.None);

        BaseUpdate.NotFetched notFetched = update.ShouldBeOfType<BaseUpdate.NotFetched>();
        notFetched.Base.ShouldBe(OriginMain);
        notFetched.Reason.ShouldContain("Could not resolve host: github.com");
    }

    [Fact]
    public async Task AnswerNotFetchedWithTheLimitsWordsWhenTheFetchRunsOutOfTime()
    {
        processes.Throw(IsFetchFromOrigin, new ProcessTimedOutException("git.exe", TimeSpan.FromSeconds(60)));

        BaseUpdate update = await Updater.UpdateAsync(FolderPath.From(Repository), OriginMain, BeforeBranching.Fetch, CancellationToken.None);

        update.ShouldBe(new BaseUpdate.NotFetched(OriginMain, "git.exe did not finish within 60 seconds and was ended"));
    }

    [Fact]
    public async Task LetACancellationThrough()
    {
        processes.Throw(IsFetchFromOrigin, new OperationCanceledException());

        await Should.ThrowAsync<OperationCanceledException>(() => Updater.UpdateAsync(FolderPath.From(Repository), OriginMain, BeforeBranching.Fetch, CancellationToken.None));
    }

    private static bool IsFetchFromOrigin(ProcessRequest request) => request.Arguments is ["fetch", "origin", ..];
}
