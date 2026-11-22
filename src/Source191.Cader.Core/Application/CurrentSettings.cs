using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.Application;

/// <summary>
/// The one home of the settings while the console runs. Every consumer reads <see cref="Value"/> when it starts a scan, a
/// wizard or a dialog, so a save from the settings dialog applies at once without a restart; a hand edit of
/// config.json applies on the next start.
/// </summary>
public sealed class CurrentSettings(Settings initial, IUserConfig store)
{
    private readonly SharedSnapshot<Settings> snapshot = new(initial);
    private readonly Lock gate = new();
    private Task last = Task.CompletedTask;

    /// <summary>The settings now: an immutable snapshot, replaced whole on a save.</summary>
    public Settings Value => snapshot.Value;

    public static async Task<CurrentSettings> LoadAsync(IUserConfig store, FolderPath home) => new(SettingsFile.From(await store.ReadConfigAsync(), home), store);

    /// <summary>Writes config.json, then makes the saved settings the current ones, after any change already being saved.</summary>
    public Task SaveAsync(Settings settings) => UpdateAsync(_ => settings);

    /// <summary>Applies the change to the settings the change before it saved, writes them and then makes them the current ones, answering what was saved.</summary>
    public Task<Settings> UpdateAsync(Func<Settings, Settings> change)
    {
        // Each change waits for the one before it, so it reads what that one saved; a lock cannot be held across the write's await.
        lock (gate)
        {
            Task<Settings> update = ApplyAfterAsync(last, change);
            last = update;

            return update;
        }
    }

    private async Task<Settings> ApplyAfterAsync(Task before, Func<Settings, Settings> change)
    {
        // A change that failed saved nothing, so the next applies to the settings as they were.
        await before.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        Settings changed = change(Value);
        await store.WriteConfigAsync(SettingsFile.ToConfig(changed));
        snapshot.Replace(changed);

        return changed;
    }
}