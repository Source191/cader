namespace Source191.Cader.Application.Launch;

/// <summary>One step of a launch as the status line names it, in the order a launch runs them.</summary>
public enum LaunchStage
{
    Fetching,
    Pulling,
    CreatingWorktree,
    StartingClaude,
    FindingSession,
    OpeningTab,
}