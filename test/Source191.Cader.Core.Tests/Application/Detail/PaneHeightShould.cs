using Source191.Cader.Application.Detail;

namespace Source191.Cader.Core.Tests.Application.Detail;

public sealed class PaneHeightShould
{
    [Fact]
    public void LeaveTheTableItsHeadingAndThreeRowsAndTheDivider()
    {
        PaneHeight.Room(tabRows: 30).ShouldBe(23);
    }

    [Theory]
    [InlineData(10, 200, 23, 10)]
    [InlineData(0, 200, 23, 1)]
    [InlineData(-3, 200, 23, 1)]
    [InlineData(9, 6, 23, 6)]
    [InlineData(20, 200, 12, 12)]
    [InlineData(20, 200, 0, 1)]
    public void FitTheWantedLinesWithinTheLimits(int wanted, int most, int room, int shown)
    {
        PaneHeight.Fit(wanted, most, room).ShouldBe(shown);
    }

    [Theory]
    [InlineData(10, 1, 11)]
    [InlineData(6, -1, 5)]
    [InlineData(1, -1, 1)]
    [InlineData(23, 1, 23)]
    public void StepFromTheShownLines(int shown, int step, int wanted)
    {
        PaneHeight.Step(shown, step, most: 200, room: 23).ShouldBe(wanted);
    }

    [Fact]
    public void KeepTheSessionsPaneToItsSixRowsOfFacts()
    {
        PaneHeight.Step(shown: 6, step: 1, most: 6, room: 23).ShouldBe(6);
    }
}