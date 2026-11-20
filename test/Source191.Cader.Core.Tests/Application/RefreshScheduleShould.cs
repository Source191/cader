using Microsoft.Extensions.Time.Testing;
using Source191.Cader.Application;
using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Application;

public sealed class RefreshScheduleShould
{
    private readonly FakeTimeProvider clock = new(Fixtures.Noon);
    private readonly LaunchHold hold = new();

    [Fact]
    public void BeDueOnceItsIntervalHasPassedSinceItStarted()
    {
        RefreshSchedule schedule = new(() => RefreshInterval.From(5), clock, hold);

        schedule.Started();
        clock.Advance(TimeSpan.FromSeconds(4.75));
        schedule.IsDue().ShouldBeFalse();

        clock.Advance(TimeSpan.FromSeconds(0.25));
        schedule.IsDue().ShouldBeTrue();
    }

    [Fact]
    public void CountFromWhenItWasMadeUntilItFirstStarts()
    {
        RefreshSchedule schedule = new(() => RefreshInterval.From(5), clock, hold);

        clock.Advance(TimeSpan.FromSeconds(4));
        schedule.IsDue().ShouldBeFalse();

        clock.Advance(TimeSpan.FromSeconds(1));
        schedule.IsDue().ShouldBeTrue();
    }

    [Fact]
    public void NeverBeDueWhileOff()
    {
        RefreshSchedule schedule = new(() => RefreshInterval.Off, clock, hold);

        schedule.Started();
        clock.Advance(TimeSpan.FromMinutes(10));

        schedule.IsDue().ShouldBeFalse();
    }

