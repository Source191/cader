namespace Source191.Cader.Application.Diagnostics;

public sealed record ProcessTimedOut(ChildCommand Command, TimeSpan Limit) : DiagnosticEvent;