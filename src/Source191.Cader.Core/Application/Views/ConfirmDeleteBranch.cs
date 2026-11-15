using Source191.Cader.Domain;

namespace Source191.Cader.Application.Views;

/// <summary>d on a branch row with nothing at risk: the TUI asks the branch question, then raises DeleteBranch on a yes.</summary>
public sealed record ConfirmDeleteBranch(BranchToConfirm Branch) : ViewEffect, IDialogEffect;