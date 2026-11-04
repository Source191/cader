using Source191.Cader.Adapters.Git;
using Source191.Cader.Application.Actions;
using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;
using static Source191.Cader.Core.Tests.Application.Actions.SessionTestData;

namespace Source191.Cader.Core.Tests.Application.Actions;

public sealed class DeleteBranchHandlerShould
{
    private readonly FakeProcessRunner processes = Processes();

    [Fact]
    public async Task DeleteABranchOnRequestWithForce()
    {
        DeleteBranchHandler handler = new(new GitCli(processes, "git"));

        Outcome message = await handler.HandleAsync(new DeleteBranch(new BranchToConfirm(FolderPath.From(Repository), BranchName.From("fix-123"))), CancellationToken.None);

        processes.CapturedRequests.Single().Arguments.ShouldBe(["branch", "-D", "fix-123"]);
        message.ShouldBe(new DeleteBranchOutcome());
    }

    [Fact]
    public async Task LetAGitFailureReachTheCaller()
    {
        processes.Answer(request => request.Arguments.Contains("branch"), new ProcessResult(1, string.Empty, "error: branch 'fix-123' not found."));
        DeleteBranchHandler handler = new(new GitCli(processes, "git"));

        await Should.ThrowAsync<GitException>(() => handler.HandleAsync(new DeleteBranch(new BranchToConfirm(FolderPath.From(Repository), BranchName.From("fix-123"))), CancellationToken.None));
    }
}