using Source191.Vantage.Application.Effects;
using Source191.Vantage.Application.Views;
using Source191.Vantage.Ports;

namespace Source191.Vantage.Application.Actions;

/// <summary>
/// Opens the update tab: a running vantage.exe locks its own files, so the upgrade runs in a tab that waits for this
/// process to exit first. <see cref="TabScripts.Update"/> says what it runs. Without winget no tab opens and Vantage stays.
/// </summary>
public sealed class StartUpdateHandler(IWindowsTerminal terminal, TabScripts scripts, VantageProcess vantage) : IEffectHandler<StartUpdate>
{
    public async Task<Outcome> HandleAsync(StartUpdate effect, CancellationToken cancellationToken)
    {
        if (vantage.Winget is not ProgramLocation.Found winget)
        {
            return new WingetMissingOutcome(effect.Newest.ToString());
        }

        await terminal.OpenTabAsync(scripts.Update(vantage.ProcessId, vantage.Home, winget.Path), cancellationToken);

        return new UpdateStartedOutcome(effect.Newest.ToString());
    }
}