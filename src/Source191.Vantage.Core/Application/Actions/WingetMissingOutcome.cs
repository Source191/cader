using Source191.Vantage.Application.Effects;

namespace Source191.Vantage.Application.Actions;

/// <summary>winget is not on the PATH, so no update tab was opened and Vantage stays; the release page has the zip.</summary>
public sealed record WingetMissingOutcome(string Newest) : Outcome;