using Source191.Cader.Application.Views;

namespace Source191.Cader.Core.Tests.Application.Views;

public sealed class WizardFitShould
{
    [Fact]
    public void FitTheOverviewWizardAt120By36WithFourListRows()
    {
        WizardFit fit = WizardFit.For(screenColumns: 120, screenRows: 36, chooser: true);

        fit.ShouldBe(new WizardFit.Fits(Width: 80, Height: 34, ListRows: 5));
    }

    [Fact]
    public void FitAt120By30WithOneListRow()
    {
        WizardFit fit = WizardFit.For(screenColumns: 120, screenRows: 30, chooser: true);

        fit.ShouldBe(new WizardFit.Fits(Width: 80, Height: 30, ListRows: 1));
    }

    [Fact]
    public void KeepAMarginOfUpToTwoRows()
    {
        Height(screenRows: 30).ShouldBe(30);
        Height(screenRows: 31).ShouldBe(30);
        Height(screenRows: 32).ShouldBe(30);
        Height(screenRows: 50).ShouldBe(48);
    }

    [Theory]
    [MemberData(nameof(ScreenRows))]
    public void NeverShrinkAsTheScreenGrows(int screenRows)
    {
        Height(screenRows + 1).ShouldBeGreaterThanOrEqualTo(Height(screenRows));
    }

    [Fact]
    public void RefuseBelowItsMinimum()
    {
        WizardFit.TooSmall needed = new(NeededColumns: 80, NeededRows: 30);

        WizardFit.For(screenColumns: 79, screenRows: 36, chooser: true).ShouldBe(needed);
        WizardFit.For(screenColumns: 120, screenRows: 29, chooser: true).ShouldBe(needed);
    }

    [Fact]
    public void FitTheFixedFolderWizardAt84By23()
    {
        WizardFit.Fits fixedSize = new(Width: 84, Height: 23, ListRows: 0);

        WizardFit.For(screenColumns: 84, screenRows: 23, chooser: false).ShouldBe(fixedSize);
        WizardFit.For(screenColumns: 120, screenRows: 36, chooser: false).ShouldBe(fixedSize);
    }

    [Fact]
    public void RefuseTheFixedFolderWizardBelow84By23()
    {
        WizardFit.TooSmall needed = new(NeededColumns: 84, NeededRows: 23);

        WizardFit.For(screenColumns: 83, screenRows: 36, chooser: false).ShouldBe(needed);
        WizardFit.For(screenColumns: 120, screenRows: 22, chooser: false).ShouldBe(needed);
    }

    public static TheoryData<int> ScreenRows() => [.. Enumerable.Range(30, 21)];

    private static int Height(int screenRows)
    {
        WizardFit fit = WizardFit.For(screenColumns: 120, screenRows, chooser: true);

        return fit.ShouldBeOfType<WizardFit.Fits>().Height;
    }
}