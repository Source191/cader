namespace Source191.Cader.Application.Diagnostics;

public sealed record ProcessCount(ChildCommand Command, int ExitCode, DurationFigures Durations);