using Source191.Cader.Domain;

namespace Source191.Cader.Ports;

/// <summary>
/// config.json as written: every member may be missing, as it is in a file from an earlier version or one edited by
/// hand, and <c>SettingsFile</c> resolves what is missing to its default. A file written by this version names all six.
/// </summary>
public sealed record CaderConfig(IReadOnlyList<string>? RepositoryRoots, WorktreeBase? DefaultBase, string? WorktreeRoot, EditorSetting? Editor, bool? CheckForUpdates = null, LaunchMode? DefaultPlace = null)
{
    /// <summary>No config.json, or one that cannot be read: every member missing, so every setting reads as its default.</summary>
    public static readonly CaderConfig Missing = new(RepositoryRoots: null, DefaultBase: null, WorktreeRoot: null, Editor: null);

    public bool Equals(CaderConfig? other)
    {
        return other is not null
            && DefaultBase == other.DefaultBase
            && string.Equals(WorktreeRoot, other.WorktreeRoot, StringComparison.Ordinal)
            && Equals(Editor, other.Editor)
            && CheckForUpdates == other.CheckForUpdates
            && DefaultPlace == other.DefaultPlace
            && (RepositoryRoots ?? []).SequenceEqual(other.RepositoryRoots ?? []);
    }

    public override int GetHashCode() => HashCode.Combine(DefaultBase, WorktreeRoot, CheckForUpdates, DefaultPlace, RepositoryRoots?.Count);
}