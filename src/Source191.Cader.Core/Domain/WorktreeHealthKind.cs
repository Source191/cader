namespace Source191.Cader.Domain;

public enum WorktreeHealthKind
{
    Ok,
    Locked,
    Orphan,
    Broken,
    Empty,

    /// <summary>A live worktree of a repository the console did not scan, because it is not under the roots.</summary>
    Elsewhere,

    /// <summary>A local branch checked out in no worktree; its row has no folder.</summary>
    Branch,

    /// <summary>A branch only on origin, as of the last fetch; its row has no folder.</summary>
    Origin,
}