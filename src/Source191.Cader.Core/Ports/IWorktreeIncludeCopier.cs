using Source191.Cader.Domain;
namespace Source191.Cader.Ports;

/// <summary>
/// Copies the gitignored files a repository's .worktreeinclude names into a new worktree. Throws
/// <see cref="IOException"/> or <see cref="UnauthorizedAccessException"/> when the file system refuses a file.
/// </summary>
public interface IWorktreeIncludeCopier
{
    Task<IReadOnlyList<string>> CopyAsync(FolderPath repository, FolderPath worktree, CancellationToken cancellationToken);
}