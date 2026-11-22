using Source191.Cader.Application.Views;

namespace Source191.Cader.Application;

/// <summary>The two rows of the key bar; <see cref="Width"/> is the cells the wider row reaches.</summary>
public sealed record KeyBar(IReadOnlyList<KeyBarItem> First, IReadOnlyList<KeyBarItem> Second)
{
    private static readonly Key[] Kept = [Key.Of('?'), Key.Of('q')];

    public int Width => First.Concat(Second).Select(item => item.End).DefaultIfEmpty(0).Max();

    /// <summary>The bar with each row that ends past <paramref name="width"/> cut to fit, or this bar when both rows fit.</summary>
    public KeyBar FittedTo(int width)
    {
        IReadOnlyList<KeyBarItem> first = Fitted(First, width);
        IReadOnlyList<KeyBarItem> second = Fitted(Second, width);

        return first == First && second == Second ? this : new KeyBar(first, second);
    }

    /// <summary>
    /// Drops the row's last item other than Help and Quit until it fits (the last item of all once only those are left),
    /// moving the items after a dropped one left by its width and spacing.
    /// </summary>
    private static IReadOnlyList<KeyBarItem> Fitted(IReadOnlyList<KeyBarItem> row, int width)
    {
        if (row.Count == 0 || row[^1].End <= width)
        {
            return row;
        }

        List<KeyBarItem> items = [.. row];
        while (items.Count > 0 && items[^1].End > width)
        {
            Drop(items, items.FindLastIndex(item => !Kept.Contains(item.Press)) is int index and >= 0 ? index : items.Count - 1);
        }

        return items;
    }

    /// <summary>Removes one item and moves the items after it left by its width and spacing.</summary>
    private static void Drop(List<KeyBarItem> items, int dropped)
    {
        int shift = dropped + 1 < items.Count ? items[dropped + 1].X - items[dropped].X : 0;
        items.RemoveAt(dropped);
        for (int moved = dropped; moved < items.Count; moved++)
        {
            items[moved] = items[moved] with { X = items[moved].X - shift };
        }
    }
}
