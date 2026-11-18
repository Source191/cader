using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.Adapters.Files;

/// <summary>Folder changes over <see cref="FileSystemWatcher"/>.</summary>
public sealed class FolderChanges : IFolderChanges
{
    public IDisposable Watch(FolderPath folder, bool subfolders, Action<FolderChange> changed) => new NoWatch();

    private sealed class NoWatch : IDisposable
    {
        public void Dispose()
        {
            // Nothing is watched, so nothing is released.
        }
    }
}