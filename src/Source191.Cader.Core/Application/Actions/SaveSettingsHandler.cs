using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Views;

namespace Source191.Cader.Application.Actions;

/// <summary>Writes the settings the dialog settled on to config.json and makes them the current ones.</summary>
public sealed class SaveSettingsHandler(CurrentSettings settings) : IEffectHandler<SaveSettings>
{
    public async Task<Outcome> HandleAsync(SaveSettings effect, CancellationToken cancellationToken)
    {
        // The dialog's settings replace the whole, after any toggle already being saved.
        await settings.UpdateAsync(_ => effect.Settings);

        return new SaveSettingsOutcome();
    }
}