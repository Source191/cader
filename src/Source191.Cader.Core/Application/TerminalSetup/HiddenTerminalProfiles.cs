using Source191.Cader.Ports;

namespace Source191.Cader.Application.TerminalSetup;

/// <summary>Which Cader profiles Windows Terminal is hiding as deleted.</summary>
public static class HiddenTerminalProfiles
{
    public static IReadOnlyList<TerminalProfileEntry> Of(TerminalProfileRecords records) => [];
}
