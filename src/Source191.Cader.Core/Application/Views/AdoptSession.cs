using Source191.Cader.Domain;

namespace Source191.Cader.Application.Views;

/// <summary>Move a session started as plain claude in another tab under the supervisor, so Cader can manage it.</summary>
public sealed record AdoptSession(Session.Interactive Session) : ViewEffect;