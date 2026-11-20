using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.Application;

/// <summary>
/// The one home of the settings while the console runs. Every consumer reads <see cref="Value"/> when it starts a scan, a
/// wizard or a dialog, so a save from the settings dialog applies at once without a restart; a hand edit of
/// config.json applies on the next start. Saves are written one at a time, each over the settings of its moment, so a
/// settled pane height and a dialog's save that land together both reach the file.
/// </summary>
public sealed class CurrentSettings(Settings initial, IUserConfig store)
{
    private readonly SharedSnapshot<Settings> snapshot = new(initial);
    private readonly Lock gate = new();

    /// <summary>The last save asked for; the next one starts once it has ended, whether it was written or failed.</summary>
    private Task writing = Task.CompletedTask;

    /// <summary>The settings now: an immutable snapshot, replaced whole on a save.</summary>
    public Settings Value => snapshot.Value;

    public static async Task<CurrentSettings> LoadAsync(IUserConfig store, FolderPath home) => new(SettingsFile.From(await store.ReadConfigAsync(), home), store);

    /// <summary>
    /// Writes config.json, then makes the saved settings the current ones. The panes' heights stay as they are when it writes,
    /// because only <see cref="SavePaneHeightsAsync"/> changes them and a dialog's settings know nothing of them.
    /// </summary>
    public Task SaveAsync(Settings settings) => WriteAsync(now => settings.WithPaneHeights(now.PaneHeights));

    /// <summary>Writes the panes' heights over the settings as they are when it writes.</summary>
    public Task SavePaneHeightsAsync(PaneHeights heights) => WriteAsync(now => now.WithPaneHeights(heights));

    private Task WriteAsync(Func<Settings, Settings> change)
    {
        lock (gate)
        {
            writing = WriteAfterAsync(writing, change);
            return writing;
        }
    }

    /// <summary>Waits for the save before, whose own caller hears how it ended, then writes this one over the settings it left.</summary>
    private async Task WriteAfterAsync(Task before, Func<Settings, Settings> change)
    {
        await before.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        Settings saved = change(snapshot.Value);
        await store.WriteConfigAsync(SettingsFile.ToConfig(saved));
        snapshot.Replace(saved);
    }
}