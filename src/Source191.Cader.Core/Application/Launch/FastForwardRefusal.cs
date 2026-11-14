using Source191.Cader.Domain;

namespace Source191.Cader.Application.Launch;

/// <summary>Why the local default branch was not fast-forwarded: a linked worktree has it checked out, or git refused, in its own words.</summary>
public abstract record FastForwardRefusal
{
    public sealed record CheckedOutElsewhere(FolderPath Folder) : FastForwardRefusal;

    public sealed record Refused(string Reason) : FastForwardRefusal;
}
