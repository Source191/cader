using Source191.Cader.Domain;
using Source191.Cader.Ports;
using System.ComponentModel;

namespace Source191.Cader.Adapters.Files;

/// <summary>
/// Folder changes over one <see cref="FileSystemWatcher"/> per watch, for file names, folder names and writes, never
/// opening a file. Its buffer is 64 KB, the most Windows allows; any error it raises (a full buffer, or the folder going)
/// is reported as an overflow, so the caller refreshes rather than trusting what it missed.
/// </summary>
public sealed class FolderChanges : IFolderChanges
{
    private const int MostBuffer = 64 * 1024;

    public IDisposable Watch(FolderPath folder, bool subfolders, Action<FolderChange> changed)
    {
        if (!Directory.Exists(folder.Value))
        {
            return new NoWatch();
        }

        FileSystemWatcher watcher = new()
        {
            IncludeSubdirectories = subfolders,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite,
            InternalBufferSize = MostBuffer,
        };
        try
        {
            watcher.Path = folder.Value;
            watcher.Created += (_, change) => changed(new FolderChange(change.Name ?? string.Empty, FolderChangeKind.Created));
            watcher.Changed += (_, change) => changed(new FolderChange(change.Name ?? string.Empty, FolderChangeKind.Changed));
            watcher.Deleted += (_, change) => changed(new FolderChange(change.Name ?? string.Empty, FolderChangeKind.Deleted));
            watcher.Renamed += (_, change) => changed(new FolderChange(change.Name ?? string.Empty, FolderChangeKind.Renamed));
            watcher.Error += (_, _) => changed(new FolderChange(string.Empty, FolderChangeKind.Overflowed));
            watcher.EnableRaisingEvents = true;

            return watcher;
        }
        // A folder deleted since the check above is refused with ArgumentException; the cause is never shown.
        catch (Exception refused) when (refused is ArgumentException or IOException or UnauthorizedAccessException or Win32Exception)
        {
            watcher.Dispose();
            throw new FolderWatchException(refused);
        }
    }

    private sealed class NoWatch : IDisposable
    {
        public void Dispose()
        {
            // Nothing is watched, so nothing is released.
        }
    }
}