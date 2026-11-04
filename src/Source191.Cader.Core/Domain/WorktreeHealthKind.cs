namespace Source191.Cader.Domain;

public enum WorktreeHealthKind
{
    Ok,
    Locked,
    Orphan,
    Broken,
    Empty,

    /// <summary>A live worktree of a repository Cader did not scan, because it is not under the roots.</summary>
    Elsewhere,
}