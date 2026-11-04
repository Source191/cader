namespace Source191.Cader.Application.Views;

/// <summary>The dialog effect behind <c>n</c> on a worktree row: the wizard fixed on that folder (<see cref="WizardOrigin.In"/>).</summary>
public sealed record StartSessionIn(WorktreeRow Row) : ViewEffect, IDialogEffect;