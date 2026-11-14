using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Domain;

public sealed class TerminalProfileIdShould
{
    /// <summary>The ids Windows Terminal 1.25.2733.0 gave the fragment's profiles under the tool's earlier name, observed in issue #218 on 4 November 2026.</summary>
    [Theory]
    [InlineData("Vantage", "{58541d95-3e2c-5677-9092-a958bd152f67}")]
    [InlineData("Vantage session", "{786e9622-4c30-5cd8-aa6f-cdf433f291f6}")]
    public void DeriveTheIdWindowsTerminalGivesAFragmentProfile(string name, string observed)
    {
        TerminalProfileId.ForFragment("Vantage", name).Text.ShouldBe(observed);
    }

    [Fact]
    public void SpellItInLowerCaseInBraces()
    {
        TerminalProfileId id = new(Guid.Parse("58541D95-3E2C-5677-9092-A958BD152F67"));

        id.Text.ShouldBe("{58541d95-3e2c-5677-9092-a958bd152f67}");
    }
}