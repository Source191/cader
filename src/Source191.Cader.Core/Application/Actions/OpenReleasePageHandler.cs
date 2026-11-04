using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Views;
using Source191.Cader.Ports;

namespace Source191.Cader.Application.Actions;

/// <summary>Opens a release's GitHub page (<see cref="ReleasePage"/>) in the default browser.</summary>
public sealed class OpenReleasePageHandler(IDesktop desktop) : IEffectHandler<OpenReleasePage>
{
    public async Task<Outcome> HandleAsync(OpenReleasePage effect, CancellationToken cancellationToken)
    {
        try
        {
            await desktop.OpenAsync(ReleasePage.For(effect.Version).AbsoluteUri, cancellationToken);
        }
        catch (ProgramNotStartedException failed)
        {
            return new OpenReleasePageOutcome.NotOpened(effect.Version.ToString(), failed.Reason);
        }

        return new OpenReleasePageOutcome.Opened(effect.Version.ToString());
    }
}