using Source191.Cader.Ports;

namespace Source191.Cader.TestDoubles;

/// <summary>Windows Terminal's files in memory: answers the records it is given and records every entry added.</summary>
public sealed class FakeTerminalSettings(TerminalProfileRecords records) : ITerminalSettings
{
    private readonly List<IReadOnlyList<TerminalProfileEntry>> added = [];

    public FakeTerminalSettings()
        : this(new TerminalProfileRecords.Missing())
    {
    }

    public string Location => @"C:\Users\me\AppData\Local\Packages\Microsoft.WindowsTerminal_8wekyb3d8bbwe\LocalState\settings.json";

    /// <summary>Every call's entries, oldest first.</summary>
    public IReadOnlyList<IReadOnlyList<TerminalProfileEntry>> Added => added;

    /// <summary>When set, the next write throws it.</summary>
    public IOException? FailTheWriteWith { get; set; }

    public Task<TerminalProfileRecords> ReadAsync(CancellationToken cancellationToken) => Task.FromResult(records);

    public Task AddProfileEntriesAsync(IReadOnlyList<TerminalProfileEntry> entries, CancellationToken cancellationToken)
    {
        if (FailTheWriteWith is not null)
        {
            throw FailTheWriteWith;
        }

        added.Add(entries);

        return Task.CompletedTask;
    }
}