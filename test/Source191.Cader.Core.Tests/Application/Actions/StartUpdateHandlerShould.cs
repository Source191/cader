using Source191.Cader.Application;
using Source191.Cader.Application.Actions;
using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application.Actions;

public sealed class StartUpdateHandlerShould
{
    private const string Cader = @"C:\Users\me\AppData\Local\Microsoft\WinGet\Links\cader.exe";
    private const string Winget = @"C:\Users\me\AppData\Local\Microsoft\WindowsApps\winget.exe";
    private static readonly FolderPath Home = FolderPath.From(@"C:\Users\me");

    private readonly FakeWindowsTerminal terminal = new();

    [Fact]
    public async Task OpenTheUpdateTabAndReportIt()
    {
        StartUpdateHandler handler = Handler(new ProgramLocation.Found(Winget));

        Outcome outcome = await handler.HandleAsync(new StartUpdate(ReleaseVersions.Version("0.10.0")), CancellationToken.None);

        TabLaunch tab = terminal.Opened.Single();
        tab.Title.ShouldBe("Update Cader");
        tab.StartingDirectory.ShouldBe(Home);
        tab.Script.ShouldContain("Wait-Process -Id '4242'");
        tab.Script.ShouldContain($"& '{Winget}' 'upgrade'");
        outcome.ShouldBe(new UpdateStartedOutcome("0.10.0"));
    }

    [Fact]
    public async Task OpenNoTabWithoutWinget()
    {
        StartUpdateHandler handler = Handler(new ProgramLocation.Missing());

        Outcome outcome = await handler.HandleAsync(new StartUpdate(ReleaseVersions.Version("0.21.0")), CancellationToken.None);

        terminal.Opened.ShouldBeEmpty();
        outcome.ShouldBe(new WingetMissingOutcome("0.21.0"));
    }

    private StartUpdateHandler Handler(ProgramLocation winget)
    {
        return new StartUpdateHandler(terminal, new TabScripts(@"C:\Users\me\.local\bin\claude.exe", Cader), new CaderProcess(4242, Home, winget));
    }
}