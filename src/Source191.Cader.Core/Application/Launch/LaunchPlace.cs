using Source191.Cader.Domain;

namespace Source191.Cader.Application.Launch;

/// <summary>
/// Where a planned session starts, as the plan builder found it: on the repository's current branch, in a worktree
/// to be made first, or in a worktree whose folder already exists and is reused as it is.
/// </summary>
public abstract record LaunchPlace
{
    public sealed record CurrentBranch : LaunchPlace;

    public sealed record NewWorktree(PlannedWorktree Worktree) : LaunchPlace;

    public sealed record ExistingWorktree(PlannedWorktree Worktree) : LaunchPlace;
}