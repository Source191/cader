using Source191.Cader.Ports;
using System.Text;

namespace Source191.Cader.Adapters.Diagnostics;

/// <summary>
/// The log file: each diagnostic line is appended as it is recorded (UTF-8, no byte order mark) while <paramref name="isOn"/>
/// answers true, which is asked on every line so the Diagnostic log setting applies at once. When a line would take the
/// file past <see cref="MaximumBytes"/> it first becomes <c>.1</c> beside it, replacing an older one, so no more than two
/// files are kept. A file that cannot be written or moved is skipped, because a log must never take the console down.
/// </summary>
public sealed class DiagnosticFile(string path, Func<bool> isOn) : IDiagnosticSink
{
    public const long MaximumBytes = 10_485_760;

    public void Write(string line)
    {
        if (!isOn())
        {
            return;
        }

        string text = line + Environment.NewLine;
        try
        {
            RollOverWhenFull(Encoding.UTF8.GetByteCount(text));
            File.AppendAllText(path, text);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // The line stays in the in-memory log.
        }
    }

    private void RollOverWhenFull(int incomingBytes)
    {
        FileInfo current = new(path);
        if (!current.Exists || current.Length + incomingBytes <= MaximumBytes)
        {
            return;
        }

        try
        {
            File.Move(path, path + ".1", overwrite: true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // The line goes to the old file, and the next one tries the move again.
        }
    }
}
