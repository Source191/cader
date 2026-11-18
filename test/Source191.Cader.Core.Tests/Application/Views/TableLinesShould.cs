using Source191.Cader.Application.Views;

namespace Source191.Cader.Core.Tests.Application.Views;

public sealed class TableLinesShould
{
    [Fact]
    public void PutAHeadingBeforeEachRunOfOneKey()
    {
        IReadOnlyList<TableLine<string>> lines = TableLines.Grouped(["apple", "avocado", "banana"], FirstLetter);

        lines.ShouldBe(
        [
            new TableLine<string>.Heading("apple"),
            new TableLine<string>.Row("apple"),
            new TableLine<string>.Row("avocado"),
            new TableLine<string>.Heading("banana"),
            new TableLine<string>.Row("banana"),
        ]);
    }

    private static string FirstLetter(string row) => row[..1];
}
