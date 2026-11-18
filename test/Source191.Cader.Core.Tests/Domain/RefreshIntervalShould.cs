using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Domain;

public sealed class RefreshIntervalShould
{
    [Fact]
    public void BeOffAtZero()
    {
        RefreshInterval.From(0).ShouldBe(RefreshInterval.Off);
        RefreshInterval.Off.IsOff.ShouldBeTrue();
        RefreshInterval.From(1).IsOff.ShouldBeFalse();
    }

    [Fact]
    public void ReachAnHour()
    {
        RefreshInterval.From(3600).Every.ShouldBe(TimeSpan.FromHours(1));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3601)]
    public void RefuseSecondsOutOfRange(int seconds)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => RefreshInterval.From(seconds));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(-1)]
    [InlineData(86400)]
    public void AnswerFalseForAFileValueItCannotUse(int? seconds)
    {
        RefreshInterval.TryFrom(seconds, out RefreshInterval? _).ShouldBeFalse();
    }

    [Fact]
    public void ReadAFileValueInRange()
    {
        RefreshInterval.TryFrom(30, out RefreshInterval? interval).ShouldBeTrue();

        interval.ShouldBe(RefreshInterval.From(30));
    }

    [Fact]
    public void GiveItsSecondsAsATimeSpan()
    {
        RefreshInterval.From(15).Every.ShouldBe(TimeSpan.FromSeconds(15));
        RefreshInterval.From(15).Seconds.ShouldBe(15);
    }
}
