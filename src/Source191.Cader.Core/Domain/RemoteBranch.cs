using System.Diagnostics.CodeAnalysis;

namespace Source191.Cader.Domain;

/// <summary>
/// A remote-tracking branch as git names it, such as <c>origin/main</c>: the remote before the first slash and the
/// branch after it, slashes and all (<c>origin/release/2026</c>). It comes from git's own output (origin's HEAD), so
/// the rule only guards against text that names no remote or no branch, and against a remote git would read as an option.
/// </summary>
public sealed record RemoteBranch : ITextValue<RemoteBranch>
{
    private RemoteBranch(string remote, BranchName branch)
    {
        Remote = remote;
        Branch = branch;
    }

    public string Remote { get; }

    public BranchName Branch { get; }

    public string Value => $"{Remote}/{Branch.Value}";

    public static RemoteBranch From(string text) => TryParse(text, out RemoteBranch? branch)
        ? branch
        : throw new ArgumentException($"'{text}' is not a remote branch: it needs a remote, a slash and a branch.", nameof(text));

    /// <summary>A remote branch from git's output; false when the text has no remote, no branch, or whitespace in either.</summary>
    public static bool TryParse(string? text, [MaybeNullWhen(false)] out RemoteBranch value)
    {
        string[] parts = text?.Split('/', 2) ?? [];
        value = parts is [string remote, string rest] && IsRemote(remote) && BranchName.TryParse(rest, out BranchName? branch) ? new RemoteBranch(remote, branch) : null;

        return value is not null;
    }

    public override string ToString() => Value;

    private static bool IsRemote(string remote) => remote.Length > 0 && !remote.StartsWith('-') && !remote.Any(char.IsWhiteSpace);
}
