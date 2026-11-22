using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.Tui;
using Terminal.Gui.App;

namespace Source191.Cader.Core.Tests.Tui;

/// <summary>
/// Every effect marked <see cref="IDialogEffect"/> has a flow in <see cref="DialogEffects"/>; without one the user would
/// meet "No dialog is defined for..." at runtime. The same shape as the registration fact in <c>EffectDispatcherShould</c>.
/// </summary>
public sealed class DialogEffectsShould
{
    public static TheoryData<Type> DialogEffectTypes =>
        [.. typeof(ViewEffect).Assembly.GetTypes().Where(type => !type.IsAbstract && type.IsAssignableTo(typeof(IDialogEffect)))];

    [Theory]
    [MemberData(nameof(DialogEffectTypes))]
    public void HaveAFlowForEveryDialogEffect(Type type)
    {
        DialogEffects.Handles(type).ShouldBeTrue($"{type.Name} is an IDialogEffect with no flow in DialogEffects");
    }

    [Fact]
    public void AskTheBranchQuestionForABranchRow()
    {
        DialogEffects.Handles(typeof(ConfirmDeleteBranch)).ShouldBeTrue();
    }

    [Fact]
    public void NotClaimABackgroundEffect()
    {
        DialogEffects.Handles(typeof(StopSession)).ShouldBeFalse();
    }

    [Fact]
    public void RefuseTheWizardOnATooSmallScreen()
    {
        StatusContext context = new();

        WizardFit fit = DialogEffects.FitOrRefuse(context, columns: 70, rows: 24, new WizardOrigin.Anywhere());

        fit.ShouldBeOfType<WizardFit.TooSmall>();
        context.Shown.ShouldBe([new StatusMessage.WizardTooSmall(NeededColumns: 80, NeededRows: 30, Columns: 70, Rows: 24)]);
    }

    [Fact]
    public void RefuseTheFixedFolderWizardBySizeItNeeds()
    {
        StatusContext context = new();

        DialogEffects.FitOrRefuse(context, columns: 70, rows: 24, WizardOrigin.In(FolderPath.From(@"C:\Users\me\source\Northwind.Orders-fix-123")));

        context.Shown.ShouldBe([new StatusMessage.WizardTooSmall(NeededColumns: 84, NeededRows: 23, Columns: 70, Rows: 24)]);
    }

    [Fact]
    public void OpenTheWizardWithNothingSaidWhenItFits()
    {
        StatusContext context = new();

        WizardFit fit = DialogEffects.FitOrRefuse(context, columns: 120, rows: 30, new WizardOrigin.Anywhere());

        fit.ShouldBe(new WizardFit.Fits(Width: 80, Height: 30, ListRows: 1));
        context.Shown.ShouldBeEmpty();
    }

    /// <summary>Records the status messages a flow shows; a refusal touches nothing else.</summary>
    private sealed class StatusContext : IEffectContext
    {
        public List<StatusMessage> Shown { get; } = [];

        public IApplication App => throw new NotSupportedException();

        public DiagnosticLog Diagnostics => throw new NotSupportedException();

        public string HelpText => throw new NotSupportedException();

        public void ShowMessage(StatusMessage message) => Shown.Add(message);

        public void Reduce(ViewInput input) => throw new NotSupportedException();

        public void Dispatch(ViewEffect effect) => throw new NotSupportedException();

        public void Dispatch(ViewEffect effect, Func<Outcome, StatusMessage> onUiThread) => throw new NotSupportedException();

        public void RunInBackground<T>(StatusMessage working, Func<CancellationToken, Task<T>> action, Func<T, StatusMessage> onUiThread) => throw new NotSupportedException();
    }
}