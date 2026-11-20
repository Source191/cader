namespace Source191.Cader.Application;

/// <summary>
/// Whether a session started from the console is being launched: each launch takes a hold for its whole length, and the
/// refresh schedules answer not due while any is open. A hold releases once, however often it is disposed. Launches run
/// on pool threads and the console asks on the UI thread, so the count sits behind a lock.
/// </summary>
public sealed class LaunchHold
{
    private readonly Lock gate = new();
    private int open;

    public bool IsHeld
    {
        get
        {
            lock (gate)
            {
                return open > 0;
            }
        }
    }

    /// <summary>Takes a hold until the answer is disposed.</summary>
    public IDisposable Hold()
    {
        lock (gate)
        {
            open++;
        }

        return new Release(this);
    }

    private void End()
    {
        lock (gate)
        {
            open--;
        }
    }

    private sealed class Release(LaunchHold hold) : IDisposable
    {
        private int released;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref released, 1) == 0)
            {
                hold.End();
            }
        }
    }
}