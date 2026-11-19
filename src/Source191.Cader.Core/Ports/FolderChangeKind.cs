namespace Source191.Cader.Ports;

/// <summary>What happened to a name in a watched folder, or that the watch lost track of what happened.</summary>
public enum FolderChangeKind
{
    Created,
    Changed,
    Deleted,

    /// <summary>Renamed to the change's path.</summary>
    Renamed,

    /// <summary>The watch's buffer filled and changes were lost; the change has no path.</summary>
    Overflowed,
}