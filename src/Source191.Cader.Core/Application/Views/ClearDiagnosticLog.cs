using Source191.Cader.Ports;

namespace Source191.Cader.Application.Views;

/// <summary>Delete the diagnostic log's files; <paramref name="Files"/> is what the tab last read, for the question, and the handler acts on the disc as it is.</summary>
public sealed record ClearDiagnosticLog(LogFiles Files) : ViewEffect;