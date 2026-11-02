using Source191.Vantage.Domain;

namespace Source191.Vantage.Core.Tests.Domain;

public sealed class InstallChannelShould
{
    [Fact]
    public void BeWingetForASingleFileBundle()
    {
        InstallChannels.Of(runsAsSingleFileBundle: true).ShouldBe(InstallChannel.Winget);
    }

    [Fact]
    public void BeTheDotnetToolOtherwise()
    {
        InstallChannels.Of(runsAsSingleFileBundle: false).ShouldBe(InstallChannel.DotnetTool);
    }
}