using Source191.Cader.Application.Actions;
using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;
using static Source191.Cader.Core.Tests.Application.Actions.WorktreeTestData;

namespace Source191.Cader.Core.Tests.Application.Actions;

public sealed class OpenShellHandlerShould
{
    [Fact]
    public async Task OpenAPlainShellTabInTheWorktree()
    {
        FakeWindowsTerminal terminal = new();
        OpenShellHandler handler = new(terminal);

        Outcome message = await handler.HandleAsync(new OpenShellTab(Row(WorktreeHealthKind.Ok)), CancellationToken.None);

        TabLaunch tab = terminal.Opened.Single();
        tab.Title.ShouldBe("Northwind.Orders-fix-123");
        tab.StartingDirectory.Value.ShouldBe(Worktree);
        message.ShouldBe(new OpenShellOutcome("Northwind.Orders-fix-123"));
    }
}