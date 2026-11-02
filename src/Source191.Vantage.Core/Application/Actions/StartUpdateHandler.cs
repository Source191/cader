using Source191.Vantage.Application.Effects;
using Source191.Vantage.Application.Views;
using Source191.Vantage.Domain;
using Source191.Vantage.Ports;

namespace Source191.Vantage.Application.Actions;

/// <summary>
/// Opens the update tab, or for a dotnet tool install the move tab: a running vantage.exe locks its own files, so either
/// runs in a tab that waits for this process to exit first. <see cref="TabScripts.Update"/> and
/// <see cref="TabScripts.Migrate"/> say what they run. Without winget, or dotnet for the move, no tab opens and Vantage stays.
/// </summary>
public sealed class StartUpdateHandler(IWindowsTerminal terminal, TabScripts scripts, VantageProcess vantage) : IEffectHandler<StartUpdate>
{
    public async Task<Outcome> HandleAsync(StartUpdate effect, CancellationToken cancellationToken)
    {
        string newest = effect.Newest.ToString();
        VantageInstall install = vantage.Install;
        if (install.Winget is not ProgramLocation.Found winget)
        {
            return new UpdaterMissingOutcome("winget", newest);
        }

        return install.Channel switch
        {
            InstallChannel.Winget => await OpenAsync(scripts.Update(vantage.ProcessId, vantage.Home, winget.Path), new UpdateStartedOutcome(newest), cancellationToken),
            InstallChannel.DotnetTool => install.Dotnet is ProgramLocation.Found dotnet
                ? await OpenAsync(TabScripts.Migrate(vantage.ProcessId, vantage.Home, winget.Path, dotnet.Path, install.LocalApplicationData), new MoveToWingetStartedOutcome(newest), cancellationToken)
                : new UpdaterMissingOutcome("dotnet", newest),
        };
    }

    private async Task<Outcome> OpenAsync(TabLaunch tab, Outcome started, CancellationToken cancellationToken)
    {
        await terminal.OpenTabAsync(tab, cancellationToken);

        return started;
    }
}