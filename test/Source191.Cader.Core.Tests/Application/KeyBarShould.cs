using Source191.Cader.Application;
using Source191.Cader.Application.Views;

namespace Source191.Cader.Core.Tests.Application;

public sealed class KeyBarShould
{
    [Fact]
    public void ReachAsFarAsTheWiderRow()
    {
        KeyBarItem narrow = new("n", "New", X: 0, Tip: "Start a session.", Key.Of('n'));
        KeyBarItem wide = new("d", "Delete", X: 20, Tip: "Delete the session.", Key.Of('d'));

        new KeyBar([narrow], [wide]).Width.ShouldBe(wide.End);
        new KeyBar([wide], [narrow]).Width.ShouldBe(wide.End);
    }

    [Fact]
    public void ReachAsFarAsTheEndOfTheLastItemOfARow()
    {
        KeyBarItem first = new("n", "New", X: 0, Tip: "Start a session.", Key.Of('n'));
        KeyBarItem second = new("d", "Delete", X: 8, Tip: "Delete the session.", Key.Of('d'));

        new KeyBar([first, second], []).Width.ShouldBe(second.End);
    }

    [Fact]
    public void BeZeroCellsWideWithNoItems()
    {
        new KeyBar([], []).Width.ShouldBe(0);
    }
}