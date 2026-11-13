using Source191.Cader.Domain;

namespace Source191.Cader.Application.Views;

/// <summary>A dialog effect: confirm deleting the session, then ask about its branch when that is not merged.</summary>
public sealed record DeleteSession(Session.Background Session) : ViewEffect, IDialogEffect;