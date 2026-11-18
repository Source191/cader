using Microsoft.Extensions.Time.Testing;
using Source191.Cader.Application;
using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Application;

public sealed class RefreshScheduleShould
{
    private readonly FakeTimeProvider clock = new(Fixtures.Noon);

    [Fact]
    public void BeDueOnceItsIntervalHasPassedSinceItStarted()
    {
        RefreshSchedule schedule = new(() => RefreshInterval.From(5), clock);

        schedule.Started();
        clock.Advance(TimeSpan.FromSeconds(4.75));
        schedule.IsDue().ShouldBeFalse();

        clock.Advance(TimeSpan.FromSeconds(0.25));
        schedule.IsDue().ShouldBeTrue();
    }

    [Fact]
    public void CountFromWhenItWasMadeUntilItFirstStarts()
    {
        RefreshSchedule schedule = new(() => RefreshInterval.From(5), clock);

        clock.Advance(TimeSpan.FromSeconds(4));
        schedule.IsDue().ShouldBeFalse();

        clock.Advance(TimeSpan.FromSeconds(1));
        schedule.IsDue().ShouldBeTrue();
    }

    [Fact]
    public void NeverBeDueWhileOff()
    {
        RefreshSchedule schedule = new(() => RefreshInterval.Off, clock);

        schedule.Started();
        clock.Advance(TimeSpan.FromMinutes(10));

        schedule.IsDue().ShouldBeFalse();
    }

    [Fact]
    public void PickUpANewIntervalOnTheNextAsk()
    {
        RefreshInterval interval = RefreshInterval.From(5);
        RefreshSchedule schedule = new(() => interval, clock);
        schedule.Started();
        clock.Advance(TimeSpan.FromSeconds(10));
        schedule.IsDue().ShouldBeTrue();

        interval = RefreshInterval.From(30);
        schedule.IsDue().ShouldBeFalse();

        clock.Advance(TimeSpan.FromSeconds(20));
        schedule.IsDue().ShouldBeTrue();

        interval = RefreshInterval.Off;
        schedule.IsDue().ShouldBeFalse();
    }

    [Fact]
    public void PushTheNextDueTimeBackAWholeIntervalWhenStartedForAnyCause()
    {
        RefreshSchedule schedule = new(() => RefreshInterval.From(5), clock);
        schedule.Started();

        clock.Advance(TimeSpan.FromSeconds(4));
        schedule.Started();
        clock.Advance(TimeSpan.FromSeconds(4));
        schedule.IsDue().ShouldBeFalse();

        clock.Advance(TimeSpan.FromSeconds(1));
        schedule.IsDue().ShouldBeTrue();
    }
}
