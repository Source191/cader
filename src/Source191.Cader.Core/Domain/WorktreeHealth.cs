using System.Diagnostics;

namespace Source191.Cader.Domain;

public sealed record WorktreeHealth(WorktreeHealthKind Kind, HealthReason Reason)
{
    /// <summary>A branch row's health: a local branch checked out in no worktree.</summary>
    public static WorktreeHealth CheckedOutNowhere { get; } = new(WorktreeHealthKind.Branch, new HealthReason.CheckedOutNowhere());

    /// <summary>An origin row's health: a branch only on origin.</summary>
    public static WorktreeHealth OnlyOnOrigin { get; } = new(WorktreeHealthKind.Origin, new HealthReason.OnlyOnOrigin());

    public static WorktreeHealth From(WorktreeObservation observation)
    {
        return observation switch
        {
            WorktreeObservation.Registered registered => FromRegistered(registered),
            WorktreeObservation.Unregistered unregistered => FromUnregistered(unregistered),
            _ => throw new UnreachableException(),
        };
    }

    private static WorktreeHealth FromRegistered(WorktreeObservation.Registered observation)
    {
        return observation switch
        {
            { FolderExists: false } => new(WorktreeHealthKind.Broken, new HealthReason.FolderGone()),
            { Prunable: true } => new(WorktreeHealthKind.Broken, new HealthReason.GitFileGone()),
            { Lock: WorktreeLock.Locked locked } => new(WorktreeHealthKind.Locked, new HealthReason.Locked(locked.Reason)),
            _ => new(WorktreeHealthKind.Ok, new HealthReason.None()),
        };
    }

    private static WorktreeHealth FromUnregistered(WorktreeObservation.Unregistered observation)
    {
        return observation switch
        {
            { GitFile: GitPointer.RegisteredElsewhere elsewhere } => new(WorktreeHealthKind.Elsewhere, new HealthReason.RepositoryOutsideRoots(elsewhere.Owner)),
            { GitFile: GitPointer.TargetGone } => new(WorktreeHealthKind.Broken, new HealthReason.RegistrationGone()),
            { Folder: UnregisteredFolder.UnderClaudeWorktrees } => new(WorktreeHealthKind.Orphan, new HealthReason.UnderClaudeWorktrees()),
            { Folder: UnregisteredFolder.EmptyLeftover } => new(WorktreeHealthKind.Empty, new HealthReason.EmptyLeftover()),
            _ => new(WorktreeHealthKind.Orphan, new HealthReason.NotRegistered()),
        };
    }
}