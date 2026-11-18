namespace Source191.Cader.Application.Views;

/// <summary>
/// One line of a table as the screen draws it: a group's heading, which is never a row and carries its group's first row
/// so the screen can name it, or a row.
/// </summary>
public abstract record TableLine<TRow>
{
    public sealed record Heading(TRow First) : TableLine<TRow>;

    public sealed record Row(TRow Item) : TableLine<TRow>;
}
