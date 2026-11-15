namespace Source191.Cader.Domain;

/// <summary>What kind of folder git's listing omits, before its .git file is read: at most one of these holds.</summary>
public enum UnregisteredFolder
{
    /// <summary>The folder sits under a repository's .claude\worktrees directory.</summary>
    UnderClaudeWorktrees,

    /// <summary>The folder is an empty leftover such as a .worktrees folder.</summary>
    EmptyLeftover,

    /// <summary>Neither: its .git file alone says what it is.</summary>
    Other,
}