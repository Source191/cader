using Source191.Cader.Domain;

namespace Source191.Cader.Application.Detail;

/// <summary>Whether Cader created the worktree, with its record; one whose records cannot be read reads as not by Cader.</summary>
public abstract record WorktreeOrigin
{
    public sealed record CreatedByCader(CreatedWorktree Record) : WorktreeOrigin;

    public sealed record NotByCader : WorktreeOrigin;
}