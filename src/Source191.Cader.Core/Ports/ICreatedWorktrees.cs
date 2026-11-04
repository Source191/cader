using Source191.Cader.Domain;

namespace Source191.Cader.Ports;

/// <summary>The worktrees and branches Cader created, which decide the branch-deletion rules.</summary>
public interface ICreatedWorktrees
{
    Task<IReadOnlyList<CreatedWorktree>> ReadCreatedWorktreesAsync();

    /// <summary>Throws <see cref="IOException"/> or <see cref="UnauthorizedAccessException"/> when the file system refuses the write.</summary>
    Task RecordCreatedWorktreeAsync(CreatedWorktree worktree);

    Task ForgetCreatedWorktreeAsync(FolderPath path);
}