namespace Source191.Cader.Domain;

/// <summary>
/// What the file system and git say about one worktree folder, before any judgement is made about it: a folder git
/// lists, or one it does not.
/// </summary>
public abstract record WorktreeObservation
{
    /// <summary>A folder that appears in the repository's worktree listing.</summary>
    /// <param name="FolderExists">The folder exists on disc.</param>
    /// <param name="Lock">Whether git has the worktree locked, with its reason.</param>
    /// <param name="Prunable">Git lists the worktree as prunable: the .git file its registration names is missing.</param>
    public sealed record Registered(bool FolderExists, WorktreeLock Lock, bool Prunable) : WorktreeObservation;

    /// <summary>A folder that exists on disc and that git's listing omits.</summary>
    /// <param name="GitFile">What its .git file points at.</param>
    /// <param name="Folder">What kind of folder it is, which matters only when the .git file does not decide.</param>
    public sealed record Unregistered(GitPointer GitFile, UnregisteredFolder Folder) : WorktreeObservation;
}