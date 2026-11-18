using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.TestDoubles;

/// <summary>Holds each watch started and raises changes on demand, as a folder's watcher would on a pool thread; a refused folder throws as Windows would.</summary>
public sealed class FakeFolderChanges : IFolderChanges
{
    private readonly List<Watching> watches = [];

    /// <summary>Folders whose watch Windows refuses.</summary>
    public HashSet<FolderPath> Refused { get; } = [];

    /// <summary>Every watch started, disposed or not, in order.</summary>
    public IReadOnlyList<FolderPath> Started => [.. watches.Select(watch => watch.Folder)];

    /// <summary>The folders watched now, each with whether its subfolders are.</summary>
    public IReadOnlyList<(FolderPath Folder, bool Subfolders)> Live => [.. watches.Where(watch => !watch.Disposed).Select(watch => (watch.Folder, watch.Subfolders))];

    public IDisposable Watch(FolderPath folder, bool subfolders, Action<FolderChange> changed)
    {
        if (Refused.Contains(folder))
        {
            throw new FolderWatchException(new UnauthorizedAccessException());
        }

        Watching watch = new(folder, subfolders, changed);
        watches.Add(watch);

        return watch;
    }

    /// <summary>Reports the change to every live watch of the folder.</summary>
    public void Raise(FolderPath folder, FolderChange change)
    {
        foreach (Watching watch in watches.Where(watch => !watch.Disposed && watch.Folder == folder).ToList())
        {
            watch.Changed(change);
        }
    }

    private sealed class Watching(FolderPath folder, bool subfolders, Action<FolderChange> changed) : IDisposable
    {
        public FolderPath Folder { get; } = folder;

        public bool Subfolders { get; } = subfolders;

        public Action<FolderChange> Changed { get; } = changed;

        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Disposed = true;
        }
    }
}