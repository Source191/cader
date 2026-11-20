namespace Source191.Cader.Application.Detail;

/// <summary>
/// The one rule for a detail pane's height, whether a key or a drag changed it: at least one line under the divider, no more
/// than the tab's detail can fill, and no more than the tab leaves once its table keeps its heading and three rows.
/// </summary>
public static class PaneHeight
{
    /// <summary>The fewest lines a pane keeps under its divider.</summary>
    public const int Least = 1;

    /// <summary>The table's heading: the line over the names, the names and the line under them.</summary>
    public const int TableHeadingRows = 3;

    /// <summary>The fewest rows the table keeps under its heading.</summary>
    public const int TableLeastRows = 3;

    /// <summary>The pane's divider, its top border.</summary>
    public const int DividerRows = 1;

    /// <summary>The most lines a tab of <paramref name="tabRows"/> leaves under the pane's divider.</summary>
    public static int Room(int tabRows) => tabRows - TableHeadingRows - TableLeastRows - DividerRows;

    /// <summary>The lines shown for the lines wanted: the wanted lines within the pane's limits.</summary>
    public static int Fit(int wanted, int most, int room) => Math.Max(Least, Math.Min(wanted, Math.Min(most, room)));

    /// <summary>The lines a key's step asks for, from the lines shown, so a press on a short terminal grows no reserve it cannot show.</summary>
    public static int Step(int shown, int step, int most, int room) => Fit(shown + step, most, room);
}