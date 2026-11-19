using Source191.Cader.Application;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application;

public sealed class WordingOfConfigShould
{
    private static readonly DateTimeOffset Now = new(2026, 10, 30, 15, 0, 0, TimeSpan.Zero);
    private static readonly TimeZoneInfo UnitedKingdom = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
    private const string LogFile = @"C:\Users\me\AppData\Local\Cader\cader.log";
    private const string Fragment = @"C:\Users\me\AppData\Local\Microsoft\Windows Terminal\Fragments\Cader\cader.json";

    private static readonly KnownAccount Work = new(OrganisationId.From("6f1c2a8e-4b7d-4c1e-9a3f-2d5e8b0c7a91"), "me@northwind.example", "Northwind Traders", AccountLabel.From("work"));

    public static TheoryData<ConfigRow, string, string> Rows => new()
    {
        { new ConfigRow.RepositoryRoot(0, FolderPath.From(@"C:\Users\me\source\repos")), "Repository roots", @"C:\Users\me\source\repos" },
        { new ConfigRow.RepositoryRoot(1, FolderPath.From(@"D:\work")), string.Empty, @"D:\work" },
        { new ConfigRow.RepositoryRoot(1, FolderPath.From(@"D:\work")) { Found = false }, string.Empty, @"D:\work (not found)" },
        { new ConfigRow.WorktreeRoot(FolderPath.From(@"C:\Users\me\source\worktrees")), "Worktree root", @"C:\Users\me\source\worktrees" },
        { new ConfigRow.OpenFolderWith(EditorChoice.VsCode), "Open folder with", "VS Code" },
        { new ConfigRow.OpenFolderWith(EditorChoice.Custom(@"C:\Tools\subl.exe", ["-n", "{folder}"])), "Open folder with", @"C:\Tools\subl.exe -n {folder}" },
        { new ConfigRow.DefaultBase(WorktreeBase.RemoteDefault), "Default base", "origin's default branch" },
        { new ConfigRow.DefaultBase(WorktreeBase.Head), "Default base", "the commit checked out now (HEAD)" },
        { new ConfigRow.BeforeBranching(BeforeBranching.Fetch), "Before branching", "Fetch origin" },
        { new ConfigRow.BeforeBranching(BeforeBranching.Pull), "Before branching", "Pull default branch" },
        { new ConfigRow.BeforeBranching(BeforeBranching.None), "Before branching", "Nothing" },
        { new ConfigRow.DefaultPlace(LaunchMode.NewWorktree), "Default place", "New worktree" },
        { new ConfigRow.DefaultPlace(LaunchMode.CurrentBranch), "Default place", "Current branch" },
        { new ConfigRow.SessionsRefresh(RefreshInterval.From(5)), "Sessions refresh", "every 5 seconds" },
        { new ConfigRow.SessionsRefresh(RefreshInterval.From(1)), "Sessions refresh", "every second" },
        { new ConfigRow.SessionsRefresh(RefreshInterval.Off), "Sessions refresh", "off" },
        { new ConfigRow.WorktreesRefresh(RefreshInterval.From(15)), "Worktrees refresh", "every 15 seconds" },
        { new ConfigRow.WorktreesRefresh(RefreshInterval.Off), "Worktrees refresh", "off" },
        { new ConfigRow.CheckForUpdates(On: true), "Check for updates", "on" },
        { new ConfigRow.CheckForUpdates(On: false), "Check for updates", "off" },
        { new ConfigRow.DiagnosticLog(On: true), "Diagnostic log", "on" },
        { new ConfigRow.DiagnosticLog(On: false), "Diagnostic log", "off" },
        { new ConfigRow.Setup(SetupItem.SettingsFile, new SetupState.At(@"C:\Users\me\AppData\Local\Cader\config.json")), "Settings file", @"C:\Users\me\AppData\Local\Cader\config.json" },
        { new ConfigRow.Setup(SetupItem.ClaudeFolder, new SetupState.Folder(new ClaudeFolder.Unset(FolderPath.From(@"C:\Users\me\.claude")))), "Claude folder", @"C:\Users\me\.claude" },
        { new ConfigRow.Setup(SetupItem.ClaudeFolder, new SetupState.Folder(new ClaudeFolder.FromVariable(FolderPath.From(@"C:\Users\me\.claude-personal")))), "Claude folder", @"C:\Users\me\.claude-personal, from CLAUDE_CONFIG_DIR" },
        { new ConfigRow.Setup(SetupItem.ClaudeFolder, new SetupState.Folder(new ClaudeFolder.NotUsable(FolderPath.From(@"C:\Users\me\.claude"), "claude-personal"))), "Claude folder", @"C:\Users\me\.claude, CLAUDE_CONFIG_DIR was not a full path and was not used" },
        { new ConfigRow.Setup(SetupItem.DiagnosticLogFile, new SetupState.LogOff(new FakeDiagnosticLogFiles().Read())), "Diagnostic log file", $"off, {LogFile}" },
        { new ConfigRow.Setup(SetupItem.DiagnosticLogFile, new SetupState.LogFile(new FakeDiagnosticLogFiles().Read())), "Diagnostic log file", $"on, not written yet, {LogFile}" },
        { new ConfigRow.Setup(SetupItem.DiagnosticLogFile, new SetupState.LogFile(new FakeDiagnosticLogFiles().WithCurrent(2_516_582).Read())), "Diagnostic log file", $"on, 2.4 MB, {LogFile}" },
        { new ConfigRow.Setup(SetupItem.DiagnosticLogFile, new SetupState.LogOff(new FakeDiagnosticLogFiles().WithCurrent(512_000).Read())), "Diagnostic log file", $"off, 500 KB, {LogFile}" },
        { new ConfigRow.Setup(SetupItem.OlderDiagnosticLogFile, new SetupState.OlderLog(new FakeDiagnosticLogFiles().WithCurrent(300).WithOlder(10_485_700).Read())), "Diagnostic log file (older)", $"10.0 MB, {LogFile}.1" },
        { new ConfigRow.Setup(SetupItem.TerminalProfiles, new SetupState.At(Fragment)), "Windows Terminal profiles", $"installed, {Fragment}" },
        { new ConfigRow.Setup(SetupItem.TerminalProfiles, new SetupState.NotSetUp()), "Windows Terminal profiles", "not set up, run cader setup-terminal" },
        { new ConfigRow.Setup(SetupItem.TerminalProfiles, new SetupState.HiddenByTerminal(Fragment)), "Windows Terminal profiles", "hidden by Windows Terminal, run cader setup-terminal" },
        { new ConfigRow.Setup(SetupItem.ClaudeBell, new SetupState.Enabled()), "Claude bell", "on" },
        { new ConfigRow.Setup(SetupItem.ClaudeBell, new SetupState.NotSetUp()), "Claude bell", "not set, run cader setup-terminal" },
        { new ConfigRow.Setup(SetupItem.ClaudeBell, new SetupState.OtherChannel("iterm2")), "Claude bell", "iterm2, not the terminal bell" },
        { new ConfigRow.Setup(SetupItem.ClaudeBell, new SetupState.Unreadable()), "Claude bell", "could not be read" },
        { new ConfigRow.Setup(SetupItem.LastUpdateCheck, new SetupState.Checked(new DateTimeOffset(2026, 10, 30, 9, 12, 0, TimeSpan.Zero), new NewestRelease.Found(ReleaseVersions.Version("0.18.0")))), "Last update check", "09:12 today, newest 0.18.0" },
        { new ConfigRow.Setup(SetupItem.LastUpdateCheck, new SetupState.Checked(new DateTimeOffset(2026, 10, 30, 9, 12, 0, TimeSpan.Zero), new NewestRelease.Unreadable("rubbish"))), "Last update check", "09:12 today, newest rubbish" },
        { new ConfigRow.Setup(SetupItem.LastUpdateCheck, new SetupState.Checked(new DateTimeOffset(2026, 10, 28, 18, 5, 0, TimeSpan.Zero), new NewestRelease.None())), "Last update check", "28 October 2026 18:05" },
        { new ConfigRow.Setup(SetupItem.LastUpdateCheck, new SetupState.Never()), "Last update check", "never" },
        { new ConfigRow.Heading(ConfigSection.Setup), "Setup", string.Empty },
        { new ConfigRow.Heading(ConfigSection.Accounts), "Accounts", string.Empty },
        { new ConfigRow.Account(new RowAccount.Known(Work, LoggedInNow: true)), "Account work", "me@northwind.example · Northwind Traders · logged in now" },
        { new ConfigRow.Account(new RowAccount.Known(Work with { OrganisationName = string.Empty }, LoggedInNow: false)), "Account work", "me@northwind.example" },
    };

