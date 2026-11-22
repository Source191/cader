namespace Source191.Cader.Application.Views;

/// <summary>
/// How the new-session wizard fits the terminal it opens in: <see cref="Fits"/> with the dialog's size and the rows its
/// repository list gets, or <see cref="TooSmall"/> with the size it needs, when it would not show every control.
/// </summary>
public abstract record WizardFit
{
    public static WizardFit For(int screenColumns, int screenRows, bool chooser) => new TooSmall(0, 0);

    public sealed record Fits(int Width, int Height, int ListRows) : WizardFit;

    public sealed record TooSmall(int NeededColumns, int NeededRows) : WizardFit;
}
