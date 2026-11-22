namespace Source191.Cader.Application.Views;

/// <summary>
/// How the new-session wizard fits the terminal it opens in: <see cref="Fits"/> with the dialog's size and the rows its
/// repository list gets, or <see cref="TooSmall"/> with the size it needs, when it would not show every control.
/// </summary>
public abstract record WizardFit
{
    private const int OverviewWidth = 80;

    /// <summary>The form's rows, the caption and the filter, and a dialog's border and button area (measured, terminal-gui.md).</summary>
    private const int RowsBesideTheList = 21 + 2 + 5;

    private const int Margin = 2;

    /// <summary>As tall as the screen allows less a margin of up to two rows, the list taking what the rest leaves.</summary>
    public static WizardFit For(int screenColumns, int screenRows, bool chooser)
    {
        int minimum = RowsBesideTheList + 1;
        if (screenColumns < OverviewWidth || screenRows < minimum)
        {
            return new TooSmall(OverviewWidth, minimum);
        }

        int height = screenRows - Math.Min(Margin, screenRows - minimum);

        return new Fits(OverviewWidth, height, height - RowsBesideTheList);
    }

    public sealed record Fits(int Width, int Height, int ListRows) : WizardFit;

    public sealed record TooSmall(int NeededColumns, int NeededRows) : WizardFit;
}
