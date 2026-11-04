namespace Source191.Cader.Application.Diagnostics;

public sealed record TimedEvent(DateTimeOffset At, DiagnosticEvent Event);