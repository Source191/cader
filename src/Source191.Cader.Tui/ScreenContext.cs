using Source191.Cader.Application;
using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Application.Views;

namespace Source191.Cader.Tui;

/// <summary>
/// What the console screen is handed from the tool's <c>ConsoleComposition</c>: the tab title, the settings row for a width (asked for on
/// every redraw so a saved setting shows at once), whether a newer release is known, the announcement of a newly found one (taken once, on the next redraw), the clipboard the copy key writes to
/// the diagnostic log that loads, background failures and dialogs are recorded in, the heartbeat that watches the UI
/// thread, and the effects running in any tab, so the same action on the same row is not started twice from any of them.
/// </summary>
public sealed record ScreenContext(string Title, Func<int, IReadOnlyList<IReadOnlyList<TextPart>>> SettingsRows, Func<bool> UpdateKnown, Func<StatusMessage> TakeAnnouncement, ScreenClipboard Clipboard, DiagnosticLog Diagnostics, Heartbeat Heartbeat, RunningEffects RunningEffects);