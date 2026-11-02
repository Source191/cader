using Source191.Vantage.Application.Effects;
using Source191.Vantage.Domain;

namespace Source191.Vantage.Application.Actions;

/// <summary>
/// Which tabs were closed, which sessions were ended without a tab to close, which were left alone for having messages,
/// and which were left alone because their listed process is no longer theirs.
/// </summary>
public sealed record CloseEmptyTerminalsOutcome(IReadOnlyList<ListedName> Closed, IReadOnlyList<ListedName> Ended, IReadOnlyList<SkippedSession> Skipped, IReadOnlyList<ListedName> Gone) : Outcome;