using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Views;
using Source191.Cader.Ports;

namespace Source191.Cader.Application.Actions;

/// <summary>Opens a pull request's page in the default browser.</summary>
public sealed class OpenPullRequestHandler(IDesktop desktop) : IEffectHandler<OpenPullRequest>
{
    public async Task<Outcome> HandleAsync(OpenPullRequest effect, CancellationToken cancellationToken)
    {
        await desktop.OpenAsync(effect.PullRequest.Url.AbsoluteUri, cancellationToken);

        return new OpenPullRequestOutcome(effect.PullRequest.Host, effect.PullRequest.Number);
    }
}