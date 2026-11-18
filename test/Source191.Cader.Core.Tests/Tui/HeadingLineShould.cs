using Source191.Cader.Tui;

namespace Source191.Cader.Core.Tests.Tui;

public sealed class HeadingLineShould
{
    private const string LongRepository = "Northwind.Integration.CosmosDb.ChangeFeedMonitoring";

    [Fact]
    public void ShowAHeadingWholeWhenItFitsThenARuleToTheEndOfTheLine()
    {
        HeadingLine line = HeadingLine.For("Northwind.Orders", textWidth: 60, lineWidth: 100);

        line.Text.ShouldBe("Northwind.Orders");
        line.Rule.ShouldBe(" " + new string('─', 83));
        (line.Text + line.Rule).Length.ShouldBe(100);
    }

    [Fact]
    public void ElideALongHeadingInItsMiddleToTheColumnsWidth()
    {
        HeadingLine line = HeadingLine.For(LongRepository, textWidth: 20, lineWidth: 40);

        line.Text.Length.ShouldBe(20);
        line.Text.ShouldStartWith("Northwind.");
        line.Text.ShouldEndWith("toring");
        line.Text.ShouldContain("…");
        (line.Text + line.Rule).Length.ShouldBe(40);
    }

    [Fact]
    public void LeaveNoRuleWhereTheLineHasNoRoomForOne()
    {
        HeadingLine.For("Northwind.Orders", textWidth: 60, lineWidth: 10).Rule.ShouldBeEmpty();
        HeadingLine.For("Northwind.Orders", textWidth: 60, lineWidth: 17).Rule.ShouldBe(" ");
    }
}