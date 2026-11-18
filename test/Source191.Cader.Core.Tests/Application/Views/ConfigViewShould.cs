using Source191.Cader.Application;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application.Views;

public sealed class ConfigViewShould
{
    private const string Repos = @"C:\Users\me\source\repos";
    private const string Work = @"D:\work";
    private const string SettingsFile = @"C:\Users\me\AppData\Local\Cader\config.json";
    private const string LogFile = @"C:\Users\me\AppData\Local\Cader\cader.log";

    private static readonly SetupFacts Facts = new(
        new SetupState.At(SettingsFile),
        new SetupState.Folder(TestSettings.DefaultClaudeFolder),
        new SetupState.LogFile(new FakeDiagnosticLogFiles().WithCurrent(300).Read()),
        new SetupState.NotSetUp(),
        new SetupState.Enabled(),
        new SetupState.Never());

    private static readonly ConfigViewState Loaded = ConfigView.Reduce(ConfigViewState.Empty, new ConfigRowsLoaded(ConfigRows.For(TestSettings.For(Repos, Work), Facts, []))).State;

    [Fact]
    public void LoadTheRowsWithTheFirstSelected()
    {
        Loaded.List.Loaded.ShouldBeTrue();
        Loaded.Selected.ShouldBe(new ConfigRow.RepositoryRoot(0, FolderPath.From(Repos)));
        Loaded.Message.ShouldBe(StatusMessage.None);
    }

    [Fact]
    public void ShowALoadFailureAsABannerUntilTheNextLoad()
    {
        ConfigViewState failed = ConfigView.Reduce(Loaded, new LoadFailed("config.json is locked")).State;

        failed.Banner.ShouldBe(new LoadBanner.Failed("config.json is locked"));
        ConfigView.Reduce(failed, new ConfigRowsLoaded(Loaded.List.Rows)).State.Banner.ShouldBe(new LoadBanner.None());
    }

    [Theory]
    [InlineData("root:1")]
    [InlineData("worktree-root")]
    [InlineData("open-folder-with")]
    [InlineData("default-base")]
    [InlineData("before-branching")]
    [InlineData("default-place")]
    public void EditASettingOnEnter(string key)
    {
        ConfigViewState state = At(key);

        ConfigView.Reduce(state, new KeyPressed(Key.Enter)).Effects.ShouldBe([new EditSetting(SettingsScope.For(state.Selected!))]);
    }

    [Fact]
    public void ToggleTheUpdateCheckOnEnter()
    {
        ConfigView.Reduce(At("check-for-updates"), new KeyPressed(Key.Enter)).Effects.ShouldBe([new ToggleUpdateCheck()]);
    }

    [Fact]
    public void ToggleTheDiagnosticLogOnEnter()
    {
        ConfigView.Reduce(At("diagnostic-log"), new KeyPressed(Key.Enter)).Effects.ShouldBe([new ToggleDiagnosticLog()]);
    }

    [Theory]
    [InlineData(SetupItem.SettingsFile)]
    [InlineData(SetupItem.DiagnosticLogFile)]
    [InlineData(SetupItem.TerminalProfiles)]
    [InlineData(SetupItem.ClaudeBell)]
    [InlineData(SetupItem.LastUpdateCheck)]
    public void AdviseOnASetupRowOnEnter(SetupItem item)
    {
        ViewUpdate<ConfigViewState> update = ConfigView.Reduce(At($"setup:{item}"), new KeyPressed(Key.Enter));

        update.Effects.ShouldBeEmpty();
        update.State.Message.ShouldBe(new StatusMessage.SetupAdvice(item));
    }

    [Theory]
    [InlineData("root:0")]
    [InlineData("root:1")]
    public void EditEveryRootTogetherFromAnyRootRow(string key)
    {
        ConfigView.Reduce(At(key), new KeyPressed(Key.Enter)).Effects.ShouldBe([new EditSetting(new SettingsScope.Roots())]);
    }

    [Theory]
    [InlineData('a')]
    [InlineData('d')]
    public void LeaveAddingAndRemovingRootsToTheDialog(char key)
    {
        ConfigView.Reduce(At("root:1"), new KeyPressed(Key.Of(key))).Effects.ShouldBeEmpty();
    }

    [Fact]
    public void CopyAPathWhole()
    {
        ConfigView.Reduce(At("root:1"), new KeyPressed(Key.Of('c'))).Effects.ShouldBe([new CopyFolderPath(FolderPath.From(Work))]);
        ConfigView.Reduce(At("setup:SettingsFile"), new KeyPressed(Key.Of('c'))).Effects.ShouldBe([new CopyFolderPath(FolderPath.From(SettingsFile))]);
    }

