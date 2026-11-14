namespace Source191.Cader.Domain;

/// <summary>
/// What happens before a new worktree branches from origin's default branch: fetch that branch from origin, fetch it and
/// fast-forward the local branch of the same name too, or nothing, so the worktree branches from it as last fetched.
/// </summary>
public enum BeforeBranching
{
    Fetch,
    Pull,
    None,
}
