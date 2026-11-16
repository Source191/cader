namespace Source191.Cader.Domain;

/// <summary>
/// The commit a pull request's source branch pointed at as its host last recorded it: GitHub's head commit, or Azure
/// DevOps's last merge source commit. A named case stands where a null would, for a host that recorded none or wrote
/// something that is not a commit id.
/// </summary>
public abstract record HeadCommit
{
    public sealed record Known(CommitId Commit) : HeadCommit;

    public sealed record NotRecorded : HeadCommit;
}