    [Fact]
    public void OpenAFolderOrTheFolderHoldingAFile()
    {
        ConfigView.Reduce(At("worktree-root"), new KeyPressed(Key.Of('f'))).Effects.ShouldBe([new OpenFolderInEditor(FolderPath.From(@"C:\Users\me\source\worktrees"))]);
        string fragment = @"C:\Users\me\AppData\Local\Microsoft\Windows Terminal\Fragments\Cader\cader.json";
        SetupFacts installed = Facts with { TerminalProfiles = new SetupState.At(fragment) };
        ConfigViewState loaded = ConfigView.Reduce(ConfigViewState.Empty, new ConfigRowsLoaded(ConfigRows.For(TestSettings.For(Repos, Work), installed, []))).State;

        ConfigView.Reduce(loaded with { List = loaded.List.Select("setup:TerminalProfiles") }, new KeyPressed(Key.Of('f'))).Effects.ShouldBe([new OpenFolderInEditor(FolderPath.From(@"C:\Users\me\AppData\Local\Microsoft\Windows Terminal\Fragments\Cader"))]);
    }

    [Fact]
    public void CopyAndOpenTheFragmentsFolderWhileTerminalHidesAProfile()
    {
        string fragment = @"C:\Users\me\AppData\Local\Microsoft\Windows Terminal\Fragments\Cader\cader.json";
        SetupFacts hidden = Facts with { TerminalProfiles = new SetupState.HiddenByTerminal(fragment) };
        ConfigViewState loaded = ConfigView.Reduce(ConfigViewState.Empty, new ConfigRowsLoaded(ConfigRows.For(TestSettings.For(Repos, Work), hidden, []))).State;
        ConfigViewState onRow = loaded with { List = loaded.List.Select("setup:TerminalProfiles") };

        ConfigView.Reduce(onRow, new KeyPressed(Key.Of('c'))).Effects.ShouldBe([new CopyFolderPath(FolderPath.From(fragment))]);
        ConfigView.Reduce(onRow, new KeyPressed(Key.Of('f'))).Effects.ShouldBe([new OpenFolderInEditor(FolderPath.From(@"C:\Users\me\AppData\Local\Microsoft\Windows Terminal\Fragments\Cader"))]);
    }

    [Fact]
    public void CopyAndOpenTheClaudeFolderItself()
    {
        ConfigView.Reduce(At("setup:ClaudeFolder"), new KeyPressed(Key.Of('c'))).Effects.ShouldBe([new CopyFolderPath(FolderPath.From(@"C:\Users\me\.claude"))]);
        ConfigView.Reduce(At("setup:ClaudeFolder"), new KeyPressed(Key.Of('f'))).Effects.ShouldBe([new OpenFolderInEditor(FolderPath.From(@"C:\Users\me\.claude"))]);
    }

    [Fact]
    public void OpenTheSettingsFileAndTheDiagnosticLogFileThemselves()
    {
        ConfigView.Reduce(At("setup:SettingsFile"), new KeyPressed(Key.Of('f'))).Effects.ShouldBe([new OpenFileInEditor(FolderPath.From(SettingsFile))]);
        ConfigView.Reduce(At("setup:DiagnosticLogFile"), new KeyPressed(Key.Of('f'))).Effects.ShouldBe([new OpenFileInEditor(FolderPath.From(LogFile))]);
    }

    [Fact]
    public void CopyAndOpenTheOlderLogFileItself()
    {
        ConfigViewState older = WithOlderLog() with { List = WithOlderLog().List.Select("setup:OlderDiagnosticLogFile") };

        ConfigView.Reduce(older, new KeyPressed(Key.Of('c'))).Effects.ShouldBe([new CopyFolderPath(FolderPath.From(LogFile + ".1"))]);
        ConfigView.Reduce(older, new KeyPressed(Key.Of('f'))).Effects.ShouldBe([new OpenFileInEditor(FolderPath.From(LogFile + ".1"))]);
    }

    [Theory]
    [InlineData("setup:DiagnosticLogFile")]
    [InlineData("setup:OlderDiagnosticLogFile")]
    public void AskToClearTheLogFromEitherLogRow(string key)
    {
        ConfigViewState state = WithOlderLog() with { List = WithOlderLog().List.Select(key) };

        ViewUpdate<ConfigViewState> update = ConfigView.Reduce(state, new KeyPressed(Key.Of('d')));

        update.Effects.ShouldBe([new ClearDiagnosticLog(new FakeDiagnosticLogFiles().WithCurrent(300).WithOlder(10_485_700).Read())]);
        update.State.Message.ShouldBe(StatusMessage.None);
    }

