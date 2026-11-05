using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Views;
using Source191.Cader.Ports;

namespace Source191.Cader.Application.Actions;

/// <summary>
/// Opens the update tab: a running cader.exe locks its own files, so the upgrade runs in a tab that waits for this
/// process to exit first. <see cref="TabScripts.Update"/> says what it runs. Without winget no tab opens and Cader stays.
/// </summary>
public sealed class StartUpdateHandler(IWindowsTerminal terminal, TabScripts scripts, OwnProcess cader) : IEffectHandler<StartUpdate>
{
    public async Task<Outcome> HandleAsync(StartUpdate effect, CancellationToken cancellationToken)
    {
        if (cader.Winget is not ProgramLocation.Found winget)
        {
            return new WingetMissingOutcome(effect.Newest.ToString());
        }

        await terminal.OpenTabAsync(scripts.Update(cader.ProcessId, cader.Home, winget.Path), cancellationToken);

        return new UpdateStartedOutcome(effect.Newest.ToString());
    }
}