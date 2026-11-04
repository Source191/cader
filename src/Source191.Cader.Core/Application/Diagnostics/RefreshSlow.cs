namespace Source191.Cader.Application.Diagnostics;

public sealed record RefreshSlow(RefreshPart Part, TimeSpan Duration) : DiagnosticEvent;