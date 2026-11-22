using Source191.Cader.Application;
using Source191.Cader.Application.Actions;
using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application.Actions;

public sealed class ToggleUpdateCheckHandlerShould
{
    [Fact]
    public async Task TurnTheCheckOffAndSaveIt()
    {
        InMemoryState state = new();
        CurrentSettings current = new(TestSettings.For(@"C:\Users\me\source\repos"), state);

        object outcome = await new ToggleUpdateCheckHandler(current).HandleAsync(new ToggleUpdateCheck(), CancellationToken.None);

        outcome.ShouldBe(new ToggleUpdateCheckOutcome(On: false));
        current.Value.CheckForUpdates.ShouldBeFalse();
        SettingsFile.From(await state.ReadConfigAsync(), TestSettings.Home).CheckForUpdates.ShouldBeFalse();
    }

    [Fact]
    public async Task TurnTheCheckBackOn()
    {
        CurrentSettings current = new(TestSettings.For(@"C:\Users\me\source\repos").WithUpdateCheckToggled(), new InMemoryState());

        object outcome = await new ToggleUpdateCheckHandler(current).HandleAsync(new ToggleUpdateCheck(), CancellationToken.None);

        outcome.ShouldBe(new ToggleUpdateCheckOutcome(On: true));
        current.Value.CheckForUpdates.ShouldBeTrue();
    }

    [Fact]
    public async Task KeepADiagnosticLogToggleMadeMeanwhile()
    {
        InMemoryState state = new();
        CurrentSettings current = new(TestSettings.For(@"C:\Users\me\source\repos"), state);
        TaskCompletionSource written = new(TaskCreationOptions.RunContinuationsAsynchronously);
        state.HoldNextConfigWrite(written.Task);

        Task<Outcome> logToggled = new ToggleDiagnosticLogHandler(current, @"C:\Users\me\AppData\Local\Cader\cader.log").HandleAsync(new ToggleDiagnosticLog(), CancellationToken.None);
        Task<Outcome> checkToggled = new ToggleUpdateCheckHandler(current).HandleAsync(new ToggleUpdateCheck(), CancellationToken.None);
        written.SetResult();
        await Task.WhenAll(logToggled, checkToggled);

        Settings saved = SettingsFile.From(await state.ReadConfigAsync(), TestSettings.Home);
        saved.DiagnosticLog.ShouldBeTrue();
        saved.CheckForUpdates.ShouldBeFalse();
        current.Value.ShouldBe(saved);
    }
}
