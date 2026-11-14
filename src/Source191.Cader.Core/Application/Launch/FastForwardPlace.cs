using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.Application.Launch;

/// <summary>Where Pull fast-forwards the local default branch, read from git's worktree listing: in the main checkout, nowhere because a linked worktree has it checked out, or as a branch no worktree has.</summary>
public abstract record FastForwardPlace
{
    /// <summary>The main checkout has the branch checked out, so its files move with it.</summary>
    public sealed record MainCheckout : FastForwardPlace;

    /// <summary>A linked worktree has the branch checked out; it is left alone.</summary>
    public sealed record ElsewhereAt(FolderPath Folder) : FastForwardPlace;

    /// <summary>No worktree has the branch checked out, so the branch is moved and no files change.</summary>
    public sealed record NotCheckedOut : FastForwardPlace;

    /// <summary>
    /// A linked worktree whose folder git gives as no full path is taken as not checked out: the move without a checkout
    /// then meets git's own refusal of a branch checked out anywhere.
    /// </summary>
    public static FastForwardPlace Of(IReadOnlyList<GitWorktreeEntry> entries, BranchName branch)
    {
        Head.OnBranch head = new(branch);

        return entries.FirstOrDefault(entry => entry.Head.Equals(head)) switch
        {
            { IsMain: true } => new MainCheckout(),
            GitWorktreeEntry linked when FolderPath.TryParse(linked.Path, out FolderPath? folder) => new ElsewhereAt(folder),
            _ => new NotCheckedOut(),
        };
    }
}