using Source191.Cader.Domain;

namespace Source191.Cader.Application.Detail;

/// <summary>Whether the worktree was created here, with its record; one whose records cannot be read reads as not created here.</summary>
public abstract record WorktreeOrigin
{
    public sealed record CreatedHere(CreatedWorktree Record) : WorktreeOrigin;

    public sealed record NotCreatedHere : WorktreeOrigin;
}