using Source191.Cader.Domain;

namespace Source191.Cader.Ports;

/// <summary>A host's answer for one branch: every pull request it listed, or why it could not list them.</summary>
public abstract record PullRequestListing
{
    public sealed record Listed(IReadOnlyList<BranchPullRequest> PullRequests) : PullRequestListing;

    public sealed record Unavailable(PullRequestGap Gap) : PullRequestListing;
}