using Source191.Cader.Domain;
using System.Text;

namespace Source191.Cader.Core.Tests.Domain;

/// <summary>The one rule for which characters a person can see, shared by the wizard's problem line and its diagnostic events.</summary>
public sealed class CharacterKindsShould
{
    [Theory]
    [InlineData("o", CharacterKind.LetterOrDigit)]
    [InlineData("7", CharacterKind.LetterOrDigit)]
    [InlineData(" ", CharacterKind.Whitespace)]
    [InlineData("\t", CharacterKind.Whitespace)]
    [InlineData("\u00A0", CharacterKind.Whitespace)]
    [InlineData("\u001B", CharacterKind.Control)]
    [InlineData("\u200B", CharacterKind.Format)]
    [InlineData("-", CharacterKind.Other)]
    [InlineData("\U0001F600", CharacterKind.Other)]
    public void ClassifyEachKind(string text, CharacterKind kind)
    {
        CharacterKinds.Of(text.EnumerateRunes().First()).ShouldBe(kind);
        CharacterKinds.First(text).ShouldBe(kind);
    }

    [Fact]
    public void ClassifyTheFirstCharacterOfALongerText()
    {
        CharacterKinds.First(" orders").ShouldBe(CharacterKind.Whitespace);
        CharacterKinds.First("orders ").ShouldBe(CharacterKind.LetterOrDigit);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData(" ", true)]
    [InlineData("\u200B", true)]
    [InlineData(" \t\u001B", true)]
    [InlineData(" o", false)]
    [InlineData("-", false)]
    public void CallATextInvisibleOnlyWhenEveryCharacterIs(string text, bool invisible)
    {
        CharacterKinds.AllInvisible(text).ShouldBe(invisible);
    }

    [Fact]
    public void ReadACharacterOutsideTheBasicPlaneAsOne()
    {
        Rune emoji = "\U0001F600".EnumerateRunes().Single();

        CharacterKinds.Of(emoji).ShouldBe(CharacterKind.Other);
    }
}