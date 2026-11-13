using Source191.Cader.Application.Effects;

namespace Source191.Cader.Application.Actions;

/// <summary>The update tab was opened for this release; the console quits next.</summary>
public sealed record UpdateStartedOutcome(string Newest) : Outcome;