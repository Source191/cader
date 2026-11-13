namespace Source191.Cader.Application.Diagnostics;

public sealed record ProcessSlow(ChildCommand Command, int ExitCode, TimeSpan Duration) : DiagnosticEvent;