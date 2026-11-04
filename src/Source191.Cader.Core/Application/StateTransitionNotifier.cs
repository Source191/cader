using Source191.Cader.Application.Views;
using Source191.Cader.Domain;

namespace Source191.Cader.Application;

/// <summary>
/// Rings the terminal bell when a background session starts waiting for input between two refreshes, so Windows
/// Terminal flags Cader's tab. Claude Code's own terminal-bell channel covers the tab a session is attached in.
/// </summary>
public sealed class StateTransitionNotifier(TextWriter terminal)
{
    public void Observe(IReadOnlyList<SessionRow> previous, IReadOnlyList<SessionRow> current)
    {
        if (previous.Count == 0)
        {
            return;
        }

        Dictionary<SessionId, DisplayStateKind> before = previous.ToDictionary(row => row.Session.SessionId, row => row.Session.State.Kind);
        bool someoneStartedWaiting = current
            .Where(row => row.Session is Session.Background { State.Kind: DisplayStateKind.NeedsInput })
            .Any(row => before.TryGetValue(row.Session.SessionId, out DisplayStateKind was) && was != DisplayStateKind.NeedsInput);
        if (someoneStartedWaiting)
        {
            terminal.Write('\a');
        }
    }
}