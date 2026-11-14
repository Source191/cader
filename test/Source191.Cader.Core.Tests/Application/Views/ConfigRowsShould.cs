using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application.Views;

public sealed class ConfigRowsShould
{
    private const string Repos = @"C:\Users\me\source\repos";
    private const string SettingsFile = @"C:\Users\me\AppData\Local\Cader\config.json";
    private const string LogFile = @"C:\Users\me\AppData\Local\Cader\cader.log";
    private const string Fragment = @"C:\Users\me\AppData\Local\Microsoft\Windows Terminal\Fragments\Cader\cader.json";

    private static readonly SetupFacts AllSetUp = new(
        new SetupState.At(SettingsFile),
        new SetupState.Folder(TestSettings.DefaultClaudeFolder),
        new SetupState.LogFile(LogFile, Exists: true),
        new SetupState.At(Fragment),
        new SetupState.Enabled(),
        new SetupState.Checked(new DateTimeOffset(2026, 10, 30, 9, 12, 0, TimeSpan.Zero), new NewestRelease.Found(ReleaseVersions.Version("0.18.0"))));

    [Fact]
    public void ListTheDefaultSettingsFirstInTheirOrder()
    {
        IReadOnlyList<ConfigRow> rows = ConfigRows.For(TestSettings.For(Repos), AllSetUp);

        rows.Take(8).ShouldBe(
        [
            new ConfigRow.RepositoryRoot(0, FolderPath.From(Repos)),
            new ConfigRow.WorktreeRoot(FolderPath.From(@"C:\Users\me\source\worktrees")),
            new ConfigRow.OpenFolderWith(EditorChoice.VsCode),
            new ConfigRow.DefaultBase(WorktreeBase.RemoteDefault),
            new ConfigRow.BeforeBranching(BeforeBranching.Fetch),
            new ConfigRow.DefaultPlace(LaunchMode.NewWorktree),
            new ConfigRow.CheckForUpdates(On: true),
            new ConfigRow.DiagnosticLog(On: false),
        ]);
    }

    [Fact]
    public void ShowTheDiagnosticLogTurnedOn()
    {
        Settings on = TestSettings.For(Repos).WithDiagnosticLogToggled();

        ConfigRows.For(on, AllSetUp).OfType<ConfigRow.DiagnosticLog>().ShouldHaveSingleItem().ShouldBe(new ConfigRow.DiagnosticLog(On: true));
    }

    [Fact]
    public void ListTheClaudeFolderAfterTheSettingsFile()
    {
        ConfigRows.For(TestSettings.For(Repos), AllSetUp).OfType<ConfigRow.Setup>().Select(row => row.Item).ShouldBe([SetupItem.SettingsFile, SetupItem.ClaudeFolder, SetupItem.DiagnosticLogFile, SetupItem.TerminalProfiles, SetupItem.ClaudeBell, SetupItem.LastUpdateCheck]);
    }

    [Fact]
    public void ShowTheLogFileAsOffWithItsPathWhileTheSettingIsOff()
    {
        ConfigRows.For(TestSettings.For(Repos), AllSetUp).OfType<ConfigRow.Setup>().Single(row => row.Item == SetupItem.DiagnosticLogFile).State.ShouldBe(new SetupState.LogOff(LogFile));
    }

    [Fact]
    public void ShowTheLogFileWhetherItIsWrittenWhileTheSettingIsOn()
    {
        Settings on = TestSettings.For(Repos).WithDiagnosticLogToggled();
        SetupFacts notWritten = AllSetUp with { DiagnosticLogFile = new SetupState.LogFile(LogFile, Exists: false) };

        ConfigRows.For(on, AllSetUp).OfType<ConfigRow.Setup>().Single(row => row.Item == SetupItem.DiagnosticLogFile).State.ShouldBe(new SetupState.LogFile(LogFile, Exists: true));
        ConfigRows.For(on, notWritten).OfType<ConfigRow.Setup>().Single(row => row.Item == SetupItem.DiagnosticLogFile).State.ShouldBe(new SetupState.LogFile(LogFile, Exists: false));
    }

    [Fact]
    public void GiveEachRootARowInOrder()
    {
        IReadOnlyList<ConfigRow> rows = ConfigRows.For(TestSettings.For(Repos, @"D:\work"), AllSetUp);

        rows.OfType<ConfigRow.RepositoryRoot>().ShouldBe([new ConfigRow.RepositoryRoot(0, FolderPath.From(Repos)), new ConfigRow.RepositoryRoot(1, FolderPath.From(@"D:\work"))]);
        rows[1].ShouldBeOfType<ConfigRow.RepositoryRoot>();
    }

