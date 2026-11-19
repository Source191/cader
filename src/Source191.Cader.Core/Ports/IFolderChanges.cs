using Source191.Cader.Domain;

namespace Source191.Cader.Ports;

/// <summary>
/// Notices names changing in a folder without reading anything: each change reaches <c>changed</c> on a pool thread, as
/// its path relative to the folder and what happened, and an overflow as <see cref="FolderChangeKind.Overflowed"/>.
/// </summary>
public interface IFolderChanges
{
    /// <summary>
    /// Watches the folder, and its subfolders when asked, until the answer is disposed. A folder that does not exist
    /// answers a watch that reports nothing. Throws <see cref="FolderWatchException"/> when Windows refuses the watch.
    /// </summary>
    IDisposable Watch(FolderPath folder, bool subfolders, Action<FolderChange> changed);
}