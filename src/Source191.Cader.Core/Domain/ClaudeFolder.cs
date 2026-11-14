namespace Source191.Cader.Domain;

/// <summary>The folder Claude Code keeps its settings and transcripts in, and where Cader's choice of it came from.</summary>
public abstract record ClaudeFolder(FolderPath Folder)
{
    public static ClaudeFolder From(string? variable, FolderPath home) => new Unset(home.Combine(".claude"));

    /// <summary>The variable was not set: <c>.claude</c> under the user's profile.</summary>
    public sealed record Unset(FolderPath Folder) : ClaudeFolder(Folder);

    /// <summary>The variable named this folder as a full path.</summary>
    public sealed record FromVariable(FolderPath Folder) : ClaudeFolder(Folder);

    /// <summary>The variable was not a full path, so the default folder is used and the refused text is kept to say so.</summary>
    public sealed record NotUsable(FolderPath Folder, string Refused) : ClaudeFolder(Folder);
}
