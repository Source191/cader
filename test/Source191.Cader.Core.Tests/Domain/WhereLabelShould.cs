using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Domain;

public sealed class WhereLabelShould
{
    [Fact]
    public void NameTheSlugOfAWorktreeMadeHere()
    {
        WhereLabel.PlaceOf("Northwind.Orders", "Northwind.Orders-fix-123").ShouldBe("fix-123");
    }

    [Fact]
    public void NameAFolderInsideTheRepository()
    {
        WhereLabel.PlaceOf("Northwind.Orders", @"Northwind.Orders\src\Api").ShouldBe(@"src\Api");
    }

    [Fact]
    public void IgnoreCaseInTheRepositorysName()
    {
        WhereLabel.PlaceOf("Northwind.Orders", "northwind.orders-fix-123").ShouldBe("fix-123");
    }

    [Theory]
    [InlineData("spike")]
    [InlineData(@"~\scratch\Northwind.Orders-old")]
    [InlineData("Northwind.Orders")]
    [InlineData("Northwind.OrdersArchive")]
    public void NameAnyOtherFolderByItsWholeLabel(string label)
    {
        WhereLabel.PlaceOf("Northwind.Orders", label).ShouldBe(label);
    }
}