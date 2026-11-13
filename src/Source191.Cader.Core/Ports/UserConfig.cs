using Source191.Cader.Domain;

namespace Source191.Cader.Ports;

/// <summary>
/// config.json as written: every member is optional and may be missing or null, as it is in a file from an earlier version or one edited by
/// hand, and <c>SettingsFile</c> resolves what is missing to its default. A file written by this version names all six.
/// </summary>
public sealed record UserConfig(IReadOnlyList<string>? RepositoryRoots = null, WorktreeBase? DefaultBase = null, string? WorktreeRoot = null, EditorSetting? Editor = null, bool? CheckForUpdates = null, LaunchMode? DefaultPlace = null)
{
    /// <summary>No config.json, or one that cannot be read: every member missing, so every setting reads as its default.</summary>
    public static readonly UserConfig Missing = new();

    public bool Equals(UserConfig? other)
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