    [Fact]
    public void AskToClearALogThatIsOn()
    {
        LogFiles files = new FakeDiagnosticLogFiles().WithCurrent(2_516_582).Read();
        SetupFacts facts = Facts with { DiagnosticLogFile = new SetupState.LogFile(files) };
        ConfigViewState loaded = ConfigView.Reduce(ConfigViewState.Empty, new ConfigRowsLoaded(ConfigRows.For(TestSettings.For(Repos, Work).WithDiagnosticLogToggled(), facts, []))).State;

        ConfigView.Reduce(loaded with { List = loaded.List.Select("setup:DiagnosticLogFile") }, new KeyPressed(Key.Of('d'))).Effects.ShouldBe([new ClearDiagnosticLog(files)]);
    }

    [Fact]
    public void AskToClearALogThatIsOff()
    {
        ConfigView.Reduce(At("setup:DiagnosticLogFile"), new KeyPressed(Key.Of('d'))).Effects.ShouldBe([new ClearDiagnosticLog(new FakeDiagnosticLogFiles().WithCurrent(300).Read())]);
    }

    [Fact]
    public void SayThereIsNoLogFileToClear()
    {
        SetupFacts facts = Facts with { DiagnosticLogFile = new SetupState.LogFile(new FakeDiagnosticLogFiles().Read()) };
        ConfigViewState loaded = ConfigView.Reduce(ConfigViewState.Empty, new ConfigRowsLoaded(ConfigRows.For(TestSettings.For(Repos, Work), facts, []))).State;

        ViewUpdate<ConfigViewState> update = ConfigView.Reduce(loaded with { List = loaded.List.Select("setup:DiagnosticLogFile") }, new KeyPressed(Key.Of('d')));

        update.Effects.ShouldBeEmpty();
        update.State.Message.ShouldBe(new StatusMessage.NoLogToClear());
    }

    [Fact]
    public void SayTheLogFilesAreNotReadYet()
    {
        ConfigViewState loaded = ConfigView.Reduce(ConfigViewState.Empty, new ConfigRowsLoaded(ConfigRows.For(TestSettings.For(Repos, Work), SetupFacts.Reading, []))).State;

        ViewUpdate<ConfigViewState> update = ConfigView.Reduce(loaded with { List = loaded.List.Select("setup:DiagnosticLogFile") }, new KeyPressed(Key.Of('d')));

        update.Effects.ShouldBeEmpty();
        update.State.Message.ShouldBe(new StatusMessage.LogFilesNotRead());
    }

    [Theory]
    [InlineData("worktree-root")]
    [InlineData("diagnostic-log")]
    [InlineData("setup:SettingsFile")]
    public void SayDClearsOnlyTheLogAndChangeNothingElsewhere(string key)
    {
        ConfigViewState state = At(key);

        ViewUpdate<ConfigViewState> update = ConfigView.Reduce(state, new KeyPressed(Key.Of('d')));

        update.Effects.ShouldBeEmpty();
        update.State.ShouldBe(state with { Message = new StatusMessage.ClearsOnlyTheLog() });
    }

