using Source191.Cader.Application.Effects;

namespace Source191.Cader.Application.Actions;

/// <summary>A plain shell tab was opened in the worktree.</summary>
public sealed record OpenShellOutcome(string FolderName) : Outcome;