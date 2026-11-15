using Source191.Cader.Ports;

namespace Source191.Cader.Adapters.Diagnostics;

/// <summary>The diagnostic log's two files at <paramref name="path"/> and <see cref="DiagnosticFile.OlderPath"/> beside it, the same files <see cref="DiagnosticFile"/> writes.</summary>
public sealed class DiagnosticLogFiles(string path) : IDiagnosticLogFiles
{
    public LogFiles Read() => new(OnDisc(path), OnDisc(DiagnosticFile.OlderPath(path)));

    /// <summary>Exists reads the file's facts once and answers false for a missing file or folder, or one it may not read, so Length never reads the disc again.</summary>
    private static LogFileOnDisc OnDisc(string file)
    {
        FileInfo info = new(file);

        return info.Exists ? new LogFileOnDisc.Written(file, info.Length) : new LogFileOnDisc.Absent(file);
    }
}