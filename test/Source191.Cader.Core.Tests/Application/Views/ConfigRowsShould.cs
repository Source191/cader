using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application.Views;

public sealed class ConfigRowsShould
{
    private const string Repos = @"C:\Users\me\source\repos";
    private const string SettingsFile = @"C:\Users\me\AppData\Local\Cader\config.json";
    private const string Fragment = @"C:\Users\me\AppData\Local\Microsoft\Windows Terminal\Fragments\Cader\cader.json";

    private static readonly SetupFacts AllSetUp = new(
        new SetupState.At(SettingsFile),
        new SetupState.At(Fragment),
        new SetupState.Enabled(),
        new SetupState.Checked(new DateTimeOffset(2026, 10, 30, 9, 12, 0, TimeSpan.Zero), new NewestRelease.Found(ReleaseVersions.Version("0.18.0"))));

    [Fact]
    public void ListTheDefaultSettingsFirstInTheirOrder()
    {
        IReadOnlyList<ConfigRow> rows = ConfigRows.For(TestSettings.For(Repos), AllSetUp, TestSettings.DiagnosticLogFile);

        rows.Take(7).ShouldBe(
        [
            new ConfigRow.RepositoryRoot(0, FolderPath.From(Repos)),
            new ConfigRow.WorktreeRoot(FolderPath.From(@"C:\Users\me\source\worktrees")),
            new ConfigRow.OpenFolderWith(EditorChoice.VsCode),
            new ConfigRow.DefaultBase(WorktreeBase.RemoteDefault),
            new ConfigRow.DefaultPlace(LaunchMode.NewWorktree),
            new ConfigRow.CheckForUpdates(On: true),
            new ConfigRow.DiagnosticLog(On: false, File: TestSettings.DiagnosticLogFile),
        ]);
    }

    [Fact]
    public void ShowTheDiagnosticLogTurnedOnWithItsFile()
    {
        Settings on = TestSettings.For(Repos).WithDiagnosticLogToggled();

        ConfigRows.For(on, AllSetUp, TestSettings.DiagnosticLogFile).OfType<ConfigRow.DiagnosticLog>().ShouldHaveSingleItem().ShouldBe(new ConfigRow.DiagnosticLog(On: true, File: TestSettings.DiagnosticLogFile));
    }

    [Fact]
    public void HoldNoDiagnosticLogAmongTheSetup()
    {
        Enum.GetNames<SetupItem>().ShouldNotContain("DiagnosticLog");
        ConfigRows.For(TestSettings.For(Repos), AllSetUp, TestSettings.DiagnosticLogFile).OfType<ConfigRow.Setup>().Select(row => row.Item).ShouldBe([SetupItem.SettingsFile, SetupItem.TerminalProfiles, SetupItem.ClaudeBell, SetupItem.LastUpdateCheck]);
    }

    [Fact]
    public void GiveEachRootARowInOrder()
    {
        IReadOnlyList<ConfigRow> rows = ConfigRows.For(TestSettings.For(Repos, @"D:\work"), AllSetUp, TestSettings.DiagnosticLogFile);

        rows.OfType<ConfigRow.RepositoryRoot>().ShouldBe([new ConfigRow.RepositoryRoot(0, FolderPath.From(Repos)), new ConfigRow.RepositoryRoot(1, FolderPath.From(@"D:\work"))]);
        rows[1].ShouldBeOfType<ConfigRow.RepositoryRoot>();
    }

    [Fact]
    public void CarryACustomEditor()
    {
        EditorChoice subl = EditorChoice.Custom(@"C:\Tools\subl.exe", ["-n", "{folder}"]);
        Settings settings = Settings.Resolve([FolderPath.From(Repos)], new WorktreeRootChoice.BesideFirstRoot(), subl, WorktreeBase.RemoteDefault, TestSettings.Home);

        ConfigRows.For(settings, AllSetUp, TestSettings.DiagnosticLogFile).OfType<ConfigRow.OpenFolderWith>().Single().Editor.ShouldBe(subl);
    }

