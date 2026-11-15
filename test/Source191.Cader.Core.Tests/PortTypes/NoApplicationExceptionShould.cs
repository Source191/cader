using Source191.Cader.Ports;

namespace Source191.Cader.Core.Tests.PortTypes;

public sealed class NoApplicationExceptionShould
{
    [Fact]
    public void NameTheExtensionNothingOpens()
    {
        NoApplicationException none = new(".slnx");

        none.Extension.ShouldBe(".slnx");
        none.Message.ShouldBe("No application opens .slnx files.");
    }
}