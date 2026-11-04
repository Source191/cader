using Source191.Cader.Adapters.Terminal;
using Source191.Cader.Domain;

namespace Source191.Cader.Infrastructure.Tests;

public sealed class TabEscapesShould
{
    [Fact]
    public void SetTheTabTitleWithOsc2()
    {
        TabEscapes.Title("Cader").ShouldBe("\u001b]2;Cader\u0007");
        TabEscapes.ClearTitle.ShouldBe("\u001b]2;\u0007");
    }

    [Fact]
    public void SetTheTabBackgroundThroughPaletteEntry264()
    {
        TabEscapes.Background(new TabColour("Green", "#16C60C")).ShouldBe("\u001b]4;264;rgb:16/c6/0c\u001b\\");
    }

    [Fact]
    public void ResetPaletteEntry264ThenTheTabColourIndices()
    {
        TabEscapes.ResetBackground.ShouldBe("\u001b]104;264\u001b\\" + "\u001b[2;263;264,|");
    }
}