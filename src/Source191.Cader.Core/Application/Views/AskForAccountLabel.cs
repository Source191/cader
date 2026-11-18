using Source191.Cader.Ports;

namespace Source191.Cader.Application.Views;

/// <summary>The TUI asks for an account's new label, then raises <see cref="AccountLabelConfirmed"/> with what was typed, or <see cref="AccountLabelCancelled"/>.</summary>
public sealed record AskForAccountLabel(KnownAccount Account) : ViewEffect, IDialogEffect;
