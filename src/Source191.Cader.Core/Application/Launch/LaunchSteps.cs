using Source191.Cader.Domain;

namespace Source191.Cader.Application.Launch;

/// <summary>
/// The stages one launch runs, in order, so each step it reports is numbered out of the steps it really runs: five for a
/// new worktree whose base is fetched or pulled first, four for a new worktree with nothing to update, three with no new worktree.
/// </summary>
public sealed class LaunchSteps
{
    private readonly SessionName name;
    private readonly LaunchBase remoteBase;
    private readonly LaunchStage[] stages;

    private LaunchSteps(SessionName name, LaunchBase remoteBase, LaunchStage[] stages)
    {
        this.name = name;
        this.remoteBase = remoteBase;
        this.stages = stages;
    }

    /// <summary>A new worktree branching from origin's default branch, fetched or pulled first as Before branching says.</summary>
    public static LaunchSteps ForNewWorktree(SessionName name, RemoteBranch remoteBranch, BeforeBranching choice)
    {
        LaunchStage[] rest = [LaunchStage.CreatingWorktree, LaunchStage.StartingClaude, LaunchStage.FindingSession, LaunchStage.OpeningTab];
        LaunchStage[] stages = choice switch
        {
            BeforeBranching.Fetch => [LaunchStage.Fetching, .. rest],
            BeforeBranching.Pull => [LaunchStage.Pulling, .. rest],
            BeforeBranching.None => rest,
        };

        return new LaunchSteps(name, new LaunchBase.Remote(remoteBranch), stages);
    }

    /// <summary>A new worktree branching from HEAD, as asked or because origin has no HEAD.</summary>
    public static LaunchSteps ForNewWorktree(SessionName name) =>
        new(name, new LaunchBase.None(), [LaunchStage.CreatingWorktree, LaunchStage.StartingClaude, LaunchStage.FindingSession, LaunchStage.OpeningTab]);

    /// <summary>The current branch, a folder choice or a worktree that already exists.</summary>
    public static LaunchSteps WithoutANewWorktree(SessionName name) =>
        new(name, new LaunchBase.None(), [LaunchStage.StartingClaude, LaunchStage.FindingSession, LaunchStage.OpeningTab]);

    /// <summary>The stage as this launch numbers it; a stage the launch does not run is a bug.</summary>
    public LaunchStep Step(LaunchStage stage)
    {
        int index = Array.IndexOf(stages, stage);

        return index < 0
            ? throw new ArgumentException($"This launch runs no {stage} step.", nameof(stage))
            : new LaunchStep(name, stage, index + 1, stages.Length, remoteBase);
    }
}