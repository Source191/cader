using Source191.Cader.Domain;
using Terminal.Gui.Views;

namespace Source191.Cader.Tui;

/// <summary>
/// Swaps a table's rows without losing its place. Setting <see cref="TableView.Table"/> selects the first cell and scrolls
/// to it, and <see cref="TableView.SetSelection"/> never scrolls, so the row offset is put back after the swap; the table
/// scrolls to the cursor only when the selection or the filter changed since the last swap, so a refresh, or a view
/// scrolled with the mouse wheel, stays where it was.
/// </summary>
internal sealed class TableScroll(TableView table)
{
    private (string? Key, RowFilter Filter) followed = (null, RowFilter.None);

    public void Show(ITableSource source, int cursorRow, string? selectedKey, RowFilter filter)
    {
        int rowOffset = table.RowOffset;
        table.Table = source;
        table.RowOffset = rowOffset;
        if (source.Rows > 0)
        {
            table.SetSelection(0, cursorRow, false, null);
        }

        table.EnsureValidScrollOffsets();
        (string? Key, RowFilter Filter) shown = (selectedKey, filter);
        if (shown != followed)
        {
            table.EnsureCursorIsVisible();
        }

        followed = shown;
    }
}