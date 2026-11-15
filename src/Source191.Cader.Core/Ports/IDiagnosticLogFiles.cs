namespace Source191.Cader.Ports;

/// <summary>
/// The diagnostic log's two files, <c>cader.log</c> and the <c>cader.log.1</c> it rolls over to, at the one path the adapter
/// was built with. It takes no path, so nothing a person typed or a listing named can choose a file it touches.
/// </summary>
public interface IDiagnosticLogFiles
{
    /// <summary>Each file's size, or that it is absent; never throws.</summary>
    LogFiles Read();

    /// <summary>Deletes <c>cader.log</c>, then <c>cader.log.1</c>, and nothing else; a missing file is skipped and one another program holds open is kept and counted. Never throws.</summary>
    LogFilesCleared Clear();
}