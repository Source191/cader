using Source191.Cader.Ports;

namespace Source191.Cader.TestDoubles;

/// <summary>Keeps the diagnostic log's two files as sizes in memory instead of <c>%LOCALAPPDATA%\Cader\cader.log</c> and <c>cader.log.1</c>; a file held open is kept by a clear.</summary>
public sealed class FakeDiagnosticLogFiles(string path) : IDiagnosticLogFiles
{
    private readonly FakeLogFile current = new(path);
    private readonly FakeLogFile older = new(path + ".1");

    /// <summary>The log at the tests' made-up state folder, <see cref="TestSettings.DiagnosticLogFile"/>.</summary>
    public FakeDiagnosticLogFiles()
        : this(TestSettings.DiagnosticLogFile)
    {
    }

    /// <summary>How many times <see cref="Clear"/> ran.</summary>
    public int Clears { get; private set; }

    public FakeDiagnosticLogFiles WithCurrent(long bytes)
    {
        current.Bytes = bytes;
        return this;
    }

    public FakeDiagnosticLogFiles WithOlder(long bytes)
    {
        older.Bytes = bytes;
        return this;
    }

    public FakeDiagnosticLogFiles HoldingCurrentOpen()
    {
        current.HeldOpen = true;
        return this;
    }

    public FakeDiagnosticLogFiles HoldingOlderOpen()
    {
        older.HeldOpen = true;
        return this;
    }

    public LogFiles Read() => new(current.OnDisc(), older.OnDisc());

    public LogFilesCleared Clear()
    {
        Clears++;
        LogFilesCleared first = current.Delete();
        LogFilesCleared second = older.Delete();

        return new LogFilesCleared(first.Removed + second.Removed, first.Bytes + second.Bytes, first.Kept + second.Kept);
    }

    private sealed class FakeLogFile(string path)
    {
        public long? Bytes { get; set; }

        public bool HeldOpen { get; set; }

        public LogFileOnDisc OnDisc() => Bytes is long bytes ? new LogFileOnDisc.Written(path, bytes) : new LogFileOnDisc.Absent(path);

        public LogFilesCleared Delete()
        {
            if (Bytes is not long bytes)
            {
                return new LogFilesCleared(Removed: 0, Bytes: 0, Kept: 0);
            }

            if (HeldOpen)
            {
                return new LogFilesCleared(Removed: 0, Bytes: 0, Kept: 1);
            }

            Bytes = null;
            return new LogFilesCleared(Removed: 1, bytes, Kept: 0);
        }
    }
}