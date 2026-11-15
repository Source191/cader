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

    /// <summary>The kind of the text's first character.</summary>
    public static CharacterKind First(string text) => Of(Rune.GetRuneAt(text, 0));

    /// <summary>Whether the text holds characters and none of them can be seen: each is whitespace, a control or a format character.</summary>
    public static bool AllInvisible(string text) => text.Length > 0 && text.EnumerateRunes().All(rune => Of(rune) is CharacterKind.Whitespace or CharacterKind.Control or CharacterKind.Format);
}