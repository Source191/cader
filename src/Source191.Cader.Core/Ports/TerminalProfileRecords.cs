using Source191.Cader.Domain;

namespace Source191.Cader.Ports;

/// <summary>What Windows Terminal's state and settings files say about the profiles it has generated and listed.</summary>
public abstract record TerminalProfileRecords
{
    /// <summary>No state file or no settings file: Terminal has not run, or is not the Store package.</summary>
    public sealed record Missing : TerminalProfileRecords;

    /// <summary>A file could not be read; the reason is the system's, shown only to report what failed.</summary>
    public sealed record Unreadable(string Reason) : TerminalProfileRecords;

    /// <summary>
    /// The ids in the state file's <c>generatedProfiles</c>, and the id of every entry in the settings file's profile
    /// list, hidden or not.
    /// </summary>
    public sealed record Read(IReadOnlySet<TerminalProfileId> Generated, IReadOnlySet<TerminalProfileId> Listed) : TerminalProfileRecords;
}