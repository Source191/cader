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
    private const int Indent = 2;

    private readonly string[] names = [.. columnNames];

    /// <summary>Worked out once, as the table asks for every cell on every draw.</summary>
    private readonly string indent = new(' ', IndentOf(lines));

    /// <summary>Each cell once it is asked for, so the width pass and every draw of this source share one string.</summary>
    private readonly string?[] cells = new string?[lines.Count * columnNames.Count];

    public int Rows => lines.Count;

    public int Columns => names.Length;

    public string[] ColumnNames => names;

    public object this[int row, int col] => CellAt(row, col);

    public TableLine<TRow> LineAt(int index) => lines[index];

    /// <summary>The length of a column's longest cell over the lines, a heading's empty cells included.</summary>
    public int Widest(int column)
    {
        int widest = 0;
        for (int line = 0; line < lines.Count; line++)
        {
            widest = Math.Max(widest, CellAt(line, column).Length);
        }

        return widest;
    }

    private string CellAt(int row, int col) => cells[(row * names.Length) + col] ??= lines[row] switch
    {
        TableLine<TRow>.Row line when col == 0 => indent + cell(line.Item, col),
        TableLine<TRow>.Row line => cell(line.Item, col),
        _ => string.Empty,
    };

    /// <summary>The cells a row's first cell is indented by: none on a view with no headings.</summary>
    public static int IndentOf(IReadOnlyList<TableLine<TRow>> lines) => lines.Any(line => line is TableLine<TRow>.Heading) ? Indent : 0;
}