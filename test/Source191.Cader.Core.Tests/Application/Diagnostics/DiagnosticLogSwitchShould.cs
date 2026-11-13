using Source191.Cader.Application.Diagnostics;

namespace Source191.Cader.Core.Tests.Application.Diagnostics;

public sealed class DiagnosticLogSwitchShould
{
    [Fact]
    public void StartOff()
    {
        new DiagnosticLogSwitch().IsOn().ShouldBeFalse();
    }

    [Fact]
    public void FollowTheFunctionItIsPointedAtOnEveryRead()
    {
        bool setting = false;
        DiagnosticLogSwitch logSwitch = new();
        logSwitch.PointAt(() => setting);

        logSwitch.IsOn().ShouldBeFalse();
        setting = true;
        logSwitch.IsOn().ShouldBeTrue();
        setting = false;
        logSwitch.IsOn().ShouldBeFalse();
    }

    [Fact]
    public async Task BePointedAgainFromAnotherThread()
    {
        DiagnosticLogSwitch logSwitch = new();

        await Task.Run(() => logSwitch.PointAt(() => true), TestContext.Current.CancellationToken);

        logSwitch.IsOn().ShouldBeTrue();
    }
}
