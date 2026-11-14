using Source191.Cader.Application.TerminalSetup;
using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.Core.Tests.Application.TerminalSetup;

public sealed class HiddenTerminalProfilesShould
{
    private static readonly TerminalProfileEntry Cader = TerminalProfiles.Entries[0];
    private static readonly TerminalProfileEntry CaderSession = TerminalProfiles.Entries[1];
    private static readonly TerminalProfileId PowerShell = new(Guid.Parse("61c54bbd-c2c6-5271-96e7-009a87ff44bf"));

    [Fact]
    public void AnswerAProfileGeneratedAndNotListed()
    {
        TerminalProfileRecords records = new TerminalProfileRecords.Read(Ids(Cader.Id, CaderSession.Id, PowerShell), Ids(Cader.Id, PowerShell));

        HiddenTerminalProfiles.Of(records).ShouldBe([CaderSession]);
    }

    [Fact]
    public void AnswerBothInTheFragmentsOrder()
    {
        TerminalProfileRecords records = new TerminalProfileRecords.Read(Ids(CaderSession.Id, Cader.Id), Ids(PowerShell));

        HiddenTerminalProfiles.Of(records).ShouldBe([Cader, CaderSession]);
    }

    [Fact]
    public void AnswerNoneWhenEachGeneratedProfileIsListedHiddenOrNot()
    {
        TerminalProfileRecords records = new TerminalProfileRecords.Read(Ids(Cader.Id, CaderSession.Id), Ids(Cader.Id, CaderSession.Id));

        HiddenTerminalProfiles.Of(records).ShouldBeEmpty();
    }

    [Fact]
    public void AnswerNoneWhenTerminalHasNotLoadedTheProfiles()
    {
        TerminalProfileRecords records = new TerminalProfileRecords.Read(Ids(PowerShell), Ids(PowerShell));

        HiddenTerminalProfiles.Of(records).ShouldBeEmpty();
    }

    [Fact]
    public void AnswerNoneWhenTerminalsFilesAreMissingOrUnreadable()
    {
        HiddenTerminalProfiles.Of(new TerminalProfileRecords.Missing()).ShouldBeEmpty();
        HiddenTerminalProfiles.Of(new TerminalProfileRecords.Unreadable("not JSON")).ShouldBeEmpty();
    }

    private static HashSet<TerminalProfileId> Ids(params TerminalProfileId[] ids) => [.. ids];
}