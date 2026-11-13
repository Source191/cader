using Source191.Cader.Domain;

namespace Source191.Cader.Application.Views;

/// <summary>Rename by swapping the job for one resumed under the new name; the session id and conversation stay.</summary>
public sealed record RenameSession(Session.Background Session, SessionName NewName) : ViewEffect;