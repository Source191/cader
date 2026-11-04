using Source191.Cader.Domain;

namespace Source191.Cader.Application.Views;

/// <summary>The TUI asks for a new name, then raises <see cref="RenameConfirmed"/> with what was typed, or <see cref="RenameCancelled"/>.</summary>
public sealed record AskForNewName(Session.Background Session) : ViewEffect, IDialogEffect;