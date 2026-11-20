using Source191.Cader.Application;

namespace Source191.Cader.Core.Tests.Application;

public sealed class LaunchHoldShould
{
    private readonly LaunchHold hold = new();

    [Fact]
    public void BeHeldWhileAnyHoldIsOpen()
    {
        hold.IsHeld.ShouldBeFalse();

        using IDisposable first = hold.Hold();
        using IDisposable second = hold.Hold();
        first.Dispose();

        hold.IsHeld.ShouldBeTrue();
    }

    [Fact]
    public void BeReleasedWhenTheLastHoldEnds()
    {
        IDisposable first = hold.Hold();
        IDisposable second = hold.Hold();

        second.Dispose();
        first.Dispose();

        hold.IsHeld.ShouldBeFalse();
    }

    [Fact]
    public void ReleaseOnceWhenDisposedTwice()
    {
        using IDisposable kept = hold.Hold();
        IDisposable twice = hold.Hold();

        twice.Dispose();
        twice.Dispose();

        hold.IsHeld.ShouldBeTrue("the second dispose of one hold must not end another");
    }
}