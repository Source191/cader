namespace Source191.Cader.Domain;

/// <summary>A worktree and branch created here, so removal knows the branch is its own to delete.</summary>
public sealed record CreatedWorktree(FolderPath Path, BranchName Branch, FolderPath Repository, DateTimeOffset CreatedAt);