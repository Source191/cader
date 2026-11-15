using Source191.Cader.Ports;

namespace Source191.Cader.Adapters.Diagnostics;

/// <summary>The diagnostic log's two files at <paramref name="path"/> and <see cref="DiagnosticFile.OlderPath"/> beside it, the same files <see cref="DiagnosticFile"/> writes.</summary>
public sealed class DiagnosticLogFiles(string path) : IDiagnosticLogFiles
{
    public LogFiles Read() => new(OnDisc(path), OnDisc(DiagnosticFile.OlderPath(path)));

    /// <summary>
    /// <c>cader.log</c> goes first: another Cader's rollover landing between the two deletes then finds no file to move, and one
    /// that landed just before is undone by the second delete.
    /// </summary>
    public LogFilesCleared Clear()
    {
        LogFilesCleared current = Delete(path);
        LogFilesCleared older = Delete(DiagnosticFile.OlderPath(path));

        return new LogFilesCleared(current.Removed + older.Removed, current.Bytes + older.Bytes, current.Kept + older.Kept);
    }

    /// <summary>Exists reads the file's facts once and answers false for a missing file or folder, or one it may not read, so Length never reads the disc again.</summary>
    private static LogFileOnDisc OnDisc(string file)
    {
        FileInfo info = new(file);

        return info.Exists ? new LogFileOnDisc.Written(file, info.Length) : new LogFileOnDisc.Absent(file);
    }

    /// <summary>A file open without shared deletion (an append in progress, or another program) cannot be deleted, so it is kept for the next clear.</summary>
    private static LogFilesCleared Delete(string file)
    {
        if (OnDisc(file) is not LogFileOnDisc.Written written)
        {
            return new LogFilesCleared(Removed: 0, Bytes: 0, Kept: 0);
        }

        try
        {
            File.Delete(file);
            return new LogFilesCleared(Removed: 1, written.Bytes, Kept: 0);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return new LogFilesCleared(Removed: 0, Bytes: 0, Kept: 1);
        }
    }
}