namespace Source191.Cader.Application.Diagnostics;

public sealed record ProcessFailedToStart(ChildCommand Command, ExceptionSummary Failure) : DiagnosticEvent;