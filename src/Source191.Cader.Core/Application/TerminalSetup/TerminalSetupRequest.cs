namespace Source191.Cader.Application.TerminalSetup;

/// <summary>What <c>cader setup-terminal</c> was asked to do; an omitted sound means "use the default".</summary>
public sealed record TerminalSetupRequest(SoundArgument Sound, SoundArgument SessionSound, bool Yes, bool Remove);