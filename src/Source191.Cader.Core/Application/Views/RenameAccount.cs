using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.Application.Views;

/// <summary>Gives a remembered account a new label, checked by the label rules and against the other accounts.</summary>
public sealed record RenameAccount(KnownAccount Account, AccountLabel NewLabel) : ViewEffect;