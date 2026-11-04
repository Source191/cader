using Source191.Cader.Application;

namespace Source191.Cader.Core.Tests.Application;

public sealed class ReleasePageShould
{
    [Fact]
    public void BuildTheUrlFromTheVersionAlone()
    {
        ReleasePage.For(ReleaseVersions.Version("0.21.0")).AbsoluteUri.ShouldBe("https://github.com/Source191/cader/releases/tag/v0.21.0");
    }
}