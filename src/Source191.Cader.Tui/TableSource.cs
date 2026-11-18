using Source191.Cader.Application.Views;
using Terminal.Gui.Views;

namespace Source191.Cader.Tui;

/// <summary>
/// Presents a view's lines to Terminal.Gui's TableView through the definition's columns and cells: a heading's cells are empty,
/// so it never widens a column (<see cref="ColumnSizing{TRow}"/> draws it across the line), and under a heading a row's first cell
/// is indented.
/// </summary>
internal sealed class TableSource<TRow>(IReadOnlyList<TableLine<TRow>> lines, IReadOnlyList<string> columnNames, Func<TRow, int, string> cell) : ITableSource
{
    private readonly string[] names = [.. columnNames];

    public int Rows => lines.Count;

    public int Columns => names.Length;

    public string[] ColumnNames => names;

    public object this[int row, int col] => lines[row] switch
    {
        TableLine<TRow>.Row line => cell(line.Item, col),
        _ => string.Empty,
    };

    public TableLine<TRow> LineAt(int index) => lines[index];

    /// <summary>The cells a row's first cell is indented by: none on a view with no headings.</summary>
    public static int IndentOf(IReadOnlyList<TableLine<TRow>> lines) => 0;
}
