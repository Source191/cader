using Source191.Cader.Application.Views;

namespace Source191.Cader.Core.Tests.Application.Views;

public sealed class DurationsShould
{
    [Fact]
    public void FormatAges()
    {
        Durations.Age(TimeSpan.FromMinutes(45)).ShouldBe("45m");
        Durations.Age(new TimeSpan(3, 5, 0)).ShouldBe("3h05m");
        Durations.Age(new TimeSpan(2, 4, 30, 0)).ShouldBe("2d4h");
        Durations.Age(TimeSpan.FromMinutes(-5)).ShouldBe("0m");
    }

    [Fact]
    public void ChangeUnitExactlyAtAnHourAndADay()
    {
        Durations.Age(new TimeSpan(0, 59, 59)).ShouldBe("59m");
        Durations.Age(TimeSpan.FromHours(1)).ShouldBe("1h00m");
        Durations.Age(TimeSpan.FromHours(24)).ShouldBe("1d0h");
    }
}