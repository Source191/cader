namespace Source191.Cader.Ports;

/// <summary>What is on the disc of the diagnostic log: <c>cader.log</c>, and the older <c>cader.log.1</c> a rollover left.</summary>
public sealed record LogFiles(LogFileOnDisc Current, LogFileOnDisc Older);