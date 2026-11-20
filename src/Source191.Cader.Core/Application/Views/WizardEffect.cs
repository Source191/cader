using Source191.Cader.Domain;
using Source191.Cader.Application.Launch;

namespace Source191.Cader.Application.Views;

/// <summary>What the dialog does after a reduce: nothing, start fetching a repository's base early, start the plan, or close.</summary>
public abstract record WizardEffect
{
    /// <summary>Nothing: the dialog pushes the new state back to its controls.</summary>
    public sealed record None : WizardEffect;

    /// <summary>Start fetching origin's default branch in the repository now, and push the state back as for <see cref="None"/>.</summary>
    public sealed record Prefetch(FolderPath Repository) : WizardEffect;

    public sealed record Launch(LaunchPlan Plan) : WizardEffect;

    public sealed record Close : WizardEffect;
}