using Source191.Cader.Domain;

namespace Source191.Cader.Application;

/// <summary>Whether a tab's timed refresh is due.</summary>
public sealed class RefreshSchedule(Func<RefreshInterval> interval, TimeProvider clock)
{
    private DateTimeOffset lastStarted = clock.GetUtcNow();

    public bool IsDue() => interval().IsOff && lastStarted > DateTimeOffset.MaxValue;

    public void Started() => lastStarted = DateTimeOffset.MinValue;
}
