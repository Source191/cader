namespace Source191.Cader.Application.Views;

/// <summary>Whether a worktrees row has a folder of its own: a worktree or a leftover does, a branch or origin row does not, so the folder keys only say so.</summary>
public enum Placement
{
    Folder,
    NoFolder,
}