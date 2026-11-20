using Microsoft.Extensions.Time.Testing;
using Source191.Cader.Application;
using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application;

public sealed class PaneHeightSavesShould
{
    private readonly FakeTimeProvider clock = new(Fixtures.Noon);
    private readonly InMemoryState state = new();

    [Fact]
    public async Task WriteARunOfKeyPressesOnceASecondAfterTheLast()
    {
        CurrentSettings current = new(TestSettings.For(@"C:\Users\me\source\repos"), state);
        SettlingPaneHeights heights = new(current.Value.PaneHeights, clock);
        PaneHeightSaves saves = new(heights, current, new DiagnosticLog(clock, []));

        for (int press = 1; press <= 5; press++)
        {
            heights.Changed(ConsoleTab.Worktrees, heights.Of(ConsoleTab.Worktrees) + 1);
            await TickFor(saves, TimeSpan.FromMilliseconds(200));
        }

        await TickFor(saves, TimeSpan.FromSeconds(3));

        state.ConfigWrites.ShouldBe(1);
        current.Value.PaneHeights.ShouldBe(new PaneHeights(Sessions: 6, Worktrees: 15));
        (await state.ReadConfigAsync()).WorktreesDetailRows.ShouldBe(15);
    }

    [Fact]
    public async Task RecordAFailedWriteAndTryAgainOnTheNextSettledChange()
    {
        CurrentSettings current = new(TestSettings.For(@"C:\Users\me\source\repos"), state);
        SettlingPaneHeights heights = new(current.Value.PaneHeights, clock);
        DiagnosticLog log = new(clock, []);
        PaneHeightSaves saves = new(heights, current, log);
        state.HoldNextConfigWrite(Task.FromException(new IOException("The disc is full.")));

        heights.Changed(ConsoleTab.Sessions, 4);
        await TickFor(saves, TimeSpan.FromSeconds(2));
        heights.Changed(ConsoleTab.Sessions, 3);
        await TickFor(saves, TimeSpan.FromSeconds(2));

        log.Snapshot().Select(timed => timed.Event).OfType<Failed>().ShouldHaveSingleItem().Site.ShouldBe(FailureSite.PaneHeightSave);
        state.ConfigWrites.ShouldBe(1);
        (await state.ReadConfigAsync()).SessionsDetailRows.ShouldBe(3);
    }

    /// <summary>The console's ticker: a save asked for every quarter of a second.</summary>
    private async Task TickFor(PaneHeightSaves saves, TimeSpan span)
    {
        for (TimeSpan passed = TimeSpan.Zero; passed < span; passed += Heartbeat.Interval)
        {
            clock.Advance(Heartbeat.Interval);
            await saves.SaveIfSettledAsync();
        }
    }
}