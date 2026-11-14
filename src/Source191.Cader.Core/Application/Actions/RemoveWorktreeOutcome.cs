using Source191.Cader.Application.Effects;
using Source191.Cader.Domain;

namespace Source191.Cader.Application.Actions;

/// <summary>
/// What removing a worktree did, and in one case the branch the user is asked about. FolderLeft on a removal: git
/// removed the worktree but its folder is still held open by a tab or program, so it was left behind, empty.
/// </summary>
public abstract record RemoveWorktreeOutcome : Outcome
{
    public sealed record Refused(RemovalReason Reason) : RemoveWorktreeOutcome;

    public sealed record Removed(string FolderName, bool FolderLeft = false) : RemoveWorktreeOutcome;

    public sealed record RemovedAndDeletedBranch(string FolderName, BranchName Branch, bool FolderLeft = false) : RemoveWorktreeOutcome;

    /// <summary>The branch was kept: it was not created here, or was created here and it is not merged. The user is asked.</summary>
    public sealed record BranchKept(string FolderName, BranchToConfirm Branch, bool CreatedHere, bool FolderLeft = false) : RemoveWorktreeOutcome;

    public sealed record DeletedFolder(string FolderName) : RemoveWorktreeOutcome;

    public sealed record DeletedFolderAndPruned(string FolderName, int Pruned, string RepositoryName) : RemoveWorktreeOutcome;

    /// <summary>Git could not delete the folder because a tab or program still has it open; what it could delete is gone.</summary>
    public sealed record InUse(string FolderName) : RemoveWorktreeOutcome;
}