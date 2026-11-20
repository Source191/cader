using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Domain;

public sealed class SettingsShould
{
    private static readonly FolderPath Home = FolderPath.From(@"C:\Users\me");
    private const string Root = @"C:\Users\me\source\repos";

    private static readonly SettingsText Typed = new(
        Root,
        @"C:\Users\me\source\worktrees",
        EditorPreset.VsCode,
        Executable: string.Empty,
        Arguments: string.Empty,
        WorktreeBase.RemoteDefault,
        CheckForUpdates: Settings.ChecksForUpdatesWhenUnset,
        Place: Settings.PlaceWhenUnset,
        DiagnosticLog: Settings.DiagnosticLogWhenUnset,
        BeforeBranching: Settings.BeforeBranchingWhenUnset,
        SessionsRefresh: "5",
        WorktreesRefresh: "15");

    [Fact]
    public void ResolveAMissingFileToEveryDefault()
    {
        Settings settings = Settings.Resolve(roots: [], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.VsCode, Settings.BaseWhenUnset, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset);

        settings.RepositoryRoots.ShouldBe([FolderPath.From(Root)]);
        settings.WorktreeRoot.ShouldBe(FolderPath.From(@"C:\Users\me\source\worktrees"));
        settings.Editor.ShouldBe(EditorChoice.VsCode);
        settings.DefaultBase.ShouldBe(WorktreeBase.RemoteDefault);
        settings.DefaultPlace.ShouldBe(LaunchMode.NewWorktree);
    }

    [Fact]
    public void NameTheDefaultsForWhatAFileLeavesOut()
    {
        Settings.BaseWhenUnset.ShouldBe(WorktreeBase.RemoteDefault);
        Settings.ChecksForUpdatesWhenUnset.ShouldBeTrue();
        Settings.PlaceWhenUnset.ShouldBe(LaunchMode.NewWorktree);
    }

    [Fact]
    public void ResolveAFileFromAnEarlierVersion()
    {
        Settings settings = Settings.Resolve([FolderPath.From(@"D:\work")], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.VsCode, WorktreeBase.Head, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset);

        settings.RepositoryRoots.ShouldBe([FolderPath.From(@"D:\work")]);
        settings.WorktreeRoot.ShouldBe(FolderPath.From(@"D:\worktrees"));
        settings.Editor.ShouldBe(EditorChoice.VsCode);
        settings.DefaultBase.ShouldBe(WorktreeBase.Head);
        settings.DefaultPlace.ShouldBe(LaunchMode.NewWorktree);
    }

    [Fact]
    public void KeepADefaultPlaceThatIsGiven()
    {
        Settings settings = Settings.Resolve(roots: [], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.VsCode, WorktreeBase.RemoteDefault, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: LaunchMode.CurrentBranch, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset);

        settings.DefaultPlace.ShouldBe(LaunchMode.CurrentBranch);
        settings.ShouldNotBe(Settings.Resolve(roots: [], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.VsCode, WorktreeBase.RemoteDefault, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset));
    }

    [Fact]
    public void ResolveAnEmptyRootListToTheDefaultRoot()
    {
        Settings settings = Settings.Resolve([], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.VsCode, WorktreeBase.RemoteDefault, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset);

        settings.RepositoryRoots.ShouldBe([FolderPath.From(Root)]);
    }

