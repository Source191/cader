using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Views;
using Source191.Cader.Ports;

namespace Source191.Cader.Application.Actions;

/// <summary>Prunes a repository's stale worktree registrations and says how many went.</summary>
public sealed class PruneRepositoryHandler(IGit git) : IEffectHandler<PruneRepository>
{
    public async Task<Outcome> HandleAsync(PruneRepository effect, CancellationToken cancellationToken)
    {
        int pruned = await git.PruneWorktreesAsync(effect.Repository.Value, cancellationToken);

        return new PruneRepositoryOutcome(pruned, effect.Repository.Name);
    }
}