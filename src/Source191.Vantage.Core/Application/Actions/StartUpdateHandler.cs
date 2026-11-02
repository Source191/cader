using Source191.Vantage.Application.Effects;
using Source191.Vantage.Application.Views;
using Source191.Vantage.Ports;

namespace Source191.Vantage.Application.Actions;

/// <summary>
/// Opens the update tab: a running vantage.exe locks its own files, so the update runs in a tab that waits for this
/// process to exit first. <see cref="TabScripts.Update"/> says what it runs.
/// </summary>
public sealed class StartUpdateHandler(IWindowsTerminal terminal, TabScripts scripts, VantageProcess vantage) : IEffectHandler<StartUpdate>
{
    public async Task<Outcome> HandleAsync(StartUpdate effect, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(effect);
        await terminal.OpenTabAsync(scripts.Update(vantage.ProcessId, vantage.Home), cancellationToken);

        return new UpdateStartedOutcome(effect.Newest.ToString());
    }
}