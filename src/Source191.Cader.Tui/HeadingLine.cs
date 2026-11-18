namespace Source191.Cader.Tui;

/// <summary>A group's heading as the table draws it across one line: its text, then a rule to the end of the line.</summary>
internal sealed record HeadingLine(string Text, string Rule)
{
    public static HeadingLine For(string heading, int textWidth, int lineWidth) => new(heading, string.Empty);
}