    [Fact]
    public void PickUpANewIntervalOnTheNextAsk()
    {
        RefreshInterval interval = RefreshInterval.From(5);
        RefreshSchedule schedule = new(() => interval, clock, hold);
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
    public void BeDueHalfASecondAfterAChangeEvenWhileOff()
    {
        RefreshSchedule schedule = new(() => RefreshInterval.Off, clock, hold);
        schedule.Started();
        clock.Advance(TimeSpan.FromSeconds(2));

        schedule.Changed();
        clock.Advance(TimeSpan.FromMilliseconds(450));
        schedule.IsDue().ShouldBeFalse();
        schedule.IsDueForChange().ShouldBeFalse();

        clock.Advance(TimeSpan.FromMilliseconds(50));
        schedule.IsDue().ShouldBeTrue();
        schedule.IsDueForChange().ShouldBeTrue();
    }

    [Fact]
    public void WaitASecondAfterTheLastStartBeforeRefreshingForAChange()
    {
        RefreshSchedule schedule = new(() => RefreshInterval.Off, clock, hold);
        schedule.Started();

        schedule.Changed();
        clock.Advance(TimeSpan.FromMilliseconds(950));
        schedule.IsDueForChange().ShouldBeFalse();

        clock.Advance(TimeSpan.FromMilliseconds(50));
        schedule.IsDueForChange().ShouldBeTrue();
    }

    [Fact]
    public void CountFromTheFirstChangeNotTheLatest()
    {
        RefreshSchedule schedule = new(() => RefreshInterval.Off, clock, hold);
        clock.Advance(TimeSpan.FromSeconds(2));

        schedule.Changed();
        clock.Advance(TimeSpan.FromMilliseconds(400));
        schedule.Changed();
        clock.Advance(TimeSpan.FromMilliseconds(100));

        schedule.IsDueForChange().ShouldBeTrue();
    }

    [Fact]
    public void ForgetAChangeOnceStartedAndHearTheNextOne()
    {
        RefreshSchedule schedule = new(() => RefreshInterval.Off, clock, hold);
        schedule.Changed();
        clock.Advance(TimeSpan.FromSeconds(2));
        schedule.IsDueForChange().ShouldBeTrue();

        schedule.Started();
        clock.Advance(TimeSpan.FromSeconds(5));
        schedule.IsDue().ShouldBeFalse();

        schedule.Changed();
        clock.Advance(TimeSpan.FromMilliseconds(500));
        schedule.IsDueForChange().ShouldBeTrue();
    }

    [Fact]
    public void NotCountATimedRefreshAsAChange()
    {
        RefreshSchedule schedule = new(() => RefreshInterval.From(5), clock, hold);
        schedule.Started();
        clock.Advance(TimeSpan.FromSeconds(10));

        schedule.IsDue().ShouldBeTrue();
        schedule.IsDueForChange().ShouldBeFalse();
    }

    [Fact]
    public void RefreshABusyFolderAtMostOnceASecondAndAfterItsLastChange()
    {
        RefreshSchedule schedule = new(() => RefreshInterval.Off, clock, hold);
        schedule.Started();
        List<DateTimeOffset> starts = [];
        DateTimeOffset lastChange = clock.GetUtcNow();
        TimeSpan tick = TimeSpan.FromMilliseconds(50);

        for (TimeSpan elapsed = TimeSpan.Zero; elapsed < TimeSpan.FromSeconds(63); elapsed += tick)
        {
            if (elapsed < TimeSpan.FromMinutes(1) && elapsed.TotalMilliseconds % 200 == 0)
            {
                schedule.Changed();
                lastChange = clock.GetUtcNow();
            }

            if (elapsed.TotalMilliseconds % 250 == 0 && schedule.IsDue())
            {
                starts.Add(clock.GetUtcNow());
                schedule.Started();
            }

            clock.Advance(tick);
        }

        starts.Zip(starts.Skip(1), (earlier, later) => later - earlier).ShouldAllBe(gap => gap >= TimeSpan.FromSeconds(1));
        starts.Count.ShouldBeGreaterThan(50);
        starts[^1].ShouldBeGreaterThan(lastChange);
    }

    [Fact]
    public void PushTheNextDueTimeBackAWholeIntervalWhenStartedForAnyCause()
    {
        RefreshSchedule schedule = new(() => RefreshInterval.From(5), clock, hold);
        schedule.Started();

        clock.Advance(TimeSpan.FromSeconds(4));
        schedule.Started();
        clock.Advance(TimeSpan.FromSeconds(4));
        schedule.IsDue().ShouldBeFalse();

        clock.Advance(TimeSpan.FromSeconds(1));
        schedule.IsDue().ShouldBeTrue();
    }

    [Fact]
    public void NotBeDueWhileHeld()
    {
        RefreshSchedule schedule = new(() => RefreshInterval.From(5), clock, hold);
        schedule.Started();

        using (hold.Hold())
        {
            schedule.Changed();
            clock.Advance(TimeSpan.FromSeconds(9));

            schedule.IsDue().ShouldBeFalse("neither the timer nor the change starts a refresh during a launch");
        }
    }

    [Fact]
    public void NotBeDueForAChangeWhileHeld()
    {
        RefreshSchedule schedule = new(() => RefreshInterval.Off, clock, hold);
        schedule.Started();

        using (hold.Hold())
        {
            schedule.Changed();
            clock.Advance(TimeSpan.FromSeconds(2));

            schedule.IsDueForChange().ShouldBeFalse();
        }
    }

    [Fact]
    public void KeepAChangeSeenWhileHeld()
    {
        RefreshSchedule schedule = new(() => RefreshInterval.Off, clock, hold);
        schedule.Started();

        using (hold.Hold())
        {
            clock.Advance(TimeSpan.FromSeconds(2));
            schedule.Changed();
            clock.Advance(TimeSpan.FromSeconds(9));
        }

        schedule.IsDueForChange().ShouldBeTrue();
        schedule.IsDue().ShouldBeTrue();
    }

    [Fact]
    public void BeDueOnceTheHoldEnds()
    {
        RefreshSchedule schedule = new(() => RefreshInterval.From(5), clock, hold);
        schedule.Started();
        IDisposable launch = hold.Hold();
        clock.Advance(TimeSpan.FromSeconds(9));
        schedule.IsDue().ShouldBeFalse();

        launch.Dispose();

        schedule.IsDue().ShouldBeTrue();
        schedule.Started();
        schedule.IsDue().ShouldBeFalse("one refresh after the launch, then the timer counts again");
    }
}