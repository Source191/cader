using System.Globalization;
using System.Text;

namespace Source191.Cader.Domain;

/// <summary>
/// Which characters a person can see. Read by rune, so a character outside the basic plane counts once; the Unicode
/// categories are CoreLib's own tables, so the rule holds in invariant globalisation mode.
/// </summary>
public static class CharacterKinds
{
    public static CharacterKind Of(Rune rune) => rune switch
    {
        _ when Rune.IsWhiteSpace(rune) => CharacterKind.Whitespace,
        _ when Rune.GetUnicodeCategory(rune) is UnicodeCategory.Control => CharacterKind.Control,
        _ when Rune.GetUnicodeCategory(rune) is UnicodeCategory.Format => CharacterKind.Format,
        _ when Rune.IsLetterOrDigit(rune) => CharacterKind.LetterOrDigit,
        _ => CharacterKind.Other,
    };

    /// <summary>The kind of the text's first character; an invalid one, such as a lone surrogate, reads as U+FFFD.</summary>
    public static CharacterKind First(string text)
    {
        Rune.DecodeFromUtf16(text, out Rune first, out _);

        return Of(first);
    }

    /// <summary>The text from its first visible character; empty when it has none. Characters after that one are kept, so a space between words still counts.</summary>
    public static string WithoutInvisibleStart(string text)
    {
        int invisible = text.EnumerateRunes().TakeWhile(IsInvisible).Sum(rune => rune.Utf16SequenceLength);

        return text[invisible..];
    }

    private static bool IsInvisible(Rune rune) => Of(rune) is CharacterKind.Whitespace or CharacterKind.Control or CharacterKind.Format;
}