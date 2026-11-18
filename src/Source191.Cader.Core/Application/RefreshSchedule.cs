using Source191.Cader.Domain;

namespace Source191.Cader.Application;

/// <summary>
/// Whether a tab's timed refresh is due: its interval, read afresh on every ask so a saved setting applies on the next
/// tick, is not off and has passed since the tab's last refresh started, whatever started it (the console opening, the
/// timer, a show or F5). The console asks and tells it only on the UI thread.
/// </summary>
public sealed class RefreshSchedule(Func<RefreshInterval> interval, TimeProvider clock)
{
    private DateTimeOffset lastStarted = clock.GetUtcNow();

    public bool IsDue() => interval() is { IsOff: false } current && clock.GetUtcNow() - lastStarted >= current.Every;

    /// <summary>The tab's refresh has just started, so the next timed one is a whole interval away.</summary>
    public void Started() => lastStarted = clock.GetUtcNow();
}
