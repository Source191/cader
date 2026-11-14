namespace Source191.Cader.Ports;

/// <summary>
/// Windows Terminal's own settings and state files, which Terminal owns: read to find a Cader profile it hides as
/// deleted. Implemented over System.IO at the boundary and faked in tests.
/// </summary>
public interface ITerminalSettings
{
    /// <summary>The settings file's full path.</summary>
    string Location { get; }

    /// <summary>Never throws: a file that cannot be read answers <see cref="TerminalProfileRecords.Unreadable"/>.</summary>
    Task<TerminalProfileRecords> ReadAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Adds each entry to the settings file's profile list, by insertion, leaving every other character as it was.
    /// Throws <see cref="IOException"/> or <see cref="UnauthorizedAccessException"/> when the file cannot be written, and
    /// an <see cref="IOException"/> naming the reason when it no longer holds a readable profile list.
    /// </summary>
    Task AddProfileEntriesAsync(IReadOnlyList<TerminalProfileEntry> entries, CancellationToken cancellationToken);
}
