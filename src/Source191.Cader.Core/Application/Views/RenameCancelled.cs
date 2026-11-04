using Source191.Cader.Domain;

namespace Source191.Cader.Application.Views;

/// <summary>The new name dialog was closed without an answer.</summary>
public sealed record RenameCancelled(Session.Background Session) : ViewInput;