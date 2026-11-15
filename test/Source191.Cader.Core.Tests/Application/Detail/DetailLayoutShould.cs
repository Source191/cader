using Source191.Cader.Application;
using Source191.Cader.Application.Detail;

namespace Source191.Cader.Core.Tests.Application.Detail;

public sealed class DetailLayoutShould
{
    private static readonly IReadOnlyList<IReadOnlyList<DetailLine>> Facts =
    [
        [new("State", "needs input", TextTone.Amber, Span: 1), new("Started", "14:02", TextTone.Plain, Span: 1), new("Prompted", "14:03", TextTone.Plain, Span: 1)],
        [new("Folder", @"C:\f", TextTone.Plain, Span: 2), new("Session", "s1", TextTone.Plain, Span: 1)],
    ];

    [Fact]
    public void LineTheFactsUpInColumnsGivingTheLastTheSpareWidth()
    {
        DetailContent content = new("fix-123", Facts, Below: []);

        IReadOnlyList<IReadOnlyList<TextPart>> rows = DetailLayout.Rows(content, width: 60, height: 5);

        Text(rows).ShouldBe(
        [
            "State  needs input  Started 14:02  Prompted 14:03           ",
            @"Folder C:\f                        Session  s1              ",
        ]);
    }

    [Fact]
    public void LayOutNothingForTheEmptyContent()
    {
        DetailContent.Empty.Title.ShouldBeEmpty();
        DetailLayout.Rows(DetailContent.Empty, width: 60, height: 5).ShouldBeEmpty();
    }

    [Fact]
    public void WidenTheColumnsAWideFactSpansBeforeGivingTheLastTheRest()
    {
        string folder = new('x', 40);
        DetailContent content = new("fix-123", [Facts[0], [new("Folder", folder, TextTone.Plain, Span: 2), new("Session", "s1", TextTone.Plain, Span: 1)]], Below: []);

        string second = Text(DetailLayout.Rows(content, width: 80, height: 5))[1];

        second.TrimEnd().ShouldBe($"Folder {folder}  Session  s1");
    }

    [Fact]
    public void ColourLabelsOrangeAndTextInItsTone()
    {
        DetailContent content = new("fix-123", Facts, Below: []);

        IReadOnlyList<TextPart> first = DetailLayout.Rows(content, width: 60, height: 5)[0];

        first[0].ShouldBe(new TextPart("State  ", TextTone.Orange));
        first[1].ShouldBe(new TextPart("needs input", TextTone.Amber));
    }

    [Fact]
    public void NarrowTheWidestColumnFirstAndCutWhatNoLongerFits()
    {
        DetailContent content = new("fix-123", Facts, Below: []);

        IReadOnlyList<IReadOnlyList<TextPart>> rows = DetailLayout.Rows(content, width: 40, height: 5);

        Text(rows)[0].ShouldBe("State  need…  Started 14:…  Prompted 14…");
        Text(rows).ShouldAllBe(row => row.Length == 40);
    }

    [Fact]
    public void KeepTheStartOfAListingWithItsLabelsAligned()
    {
        DetailContent content = new("Northwind.Orders-fix-123", Facts, [new("Unpushed", "a1b2c31", TextTone.Plain, Span: 1), new(string.Empty, "a1b2c32", TextTone.Plain, Span: 1), new(string.Empty, "… 3 more", TextTone.Grey, Span: 1)]);

        Text(DetailLayout.Rows(content, width: 60, height: 4))[2..].Select(row => row.TrimEnd()).ShouldBe(["Unpushed a1b2c31", "         a1b2c32"]);
    }

    [Fact]
    public void ShowAsManyFactRowsAsThePaneHas()
    {
        DetailContent content = new("fix-123", Facts, [new(string.Empty, "one", TextTone.Plain, Span: 1)]);

        DetailLayout.Rows(content, width: 60, height: 1).Count.ShouldBe(1);
        DetailLayout.Rows(content, width: 60, height: 0).ShouldBeEmpty();
    }

    private static string[] Text(IReadOnlyList<IReadOnlyList<TextPart>> rows) => [.. rows.Select(row => string.Concat(row.Select(part => part.Text)))];

    [Fact]
    public void LeaveALineWithNoLabelAsOnePartWithNoLabelColumn()
    {
        DetailContent content = new("fix-123", [], [new(string.Empty, "a1b2c31", TextTone.Plain, Span: 1)]);

        IReadOnlyList<TextPart> row = DetailLayout.Rows(content, width: 10, height: 3).Single();

        row.ShouldBe([new TextPart("a1b2c31   ", TextTone.Plain)]);
    }

    [Fact]
    public void GiveAFactWithNoTextTheSpaceAfterItsLabel()
    {
        DetailContent content = new("fix-123", [[new("Note", string.Empty, TextTone.Plain, Span: 1), new("Other", "x", TextTone.Plain, Span: 1)]], Below: []);

        Text(DetailLayout.Rows(content, width: 60, height: 5)).Single().TrimEnd().ShouldBe("Note    Other x");
    }

    [Fact]
    public void WidenTheFirstColumnOfASpanOnlyByTheColumnsItSpans()
    {
        string folder = new('x', 40);
        DetailContent content = new("fix-123", [Facts[0], [new("Folder", folder, TextTone.Plain, Span: 2), new("Session", "s1", TextTone.Plain, Span: 1)]], Below: []);

        string first = Text(DetailLayout.Rows(content, width: 100, height: 5))[0];

        first.IndexOf("Prompted", StringComparison.Ordinal).ShouldBe(49, "the second column takes the extra width, not the first");
    }

    [Fact]
    public void CountTheGapsInsideASpanOfThreeColumnsWhenNarrowingThePane()
    {
        DetailContent content = new("fix-123", [[Fact("A", 3), Fact("B", 12), Fact("C", 3)], [Fact("W", 30, span: 3)]], Below: []);

        Text(DetailLayout.Rows(content, width: 28, height: 5)).ShouldBe(["A xxx  B xxxxxxx…  C xxx    ", $"W {new string('x', 25)}…"]);
    }

    [Fact]
    public void CountTheGapInsideASpanOfTheLastTwoColumnsWhenNarrowingThePane()
    {
        DetailContent content = new("fix-123", [[Fact("A", 3), Fact("B", 12), Fact("C", 3)], [Fact("F", 1), Fact("W", 20, span: 2)]], Below: []);

        Text(DetailLayout.Rows(content, width: 20, height: 5)).ShouldBe(["A xxx  B xx…  C xxx ", $"F x    W {new string('x', 10)}…"]);
    }

    [Fact]
    public void KeepOneCharacterAfterEveryLabelHoweverNarrowThePaneIs()
    {
        DetailContent content = new("fix-123", Facts, Below: []);

        string[] rows = Text(DetailLayout.Rows(content, width: 10, height: 5));

        rows.ShouldBe(["State  …  Started …  Prompted …", @"Folder C:\f          Session  …"]);
    }

    [Fact]
    public void SizeTheColumnsOnlyFromTheFactsThatFitInThreeColumns()
    {
        DetailContent content = new("fix-123", [Facts[0], [new("Folder", @"C:\f", TextTone.Plain, Span: 3), new("Extra", "beyond the columns", TextTone.Plain, Span: 1)]], Below: []);

        DetailLayout.Rows(content, width: 60, height: 1).Count.ShouldBe(1);
    }

    private static DetailLine Fact(string label, int length, int span = 1) => new(label, new string('x', length), TextTone.Plain, span);
}