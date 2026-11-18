namespace Source191.Cader.Application.Views;

/// <summary>A table's lines from its visible rows, already in table order.</summary>
public static class TableLines
{
    /// <summary>A heading before each run of rows that share a group key, then the rows.</summary>
    public static IReadOnlyList<TableLine<TRow>> Grouped<TRow>(IReadOnlyList<TRow> visible, Func<TRow, string> groupKey) => [];
}
