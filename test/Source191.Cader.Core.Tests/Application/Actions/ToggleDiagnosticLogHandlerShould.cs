using Source191.Cader.Application;
using Source191.Cader.Application.Actions;
using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application.Actions;

public sealed class ToggleDiagnosticLogHandlerShould
{
    private const string LogFile = @"C:\Users\me\AppData\Local\Cader\cader.log";

    [Fact]
    public async Task TurnTheLogOnAndSaveIt()
    {
        InMemoryState state = new();
        CurrentSettings current = new(TestSettings.For(@"C:\Users\me\source\repos"), state);

        object outcome = await new ToggleDiagnosticLogHandler(current, LogFile).HandleAsync(new ToggleDiagnosticLog(), CancellationToken.None);

        outcome.ShouldBe(new ToggleDiagnosticLogOutcome(On: true, File: LogFile));
        current.Value.DiagnosticLog.ShouldBeTrue();
        SettingsFile.From(await state.ReadConfigAsync(), TestSettings.Home).DiagnosticLog.ShouldBeTrue();
    }

    [Fact]
    public async Task TurnTheLogBackOff()
    {
        CurrentSettings current = new(TestSettings.For(@"C:\Users\me\source\repos").WithDiagnosticLogToggled(), new InMemoryState());

        object outcome = await new ToggleDiagnosticLogHandler(current, LogFile).HandleAsync(new ToggleDiagnosticLog(), CancellationToken.None);

        outcome.ShouldBe(new ToggleDiagnosticLogOutcome(On: false, File: LogFile));
        current.Value.DiagnosticLog.ShouldBeFalse();
    }

    [Fact]
    public async Task ApplyToTheLogFileFromTheNextLine()
    {
        CurrentSettings current = TestSettings.Current(@"C:\Users\me\source\repos");
        DiagnosticLogSwitch logSwitch = new();
        logSwitch.PointAt(() => current.Value.DiagnosticLog);

        await new ToggleDiagnosticLogHandler(current, LogFile).HandleAsync(new ToggleDiagnosticLog(), CancellationToken.None);

        logSwitch.IsOn().ShouldBeTrue();
    }

    [Fact]
    public async Task KeepAnUpdateCheckToggleMadeMeanwhile()
    {
        InMemoryState state = new();
        CurrentSettings current = new(TestSettings.For(@"C:\Users\me\source\repos"), state);
        TaskCompletionSource written = new(TaskCreationOptions.RunContinuationsAsynchronously);
        state.HoldNextConfigWrite(written.Task);

        Task<Outcome> checkToggled = new ToggleUpdateCheckHandler(current).HandleAsync(new ToggleUpdateCheck(), CancellationToken.None);
        Task<Outcome> logToggled = new ToggleDiagnosticLogHandler(current, LogFile).HandleAsync(new ToggleDiagnosticLog(), CancellationToken.None);
        written.SetResult();
        await Task.WhenAll(checkToggled, logToggled);

        Settings saved = SettingsFile.From(await state.ReadConfigAsync(), TestSettings.Home);
        saved.CheckForUpdates.ShouldBeFalse();
        saved.DiagnosticLog.ShouldBeTrue();
        current.Value.ShouldBe(saved);
    }
}
