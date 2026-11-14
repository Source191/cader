namespace Source191.Cader.Ports;

/// <summary>Git failed or answered with something unreadable. InUse: a worktree folder could not be deleted because a tab or program still has it open.</summary>
public sealed class GitException(string message, bool inUse) : Exception(message)
{
    public bool InUse { get; } = inUse;
}