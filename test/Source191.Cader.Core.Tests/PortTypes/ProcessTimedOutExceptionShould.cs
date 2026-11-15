using Source191.Cader.Ports;

namespace Source191.Cader.Core.Tests.PortTypes;

public sealed class ProcessTimedOutExceptionShould
{
    [Theory]
    [InlineData(1, "git.exe did not finish within 1 second and was ended")]
    [InlineData(2, "git.exe did not finish within 2 seconds and was ended")]
    [InlineData(30, "git.exe did not finish within 30 seconds and was ended")]
    [InlineData(1.5, "git.exe did not finish within 1.5 seconds and was ended")]
    [InlineData(0.333, "git.exe did not finish within 0.33 seconds and was ended")]
    public void SayHowLongItWaitedInSeconds(double seconds, string expected)
    {
        new ProcessTimedOutException("git.exe", TimeSpan.FromSeconds(seconds)).Message.ShouldBe(expected);
    }

    [Fact]
    public void KeepTheProgramAndTheLimit()
    {
        ProcessTimedOutException timedOut = new("git.exe", TimeSpan.FromSeconds(30));

        timedOut.FileName.ShouldBe("git.exe");
        timedOut.Limit.ShouldBe(TimeSpan.FromSeconds(30));
    }
}