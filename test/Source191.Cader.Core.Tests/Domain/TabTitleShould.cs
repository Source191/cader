using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Domain;

public sealed class TabTitleShould
{
    [Fact]
    public void ShowASessionWhoseNameIsTheWholeTitle()
    {
        TabTitle.Shows("fix-123", "fix-123").ShouldBeTrue();
    }

    /// <summary>Claude Code's spinner and idle glyphs as seen on 16 November 2026 (CLI 2.1.325), and one outside the Basic Multilingual Plane, which is two UTF-16 units.</summary>
    [Theory]
    [InlineData("◐ fix-123")]
    [InlineData("◓ fix-123")]
    [InlineData("◑ fix-123")]
    [InlineData("◒ fix-123")]
    [InlineData("✳ fix-123")]
    [InlineData("\U0001F514 fix-123")]
    public void ShowASessionWhoseNameFollowsOneGlyphAndASpace(string tabName)
    {
        TabTitle.Shows(tabName, "fix-123").ShouldBeTrue();
    }
}
