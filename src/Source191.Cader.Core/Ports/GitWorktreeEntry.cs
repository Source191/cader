using Source191.Cader.Domain;

namespace Source191.Cader.Ports;

/// <summary>One block of git's machine-readable worktree listing, as <see cref="IGit.ListWorktreesAsync"/> returns it: Commit is the hash on its HEAD line, and IsPrunable says git gave a "prunable" line, whose reason the console never reads.</summary>
public sealed record GitWorktreeEntry(
    string Path,
    string Commit,
    Head Head,
    bool IsMain,
    WorktreeLock Lock,
    bool IsPrunable);