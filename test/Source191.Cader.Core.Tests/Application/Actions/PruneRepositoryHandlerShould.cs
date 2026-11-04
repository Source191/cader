using Source191.Cader.Adapters.Git;
using Source191.Cader.Application.Actions;
using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;
using static Source191.Cader.Core.Tests.Application.Actions.WorktreeTestData;

namespace Source191.Cader.Core.Tests.Application.Actions;

public sealed class PruneRepositoryHandlerShould
{
    private readonly FakeProcessRunner processes = Processes();

    [Fact]
    public async Task ReportHowManyRegistrationsAPruneRemoved()
    {
        processes.AnswerInTurn(request => request.Arguments.Contains("--porcelain"), new ProcessResult(0, "worktree C:/a\nHEAD a\nbranch refs/heads/main\n\nworktree C:/b\nHEAD b\nbranch refs/heads/x\n\nworktree C:/c\nHEAD c\nbranch refs/heads/y\n", string.Empty), new ProcessResult(0, "worktree C:/a\nHEAD a\nbranch refs/heads/main\n", string.Empty));
        PruneRepositoryHandler handler = new(new GitCli(processes, "git"));

        Outcome message = await handler.HandleAsync(new PruneRepository(FolderPath.From(Repository)), CancellationToken.None);

        message.ShouldBe(new PruneRepositoryOutcome(2, "Northwind.Orders"));
    }
}