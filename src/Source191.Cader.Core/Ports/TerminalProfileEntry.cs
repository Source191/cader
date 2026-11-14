using Source191.Cader.Domain;

namespace Source191.Cader.Ports;

/// <summary>A profile's entry in Windows Terminal's settings file, as setup adds it to show a hidden profile again.</summary>
public sealed record TerminalProfileEntry(TerminalProfileId Id, string Name, string Source);
