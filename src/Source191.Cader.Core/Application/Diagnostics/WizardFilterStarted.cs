using Source191.Cader.Domain;

namespace Source191.Cader.Application.Diagnostics;

/// <summary>
/// The wizard's first filter change: how long after opening it came, the kind of the filter's first character, how many
/// characters the filter then held, and whether the list had been filled before it. Never the characters themselves.
/// </summary>
public sealed record WizardFilterStarted(TimeSpan After, CharacterKind FirstCharacter, int Characters, bool ListFilled) : DiagnosticEvent;
