using Source191.Cader.Domain;

namespace Source191.Cader.Ports;

/// <summary>One branch's pull request in a host's listing.</summary>
public sealed record BranchPullRequest(BranchName Branch, PullRequest PullRequest);