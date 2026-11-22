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

    private static readonly KeyBarItem Copy = new("c", "Copy path", X: 1, Tip: "Copy the folder.", Key.Of('c'));
    private static readonly KeyBarItem Filter = new("/", "Filter", X: 14, Tip: "Filter the rows.", Key.Slash);
    private static readonly KeyBarItem Help = new("?", "Help", X: 24, Tip: "Show every key.", Key.Of('?'));
    private static readonly KeyBarItem Report = new("!", "Report", X: 32, Tip: "Report a problem.", Key.Of('!'));
    private static readonly KeyBarItem Quit = new("q", "Quit", X: 42, Tip: "Quit.", Key.Of('q'));

    [Fact]
    public void LeaveARowThatFitsUnchanged()
    {
        KeyBar bar = new([Copy, Filter], [Copy, Filter, Help, Report, Quit]);

        bar.FittedTo(Quit.End).ShouldBe(bar);
        bar.FittedTo(Quit.End).Second.ShouldBe([Copy, Filter, Help, Report, Quit]);
    }

    [Fact]
    public void DropItemsFromTheEndUntilTheRowFits()
    {
        KeyBar bar = new([Copy, Filter, Report], []);

        bar.FittedTo(Report.End - 1).First.ShouldBe([Copy, Filter]);
        bar.FittedTo(Filter.End - 1).First.ShouldBe([Copy]);
    }

    [Fact]
    public void KeepHelpAndQuitAndMoveThemLeft()
    {
        KeyBar bar = new([], [Copy, Filter, Help, Report, Quit]);

        // Report is dropped first, and Quit takes its place; then Filter, and Help and Quit follow Copy.
        bar.FittedTo(Quit.End - 1).Second.ShouldBe([Copy, Filter, Help, Quit with { X = Report.X }]);
        bar.FittedTo(33).Second.ShouldBe([Copy, Help with { X = Filter.X }, Quit with { X = Filter.X + 8 }]);
    }

    [Fact]
    public void ShowWhatFitsWhenEvenHelpAndQuitDoNot()
    {
        KeyBar bar = new([], [Copy, Help, Quit]);

        // With Copy gone, Help and Quit move to cells 1 to 25: Quit is dropped at 24 cells, and Help (cells 1 to 7) at 6.
        bar.FittedTo(24).Second.ShouldBe([Help with { X = Copy.X }]);
        bar.FittedTo(6).Second.ShouldBe([]);
    }
}
