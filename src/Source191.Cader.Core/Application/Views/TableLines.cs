namespace Source191.Cader.Application.Views;

/// <summary>A table's lines from its visible rows, already in table order.</summary>
public static class TableLines
{
    /// <summary>A heading before each run of rows that share a group key, then the rows, so a group with no visible row has no heading.</summary>
    public static IReadOnlyList<TableLine<TRow>> Grouped<TRow>(IReadOnlyList<TRow> visible, Func<TRow, string> groupKey) => [.. visible.SelectMany((row, index) => LinesOf(visible, groupKey, index))];

    /// <summary>One line per row, for a view whose rows are not grouped.</summary>
    public static IReadOnlyList<TableLine<TRow>> Ungrouped<TRow>(IReadOnlyList<TRow> visible) => [.. visible.Select(row => new TableLine<TRow>.Row(row))];

    /// <summary>The row's line, after its group's heading when it is the first of its run.</summary>
    private static IReadOnlyList<TableLine<TRow>> LinesOf<TRow>(IReadOnlyList<TRow> visible, Func<TRow, string> groupKey, int index)
    {
        TRow row = visible[index];
        bool startsGroup = index == 0 || !string.Equals(groupKey(visible[index - 1]), groupKey(row), StringComparison.Ordinal);

        return startsGroup ? [new TableLine<TRow>.Heading(row), new TableLine<TRow>.Row(row)] : [new TableLine<TRow>.Row(row)];
    }
}
