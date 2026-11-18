using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Domain;

public sealed class RowFilterShould
{
    [Fact]
    public void MatchWhenAnyFieldContainsTheTextIgnoringCase()
    {
        RowFilter filter = new("TEMPLATES");

        filter.Matches(["remove-sonar", "Northwind.Templates", "main"]).ShouldBeTrue();
    }

    [Fact]
    public void NotMatchWhenNoFieldContainsTheText()
    {
        RowFilter filter = new("catalogue");

        filter.Matches(["remove-sonar", "Northwind.Templates", "main"]).ShouldBeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void MatchEverythingWhenBlank(string text)
    {
        RowFilter filter = new(text);

        filter.Matches(["anything"]).ShouldBeTrue();
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("200B")]
    public void MatchEverythingWhenOnlyInvisibleCharacters(string text)
    {
        RowFilter filter = new(text);

        filter.Matches(["anything"]).ShouldBeTrue();
    }

    [Fact]
    public void IgnoreBlanksBeforeTheText()
    {
        RowFilter filter = new(" templates");

        filter.Matches(["Northwind.Templates"]).ShouldBeTrue();
        filter.Matches(["catalogue"]).ShouldBeFalse();
    }

    [Fact]
    public void KeepASpaceAfterTheFirstVisibleCharacter()
    {
        RowFilter filter = new("fix login");

        filter.Matches(["fix login"]).ShouldBeTrue();
        filter.Matches(["fix-login"]).ShouldBeFalse();
    }

    [Fact]
    public void MatchAmongTheFieldsARowHasAndNotWhenItHasNone()
    {
        RowFilter filter = new("main");

        filter.Matches(["remove-sonar", "main"]).ShouldBeTrue();
        filter.Matches([]).ShouldBeFalse();
    }
}