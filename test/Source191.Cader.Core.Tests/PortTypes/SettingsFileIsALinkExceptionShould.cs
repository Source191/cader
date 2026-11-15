using Source191.Cader.Ports;

namespace Source191.Cader.Core.Tests.PortTypes;

public sealed class SettingsFileIsALinkExceptionShould
{
    [Fact]
    public void NameTheFileThatIsALink()
    {
        new SettingsFileIsALinkException(@"C:\Users\me\settings.json").Message.ShouldBe(@"C:\Users\me\settings.json is a link");
    }
}