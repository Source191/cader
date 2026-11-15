namespace Source191.Cader.Application.Diagnostics;

/// <summary>
/// Watches the UI thread: the console calls <see cref="Beat"/> from a timeout every <see cref="Interval"/>, and each
/// beat is judged by how late it ran after the one before. More than <see cref="Stall"/> late is counted; more than
/// <see cref="Freeze"/> is also recorded, so a sustained slowdown shows in the counts without pushing every other event
/// out of the log. A gap longer than <see cref="Sleep"/> is the machine sleeping and only sets a new baseline, as the
/// first beat does. Terminal.Gui keeps running timeouts under a modal dialog, so a dialog needs no restart. Work the
/// screen runs through <see cref="Mark"/> is timed since the later of its start and the last beat, and a freeze names the
/// longest piece marked since the beat before it; marks may nest, and the outer piece includes the inner ones.
/// Called on the UI thread only.
/// </summary>
public sealed class Heartbeat(TimeProvider clock, StallCounts stalls, DiagnosticLog log)
{
    public static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(250);

    /// <summary>About where a key press starts to feel sluggish.</summary>
    public static readonly TimeSpan Stall = TimeSpan.FromMilliseconds(100);

    /// <summary>A freeze anyone notices.</summary>
    public static readonly TimeSpan Freeze = TimeSpan.FromMilliseconds(500);

    public static readonly TimeSpan Sleep = TimeSpan.FromSeconds(30);

    private long previous;
    private bool beaten;
    private (CodeName Work, TimeSpan Took)? longest;

    /// <summary>Runs a piece of UI-thread work and keeps it when it is the longest marked since the last beat, so a freeze can name it.</summary>
    public void Mark(CodeName work, Action run)
    {
        long started = clock.GetTimestamp();
        try
        {
            run();
        }
        finally
        {
            // A dialog opened by the work runs its own loop and the heartbeat beats in it, so only the time since the last beat is the work's.
            TimeSpan took = clock.GetElapsedTime(Math.Max(started, previous));
            if (longest is not { } kept || took > kept.Took)
            {
                longest = (work, took);
            }
        }
    }

    public void Beat()
    {
        long now = clock.GetTimestamp();
        if (beaten)
        {
            Judge(clock.GetElapsedTime(previous, now));
        }

        longest = null;
        previous = now;
        beaten = true;
    }

    private void Judge(TimeSpan gap)
    {
        TimeSpan lateness = gap - Interval;
        if (gap > Sleep || lateness <= Stall)
        {
            return;
        }

        stalls.Add(lateness);
        if (lateness > Freeze)
        {
            log.Record(longest is { } work ? new UiThreadStalledRunning(lateness, work.Work, work.Took) : new UiThreadStalled(lateness));
        }
    }
}