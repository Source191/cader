namespace Source191.Cader.Domain;

/// <summary>
/// Which of a branch's pull requests the tables show: the open one, otherwise the most recent, and NoPullRequest when it
/// has none. A finished pull request that finished strictly before the branch was created belongs to an earlier branch of
/// the same name and is left out first; a tie is no evidence, since git records the branch's creation in whole seconds.
/// </summary>
public static class PullRequests
{
    public static PullRequestLookup ForBranch(IReadOnlyList<PullRequest> forBranch, RecordedTime branchCreated)
    {
        List<PullRequest> own = [.. forBranch.Where(pullRequest => !FinishedBefore(pullRequest, branchCreated))];

        return (own.FirstOrDefault(pullRequest => pullRequest.State.IsOpen()) ?? own.MaxBy(pullRequest => pullRequest.CreatedAt)) is PullRequest shown
            ? new PullRequestLookup.Found(shown)
            : new PullRequestLookup.Missing(PullRequestGap.NoPullRequest);
    }

    private static bool FinishedBefore(PullRequest pullRequest, RecordedTime branchCreated) =>
        (pullRequest.Finished, branchCreated) is (RecordedTime.At finished, RecordedTime.At created) && !pullRequest.State.IsOpen() && finished.When < created.When;
}