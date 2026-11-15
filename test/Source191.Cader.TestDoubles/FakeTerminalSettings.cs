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

    /// <summary>The copy a write answers it kept.</summary>
    public string Backup => Location + ".2026-11-15T09-30-15.cader.backup";

    /// <summary>Every call's entries that were written, oldest first.</summary>
    public IReadOnlyList<IReadOnlyList<TerminalProfileEntry>> Added => added;

    /// <summary>When set, the next write throws it.</summary>
    public Exception? FailTheWriteWith { get; set; }

    /// <summary>What a write answers, written with <see cref="Backup"/> unless set; a refusal writes nothing.</summary>
    public TerminalSettingsEdit? Answer { get; set; }

    public Task<TerminalProfileRecords> ReadAsync(CancellationToken cancellationToken) => Task.FromResult(records);

    public Task<TerminalSettingsEdit> AddProfileEntriesAsync(IReadOnlyList<TerminalProfileEntry> entries, CancellationToken cancellationToken)
    {
        if (FailTheWriteWith is not null)
        {
            throw FailTheWriteWith;
        }

        TerminalSettingsEdit answer = Answer ?? new TerminalSettingsEdit.Written(Backup);
        if (answer is TerminalSettingsEdit.Written)
        {
            added.Add(entries);
        }

        return Task.FromResult(answer);
    }
}