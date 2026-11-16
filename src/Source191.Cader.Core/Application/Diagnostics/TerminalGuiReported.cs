namespace Source191.Cader.Application.Diagnostics;

/// <summary>
/// A Terminal.Gui warning or error, with none of Terminal.Gui's words: its messages can quote input it failed to parse. The
/// raiser is the method that logged it, read from the call stack, and is null when no frame is left to name it.
/// </summary>
public sealed record TerminalGuiReported(DiagnosticLevel Level, CodeName? Raiser, ExceptionSummary? Failure) : DiagnosticEvent;