using Source191.Cader.Application.Effects;
using Source191.Cader.Domain;

namespace Source191.Cader.Application.Actions;

/// <summary>A pull request's page was handed to the default browser.</summary>
public sealed record OpenPullRequestOutcome(PullRequestHost Host, int Number) : Outcome;