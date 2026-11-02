using Source191.Vantage.Application.Effects;

namespace Source191.Vantage.Application.Actions;

/// <summary>The update or move needs a program that is not on the PATH (winget, or dotnet for the move), so no tab was opened and Vantage stays.</summary>
public sealed record UpdaterMissingOutcome(string Program, string Newest) : Outcome;