    private static ConfigViewState WithOlderLog()
    {
        SetupFacts facts = Facts with { DiagnosticLogFile = new SetupState.LogFile(new FakeDiagnosticLogFiles().WithCurrent(300).WithOlder(10_485_700).Read()) };

        return ConfigView.Reduce(ConfigViewState.Empty, new ConfigRowsLoaded(ConfigRows.For(TestSettings.For(Repos, Work), facts, []))).State;
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void CopyTheDiagnosticLogFilesPathWhateverTheSettingAndTheFile(bool settingOn, bool fileExists)
    {
        Settings settings = settingOn ? TestSettings.For(Repos, Work).WithDiagnosticLogToggled() : TestSettings.For(Repos, Work);
        SetupFacts facts = Facts with { DiagnosticLogFile = new SetupState.LogFile(fileExists ? new FakeDiagnosticLogFiles().WithCurrent(300).Read() : new FakeDiagnosticLogFiles().Read()) };
        ConfigViewState loaded = ConfigView.Reduce(ConfigViewState.Empty, new ConfigRowsLoaded(ConfigRows.For(settings, facts, []))).State;

        ConfigView.Reduce(loaded with { List = loaded.List.Select("setup:DiagnosticLogFile") }, new KeyPressed(Key.Of('c'))).Effects.ShouldBe([new CopyFolderPath(FolderPath.From(LogFile))]);
    }

    [Fact]
    public void FilterTheRowsByAPath()
    {
        ConfigViewState filtered = Press(Loaded, Key.Slash, Key.Of('w'), Key.Of('o'), Key.Of('r'), Key.Of('k'), Key.Enter);

        filtered.List.Visible.Select(row => row.Key).ShouldBe(["root:1", "worktree-root"]);
    }

    [Theory]
    [InlineData('c')]
    [InlineData('f')]
    public void SayWhenTheValueIsNotAPath(char key)
    {
        ViewUpdate<ConfigViewState> update = ConfigView.Reduce(At("open-folder-with"), new KeyPressed(Key.Of(key)));

        update.Effects.ShouldBeEmpty();
        update.State.Message.ShouldBe(new StatusMessage.NoPathHere());
    }

    [Fact]
    public void ChooseATabWithTheTabKeys()
    {
        ConfigView.Reduce(Loaded, new KeyPressed(Key.Of('2'))).Effects.OfType<SelectTab>().ShouldBeEmpty();
        ConfigView.Reduce(Loaded, new KeyPressed(Key.Left)).Effects.ShouldBe([new PreviousTab()]);
        ConfigView.Reduce(Loaded, new KeyPressed(Key.Right)).Effects.ShouldBe([new NextTab()]);
        ConfigView.Reduce(Loaded, new KeyPressed(Key.Escape)).Effects.ShouldBe([new SelectTab(ConsoleTab.Sessions)]);
    }

    [Theory]
    [InlineData('w')]
    [InlineData(',')]
    [InlineData('e')]
    [InlineData('v')]
    public void IgnoreTheRetiredKeys(char key)
    {
        ViewUpdate<ConfigViewState> update = ConfigView.Reduce(Loaded, new KeyPressed(Key.Of(key)));

        update.Effects.ShouldBeEmpty();
        update.State.ShouldBe(Loaded);
    }

    [Fact]
    public void ClearAFilterOnEscapeBeforeLeaving()
    {
        ConfigViewState filtered = Press(Loaded, Key.Slash, Key.Of('t'), Key.Of('r'), Key.Of('e'), Key.Of('e'), Key.Enter);

        filtered.Selected.ShouldBe(new ConfigRow.WorktreeRoot(FolderPath.From(@"C:\Users\me\source\worktrees")));
        ViewUpdate<ConfigViewState> update = ConfigView.Reduce(filtered, new KeyPressed(Key.Escape));

        update.Effects.ShouldBeEmpty();
        update.State.List.Filter.Text.ShouldBeEmpty();
    }

    [Theory]
    [InlineData('q', typeof(Quit))]
    [InlineData('?', typeof(ShowHelp))]
    [InlineData('!', typeof(ShowProblemReport))]
    [InlineData('U', typeof(SelfUpdate))]
    public void EmitTheEffectForTheKey(char key, Type effect)
    {
        ConfigView.Reduce(Loaded, new KeyPressed(Key.Of(key))).Effects.Single().ShouldBeOfType(effect);
    }

    [Fact]
    public void KeepTheSelectionWhenTheRowsReload()
    {
        ConfigViewState state = At("worktree-root");

        ConfigViewState reloaded = ConfigView.Reduce(state, new ConfigRowsLoaded(ConfigRows.For(TestSettings.For([Repos, Work], @"E:\trees"), Facts, []))).State;

        reloaded.Selected.ShouldBe(new ConfigRow.WorktreeRoot(FolderPath.From(@"E:\trees")));
    }

    [Fact]
    public void MoveTheSelectionAndClearTheMessage()
    {
        ConfigViewState advised = ConfigView.Reduce(At("setup:ClaudeBell"), new KeyPressed(Key.Enter)).State;

        ConfigViewState moved = Press(advised, Key.Down);

        moved.Selected.ShouldNotBeNull().Key.ShouldBe("setup:LastUpdateCheck");
        moved.Message.ShouldBe(StatusMessage.None);
    }

    [Fact]
    public void ShowNoMessageForTheSelectedRowAlone()
    {
        ConfigViewState onWorktreeRoot = Press(Loaded, Key.Down, Key.Down);
        ConfigViewState advised = ConfigView.Reduce(At("setup:ClaudeBell"), new KeyPressed(Key.Enter)).State;

        onWorktreeRoot.Message.ShouldBe(StatusMessage.None);
        ConfigViewState moved = Press(advised, Key.Down);

        moved.Message.ShouldBe(StatusMessage.None);
    }

    [Fact]
    public void ClearAMessageWhenTheFilterChanges()
    {
        ConfigViewState advised = ConfigView.Reduce(At("setup:ClaudeBell"), new KeyPressed(Key.Enter)).State;

        Press(advised, Key.Of('/')).Message.ShouldBe(StatusMessage.None);
        Press(Press(advised, Key.Of('/')) with { Message = new StatusMessage.Kept() }, Key.Of('w')).Message.ShouldBe(StatusMessage.None);

        ConfigViewState filtered = Press(advised, Key.Of('/'), Key.Of('w'), Key.Enter) with { Message = new StatusMessage.Kept() };
        ConfigViewState cleared = Press(filtered, Key.Escape);
        cleared.List.Filter.Text.ShouldBeEmpty();
        cleared.Message.ShouldBe(StatusMessage.None);
    }

    [Fact]
    public void StepOverTheSetupHeadingOnDown()
    {
        ConfigViewState down = ConfigView.Reduce(At("diagnostic-log"), new KeyPressed(Key.Down)).State;

        down.Selected.ShouldBe(new ConfigRow.Setup(SetupItem.SettingsFile, new SetupState.At(SettingsFile)));
    }

    [Fact]
    public void StepOverTheSetupHeadingOnUp()
    {
        ConfigViewState up = ConfigView.Reduce(At("setup:SettingsFile"), new KeyPressed(Key.Up)).State;

        up.Selected.ShouldBe(new ConfigRow.DiagnosticLog(On: false));
    }

    [Fact]
    public void HideTheSetupHeadingWhileFiltering()
    {
        ConfigViewState filtered = Press(Loaded, Key.Slash, Key.Of('b'), Key.Of('e'), Key.Of('l'), Key.Of('l'), Key.Enter);

        filtered.List.Visible.ShouldBe([new ConfigRow.Setup(SetupItem.ClaudeBell, new SetupState.Enabled())]);
        Press(Loaded, Key.Slash, Key.Of('s'), Key.Of('e'), Key.Of('t'), Key.Of('u'), Key.Of('p'), Key.Enter).List.Visible.ShouldBeEmpty();
    }

    [Fact]
    public void LeaveTheSetupHeadingOutOfTheTitle()
    {
        ConfigViewState oneRoot = ConfigView.Reduce(ConfigViewState.Empty, new ConfigRowsLoaded(ConfigRows.For(TestSettings.For(Repos), Facts, []))).State;

        oneRoot.Title.Shown.ShouldBe(14);
        oneRoot.Title.Total.ShouldBe(14);
    }

    [Fact]
    public void ClearTheMessageItWasAskedToClear()
    {
        StatusMessage message = new StatusMessage.NoPathHere();
        ConfigViewState shown = Loaded with { Message = message };

        ConfigView.Reduce(shown, new MessageDismissed(message)).State.Message.ShouldBe(StatusMessage.None);
    }

    [Fact]
    public void KeepANewerMessageWithTheSameWords()
    {
        StatusMessage older = new StatusMessage.NoPathHere();
        StatusMessage newer = new StatusMessage.NoPathHere();
        ConfigViewState shown = Loaded with { Message = newer };

        ConfigView.Reduce(shown, new MessageDismissed(older)).State.Message.ShouldBeSameAs(newer);
    }

    private static readonly KnownAccount First = new(OrganisationId.From("6f1c2a8e-4b7d-4c1e-9a3f-2d5e8b0c7a91"), "me@northwind.example", "Northwind Traders", AccountLabel.From("1"));
    private static readonly KnownAccount Personal = new(OrganisationId.From("0b9d4e2f-7a6c-4f3e-8d1b-5c2a9e7f4b60"), "me@home.example", "Personal", AccountLabel.From("personal"));

    private static readonly ConfigViewState WithAccounts = ConfigView.Reduce(
        ConfigViewState.Empty,
        new ConfigRowsLoaded(ConfigRows.For(TestSettings.For(Repos, Work), Facts, [new RowAccount.Known(First, LoggedInNow: true), new RowAccount.Known(Personal, LoggedInNow: false)]))).State;

    private static string AccountKey(KnownAccount account) => new ConfigRow.Account(new RowAccount.Known(account, LoggedInNow: false)).Key;

    [Fact]
    public void StepOverTheAccountsHeading()
    {
        ConfigViewState lastSetup = WithAccounts with { List = WithAccounts.List.Select("setup:LastUpdateCheck") };

        Press(lastSetup, Key.Down).Selected.ShouldBe(new ConfigRow.Account(new RowAccount.Known(First, LoggedInNow: true)));
        Press(lastSetup, Key.Down, Key.Up).Selected?.Key.ShouldBe("setup:LastUpdateCheck");
    }

    [Theory]
    [InlineData("northwind")]
    [InlineData("Northwind Traders")]
    [InlineData("Account 1")]
    public void FindAnAccountByLabelEmailOrOrganisation(string filter)
    {
        ConfigViewState filtered = WithAccounts with { List = WithAccounts.List.StartFilter() };
        filtered = filter.Aggregate(filtered, (state, character) => ConfigView.Reduce(state, new KeyPressed(Key.Of(character))).State);

        filtered.List.Visible.ShouldContain(row => row.Key == AccountKey(First));
        filtered.List.Visible.ShouldNotContain(row => row is ConfigRow.Heading);
    }

    [Theory]
    [InlineData('c')]
    [InlineData('f')]
    public void SayAnAccountRowHoldsNoPath(char key)
    {
        ConfigViewState account = WithAccounts with { List = WithAccounts.List.Select(AccountKey(First)) };

        ViewUpdate<ConfigViewState> update = ConfigView.Reduce(account, new KeyPressed(Key.Of(key)));

        update.Effects.ShouldBeEmpty();
        update.State.Message.ShouldBe(new StatusMessage.NoPathHere());
    }

    [Fact]
    public void AskForANewLabelOnEnter()
    {
        ConfigViewState account = WithAccounts with { List = WithAccounts.List.Select(AccountKey(First)) };

        ConfigView.Reduce(account, new KeyPressed(Key.Enter)).Effects.ShouldBe([new AskForAccountLabel(First)]);
    }

    [Fact]
    public void RenameWithAGoodLabel()
    {
        ViewUpdate<ConfigViewState> update = ConfigView.Reduce(WithAccounts, new AccountLabelConfirmed(First, " work "));

        update.Effects.ShouldBe([new RenameAccount(First, AccountLabel.From("work"))]);
    }

    [Fact]
    public void RenameToTheSameLabelInAnotherCase()
    {
        ConfigView.Reduce(WithAccounts, new AccountLabelConfirmed(Personal, "Personal")).Effects.ShouldBe([new RenameAccount(Personal, AccountLabel.From("Personal"))]);
    }

    [Theory]
    [InlineData("   ", AccountLabelProblem.Blank)]
    [InlineData("northwind", AccountLabelProblem.TooLong)]
    [InlineData("wo\u0007rk", AccountLabelProblem.ControlCharacter)]
    [InlineData("PERSONAL", AccountLabelProblem.Taken)]
    public void RefuseALabelThatBreaksARule(string label, AccountLabelProblem problem)
    {
        ViewUpdate<ConfigViewState> update = ConfigView.Reduce(WithAccounts, new AccountLabelConfirmed(First, label));

        update.Effects.ShouldBeEmpty();
        update.State.Message.ShouldBe(new StatusMessage.InvalidAccountLabel(problem));
    }

    [Fact]
    public void SayARenameWasCancelled()
    {
        ConfigView.Reduce(WithAccounts, new AccountLabelCancelled()).State.Message.ShouldBe(new StatusMessage.RenameCancelled());
    }

    private static ConfigViewState At(string key)
    {
        ConfigViewState state = Loaded with { List = Loaded.List.Select(key) };
        state.Selected?.Key.ShouldBe(key, "the row named is the one selected");

        return state;
    }

    private static ConfigViewState Press(ConfigViewState state, params Key[] keys) => keys.Aggregate(state, (current, key) => ConfigView.Reduce(current, new KeyPressed(key)).State);

    [Fact]
    public void CallItsEntriesRows()
    {
        Loaded.Title.Noun.ShouldBe("row");
    }

    [Fact]
    public void ShowNoRowWhoseSettingAndPathAreNotWhatWasTyped()
    {
        ConfigViewState filtered = Press(Loaded, Key.Slash, Key.Of('s'), Key.Of('t'), Key.Of('r'), Key.Of('y'), Key.Of('k'), Key.Of('e'), Key.Of('r'), Key.Enter);

        filtered.List.Visible.ShouldBeEmpty("a row with no path has nothing of its own beyond its setting to match");
    }
}