using Source191.Cader.Domain;

namespace Source191.Cader.Tui;

/// <summary>A group's heading as the table draws it across one line: its text, elided in the middle to its column's width, then a space and a rule to the end of the line.</summary>
internal sealed record HeadingLine(string Text, string Rule)
{
    private const char RuleCharacter = '─';

    public static HeadingLine For(string heading, int textWidth, int lineWidth)
    {
        string text = TextFit.ElideMiddle(heading, Math.Min(textWidth, lineWidth));
        int rest = lineWidth - text.Length;

        return new HeadingLine(text, rest > 0 ? " " + new string(RuleCharacter, rest - 1) : string.Empty);
    }
}