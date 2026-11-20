using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Domain;

namespace Source191.Cader.Application;

/// <summary>
/// Writes the detail panes' heights to config.json once they have settled, as the console's ticker asks on every beat. The write
/// runs off the caller's thread through <see cref="CurrentSettings"/>, which never lets it race the settings dialog's save; a failed
/// write is recorded with its summary and shows nothing, and the next settled change tries again.
/// </summary>
public sealed class PaneHeightSaves(SettlingPaneHeights heights, CurrentSettings settings, DiagnosticLog log)
{
    /// <summary>Starts the save of heights that have settled and answers it, or answers at once when none have.</summary>
    public Task SaveIfSettledAsync() => heights.TryTakeDue(out PaneHeights? due)
        ? Task.Run(() => Attempt.RunAsync(log, FailureSite.PaneHeightSave, CodeName.For(typeof(PaneHeights)), () => SaveAsync(due), _ => false))
        : Task.CompletedTask;

    private async Task<bool> SaveAsync(PaneHeights due)
    {
        await settings.SavePaneHeightsAsync(due);
        return true;
    }
}