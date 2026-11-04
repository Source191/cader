using Source191.Cader.Domain;

namespace Source191.Cader.Application.Views;

public sealed record StopSession(Session.Background Session) : ViewEffect;