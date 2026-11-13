namespace Source191.Cader.Application.Diagnostics;

public sealed record UiThreadStalled(TimeSpan Lateness) : DiagnosticEvent;