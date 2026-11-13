using Source191.Cader.Domain;

namespace Source191.Cader.Application.Views;

/// <summary>Delete the branch a removal left behind, after the user agreed.</summary>
public sealed record DeleteBranch(BranchToConfirm Branch) : ViewEffect;