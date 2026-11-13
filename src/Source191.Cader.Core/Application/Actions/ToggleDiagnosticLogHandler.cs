using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;

namespace Source191.Cader.Application.Actions;

/// <summary>Turns the diagnostic log the other way and saves the settings; the log file's sink follows the saved setting from its next line.</summary>
public sealed class ToggleDiagnosticLogHandler(CurrentSettings settings, string logFile) : IEffectHandler<ToggleDiagnosticLog>
{
    public async Task<Outcome> HandleAsync(ToggleDiagnosticLog effect, CancellationToken cancellationToken)
    {
        Settings toggled = settings.Value.WithDiagnosticLogToggled();
        await settings.SaveAsync(toggled);

        return new ToggleDiagnosticLogOutcome(toggled.DiagnosticLog, logFile);
    }
}
