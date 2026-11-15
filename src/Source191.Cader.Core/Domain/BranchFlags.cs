namespace Source191.Cader.Domain;

/// <summary>Which flag a branch shows, most urgent first: left behind, gone, closed, no PR.</summary>
public static class BranchFlags
{
    /// <summary>
    /// <paramref name="commitsAfterMergedHead"/> is git's count of the branch's commits that the shown pull request's
    /// head does not contain, or null when it was not counted or git could not count it; only a count above zero after
    /// a merge with a recorded head is left behind. Only the host's own "none" is <see cref="BranchFlagKind.NoPullRequest"/>:
    /// a lookup that could not answer flags nothing but a gone upstream.
    /// </summary>
    public static BranchFlagKind Of(PullRequestLookup lookup, Upstream upstream, int? commitsAfterMergedHead) => (lookup, upstream) switch
    {
        (PullRequestLookup.Found { PullRequest: { State: PullRequestState.Merged or PullRequestState.Completed, Head: HeadCommit.Known } }, _) when commitsAfterMergedHead > 0 => BranchFlagKind.LeftBehind,
        (_, Upstream.Gone) => BranchFlagKind.Gone,
        (PullRequestLookup.Found { PullRequest.State: PullRequestState.Closed or PullRequestState.Abandoned }, _) => BranchFlagKind.Closed,
        (PullRequestLookup.Missing { Gap: PullRequestGap.NoPullRequest }, _) => BranchFlagKind.NoPullRequest,
        _ => BranchFlagKind.None,
    };
}
