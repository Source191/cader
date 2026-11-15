using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application.Actions;

/// <summary>Folders that answer from a fake and count how often a folder's files were walked, for a fact that a read is never made.</summary>
internal sealed class FoldersThatCountWalks(FakeFolders inner) : IFolders
{
    public int Walks { get; private set; }

    public bool DirectoryExists(string path) => inner.DirectoryExists(path);

    public bool FileExists(string path) => inner.FileExists(path);

    public IEnumerable<string> Directories(string path) => inner.Directories(path);

    public IEnumerable<string> Files(string path)
    {
        Walks++;

        return inner.Files(path);
    }

    public IEnumerable<string> FilesIn(string path) => inner.FilesIn(path);

    public bool IsEmpty(string path) => inner.IsEmpty(path);

    public IEnumerable<string> Lines(string file) => inner.Lines(file);
}