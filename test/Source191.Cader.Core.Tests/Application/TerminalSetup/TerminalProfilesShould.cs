using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.Core.Tests.Application.TerminalSetup;

public sealed class TerminalProfilesShould
{
    [Fact]
    public void NameTheFragmentsSourceCader()
    {
        TerminalProfiles.Source.ShouldBe("Cader");
    }

    [Fact]
    public void ListTheEntryForEachProfileInTheFragmentsOrder()
    {
        TerminalProfiles.Entries.ShouldBe([
            new TerminalProfileEntry(new TerminalProfileId(Guid.Parse("3d7a06d2-710f-51d9-90c9-dacae2c3ab46")), "Cader", "Cader"),
            new TerminalProfileEntry(new TerminalProfileId(Guid.Parse("cdf8708d-a821-5c97-95d0-fea560807dea")), "Cader session", "Cader"),
        ]);
    }
}