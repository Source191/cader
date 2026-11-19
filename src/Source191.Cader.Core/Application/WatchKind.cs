namespace Source191.Cader.Application;

/// <summary>What a watched folder is, which decides which of its changes count (<see cref="ChangeFilter"/>) and which tab they refresh.</summary>
public enum WatchKind
{
    /// <summary>The Claude folder's <c>sessions</c> folder, top level only; refreshes the Sessions tab.</summary>
    ClaudeSessions,

    /// <summary>The Claude folder's <c>jobs</c> folder and its subfolders; refreshes the Sessions tab.</summary>
    ClaudeJobs,

    /// <summary>A listed repository's git folder and its subfolders; refreshes the Worktrees tab.</summary>
    Repository,

    /// <summary>A repository root, top level only; refreshes the Worktrees tab.</summary>
    Root,
}