    [Fact]
    public void DescribeEverySettingAndEveryPieceOfSetupDifferently()
    {
        ConfigRow[] rows =
        [
            new ConfigRow.RepositoryRoot(0, FolderPath.From(@"C:\Users\me\source\repos")),
            new ConfigRow.WorktreeRoot(FolderPath.From(@"C:\Users\me\source\worktrees")),
            new ConfigRow.OpenFolderWith(EditorChoice.VsCode),
            new ConfigRow.DefaultBase(WorktreeBase.RemoteDefault),
            new ConfigRow.BeforeBranching(BeforeBranching.Fetch),
            new ConfigRow.DefaultPlace(LaunchMode.NewWorktree),
            new ConfigRow.SessionsRefresh(RefreshInterval.From(5)),
            new ConfigRow.WorktreesRefresh(RefreshInterval.From(15)),
            new ConfigRow.CheckForUpdates(On: true),
            new ConfigRow.DiagnosticLog(On: true),
            .. Enum.GetValues<SetupItem>().Select(item => new ConfigRow.Setup(item, new SetupState.Reading())),
            new ConfigRow.Account(new RowAccount.Known(Work, LoggedInNow: true)),
        ];

        string[] descriptions = [.. rows.Select(Wording.ConfigDescription)];

        descriptions.ShouldAllBe(description => description.Length > 0 && description.Split('\n', StringSplitOptions.None).All(line => line.Length <= 110));
        descriptions.Distinct().Count().ShouldBe(rows.Length);
    }

