using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Domain;

public sealed class UpdateCheckOptOutShould
{
    [Theory]
    [InlineData("1")]
    [InlineData("true")]
    [InlineData(" yes ")]
    public void ApplyToAnyNonBlankValue(string value)
    {
        UpdateCheckOptOut.Applies(value).ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void NotApplyWhenEmptyOrBlank(string value)
    {
        UpdateCheckOptOut.Applies(value).ShouldBeFalse();
    }

    [Fact]
    public void NameTheVariable()
    {
        UpdateCheckOptOut.VariableName.ShouldBe("CADER_NO_UPDATE_CHECK");
    }
}