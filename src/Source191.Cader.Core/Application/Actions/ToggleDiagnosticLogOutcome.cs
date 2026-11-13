using Source191.Cader.Application.Effects;

namespace Source191.Cader.Application.Actions;

/// <summary>The diagnostic log is now on or off, saved, and <paramref name="File"/> is where it is written while on.</summary>
public sealed record ToggleDiagnosticLogOutcome(bool On, string File) : Outcome;
