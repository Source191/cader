using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Views;

namespace Source191.Cader.Application.Actions;

/// <summary>Renames a remembered account through the catalogue, which saves accounts.json and shows the label at the next refresh.</summary>
public sealed class RenameAccountHandler(AccountCatalogue accounts) : IEffectHandler<RenameAccount>
{
    public async Task<Outcome> HandleAsync(RenameAccount effect, CancellationToken cancellationToken)
    {
        bool renamed = await accounts.RenameAsync(effect.Account.Organisation, effect.NewLabel);

        return renamed ? new RenameAccountOutcome.Renamed(effect.Account.Label, effect.NewLabel) : new RenameAccountOutcome.Unchanged(effect.Account.Label);
    }
}