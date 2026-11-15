using System.Diagnostics.CodeAnalysis;

namespace Source191.Cader.Domain;

/// <summary>
/// A full commit id as git and the pull request hosts write it: 40 hex digits (SHA-1) or 64 (SHA-256), in either case.
/// It comes from a host's listing, so the rule keeps anything that is not one from reaching git as a revision.
/// </summary>
public sealed record CommitId : ITextValue<CommitId>
{
    private CommitId(string text)
    {
        if (!IsCommitId(text))
        {
            throw new ArgumentException($"'{text}' is not a commit id: it needs 40 or 64 hex digits.", nameof(text));
        }

        Value = text;
    }

    public static CommitId From(string text) => new(text);

    public string Value { get; }

    /// <summary>A commit id from outside the program (a pull request listing); false when it is not one.</summary>
    public static bool TryParse(string? text, [MaybeNullWhen(false)] out CommitId value)
    {
        value = text is not null && IsCommitId(text) ? CommitId.From(text) : null;
        return value is not null;
    }

    public override string ToString() => Value;

    private static bool IsCommitId(string text) => text.Length is 40 or 64 && text.All(char.IsAsciiHexDigit);
}
