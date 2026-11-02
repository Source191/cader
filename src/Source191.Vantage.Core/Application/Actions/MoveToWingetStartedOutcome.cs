using Source191.Vantage.Application.Effects;

namespace Source191.Vantage.Application.Actions;

/// <summary>The move tab was opened for a dotnet tool install to install this release with winget; Vantage quits next.</summary>
public sealed record MoveToWingetStartedOutcome(string Newest) : Outcome;