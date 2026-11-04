namespace Source191.Cader.Application.Diagnostics;

public sealed record EffectRan(CodeName Effect, CodeName Outcome, TimeSpan Duration) : DiagnosticEvent;