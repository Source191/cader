using Source191.Cader.Domain;

namespace Source191.Cader.Application.Views;

public sealed record RenameConfirmed(Session.Background Session, string Name) : ViewInput;