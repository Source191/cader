using Source191.Cader.Domain;

namespace Source191.Cader.Application.Views;

/// <summary>Resume these jobs under the supervisor; raised by R directly for every shown stopped or failed session, with no checklist.</summary>
public sealed record ResumeSessions(IReadOnlyList<Session.Background> Sessions) : ViewEffect;