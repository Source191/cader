using Source191.Cader.Application.Effects;

namespace Source191.Cader.Application.Actions;

/// <summary>Another program held the clipboard, so nothing was copied.</summary>
public sealed record ClipboardBusy : Outcome;