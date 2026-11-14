using Source191.Cader.Ports;

namespace Source191.Cader.Adapters.Files;

/// <summary>The file system as <see cref="IFolders"/> sees it.</summary>
public sealed class Folders : IFolders, IFolderDeleter
{
    private static readonly EnumerationOptions EveryFile = new()
    {
        RecurseSubdirectories = true,
        IgnoreInaccessible = true,
        AttributesToSkip = FileAttributes.ReparsePoint,
    };

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public bool FileExists(string path) => File.Exists(path);

    public IEnumerable<string> Directories(string path) => Directory.EnumerateDirectories(path);

    public IEnumerable<string> Files(string path) => Directory.EnumerateFiles(path, "*", EveryFile);

    public IEnumerable<string> FilesIn(string path) => Directory.Exists(path) ? Directory.EnumerateFiles(path) : [];

    /// <summary>Whether the folder holds nothing; a folder that has gone since it was listed, or cannot be read, is not empty, so one such folder never fails a scan.</summary>
    public bool IsEmpty(string path)
    {
        try
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }
        catch (Exception failure) when (failure is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    public IEnumerable<string> Lines(string file) => File.ReadLines(file);

    public void DeleteTree(string path) => FolderDeletion.DeleteTree(path);
}