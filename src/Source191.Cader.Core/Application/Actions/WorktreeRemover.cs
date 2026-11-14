using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using System.Diagnostics;

namespace Source191.Cader.Application.Actions;

/// <summary>
/// Carries out a removal decision for a worktree, the one way both views remove one: refuse, delete a leftover folder
/// (pruning its registration when the decision says so), or remove it through git and then deal with its branch. When
/// git cannot delete a worktree's folder because something holds it, git has already removed the worktree, so the
/// folder is waited for and the removal finished either way. The last guard before a folder is deleted from disc is
/// that it lies inside a repository root or the worktree root, read from the settings as they are now, whoever asked
/// and whatever the decision said.
/// </summary>
public sealed class WorktreeRemover(IGit git, ICreatedWorktrees createdWorktrees, IFolderDeleter deleter, CurrentSettings settings, TimeProvider clock)
{
    /// <summary>About five seconds in all: long enough for a shell or session that is ending to let go of its folder.</summary>
    private const int FolderAttempts = 20;

    private static readonly TimeSpan BetweenAttempts = TimeSpan.FromMilliseconds(250);

    public async Task<RemoveWorktreeOutcome> CarryOutAsync(WorktreeRow row, RemovalDecision decision, bool createdHere, CancellationToken cancellationToken)
    {
        return decision switch
        {
            RemovalDecision.Refused refused => new RemoveWorktreeOutcome.Refused(refused.Reason),
            RemovalDecision.DeleteFolder deleteFolder => await DeleteFolderAsync(row, deleteFolder, cancellationToken),
            RemovalDecision.RemoveWorktree removeWorktree => await RemoveThroughGitAsync(row, removeWorktree, createdHere, cancellationToken),
            _ => throw new UnreachableException(),
        };
    }

    private async Task<RemoveWorktreeOutcome> RemoveThroughGitAsync(WorktreeRow row, RemovalDecision.RemoveWorktree decision, bool createdHere, CancellationToken cancellationToken)
    {
        GitRemoval removal = await RemoveWithGitAsync(row, decision.Forced, cancellationToken);
        if (removal == GitRemoval.StillRegistered)
        {
            return new RemoveWorktreeOutcome.InUse(row.FolderName);
        }

        bool folderLeft = removal == GitRemoval.FolderLeft;
        await createdWorktrees.ForgetCreatedWorktreeAsync(row.Path);
        return (decision.Branch, row.Head) switch
        {
            (BranchAction.Delete, Head.OnBranch merged) => await DeleteBranchAsync(row, merged.Branch, folderLeft, cancellationToken),
            (BranchAction.Ask, Head.OnBranch kept) => new RemoveWorktreeOutcome.BranchKept(row.FolderName, new BranchToConfirm(row.Repository, kept.Branch), createdHere, folderLeft),
            (_, Head.Detached or Head.Unknown) => new RemoveWorktreeOutcome.Removed(row.FolderName, folderLeft),
            _ => throw new UnreachableException(),
        };
    }

    /// <summary>How git's removal ended: the folder went, git removed the worktree but its folder is held, or git still lists the worktree.</summary>
    private enum GitRemoval
    {
        Removed,
        FolderLeft,
        StillRegistered,
    }

    private async Task<GitRemoval> RemoveWithGitAsync(WorktreeRow row, bool forced, CancellationToken cancellationToken)
    {
        try
        {
            await git.RemoveWorktreeAsync(row.Repository.Value, row.Path.Value, forced, cancellationToken);
            return GitRemoval.Removed;
        }
        catch (GitException exception) when (exception.InUse)
        {
            return await AfterInUseAsync(row, cancellationToken);
        }
    }

    /// <summary>
    /// Git's "in use" words are matched loosely, so before anything is deleted the repository is asked whether it still
    /// lists the worktree. When it does not, git has already deleted the contents, the .git file and the registration
    /// and failed only on the folder itself (GitWorktreeRemoveShould), so what is left is a plain folder no later remove
    /// would see.
    /// </summary>
    private async Task<GitRemoval> AfterInUseAsync(WorktreeRow row, CancellationToken cancellationToken)
    {
        if (await StillListedAsync(row, cancellationToken))
        {
            return GitRemoval.StillRegistered;
        }

        return !InsideTheRoots(row.Path) || !await DeleteWhenLetGoAsync(row.Path, cancellationToken) ? GitRemoval.FolderLeft : GitRemoval.Removed;
    }

    /// <summary>True when the repository still lists the worktree's path, or cannot be asked.</summary>
    private async Task<bool> StillListedAsync(WorktreeRow row, CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<GitWorktreeEntry> entries = await git.ListWorktreesAsync(row.Repository.Value, cancellationToken);

            return entries.Any(entry => FolderPath.TryParse(entry.Path, out FolderPath? listed) && listed.Equals(row.Path));
        }
        catch (GitException)
        {
            return true;
        }
    }

    /// <summary>
    /// Tries to delete the folder git emptied but could not delete, while whatever holds it (usually a tab's shell or a
    /// session's process that is just ending) lets go; false when it is still held after the last try.
    /// </summary>
    private async Task<bool> DeleteWhenLetGoAsync(FolderPath folder, CancellationToken cancellationToken)
    {
        for (int attempt = 1; attempt < FolderAttempts; attempt++)
        {
            if (TryDelete(folder))
            {
                return true;
            }

            await Task.Delay(BetweenAttempts, clock, cancellationToken);
        }

        return TryDelete(folder);
    }

    /// <summary>Deletes a folder; false when a tab or program still has it open.</summary>
    private bool TryDelete(FolderPath folder)
    {
        try
        {
            deleter.DeleteTree(folder.Value);
            return true;
        }
        catch (Exception failure) when (failure is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private async Task<RemoveWorktreeOutcome> DeleteBranchAsync(WorktreeRow row, BranchName branch, bool folderLeft, CancellationToken cancellationToken)
    {
        await git.DeleteBranchAsync(row.Repository.Value, branch.Value, force: false, cancellationToken);

        return new RemoveWorktreeOutcome.RemovedAndDeletedBranch(row.FolderName, branch, folderLeft);
    }

    private async Task<RemoveWorktreeOutcome> DeleteFolderAsync(WorktreeRow row, RemovalDecision.DeleteFolder decision, CancellationToken cancellationToken)
    {
        if (!InsideTheRoots(row.Path))
        {
            return new RemoveWorktreeOutcome.Refused(new RemovalReason.OutsideTheRoots(row.Path));
        }

        if (!TryDelete(row.Path))
        {
            return new RemoveWorktreeOutcome.InUse(row.FolderName);
        }

        if (!decision.Prune || !row.OwnerExists)
        {
            return new RemoveWorktreeOutcome.DeletedFolder(row.FolderName);
        }

        int pruned = await git.PruneWorktreesAsync(row.Repository.Value, cancellationToken);

        return new RemoveWorktreeOutcome.DeletedFolderAndPruned(row.FolderName, pruned, row.RepositoryName);
    }

    /// <summary>Strictly inside a repository root or the worktree root: a root itself is never deleted as a leftover.</summary>
    private bool InsideTheRoots(FolderPath folder)
    {
        Settings now = settings.Value;

        return now.RepositoryRoots.Append(now.WorktreeRoot).Any(root => folder.IsUnder(root) && !folder.Equals(root));
    }
}