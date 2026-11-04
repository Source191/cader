using Source191.Cader.Application.Effects;

namespace Source191.Cader.Application.Actions;

/// <summary>The update check is now on or off, and saved.</summary>
public sealed record ToggleUpdateCheckOutcome(bool On) : Outcome;