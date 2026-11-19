using Source191.Cader.Domain;

namespace Source191.Cader.Application;

/// <summary>
/// Whether a tab's refresh is due, on its timer or for a change. The timer half: its interval, read afresh on every ask so
/// a saved setting applies on the next tick, is not off and has passed since the tab's last refresh started, whatever
/// started it (the console opening, the timer, a show, F5 or a change). The change half: a change is pending, half a
/// second has passed since the first pending one, so a burst lands as one refresh, and a second since the last start,
/// so a folder that changes steadily refreshes at most once a second and is never put off for good. Starting clears the
/// pending change, so a change during a refresh is followed by one more. The console asks and starts on the UI thread;
/// changes arrive on pool threads, so everything sits behind one lock.
/// </summary>
public sealed class RefreshSchedule(Func<RefreshInterval> interval, TimeProvider clock)
{
    private static readonly TimeSpan Gathering = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan Spacing = TimeSpan.FromSeconds(1);

    private readonly Lock gate = new();
    private DateTimeOffset lastStarted = clock.GetUtcNow();
    private bool changePending;
    private DateTimeOffset firstChange;

    /// <summary>Due on the timer or for a change.</summary>
    public bool IsDue()
    {
        RefreshInterval current = interval();
        lock (gate)
        {
            DateTimeOffset now = clock.GetUtcNow();

            return ChangeDue(now) || (!current.IsOff && now - lastStarted >= current.Every);
        }
    }

    /// <summary>Due for a change alone, as a hidden Worktrees tab asks, which never refreshes on its timer.</summary>
    public bool IsDueForChange()
    {
        lock (gate)
        {
            return ChangeDue(clock.GetUtcNow());
        }
    }

    /// <summary>The tab's refresh has just started, so the next timed one is a whole interval away and no change is pending.</summary>
    public void Started()
    {
        lock (gate)
        {
            lastStarted = clock.GetUtcNow();
            changePending = false;
        }
    }

    /// <summary>Something the tab shows has changed; called from any thread.</summary>
    public void Changed()
    {
        lock (gate)
        {
            if (!changePending)
            {
                changePending = true;
                firstChange = clock.GetUtcNow();
            }
        }
    }

    private bool ChangeDue(DateTimeOffset now) => changePending && now - firstChange >= Gathering && now - lastStarted >= Spacing;
}