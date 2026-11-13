using Source191.Cader.Ports;

namespace Source191.Cader.Adapters.Diagnostics;

/// <summary>
/// The file named by <c>CADER_LOG</c>: each diagnostic line is appended as it is recorded (UTF-8, no byte order mark).
/// A file that cannot be written is skipped, because a log must never take the console down.
/// </summary>
public sealed class DiagnosticFile(string path) : IDiagnosticSink
{
    public void Write(string line)
    {
        try
        {
            File.AppendAllText(path, line + Environment.NewLine);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // The line stays in the in-memory log.
        }
    }
}