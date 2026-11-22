namespace Source191.Cader.Application.Views;

/// <summary>
/// How the new-session wizard fits the terminal it opens in: <see cref="Fits"/> with the dialog's size and the rows its
/// repository list gets, or <see cref="TooSmall"/> with the size it needs, when it would not show every control.
/// </summary>
public abstract record WizardFit
{
    private const int OverviewWidth = 80;

    /// <summary>The form's rows, the caption and the filter, and a dialog's border, button area and shadow row (measured, terminal-gui.md).</summary>
    private const int RowsBesideTheList = 21 + 2 + 6;

    private const int OverviewMinimumRows = RowsBesideTheList + 1;

    private const int Margin = 2;

    private const int FixedFolderWidth = 84;

    private const int FixedFolderHeight = 23;

    /// <summary>
    /// The overview's wizard is as tall as the screen allows less a margin of up to two rows, the list taking what the rest
    /// leaves; a fixed folder's wizard has no list and keeps its size.
    /// </summary>
    public static WizardFit For(int screenColumns, int screenRows, bool chooser) => chooser
        ? Overview(screenColumns, screenRows)
        : FixedFolder(screenColumns, screenRows);

    private static WizardFit Overview(int screenColumns, int screenRows)
    {
        if (screenColumns < OverviewWidth || screenRows < OverviewMinimumRows)
        {
            return new TooSmall(OverviewWidth, OverviewMinimumRows);
        }

        int height = screenRows - Math.Min(Margin, screenRows - OverviewMinimumRows);

        return new Fits(OverviewWidth, height, height - RowsBesideTheList);
    }

    private static WizardFit FixedFolder(int screenColumns, int screenRows) => screenColumns < FixedFolderWidth || screenRows < FixedFolderHeight
        ? new TooSmall(FixedFolderWidth, FixedFolderHeight)
        : new Fits(FixedFolderWidth, FixedFolderHeight, ListRows: 0);

    public sealed record Fits(int Width, int Height, int ListRows) : WizardFit;

    public sealed record TooSmall(int NeededColumns, int NeededRows) : WizardFit;
}
