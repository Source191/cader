namespace Source191.Cader.Application.Diagnostics;

/// <summary>A folder watch that could not start or lost changes, named only by its kind, never its folder.</summary>
public sealed record WatchTrouble(WatchKind Watch, WatchTroubleKind Trouble) : DiagnosticEvent;