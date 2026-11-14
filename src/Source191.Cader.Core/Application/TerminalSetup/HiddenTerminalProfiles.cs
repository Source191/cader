using Source191.Cader.Ports;

namespace Source191.Cader.Application.TerminalSetup;

/// <summary>
/// Which Cader profiles Windows Terminal is hiding as deleted: those its state file lists as generated with no entry of
/// their id in its settings file. An entry that says <c>"hidden": true</c> is listed, so a profile the user hid is never
/// answered. The setup command and the Config tab both ask here, so they cannot disagree.
/// </summary>
public static class HiddenTerminalProfiles
{
    public static IReadOnlyList<TerminalProfileEntry> Of(TerminalProfileRecords records)
    {
        return records is TerminalProfileRecords.Read read
            ? [.. TerminalProfiles.Entries.Where(entry => read.Generated.Contains(entry.Id) && !read.Listed.Contains(entry.Id))]
            : [];
    }
}