    [Fact]
    public void PutTheWorktreeRootInsideARootWithNoParent()
    {
        Settings settings = Settings.Resolve([FolderPath.From(@"D:\")], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.VsCode, WorktreeBase.RemoteDefault, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset);

        settings.WorktreeRoot.ShouldBe(FolderPath.From(@"D:\worktrees"));
    }

    [Fact]
    public void KeepAWorktreeRootThatIsGiven()
    {
        Settings settings = Settings.Resolve([FolderPath.From(Root)], new WorktreeRootChoice.At(FolderPath.From(@"E:\trees")), EditorChoice.VsCode, WorktreeBase.RemoteDefault, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset);

        settings.WorktreeRoot.ShouldBe(FolderPath.From(@"E:\trees"));
    }

    [Fact]
    public void CompareByValue()
    {
        Settings first = Settings.Resolve([FolderPath.From(Root)], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.Custom(@"C:\Tools\subl.exe", ["{folder}"]), WorktreeBase.RemoteDefault, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset);
        Settings second = Settings.Resolve([FolderPath.From(Root)], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.Custom(@"C:\Tools\subl.exe", ["{folder}"]), WorktreeBase.RemoteDefault, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset);

        first.ShouldBe(second);
    }

    [Fact]
    public void AcceptTypedSettings()
    {
        SettingsText typed = Typed with { Roots = $"{Root}\r\n\r\n  D:\\work  \n", Base = WorktreeBase.Head, Place = LaunchMode.CurrentBranch };

        Settings settings = Settings.Validate(typed, Exists(Root, @"D:\work"), Installed).ShouldBeOfType<SettingsValidation.Valid>().Settings;

        settings.RepositoryRoots.ShouldBe([FolderPath.From(Root), FolderPath.From(@"D:\work")]);
        settings.WorktreeRoot.ShouldBe(FolderPath.From(@"C:\Users\me\source\worktrees"));
        settings.DefaultBase.ShouldBe(WorktreeBase.Head);
        settings.DefaultPlace.ShouldBe(LaunchMode.CurrentBranch);
    }

    [Fact]
    public void KeepTheDefaultPlaceForACustomEditor()
    {
        SettingsText typed = Typed with { Preset = EditorPreset.Custom, Executable = @"C:\Tools\subl.exe", Arguments = "{folder}", Place = LaunchMode.CurrentBranch };

        Settings.Validate(typed, Exists(Root), Installed).ShouldBeOfType<SettingsValidation.Valid>().Settings.DefaultPlace.ShouldBe(LaunchMode.CurrentBranch);
    }

    [Fact]
    public void RefuseNoRoots()
    {
        Refusal(Typed with { Roots = " \n " }).ShouldBe(new SettingsProblem.NoRoots());
    }

    [Fact]
    public void RefuseARelativeRoot()
    {
        Refusal(Typed with { Roots = "repos" }).ShouldBe(new SettingsProblem.RootNotFullPath("repos"));
    }

    [Fact]
    public void RefuseAMissingRoot()
    {
        Refusal(Typed with { Roots = $"{Root}\nD:\\nowhere" }).ShouldBe(new SettingsProblem.RootMissing(FolderPath.From(@"D:\nowhere")));
    }

    [Fact]
    public void RefuseARelativeWorktreeRoot()
    {
        Refusal(Typed with { WorktreeRoot = "worktrees" }).ShouldBe(new SettingsProblem.WorktreeRootNotFullPath());
    }

    [Fact]
    public void AcceptAWorktreeRootThatDoesNotExistYet()
    {
        Settings.Validate(Typed with { WorktreeRoot = @"D:\worktrees" }, Exists(Root), Installed)
            .ShouldBeOfType<SettingsValidation.Valid>().Settings.WorktreeRoot.ShouldBe(FolderPath.From(@"D:\worktrees"));
    }

    [Fact]
    public void RefuseACustomEditorWithNoExecutable()
    {
        Refusal(Typed with { Preset = EditorPreset.Custom, Executable = "  ", Arguments = "{folder}" }).ShouldBe(new SettingsProblem.NoExecutable());
    }

    [Theory]
    [InlineData(@"C:\Tools\edit.cmd")]
    [InlineData(@"C:\Tools\EDIT.BAT ")]
    public void RefuseABatchFileEditor(string executable)
    {
        Refusal(Typed with { Preset = EditorPreset.Custom, Executable = executable, Arguments = "{folder}" }).ShouldBe(new SettingsProblem.BatchFileExecutable());
    }

    [Theory]
    [InlineData("subl")]
    [InlineData("subl.exe")]
    [InlineData(@"Tools\subl.exe")]
    public void RefuseACustomEditorThatIsNotAFullPath(string executable)
    {
        Refusal(Typed with { Preset = EditorPreset.Custom, Executable = executable, Arguments = "{folder}" }).ShouldBe(new SettingsProblem.ExecutableNotFullPath());
    }

    [Fact]
    public void RefuseACustomEditorWithNoPlaceholder()
    {
        Refusal(Typed with { Preset = EditorPreset.Custom, Executable = @"C:\Tools\subl.exe", Arguments = "-n" }).ShouldBe(new SettingsProblem.NoFolderPlaceholder());
    }

    [Fact]
    public void RefuseACustomEditorWhoseExecutableIsMissing()
    {
        Refusal(Typed with { Preset = EditorPreset.Custom, Executable = @"C:\Tools\gone.exe", Arguments = "-n {folder}" }).ShouldBe(new SettingsProblem.ExecutableMissing(@"C:\Tools\gone.exe"));
    }

    [Fact]
    public void AcceptACustomEditorWithItsArguments()
    {
        SettingsText typed = Typed with { Preset = EditorPreset.Custom, Executable = @" C:\Tools\subl.exe ", Arguments = "-n {folder}" };

        Settings.Validate(typed, Exists(Root), Installed).ShouldBeOfType<SettingsValidation.Valid>()
            .Settings.Editor.ShouldBe(EditorChoice.Custom(@"C:\Tools\subl.exe", ["-n", "{folder}"]));
    }

    [Fact]
    public void IgnoreTheCustomFieldsForAPreset()
    {
        SettingsText typed = Typed with { Preset = EditorPreset.Rider, Executable = @"C:\Tools\edit.cmd", Arguments = "-n" };

        Settings.Validate(typed, Exists(Root), Installed).ShouldBeOfType<SettingsValidation.Valid>().Settings.Editor.ShouldBe(EditorChoice.Of(EditorPreset.Rider));
    }

    [Fact]
    public void RefuseARootListedTwice()
    {
        SettingsText typed = Typed with { Roots = $"{Root}\n" + @"C:\Users\ME\source\repos\" };

        Settings.Validate(typed, Exists(Root), Installed).ShouldBeOfType<SettingsValidation.Refused>().Problem.ShouldBe(new SettingsProblem.RootAlreadyListed(FolderPath.From(Root)));
    }

    [Fact]
    public void TurnTheUpdateCheckOffAndOn()
    {
        Settings settings = Settings.Resolve(roots: [], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.VsCode, WorktreeBase.RemoteDefault, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset);

        settings.WithUpdateCheckToggled().CheckForUpdates.ShouldBeFalse();
        settings.WithUpdateCheckToggled().WithUpdateCheckToggled().ShouldBe(settings);
        Settings.Resolve(roots: [], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.VsCode, WorktreeBase.RemoteDefault, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: LaunchMode.CurrentBranch, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset).WithUpdateCheckToggled().DefaultPlace.ShouldBe(LaunchMode.CurrentBranch);
    }

    [Fact]
    public void GiveBackTheTextItWasTypedFrom()
    {
        Settings settings = Settings.Resolve([FolderPath.From(Root), FolderPath.From(@"D:\work")], new WorktreeRootChoice.At(FolderPath.From(@"E:\trees")), EditorChoice.Custom(@"C:\Tools\subl.exe", ["-n", "{folder}"]), WorktreeBase.Head, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: LaunchMode.CurrentBranch, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset);

        settings.ToText().ShouldBe(new SettingsText($"{Root}\nD:\\work", @"E:\trees", EditorPreset.Custom, @"C:\Tools\subl.exe", "-n {folder}", WorktreeBase.Head, CheckForUpdates: Settings.ChecksForUpdatesWhenUnset, Place: LaunchMode.CurrentBranch, DiagnosticLog: Settings.DiagnosticLogWhenUnset, BeforeBranching: Settings.BeforeBranchingWhenUnset, SessionsRefresh: "5", WorktreesRefresh: "15"));
    }

    [Fact]
    public void CheckForUpdatesUnlessTurnedOff()
    {
        Settings.Resolve(roots: [], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.VsCode, WorktreeBase.RemoteDefault, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset).CheckForUpdates.ShouldBeTrue();
        Settings off = Settings.Resolve(roots: [], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.VsCode, WorktreeBase.RemoteDefault, Home, checkForUpdates: false, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset);

        off.CheckForUpdates.ShouldBeFalse();
        off.ToText().CheckForUpdates.ShouldBeFalse();
        off.ShouldNotBe(Settings.Resolve(roots: [], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.VsCode, WorktreeBase.RemoteDefault, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset));
    }

    [Fact]
    public void LeaveTheDiagnosticLogOffUnlessTurnedOn()
    {
        Settings.Resolve(roots: [], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.VsCode, WorktreeBase.RemoteDefault, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset).DiagnosticLog.ShouldBeFalse();
        Settings on = Settings.Resolve(roots: [], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.VsCode, WorktreeBase.RemoteDefault, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: true, beforeBranching: Settings.BeforeBranchingWhenUnset, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset);

        on.DiagnosticLog.ShouldBeTrue();
        on.ToText().DiagnosticLog.ShouldBeTrue();
        on.ShouldNotBe(Settings.Resolve(roots: [], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.VsCode, WorktreeBase.RemoteDefault, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset));
    }

    [Fact]
    public void TurnTheDiagnosticLogOnAndOff()
    {
        Settings settings = Settings.Resolve(roots: [], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.VsCode, WorktreeBase.RemoteDefault, Home, checkForUpdates: false, defaultPlace: LaunchMode.CurrentBranch, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset);

        settings.WithDiagnosticLogToggled().DiagnosticLog.ShouldBeTrue();
        settings.WithDiagnosticLogToggled().WithDiagnosticLogToggled().ShouldBe(settings);
        settings.WithDiagnosticLogToggled().CheckForUpdates.ShouldBeFalse();
        settings.WithDiagnosticLogToggled().DefaultPlace.ShouldBe(LaunchMode.CurrentBranch);
        settings.WithUpdateCheckToggled().WithDiagnosticLogToggled().WithUpdateCheckToggled().DiagnosticLog.ShouldBeTrue();
    }

    [Fact]
    public void KeepTheDiagnosticLogThroughAValidatedDialog()
    {
        Settings.Validate(Typed with { DiagnosticLog = true }, Exists(Root), Installed).ShouldBeOfType<SettingsValidation.Valid>().Settings.DiagnosticLog.ShouldBeTrue();
        Settings.Validate(Typed, Exists(Root), Installed).ShouldBeOfType<SettingsValidation.Valid>().Settings.DiagnosticLog.ShouldBeFalse();
    }

    [Fact]
    public void FetchBeforeBranchingUnlessToldOtherwise()
    {
        Settings.BeforeBranchingWhenUnset.ShouldBe(BeforeBranching.Fetch);
        Settings.Resolve(roots: [], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.VsCode, WorktreeBase.RemoteDefault, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset).BeforeBranching.ShouldBe(BeforeBranching.Fetch);
        Settings pull = Settings.Resolve(roots: [], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.VsCode, WorktreeBase.RemoteDefault, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: BeforeBranching.Pull, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset);

        pull.BeforeBranching.ShouldBe(BeforeBranching.Pull);
        pull.ToText().BeforeBranching.ShouldBe(BeforeBranching.Pull);
        pull.ShouldNotBe(Settings.Resolve(roots: [], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.VsCode, WorktreeBase.RemoteDefault, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset));
        pull.ShouldNotBe(Settings.Resolve(roots: [], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.VsCode, WorktreeBase.RemoteDefault, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: BeforeBranching.None, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset));
    }

    [Fact]
    public void KeepBeforeBranchingThroughAValidatedDialogAndEitherToggle()
    {
        Settings.Validate(Typed with { BeforeBranching = BeforeBranching.None }, Exists(Root), Installed).ShouldBeOfType<SettingsValidation.Valid>().Settings.BeforeBranching.ShouldBe(BeforeBranching.None);
        Settings.Validate(Typed with { Preset = EditorPreset.Custom, Executable = @"C:\Tools\subl.exe", Arguments = "{folder}", BeforeBranching = BeforeBranching.Pull }, Exists(Root), Installed)
            .ShouldBeOfType<SettingsValidation.Valid>().Settings.BeforeBranching.ShouldBe(BeforeBranching.Pull);
        Settings.Validate(Typed, Exists(Root), Installed).ShouldBeOfType<SettingsValidation.Valid>().Settings.BeforeBranching.ShouldBe(BeforeBranching.Fetch);
        Settings pull = Settings.Resolve(roots: [], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.VsCode, WorktreeBase.RemoteDefault, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: BeforeBranching.Pull, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset);

        pull.WithUpdateCheckToggled().BeforeBranching.ShouldBe(BeforeBranching.Pull);
        pull.WithDiagnosticLogToggled().BeforeBranching.ShouldBe(BeforeBranching.Pull);
    }

    private static SettingsProblem Refusal(SettingsText typed) => Settings.Validate(typed, Exists(Root), Installed).ShouldBeOfType<SettingsValidation.Refused>().Problem;

    private static Func<string, bool> Exists(params string[] folders) => path => folders.Contains(path, StringComparer.OrdinalIgnoreCase);

    private static bool Installed(string file) => file == @"C:\Tools\subl.exe";

    public static TheoryData<Settings> SettingsThatDifferFromTheBase =>
    [
        Variant(roots: [FolderPath.From(@"D:\work")]),
        Variant(worktreeRoot: new WorktreeRootChoice.At(FolderPath.From(@"D:\trees"))),
        Variant(editor: EditorChoice.Of(EditorPreset.Rider)),
        Variant(defaultBase: WorktreeBase.Head),
        Variant(checkForUpdates: !Settings.ChecksForUpdatesWhenUnset),
        Variant(defaultPlace: LaunchMode.CurrentBranch),
        Variant(diagnosticLog: !Settings.DiagnosticLogWhenUnset),
        Variant(beforeBranching: BeforeBranching.Pull),
        Variant(sessionsRefresh: RefreshInterval.From(30)),
        Variant(worktreesRefresh: RefreshInterval.Off),
    ];

    [Theory]
    [MemberData(nameof(SettingsThatDifferFromTheBase))]
    public void BeUnequalWhenAnySettingDiffers(Settings different)
    {
        Variant().Equals(different).ShouldBeFalse();
        different.Equals(Variant()).ShouldBeFalse();
    }

    [Fact]
    public void BeEqualWhenEverySettingMatches()
    {
        Variant().Equals(Variant()).ShouldBeTrue();
        Variant().Equals(null).ShouldBeFalse();
    }

    private static Settings Variant(
        IReadOnlyList<FolderPath>? roots = null,
        WorktreeRootChoice? worktreeRoot = null,
        EditorChoice? editor = null,
        WorktreeBase defaultBase = WorktreeBase.RemoteDefault,
        bool checkForUpdates = Settings.ChecksForUpdatesWhenUnset,
        LaunchMode defaultPlace = Settings.PlaceWhenUnset,
        bool diagnosticLog = Settings.DiagnosticLogWhenUnset,
        BeforeBranching beforeBranching = Settings.BeforeBranchingWhenUnset,
        RefreshInterval? sessionsRefresh = null,
        RefreshInterval? worktreesRefresh = null)
    {
        return Settings.Resolve(roots ?? [FolderPath.From(Root)], worktreeRoot ?? new WorktreeRootChoice.BesideFirstRoot(), editor ?? EditorChoice.VsCode, defaultBase, Home, checkForUpdates, defaultPlace, diagnosticLog, beforeBranching, sessionsRefresh ?? Settings.SessionsRefreshWhenUnset, worktreesRefresh ?? Settings.WorktreesRefreshWhenUnset);
    }

    [Fact]
    public void CarryBothRefreshIntervals()
    {
        Settings settings = Variant(sessionsRefresh: RefreshInterval.From(30), worktreesRefresh: RefreshInterval.Off);

        settings.SessionsRefresh.ShouldBe(RefreshInterval.From(30));
        settings.WorktreesRefresh.ShouldBe(RefreshInterval.Off);
        Variant().SessionsRefresh.ShouldBe(RefreshInterval.From(5));
        Variant().WorktreesRefresh.ShouldBe(RefreshInterval.From(15));
        settings.WithUpdateCheckToggled().SessionsRefresh.ShouldBe(RefreshInterval.From(30));
        settings.WithDiagnosticLogToggled().WorktreesRefresh.ShouldBe(RefreshInterval.Off);
    }

    [Fact]
    public void ShowTheRefreshIntervalsAsTheirNumbers()
    {
        SettingsText text = Variant(sessionsRefresh: RefreshInterval.From(30), worktreesRefresh: RefreshInterval.Off).ToText();

        text.SessionsRefresh.ShouldBe("30");
        text.WorktreesRefresh.ShouldBe("0");
    }

    [Theory]
    [InlineData(" 30 ", 30)]
    [InlineData("0", 0)]
    [InlineData("3600", 3600)]
    public void AcceptATypedRefreshInterval(string typed, int seconds)
    {
        Settings sessions = Settings.Validate(Typed with { SessionsRefresh = typed }, Exists(Root), Installed).ShouldBeOfType<SettingsValidation.Valid>().Settings;
        Settings worktrees = Settings.Validate(Typed with { Preset = EditorPreset.Custom, Executable = @"C:\Tools\subl.exe", Arguments = "{folder}", WorktreesRefresh = typed }, Exists(Root), Installed).ShouldBeOfType<SettingsValidation.Valid>().Settings;

        sessions.SessionsRefresh.ShouldBe(RefreshInterval.From(seconds));
        sessions.WorktreesRefresh.ShouldBe(RefreshInterval.From(15));
        worktrees.WorktreesRefresh.ShouldBe(RefreshInterval.From(seconds));
        worktrees.SessionsRefresh.ShouldBe(RefreshInterval.From(5));
    }

    [Theory]
    [InlineData("2.5")]
    [InlineData("ten")]
    [InlineData("3601")]
    [InlineData("-1")]
    [InlineData("")]
    public void RefuseARefreshIntervalThatIsNotWholeSecondsInRange(string typed)
    {
        Refusal(Typed with { SessionsRefresh = typed }).ShouldBe(new SettingsProblem.RefreshIntervalNotWholeSeconds(RefreshedTab.Sessions));
        Refusal(Typed with { WorktreesRefresh = typed }).ShouldBe(new SettingsProblem.RefreshIntervalNotWholeSeconds(RefreshedTab.Worktrees));
    }

    [Fact]
    public void ResolveDefaultPaneHeightsOfSixAndTen()
    {
        Settings settings = Settings.Resolve(roots: [], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.VsCode, Settings.BaseWhenUnset, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset);

        settings.PaneHeights.ShouldBe(new PaneHeights(Sessions: 6, Worktrees: 10));
    }

    [Fact]
    public void KeepPaneHeightsThroughWithPaneHeights()
    {
        Settings settings = Settings.Resolve(roots: [], new WorktreeRootChoice.BesideFirstRoot(), EditorChoice.VsCode, Settings.BaseWhenUnset, Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset, sessionsRefresh: Settings.SessionsRefreshWhenUnset, worktreesRefresh: Settings.WorktreesRefreshWhenUnset)
            .WithPaneHeights(new PaneHeights(Sessions: 4, Worktrees: 20));

        settings.PaneHeights.ShouldBe(new PaneHeights(Sessions: 4, Worktrees: 20));
        settings.WithUpdateCheckToggled().PaneHeights.ShouldBe(settings.PaneHeights);
        settings.WithDiagnosticLogToggled().PaneHeights.ShouldBe(settings.PaneHeights);
        settings.ShouldNotBe(settings.WithPaneHeights(PaneHeights.WhenUnset));
    }
}