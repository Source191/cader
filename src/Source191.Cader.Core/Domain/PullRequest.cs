namespace Source191.Cader.Domain;

/// <summary>One pull request from a host's listing, with the head commit its host recorded and when it finished (merged, closed, completed or abandoned).</summary>
public sealed record PullRequest(PullRequestHost Host, int Number, Uri Url, PullRequestState State, DateTimeOffset CreatedAt, HeadCommit Head, RecordedTime Finished);