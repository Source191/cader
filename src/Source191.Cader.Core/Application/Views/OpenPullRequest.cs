using Source191.Cader.Domain;

namespace Source191.Cader.Application.Views;

/// <summary>Open the row's pull request in the default browser.</summary>
public sealed record OpenPullRequest(PullRequest PullRequest) : ViewEffect;