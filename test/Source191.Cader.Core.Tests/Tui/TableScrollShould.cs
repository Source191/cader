using Source191.Cader.Domain;
using Source191.Cader.Tui;
using Terminal.Gui.Views;

namespace Source191.Cader.Core.Tests.Tui;

/// <summary>A real table, twelve rows high, whose headers scroll away, so twelve rows show once it is scrolled.</summary>
public sealed class TableScrollShould : IDisposable
{
    private readonly TableView table = new() { Width = 40, Height = 12, FullRowSelect = true, MultiSelect = false };
    private readonly TableScroll scroll;

    public TableScrollShould()
    {
        table.BeginInit();
        table.EndInit();
        table.Layout();
        scroll = new TableScroll(table);
    }

    [Fact]
    public void KeepTheRowsOnScreenWhenTheSameSelectionIsShownAgain()
    {
        StepDownTo(30);

        Show(100, 30);

        table.RowOffset.ShouldBe(19);
    }

    [Fact]
    public void StayWhereTheWheelLeftItWhenTheSameSelectionIsShownAgain()
    {
        StepDownTo(30);
        table.RowOffset = 50;

        Show(100, 30);

        table.RowOffset.ShouldBe(50);
    }

    [Fact]
    public void ScrollJustFarEnoughToKeepANewSelectionOnScreenGoingDown()
    {
        StepDownTo(30);

        Show(100, 31);

        table.RowOffset.ShouldBe(20);
    }

    [Fact]
    public void NotScrollForANewSelectionAlreadyOnScreen()
    {
        StepDownTo(30);

        Show(100, 25);

        table.RowOffset.ShouldBe(19);
    }

    [Fact]
    public void ScrollJustFarEnoughToKeepANewSelectionOnScreenGoingUp()
    {
        StepDownTo(30);

        Show(100, 18);

        table.RowOffset.ShouldBe(18);
    }

    [Fact]
    public void FollowTheSelectionWhenTheFilterChanges()
    {
        StepDownTo(30);
        table.RowOffset = 50;

        scroll.Show(Rows(100), 30, Key(30), new RowFilter("fix"));

        table.RowOffset.ShouldBe(30);
    }

    [Fact]
    public void ScrollBackOnlyAsFarAsTheLastRowNeedsWhenRowsGo()
    {
        StepDownTo(30);
        table.RowOffset = 80;

        Show(20, 15);

        table.RowOffset.ShouldBe(8);
    }

    [Fact]
    public void KeepTheNewLastRowOnScreenWhenTheSelectedLastRowGoes()
    {
        StepDownTo(99);

        // ListSelection.Load selects the row nearest the one that went: the new last row.
        Show(35, 34);

        table.RowOffset.ShouldBe(23);
    }

    [Fact]
    public void ShowAnEmptyTableFromItsTopWhenEveryRowGoes()
    {
        StepDownTo(30);

        scroll.Show(Rows(0), 0, null, RowFilter.None);

        table.RowOffset.ShouldBe(0);
    }

    public void Dispose()
    {
        table.Dispose();
    }

    private void StepDownTo(int row)
    {
        for (int each = 0; each <= row; each++)
        {
            Show(100, each);
        }
    }

    private void Show(int count, int selected)
    {
        scroll.Show(Rows(count), selected, Key(selected), RowFilter.None);
    }

    private static string Key(int row)
    {
        return $"row {row}";
    }

    private static EnumerableTableSource<int> Rows(int count)
    {
        return new(Enumerable.Range(0, count), new Dictionary<string, Func<int, object>> { ["NAME"] = row => Key(row) });
    }
}