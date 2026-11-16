using Source191.Cader.Adapters.Terminal;
using Source191.Cader.Domain;

namespace Source191.Cader.Infrastructure.Tests;

/// <summary>
/// Searches real windows through UI Automation for a title no tab has, so nothing is ever selected. Each search runs
/// twice, so a COM object one call released by mistake, and the next call reuses, fails it. The test process owns no window, so only the search of every terminal lists tab items, and only where a Windows Terminal window is open (a developer's machine, not a hosted runner).
/// </summary>
[Trait("Category", "Integration")]
public sealed class UiAutomationTabsShould
{
    private readonly string title = "Cader test " + Guid.NewGuid().ToString("N");

    [Fact]
    public void AnswerFalseTwiceForATitleNoTabHasInThisProcessesWindows()
    {
        TerminalWindow window = new TerminalWindow.Owning(Environment.ProcessId);

        UiAutomationTabs.Select(window, title).ShouldBeFalse();
        UiAutomationTabs.Select(window, title).ShouldBeFalse();
    }

    [Fact]
    public void AnswerFalseTwiceForATitleNoTabHasInAnyTerminal()
    {
        TerminalWindow window = new TerminalWindow.Any();

        UiAutomationTabs.Select(window, title).ShouldBeFalse();
        UiAutomationTabs.Select(window, title).ShouldBeFalse();
    }
}