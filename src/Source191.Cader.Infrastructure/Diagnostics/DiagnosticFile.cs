using Source191.Cader.Ports;

namespace Source191.Cader.Adapters.Diagnostics;

/// <summary>
/// The log file: each diagnostic line is appended as it is recorded (UTF-8, no byte order mark) while <paramref name="isOn"/>
/// answers true, which is asked on every line so the Diagnostic log setting applies at once. A file that cannot be written
/// is skipped, because a log must never take the console down.
/// </summary>
public sealed class DiagnosticFile(string path, Func<bool> isOn) : IDiagnosticSink
{
    public void Write(string line)
    {
        if (!isOn())
        {
            return;
        }

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
