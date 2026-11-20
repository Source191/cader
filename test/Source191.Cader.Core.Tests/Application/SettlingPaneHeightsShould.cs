using Microsoft.Extensions.Time.Testing;
using Source191.Cader.Application;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Application;

public sealed class SettlingPaneHeightsShould
{
    private readonly FakeTimeProvider clock = new(Fixtures.Noon);

    [Fact]
    public void StartFromTheSettings()
    {
        SettlingPaneHeights heights = new(new PaneHeights(Sessions: 4, Worktrees: 20), clock);

        heights.Of(ConsoleTab.Sessions).ShouldBe(4);
        heights.Of(ConsoleTab.Worktrees).ShouldBe(20);
        heights.TryTakeDue(out _).ShouldBeFalse();
    }

    [Fact]
    public void AnswerTheHeightsOnceASecondAfterTheLastChange()
    {
        SettlingPaneHeights heights = new(PaneHeights.WhenUnset, clock);

        for (int press = 11; press <= 15; press++)
        {
            heights.Changed(ConsoleTab.Worktrees, press);
            clock.Advance(TimeSpan.FromMilliseconds(200));
            heights.TryTakeDue(out _).ShouldBeFalse();
        }

        clock.Advance(TimeSpan.FromMilliseconds(799));
        heights.TryTakeDue(out _).ShouldBeFalse();
        clock.Advance(TimeSpan.FromMilliseconds(1));
        heights.TryTakeDue(out PaneHeights? due).ShouldBeTrue();
        due.ShouldBe(new PaneHeights(Sessions: 6, Worktrees: 15));
        clock.Advance(TimeSpan.FromSeconds(5));
        heights.TryTakeDue(out _).ShouldBeFalse();
    }

    [Fact]
    public void KeepTheOtherTabsHeight()
    {
        SettlingPaneHeights heights = new(new PaneHeights(Sessions: 4, Worktrees: 20), clock);

        heights.Changed(ConsoleTab.Worktrees, 15);

        heights.Of(ConsoleTab.Sessions).ShouldBe(4);
        heights.Of(ConsoleTab.Worktrees).ShouldBe(15);
    }

    [Fact]
    public void SaveNothingForTheSameHeight()
    {
        SettlingPaneHeights heights = new(PaneHeights.WhenUnset, clock);

        heights.Changed(ConsoleTab.Sessions, 6);
        heights.Changed(ConsoleTab.Config, 3);
        clock.Advance(TimeSpan.FromSeconds(2));

        heights.TryTakeDue(out _).ShouldBeFalse();
        heights.Wanted.ShouldBe(PaneHeights.WhenUnset);
    }

    [Fact]
    public void AnswerAChangeAfterASave()
    {
        SettlingPaneHeights heights = new(PaneHeights.WhenUnset, clock);
        heights.Changed(ConsoleTab.Sessions, 4);
        clock.Advance(TimeSpan.FromSeconds(1));
        heights.TryTakeDue(out _).ShouldBeTrue();

        heights.Changed(ConsoleTab.Sessions, 3);
        clock.Advance(TimeSpan.FromSeconds(1));

        heights.TryTakeDue(out PaneHeights? due).ShouldBeTrue();
        due.ShouldBe(new PaneHeights(Sessions: 3, Worktrees: 10));
    }

    [Fact]
    public void AnswerTheLeastForConfigWhichHasNoPane()
    {
        new SettlingPaneHeights(PaneHeights.WhenUnset, clock).Of(ConsoleTab.Config).ShouldBe(PaneHeights.Least);
    }
}