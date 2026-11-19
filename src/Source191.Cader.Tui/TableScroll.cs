using Source191.Cader.Domain;
using Terminal.Gui.Views;

namespace Source191.Cader.Tui;

internal sealed class TableScroll(TableView table)
{
    public void Show(ITableSource source, int cursorRow, string? selectedKey, RowFilter filter)
    {
        table.Table = source;
        table.SetSelection(0, cursorRow, false, null);
    }
}
