using Source191.Cader.Application.Views;

namespace Source191.Cader.Application;

/// <summary>
/// The effects running in one console, so an equal effect (record equality: the same action on the same row) is not started
/// again while the first runs. Only the UI thread calls it, as an effect starts and as its settle runs inside <c>app.Invoke</c>,
/// so it needs no lock.
/// </summary>
public sealed class RunningEffects
{
    /// <summary>Records the effect as running and answers true, or answers false when an equal one is already running.</summary>
    private readonly List<ViewEffect> running = [];

    /// <summary>Records the effect as running and answers true, or answers false when an equal one is already running.</summary>
    public bool TryBegin(ViewEffect effect)
    {
        running.Add(effect);

        return true;
    }

    /// <summary>Releases the effect, so an equal one may start again.</summary>
    public void End(ViewEffect effect) => running.Remove(effect);
}
