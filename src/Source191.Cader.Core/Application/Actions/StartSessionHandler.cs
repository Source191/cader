using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Launch;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using System.Diagnostics;

namespace Source191.Cader.Application.Actions;

/// <summary>
/// Starts the session the launch wizard planned, from either view, and opens the new session's tab; when the workspace
/// is not trusted it opens a plain Claude tab there so the trust prompt can be accepted. The console's timed and heard
/// refreshes are held for the whole of it, released however it ends, so the launch never competes with Cader's own scans.
/// </summary>
public sealed class StartSessionHandler(LaunchService service, OpenTabHandler tabs, IWindowsTerminal terminal, TabScripts scripts, IProgress<LaunchStep> progress, LaunchHold hold)
    : IEffectHandler<StartSession>
{
    public async Task<Outcome> HandleAsync(StartSession effect, CancellationToken cancellationToken)
    {
        using IDisposable held = hold.Hold();
        // An earlier launch's last step would otherwise decorate a retry under the same name until its own first step.
        progress.Report(LaunchStep.None);
        LaunchOutcome outcome = await service.LaunchAsync(effect.Plan, cancellationToken);

        string name = effect.Plan.Name.Value;

        return outcome switch
        {
            LaunchOutcome.Listed listed => new StartSessionOutcome.StartedAndOpened(name, listed.JobId, await OpenTabAsync(listed, cancellationToken), listed.Update),
            LaunchOutcome.NotListedYet notListed => new StartSessionOutcome.NotListedYet(name, notListed.JobId, notListed.Update),
            LaunchOutcome.WorkspaceNotTrusted => await OpenTrustTabAsync(effect.Plan.Directory, cancellationToken),
            LaunchOutcome.Failed failed => new StartSessionOutcome.Failed(failed.Message),
            _ => throw new UnreachableException(),
        };
    }

    private Task<OpenTabOutcome> OpenTabAsync(LaunchOutcome.Listed listed, CancellationToken cancellationToken)
    {
        progress.Report(listed.Steps.Step(LaunchStage.OpeningTab));

        return tabs.OpenAsync(listed.Session, cancellationToken);
    }

    private async Task<StartSessionOutcome> OpenTrustTabAsync(FolderPath directory, CancellationToken cancellationToken)
    {
        await terminal.OpenTabAsync(scripts.Trust(directory), cancellationToken);

        return new StartSessionOutcome.NotTrusted(directory);
    }
}