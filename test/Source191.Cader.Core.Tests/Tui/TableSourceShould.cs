using Source191.Cader.Application.Views;
using Source191.Cader.TestDoubles;
using Source191.Cader.Tui;

namespace Source191.Cader.Core.Tests.Tui;

public sealed class TableSourceShould
{
    private static readonly IReadOnlyList<TableLine<string>> Grouped =
    [
        new TableLine<string>.Heading("apple"),
        new TableLine<string>.Row("apple"),
        new TableLine<string>.Row("avocado"),
    ];

    [Fact]
    public void LeaveEveryCellOfAHeadingEmptySoItNeverWidensAColumn()
    {
        TableSource<string> source = new(Grouped, ["FRUIT", "LENGTH"], Cell);

        source.Rows.ShouldBe(3);
        source[0, 0].ShouldBe(string.Empty);
        source[0, 1].ShouldBe(string.Empty);
        source.LineAt(0).ShouldBe(new TableLine<string>.Heading("apple"));
    }

    [Fact]
    public void IndentTheFirstCellOfARowUnderAHeadingByTwo()
    {
        TableSource<string> source = new(Grouped, ["FRUIT", "LENGTH"], Cell);

        source[1, 0].ShouldBe("  apple");
        source[2, 0].ShouldBe("  avocado");
        source[2, 1].ShouldBe("7");
        TableSource<string>.IndentOf(Grouped).ShouldBe(2);
    }

    [Fact]
    public void IndentNothingWhereNoLineIsAHeading()
    {
        IReadOnlyList<TableLine<string>> ungrouped = [new TableLine<string>.Row("apple")];
        TableSource<string> source = new(ungrouped, ["FRUIT", "LENGTH"], Cell);

        source[0, 0].ShouldBe("apple");
        TableSource<string>.IndentOf(ungrouped).ShouldBe(0);
    }

    [Fact]
    public void WorkEachCellOutOnce()
    {
        int calls = 0;
        TableSource<string> source = new(Grouped, ["FRUIT", "LENGTH"], (row, column) =>
        {
            calls++;

            return Cell(row, column);
        });

        void ReadEveryCellThreeTimesAndEachWidest()
        {
            for (int round = 0; round < 3; round++)
            {
                for (int line = 0; line < source.Rows; line++)
                {
                    _ = source[line, 0];
                    _ = source[line, 1];
                }
            }

            source.Widest(0).ShouldBe("  avocado".Length);
            source.Widest(1).ShouldBe(1);
        }

        ReadEveryCellThreeTimesAndEachWidest();

        calls.ShouldBe(4);
        Allocations.Of(ReadEveryCellThreeTimesAndEachWidest).ShouldBe(0);
    }

    private static string Cell(string row, int column) => column == 0 ? row : row.Length.ToString(System.Globalization.CultureInfo.InvariantCulture);
}