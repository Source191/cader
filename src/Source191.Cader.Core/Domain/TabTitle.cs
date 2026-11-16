using System.Buffers;
using System.Text;

namespace Source191.Cader.Domain;

/// <summary>
/// Whether a Windows Terminal tab's title shows a session's name: the name alone, as Cader titles the tabs it opens, or
/// after one glyph and a space, as Claude Code titles a session's tab while it works (<c>◐ fix-123</c>, seen with CLI
/// 2.1.325 on 16 November 2026). Anything looser could take another tab for the session's.
/// </summary>
public static class TabTitle
{
    public static bool Shows(string tabName, string sessionTitle)
    {
        if (string.IsNullOrWhiteSpace(sessionTitle))
        {
            return false;
        }

        return string.Equals(tabName, sessionTitle, StringComparison.Ordinal) || FollowsOneGlyph(tabName, sessionTitle);
    }

    /// <summary>The title is one Unicode scalar value that is neither a letter, a digit nor white space, a space, then the name.</summary>
    private static bool FollowsOneGlyph(string tabName, string sessionTitle)
    {
        int glyphLength = tabName.Length - sessionTitle.Length - 1;

        return glyphLength > 0
            && tabName.EndsWith(" " + sessionTitle, StringComparison.Ordinal)
            && Rune.DecodeFromUtf16(tabName.AsSpan(0, glyphLength), out Rune glyph, out int read) is OperationStatus.Done
            && read == glyphLength
            && !Rune.IsLetterOrDigit(glyph)
            && !Rune.IsWhiteSpace(glyph);
    }
}