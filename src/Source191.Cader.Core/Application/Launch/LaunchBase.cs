using Source191.Cader.Domain;

namespace Source191.Cader.Application.Launch;

/// <summary>The remote branch a launch's new worktree branches from, which its fetching or pulling step names, or none when it branches from HEAD or makes no worktree.</summary>
public abstract record LaunchBase
{
    public sealed record Remote(RemoteBranch Branch) : LaunchBase;

    public sealed record None : LaunchBase;
}