    [Fact]
    public void KeepTheKeysStableAcrossASave()
    {
        IReadOnlyList<ConfigRow> before = ConfigRows.For(TestSettings.For(Repos), AllSetUp, TestSettings.DiagnosticLogFile);
        IReadOnlyList<ConfigRow> after = ConfigRows.For(TestSettings.For([Repos], @"D:\worktrees"), AllSetUp, TestSettings.DiagnosticLogFile);

        after.Select(row => row.Key).ShouldBe(before.Select(row => row.Key));
        before.Select(row => row.Key).Distinct().Count().ShouldBe(before.Count);
    }

    [Fact]
    public void ListTheSetupAfterTheSettings()
    {
        IReadOnlyList<ConfigRow> rows = ConfigRows.For(TestSettings.For(Repos), AllSetUp, TestSettings.DiagnosticLogFile);

        rows.Skip(8).ShouldBe(
        [
            new ConfigRow.Setup(SetupItem.SettingsFile, new SetupState.At(SettingsFile)),
            new ConfigRow.Setup(SetupItem.TerminalProfiles, new SetupState.At(Fragment)),
            new ConfigRow.Setup(SetupItem.ClaudeBell, new SetupState.Enabled()),
            new ConfigRow.Setup(SetupItem.LastUpdateCheck, AllSetUp.LastUpdateCheck),
        ]);
    }

    [Fact]
    public void HeadTheSetupRows()
    {
        IReadOnlyList<ConfigRow> rows = ConfigRows.For(TestSettings.For(Repos), AllSetUp, TestSettings.DiagnosticLogFile);
        IReadOnlyList<ConfigRow> saved = ConfigRows.For(TestSettings.For([Repos], @"D:\worktrees"), AllSetUp, TestSettings.DiagnosticLogFile);

        rows.OfType<ConfigRow.Heading>().Count().ShouldBe(1);
        rows[5].ShouldBeOfType<ConfigRow.CheckForUpdates>();
        rows[6].ShouldBeOfType<ConfigRow.DiagnosticLog>();
        rows[7].ShouldBeOfType<ConfigRow.Heading>();
        rows[8].ShouldBe(new ConfigRow.Setup(SetupItem.SettingsFile, new SetupState.At(SettingsFile)));
        saved[7].Key.ShouldBe(rows[7].Key);
    }

    [Fact]
    public void ShowNothingSetUp()
    {
        SetupFacts nothing = AllSetUp with { TerminalProfiles = new SetupState.NotSetUp(), ClaudeBell = new SetupState.NotSetUp() };

        IReadOnlyList<ConfigRow.Setup> setup = [.. ConfigRows.For(TestSettings.For(Repos), nothing, TestSettings.DiagnosticLogFile).OfType<ConfigRow.Setup>()];

        setup.Single(row => row.Item == SetupItem.TerminalProfiles).State.ShouldBe(new SetupState.NotSetUp());
        setup.Single(row => row.Item == SetupItem.ClaudeBell).State.ShouldBe(new SetupState.NotSetUp());
    }

    [Fact]
    public void ShowTheSetupAsReadingBeforeItIsRead()
    {
        IReadOnlyList<ConfigRow.Setup> setup = [.. ConfigRows.For(TestSettings.For(Repos), SetupFacts.Reading, TestSettings.DiagnosticLogFile).OfType<ConfigRow.Setup>()];

        setup.Select(row => row.Item).ShouldBe(Enum.GetValues<SetupItem>());
        setup.ShouldAllBe(row => row.State is SetupState.Reading);
    }

    [Fact]
    public void ShowTheLastUpdateCheckAsOffWhenTheCheckIsOff()
    {
        Settings off = TestSettings.For(Repos).WithUpdateCheckToggled();

        ConfigRows.For(off, AllSetUp, TestSettings.DiagnosticLogFile).OfType<ConfigRow.Setup>().Single(row => row.Item == SetupItem.LastUpdateCheck).State.ShouldBe(new SetupState.Disabled());
    }
}