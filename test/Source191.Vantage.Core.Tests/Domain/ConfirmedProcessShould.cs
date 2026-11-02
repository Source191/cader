using Source191.Vantage.Domain;

namespace Source191.Vantage.Core.Tests.Domain;

/// <summary>A process is ended only after one of three checks has confirmed it is still the one that was recorded.</summary>
public sealed class ConfirmedProcessShould
{
    private static readonly DateTimeOffset Noon = new(2026, 10, 23, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ConfirmAProcessStartedNoLaterThanTheSessionPlusFiveSeconds()
    {
        ConfirmedProcess.TryConfirm(4242, Noon.AddSeconds(-3), sessionStartedAt: Noon, out ConfirmedProcess? before).ShouldBeTrue();
        ConfirmedProcess.TryConfirm(4242, Noon.AddSeconds(5), sessionStartedAt: Noon, out ConfirmedProcess? rounded).ShouldBeTrue();

        before.Id.ShouldBe(4242);
        before.StartedAt.ShouldBe(Noon.AddSeconds(-3));
        rounded.StartedAt.ShouldBe(Noon.AddSeconds(5));
    }

    [Fact]
    public void RefuseAProcessStartedLaterThanThat()
    {
        ConfirmedProcess.TryConfirm(4242, Noon.AddSeconds(6), sessionStartedAt: Noon, out _).ShouldBeFalse();
        ConfirmedProcess.TryConfirm(4242, Noon.AddMinutes(10), sessionStartedAt: Noon, out _).ShouldBeFalse();
    }

    [Fact]
    public void RefuseAProcessWhoseStartCannotBeRead()
    {
        ConfirmedProcess.TryConfirm(4242, processStartedAt: null, sessionStartedAt: Noon, out _).ShouldBeFalse();
    }

    [Fact]
    public void ConfirmAClaimOnlyWhenTheStartTimeIsExactlyTheRecordedOne()
    {
        ConfirmedProcess.TryConfirmClaim(4242, Noon, claimedStartedAt: Noon, out ConfirmedProcess? exact).ShouldBeTrue();

        exact.Id.ShouldBe(4242);
        exact.StartedAt.ShouldBe(Noon);
        ConfirmedProcess.TryConfirmClaim(4242, Noon.AddTicks(1), claimedStartedAt: Noon, out _).ShouldBeFalse();
        ConfirmedProcess.TryConfirmClaim(4242, Noon.AddTicks(-1), claimedStartedAt: Noon, out _).ShouldBeFalse();
        ConfirmedProcess.TryConfirmClaim(4242, processStartedAt: null, claimedStartedAt: Noon, out _).ShouldBeFalse();
    }

    [Fact]
    public void ConfirmAParentStartedNoLaterThanItsConfirmedChild()
    {
        ConfirmedProcess.TryConfirm(4242, Noon, Noon, out ConfirmedProcess? child).ShouldBeTrue();

        ConfirmedProcess.TryConfirmParent(4000, Noon.AddMinutes(-1), child, out ConfirmedProcess? older).ShouldBeTrue();
        ConfirmedProcess.TryConfirmParent(4001, Noon, child, out ConfirmedProcess? sameTime).ShouldBeTrue();

        older.Id.ShouldBe(4000);
        older.StartedAt.ShouldBe(Noon.AddMinutes(-1));
        sameTime.Id.ShouldBe(4001);
    }

    [Fact]
    public void RefuseAParentYoungerThanItsChildOrWhoseStartCannotBeRead()
    {
        ConfirmedProcess.TryConfirm(4242, Noon, Noon, out ConfirmedProcess? child).ShouldBeTrue();

        ConfirmedProcess.TryConfirmParent(4000, Noon.AddTicks(1), child, out _).ShouldBeFalse();
        ConfirmedProcess.TryConfirmParent(4000, parentStartedAt: null, child, out _).ShouldBeFalse();
    }
}
