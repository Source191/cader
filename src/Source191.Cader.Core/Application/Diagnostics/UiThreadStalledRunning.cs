namespace Source191.Cader.Application.Diagnostics;

/// <summary>A freeze of the UI thread, with the longest piece of marked work since the beat before it and how long that piece ran.</summary>
public sealed record UiThreadStalledRunning(TimeSpan Lateness, CodeName Work, TimeSpan Took) : DiagnosticEvent;