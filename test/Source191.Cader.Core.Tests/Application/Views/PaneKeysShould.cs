using Source191.Cader.Application.Views;

namespace Source191.Cader.Core.Tests.Application.Views;

public sealed class PaneKeysShould
{
    [Theory]
    [InlineData('+', 1)]
    [InlineData('-', -1)]
    public void GrowOrShrinkByALine(char character, int expected)
    {
        PaneKeys.TryFor(Key.Of(character), out int step).ShouldBeTrue();
        step.ShouldBe(expected);
    }

    [Theory]
    [InlineData('=')]
    [InlineData('<')]
    [InlineData('a')]
    public void LeaveEveryOtherCharacterAlone(char character)
    {
        PaneKeys.TryFor(Key.Of(character), out _).ShouldBeFalse();
    }

    [Fact]
    public void LeaveTheNamedKeysAlone()
    {
        PaneKeys.TryFor(Key.Up, out _).ShouldBeFalse();
    }
}