    [Fact]
    public void CarryACustomEditor()
    {
        EditorChoice subl = EditorChoice.Custom(@"C:\Tools\subl.exe", ["-n", "{folder}"]);
        Settings settings = Settings.Resolve([FolderPath.From(Repos)], new WorktreeRootChoice.BesideFirstRoot(), subl, WorktreeBase.RemoteDefault, TestSettings.Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset);

        ConfigRows.For(settings, AllSetUp).OfType<ConfigRow.OpenFolderWith>().Single().Editor.ShouldBe(subl);
    }

    [Fact]
    public void KeepTheKeysStableAcrossASave()
    {
        IReadOnlyList<ConfigRow> before = ConfigRows.For(TestSettings.For(Repos), AllSetUp);
        IReadOnlyList<ConfigRow> after = ConfigRows.For(TestSettings.For([Repos], @"D:\worktrees"), AllSetUp);

        after.Select(row => row.Key).ShouldBe(before.Select(row => row.Key));
        before.Select(row => row.Key).Distinct().Count().ShouldBe(before.Count);
    }

    [Fact]
    public void ListTheSetupAfterTheSettings()
    {
        IReadOnlyList<ConfigRow> rows = ConfigRows.For(TestSettings.For(Repos), AllSetUp);

        rows.Skip(9).ShouldBe(
        [
            new ConfigRow.Setup(SetupItem.SettingsFile, new SetupState.At(SettingsFile)),
            new ConfigRow.Setup(SetupItem.ClaudeFolder, new SetupState.Folder(TestSettings.DefaultClaudeFolder)),
            new ConfigRow.Setup(SetupItem.DiagnosticLogFile, new SetupState.LogOff(LogFile)),
            new ConfigRow.Setup(SetupItem.TerminalProfiles, new SetupState.At(Fragment)),
            new ConfigRow.Setup(SetupItem.ClaudeBell, new SetupState.Enabled()),
            new ConfigRow.Setup(SetupItem.LastUpdateCheck, AllSetUp.LastUpdateCheck),
        ]);
    }

    [Fact]
    public void HeadTheSetupRows()
    {
        IReadOnlyList<ConfigRow> rows = ConfigRows.For(TestSettings.For(Repos), AllSetUp);
        IReadOnlyList<ConfigRow> saved = ConfigRows.For(TestSettings.For([Repos], @"D:\worktrees"), AllSetUp);

        rows.OfType<ConfigRow.Heading>().Count().ShouldBe(1);
        rows[6].ShouldBeOfType<ConfigRow.CheckForUpdates>();
        rows[7].ShouldBeOfType<ConfigRow.DiagnosticLog>();
        rows[8].ShouldBeOfType<ConfigRow.Heading>();
        rows[9].ShouldBe(new ConfigRow.Setup(SetupItem.SettingsFile, new SetupState.At(SettingsFile)));
        saved[8].Key.ShouldBe(rows[8].Key);
    }

    [Fact]
    public void ShowNothingSetUp()
    {
        SetupFacts nothing = AllSetUp with { TerminalProfiles = new SetupState.NotSetUp(), ClaudeBell = new SetupState.NotSetUp() };

        IReadOnlyList<ConfigRow.Setup> setup = [.. ConfigRows.For(TestSettings.For(Repos), nothing).OfType<ConfigRow.Setup>()];

        setup.Single(row => row.Item == SetupItem.TerminalProfiles).State.ShouldBe(new SetupState.NotSetUp());
        setup.Single(row => row.Item == SetupItem.ClaudeBell).State.ShouldBe(new SetupState.NotSetUp());
    }

    [Fact]
    public void ShowTheSetupAsReadingBeforeItIsRead()
    {
        IReadOnlyList<ConfigRow.Setup> setup = [.. ConfigRows.For(TestSettings.For(Repos), SetupFacts.Reading).OfType<ConfigRow.Setup>()];

        setup.Select(row => row.Item).ShouldBe(Enum.GetValues<SetupItem>());
        setup.ShouldAllBe(row => row.State is SetupState.Reading);
    }

    [Fact]
    public void ShowTheLastUpdateCheckAsOffWhenTheCheckIsOff()
    {
        Settings off = TestSettings.For(Repos).WithUpdateCheckToggled();

        ConfigRows.For(off, AllSetUp).OfType<ConfigRow.Setup>().Single(row => row.Item == SetupItem.LastUpdateCheck).State.ShouldBe(new SetupState.Disabled());
    }
}