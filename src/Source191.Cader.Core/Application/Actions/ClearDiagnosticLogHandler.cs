using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Views;
using Source191.Cader.Ports;

namespace Source191.Cader.Application.Actions;

/// <summary>
/// Deletes the diagnostic log's files and records the clear, numbers only, before it returns, so with the log on the new
/// <c>cader.log</c> starts with why it starts mid-run. The events in memory, which a problem report carries, are kept.
/// </summary>
public sealed class ClearDiagnosticLogHandler(IDiagnosticLogFiles files, DiagnosticLog log) : IEffectHandler<ClearDiagnosticLog>
{
    public Task<Outcome> HandleAsync(ClearDiagnosticLog effect, CancellationToken cancellationToken)
    {
        LogFilesCleared cleared = files.Clear();
        log.Record(new DiagnosticLogCleared(cleared.Removed, cleared.Bytes, cleared.Kept));

        return Task.FromResult(OutcomeOf(cleared));
    }

    private static Outcome OutcomeOf(LogFilesCleared cleared)
    {
        return cleared switch
        {
            { Removed: 0, Kept: 0 } => new ClearDiagnosticLogOutcome.NothingToClear(),
            { Removed: 0 } => new ClearDiagnosticLogOutcome.NoneCleared(cleared.Kept),
            { Kept: 0 } => new ClearDiagnosticLogOutcome.Cleared(cleared.Removed, cleared.Bytes),
            _ => new ClearDiagnosticLogOutcome.PartlyCleared(cleared.Removed, cleared.Bytes, cleared.Kept),
        };
    }
}