    [Fact]
    public void SayWhatTheRefreshRowsDoInTheirTooltips()
    {
        Wording.ConfigDescription(new ConfigRow.SessionsRefresh(RefreshInterval.From(5))).ShouldBe("How often the sessions are listed on their own; showing the tab, an action and F5 list them too.\n0 turns the timer off, and a waiting session's bell then rings only at the next refresh; Enter changes it.");
        Wording.ConfigDescription(new ConfigRow.WorktreesRefresh(RefreshInterval.Off)).ShouldBe("How often the shown Worktrees tab rescans on its own; showing it, an action and F5 rescan it too.\n0 turns the timer off; Enter changes it.");
    }

    [Fact]
    public void SayAnAccountsLabelIsRenamedWithEnter()
    {
        Wording.ConfigDescription(new ConfigRow.Account(new RowAccount.Known(Work, LoggedInNow: false))).ShouldBe("The label names this account in the Sessions tab's ACCOUNT column; Enter renames it.");
    }

    [Fact]
    public void PointTheLogTooltipAtTheLogFileRowWithoutAPath()
    {
        string expected = "Writes the diagnostic log to a file, named on the Diagnostic log file row below; Enter turns it on or off.";

        Wording.ConfigDescription(new ConfigRow.DiagnosticLog(On: true)).ShouldBe(expected);
        Wording.ConfigDescription(new ConfigRow.DiagnosticLog(On: false)).ShouldBe(expected);
        expected.ShouldNotContain(@"\");
    }

    [Fact]
    public void SayWhatTheLogFileRowOffersInItsTooltip()
    {
        Wording.ConfigDescription(new ConfigRow.Setup(SetupItem.DiagnosticLogFile, new SetupState.Reading())).ShouldBe("Where the diagnostic log is written, rolling over at 10 MB; f opens it, c copies its path, d clears the log.");
        Wording.ConfigDescription(new ConfigRow.Setup(SetupItem.OlderDiagnosticLogFile, new SetupState.Reading())).ShouldBe("The older log, kept when cader.log reached 10 MB; f opens it, c copies its path, d clears the log.");
    }

    [Fact]
    public void KeepThePathOutOfTheDiagnosticLogValue()
    {
        Wording.ConfigValue(new ConfigRow.DiagnosticLog(On: true), Now, TimeZoneInfo.Utc).ShouldBe("on");
        Wording.ConfigValue(new ConfigRow.DiagnosticLog(On: false), Now, TimeZoneInfo.Utc).ShouldBe("off");
    }

    [Fact]
    public void SayWhereWorktreesGoAndWhatTheBellIsFor()
    {
        Wording.ConfigDescription(new ConfigRow.WorktreeRoot(FolderPath.From(@"C:\Users\me\source\worktrees"))).ShouldBe("Where new worktrees go, each in a folder named <repository>-<slug> on a branch named <slug>.");
        Wording.ConfigDescription(new ConfigRow.Setup(SetupItem.ClaudeBell, new SetupState.Enabled())).ShouldBe("Claude's terminal bell, which rings when a session finishes or needs permission while you are away.");
        Wording.ConfigDescription(new ConfigRow.RepositoryRoot(1, FolderPath.From(@"D:\work"))).ShouldBe(Wording.ConfigDescription(new ConfigRow.RepositoryRoot(0, FolderPath.From(@"C:\Users\me\source\repos"))));
        Wording.ConfigDescription(new ConfigRow.RepositoryRoot(1, FolderPath.From(@"D:\work")) { Found = false }).ShouldBe("This folder was not found, so the wizard offers nothing from it: create it, or remove the root; Enter edits every root, one per line.");
    }

    [Theory]
    [MemberData(nameof(Rows))]
    public void NameEachRowAndSayItsValue(ConfigRow row, string setting, string value)
    {
        Wording.ConfigSetting(row).ShouldBe(setting);
        Wording.ConfigValue(row, Now, TimeZoneInfo.Utc).ShouldBe(value);
    }

    [Theory]
    [InlineData(0L, "0 KB")]
    [InlineData(300L, "1 KB")]
    [InlineData(512_000L, "500 KB")]
    [InlineData(1_048_575L, "1024 KB")]
    [InlineData(1_048_576L, "1.0 MB")]
    [InlineData(2_516_582L, "2.4 MB")]
    [InlineData(10_485_700L, "10.0 MB")]
    public void SayAFileSizeInKilobytesBelowAMegabyteAndMegabytesToOneDecimalFromThere(long bytes, string size)
    {
        Wording.FileSize(bytes).ShouldBe(size);
    }

    [Fact]
    public void SayASummerCheckInUnitedKingdomTime()
    {
        ConfigRow row = new ConfigRow.Setup(SetupItem.LastUpdateCheck, new SetupState.Checked(new DateTimeOffset(2026, 7, 15, 8, 12, 0, TimeSpan.Zero), new NewestRelease.Found(ReleaseVersions.Version("0.18.0"))));

        Wording.ConfigValue(row, new DateTimeOffset(2026, 7, 15, 10, 0, 0, TimeSpan.Zero), UnitedKingdom).ShouldBe("09:12 today, newest 0.18.0");
    }

    [Fact]
    public void SayACheckFromBeforeTheClocksWentBackInItsOwnOffset()
    {
        ConfigRow row = new ConfigRow.Setup(SetupItem.LastUpdateCheck, new SetupState.Checked(new DateTimeOffset(2026, 10, 24, 8, 12, 0, TimeSpan.Zero), new NewestRelease.None()));

        Wording.ConfigValue(row, new DateTimeOffset(2026, 11, 1, 12, 0, 0, TimeSpan.Zero), UnitedKingdom).ShouldBe("24 October 2026 09:12");
    }

    [Fact]
    public void SayHowEachPieceOfSetupIsChanged()
    {
        Wording.Describe(SetupItem.TerminalProfiles).ShouldBe("Run cader setup-terminal, then restart Windows Terminal, to add the profiles.");
        Enum.GetValues<SetupItem>().Select(Wording.Describe).Distinct().Count().ShouldBe(Enum.GetValues<SetupItem>().Length);
    }

    [Fact]
    public void SayNothingToDescribeAHeading()
    {
        Wording.ConfigDescription(new ConfigRow.Heading(ConfigSection.Setup)).ShouldBeEmpty();
    }

    [Fact]
    public void SayASetupRowIsReadingWhileItIs()
    {
        Wording.ConfigValue(new ConfigRow.Setup(SetupItem.ClaudeBell, new SetupState.Reading()), Now, TimeZoneInfo.Utc).ShouldBe("reading…");
    }

    [Fact]
    public void SayASetupThatIsDisabledIsOff()
    {
        Wording.ConfigValue(new ConfigRow.Setup(SetupItem.ClaudeBell, new SetupState.Disabled()), Now, TimeZoneInfo.Utc).ShouldBe("off");
    }

    [Theory]
    [InlineData(SetupItem.SettingsFile, "This tab writes the settings file: edit a setting above to change it.")]
    [InlineData(SetupItem.ClaudeFolder, "Cader follows CLAUDE_CONFIG_DIR as Claude Code does: start Cader with it set to read another account.")]
    [InlineData(SetupItem.DiagnosticLogFile, "Turn the log on or off with Diagnostic log above; f opens this file, c copies its path and d clears the log.")]
    [InlineData(SetupItem.OlderDiagnosticLogFile, "cader.log becomes this file at 10 MB and a new one starts; f opens it, c copies its path and d clears the log.")]
    [InlineData(SetupItem.TerminalProfiles, "Run cader setup-terminal, then restart Windows Terminal, to add the profiles.")]
    [InlineData(SetupItem.ClaudeBell, "Run cader setup-terminal to make Claude ring the terminal bell when a session needs you.")]
    [InlineData(SetupItem.LastUpdateCheck, "The winget community repository is asked at most once a day; turn the check on or off with Check for updates.")]
    public void SayHowEachNamedPieceOfSetupIsChanged(SetupItem item, string how)
    {
        Wording.Describe(item).ShouldBe(how);
    }

    [Fact]
    public void SayTheConfigStatusMessages()
    {
        Wording.Describe(new StatusMessage.NoPathHere()).ShouldBe("This row's value is not a path.");
        Wording.Describe(new StatusMessage.NoLogToClear()).ShouldBe("There is no log file to delete.");
        Wording.Describe(new StatusMessage.LogFilesNotRead()).ShouldBe("The log files have not been read yet; press d again in a moment.");
        Wording.Describe(new StatusMessage.ClearsOnlyTheLog()).ShouldBe("d clears the diagnostic log on the Diagnostic log file row, and changes nothing here.");
    }

    [Fact]
    public void TitleEachSettingsDialog()
    {
        Wording.SettingsDialogTitle(new SettingsScope.Roots()).ShouldBe("Repository roots");
        Wording.SettingsDialogTitle(new SettingsScope.WorktreeRoot()).ShouldBe("Worktree root");
        Wording.SettingsDialogTitle(new SettingsScope.Editor()).ShouldBe("Open folder with");
        Wording.SettingsDialogTitle(new SettingsScope.Base()).ShouldBe("Worktree base");
        Wording.SettingsDialogTitle(new SettingsScope.Place()).ShouldBe("Default place");
        Wording.SettingsDialogTitle(new SettingsScope.BeforeBranching()).ShouldBe("Before branching");
        Wording.SettingsDialogTitle(new SettingsScope.SessionsRefresh()).ShouldBe("Sessions refresh");
        Wording.SettingsDialogTitle(new SettingsScope.WorktreesRefresh()).ShouldBe("Worktrees refresh");
        Wording.RefreshCaption.ShouldBe("Seconds between refreshes, from 0 to 3600; 0 turns the timer off");
    }

    /// <summary>A new worktree's base is fetched first unless config.json's beforeBranching says None, so no word may say Cader never fetches.</summary>
    [Fact]
    public void SayTheDefaultBaseIsFetchedFirstByDefault()
    {
        string tooltip = Wording.ConfigDescription(new ConfigRow.DefaultBase(WorktreeBase.RemoteDefault));

        tooltip.ShouldBe("New worktrees branch from origin's default branch (see Before branching), or from HEAD.");
        Enum.GetValues<WorktreeBase>().Select(Wording.Describe).ShouldBe(["origin's default branch", "the commit checked out now (HEAD)"]);
        string[] words = [tooltip, .. Enum.GetValues<WorktreeBase>().Select(Wording.Describe), Wording.SettingsDialogTitle(new SettingsScope.Base())];
        words.ShouldAllBe(text => !text.Contains("no fetch") && !text.Contains("never fetches") && !text.Contains("last fetched"));
    }

    [Fact]
    public void SayWhatBeforeBranchingDoes()
    {
        Wording.ConfigDescription(new ConfigRow.BeforeBranching(BeforeBranching.Fetch)).ShouldBe("What happens before a new worktree branches from origin: fetch it, fetch and fast-forward, or nothing.");
        Enum.GetValues<BeforeBranching>().Select(Wording.Describe).ShouldBe(["Fetch origin", "Pull default branch", "Nothing"]);
        Wording.WizardBeforeBranchingCaption.ShouldBe("Before branching (↑↓ to change)");
        Wording.BeforeBranchingCaption.ShouldBe("What happens before a new worktree branches from origin (↑↓ then Space)");
    }

    [Fact]
    public void NameBeforeBranchingInTheDefaultBaseTooltip()
    {
        Wording.ConfigDescription(new ConfigRow.DefaultBase(WorktreeBase.RemoteDefault)).ShouldContain("Before branching");
    }

    [Fact]
    public void NameTheBrowseButtonAndThePicker()
    {
        Wording.BrowseButton.ShouldBe("_Browse… (F3)");
        Wording.FolderPickerTitle(new SettingsScope.Roots()).ShouldBe("Add repository roots");
        Wording.FolderPickerTitle(new SettingsScope.WorktreeRoot()).ShouldBe("Choose the worktree root");
        Wording.FilePickerTitle.ShouldBe("Choose the editor's executable");
        Wording.ProgramFiles.ShouldBe("Programs");
    }

    [Fact]
    public void CaptionThePlaceDialogAndNameEachPlace()
    {
        Wording.PlaceCaption.ShouldBe("Where the wizard opened with n starts a session (↑↓ then Space)");
        Enum.GetValues<LaunchMode>().Select(Wording.Describe).ShouldBe(["Current branch", "New worktree"]);
    }

    [Fact]
    public void NameTheWingetCommunityRepositoryAsTheUpdateSource()
    {
        Wording.ConfigDescription(new ConfigRow.CheckForUpdates(On: true)).ShouldBe("Whether to ask the winget community repository once a day for a newer release, which U then installs.");
        Wording.Describe(SetupItem.LastUpdateCheck).ShouldBe("The winget community repository is asked at most once a day; turn the check on or off with Check for updates.");
        Wording.ConfigDescription(new ConfigRow.Setup(SetupItem.LastUpdateCheck, new SetupState.Never())).ShouldBe("When the winget community repository was last asked for a newer release, and the newest it found.");
    }
}