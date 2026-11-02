using Source191.Vantage.Application;
using Source191.Vantage.Application.Actions;
using Source191.Vantage.Application.Effects;
using Source191.Vantage.Application.Views;
using Source191.Vantage.Domain;
using Source191.Vantage.Ports;
using Source191.Vantage.TestDoubles;

namespace Source191.Vantage.Core.Tests.Application.Actions;

public sealed class StartUpdateHandlerShould
{
    private const string Vantage = @"C:\Users\me\AppData\Local\Microsoft\WinGet\Links\vantage.exe";
    private const string Winget = @"C:\Users\me\AppData\Local\Microsoft\WindowsApps\winget.exe";
    private const string Dotnet = @"C:\Program Files\dotnet\dotnet.exe";
    private static readonly FolderPath Home = FolderPath.From(@"C:\Users\me");
    private static readonly FolderPath LocalApplicationData = FolderPath.From(@"C:\Users\me\AppData\Local");

    private readonly FakeWindowsTerminal terminal = new();

    [Fact]
    public async Task OpenTheUpdateTabForAWingetInstall()
    {
        StartUpdateHandler handler = Handler(InstallChannel.Winget, new ProgramLocation.Found(Winget), new ProgramLocation.Found(Dotnet));

        Outcome outcome = await handler.HandleAsync(new StartUpdate(ReleaseVersions.Version("0.10.0")), CancellationToken.None);

        TabLaunch tab = terminal.Opened.Single();
        tab.Title.ShouldBe("Update Vantage");
        tab.StartingDirectory.ShouldBe(Home);
        tab.Script.ShouldContain("Wait-Process -Id '4242'");
        tab.Script.ShouldContain($"& '{Winget}' 'upgrade'");
        outcome.ShouldBe(new UpdateStartedOutcome("0.10.0"));
    }

    [Fact]
    public async Task OpenTheMoveTabForADotnetToolInstall()
    {
        StartUpdateHandler handler = Handler(InstallChannel.DotnetTool, new ProgramLocation.Found(Winget), new ProgramLocation.Found(Dotnet));

        Outcome outcome = await handler.HandleAsync(new StartUpdate(ReleaseVersions.Version("0.21.0")), CancellationToken.None);

        TabLaunch tab = terminal.Opened.Single();
        tab.Title.ShouldBe("Move Vantage to winget");
        tab.Script.ShouldContain($"& '{Winget}' 'install'");
        tab.Script.ShouldContain($"& '{Dotnet}' 'tool' 'uninstall'");
        outcome.ShouldBe(new MoveToWingetStartedOutcome("0.21.0"));
    }

    [Theory]
    [InlineData(InstallChannel.Winget)]
    [InlineData(InstallChannel.DotnetTool)]
    public async Task OpenNoTabWithoutWinget(InstallChannel channel)
    {
        StartUpdateHandler handler = Handler(channel, new ProgramLocation.Missing(), new ProgramLocation.Found(Dotnet));

        Outcome outcome = await handler.HandleAsync(new StartUpdate(ReleaseVersions.Version("0.21.0")), CancellationToken.None);

        terminal.Opened.ShouldBeEmpty();
        outcome.ShouldBe(new UpdaterMissingOutcome("winget", "0.21.0"));
    }

    [Fact]
    public async Task OpenNoMoveTabWithoutDotnet()
    {
        StartUpdateHandler handler = Handler(InstallChannel.DotnetTool, new ProgramLocation.Found(Winget), new ProgramLocation.Missing());

        Outcome outcome = await handler.HandleAsync(new StartUpdate(ReleaseVersions.Version("0.21.0")), CancellationToken.None);

        terminal.Opened.ShouldBeEmpty();
        outcome.ShouldBe(new UpdaterMissingOutcome("dotnet", "0.21.0"));
    }

    private StartUpdateHandler Handler(InstallChannel channel, ProgramLocation winget, ProgramLocation dotnet)
    {
        VantageInstall install = new(channel, winget, dotnet, LocalApplicationData);

        return new StartUpdateHandler(terminal, new TabScripts(@"C:\Users\me\.local\bin\claude.exe", Vantage), new VantageProcess(4242, Home, install));
    }
}