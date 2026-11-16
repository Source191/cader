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

    /// <summary>Only the name alone, or after exactly one glyph and one space, is the session's tab; anything looser could take another tab for it.</summary>
    [Theory]
    [InlineData("fix-124")]
    [InlineData("fix-1234")]
    [InlineData("notes on fix-123")]
    [InlineData("◐◑ fix-123")]
    [InlineData("a fix-123")]
    [InlineData("7 fix-123")]
    [InlineData("◐fix-123")]
    [InlineData("◐  fix-123")]
    [InlineData(" fix-123")]
    [InlineData("\t fix-123")]
    public void NotShowASessionInAnyOtherTitle(string tabName)
    {
        TabTitle.Shows(tabName, "fix-123").ShouldBeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("◐ ")]
    public void NotShowASessionWhoseTitleIsBlank(string tabName)
    {
        TabTitle.Shows(tabName, string.Empty).ShouldBeFalse();
    }
}
