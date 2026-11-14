using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using System.Globalization;

namespace Source191.Cader.Application;

/// <summary>The Config tab's words: each row's name and value, and what to do about a piece of setup.</summary>
public static partial class Wording
{
    private const string SetupTerminal = "run cader setup-terminal";

    /// <summary>The button of the roots, worktree root and Open folder with dialogs that opens a folder or file picker.</summary>
    public static string BrowseButton => "_Browse… (F3)";

    /// <summary>The folder picker's own title, inside the dialog; the console's tab says <see cref="DialogName.FolderPicker"/>.</summary>
    public static string FolderPickerTitle(SettingsScope scope) => scope is SettingsScope.WorktreeRoot ? "Choose the worktree root" : "Add repository roots";

    /// <summary>The file picker's own title, inside the Open folder with dialog; the console's tab says <see cref="DialogName.FilePicker"/>.</summary>
    public static string FilePickerTitle => "Choose the editor's executable";

    /// <summary>What the file picker's type list calls the files it shows.</summary>
    public static string ProgramFiles => "Programs";

    /// <summary>The caption above the Default place dialog's choice.</summary>
    public static string PlaceCaption => "Where the wizard opened with n starts a session (↑↓ then Space)";

    /// <summary>The caption above the Before branching choice, in its dialog and the wizard.</summary>
    public static string BeforeBranchingCaption => "What happens before a new worktree branches from origin (↑↓ then Space)";

    /// <summary>The caption above the wizard's Before branching choice.</summary>
    public static string WizardBeforeBranchingCaption => "Before branching (↑↓ to change)";

    /// <summary>The SETTING cell.</summary>
    public static string ConfigSetting(ConfigRow row)
    {
        return row switch
        {
            ConfigRow.RepositoryRoot { Index: 0 } => "Repository roots",
            ConfigRow.RepositoryRoot => string.Empty,
            ConfigRow.WorktreeRoot => "Worktree root",
            ConfigRow.OpenFolderWith => "Open folder with",
            ConfigRow.DefaultBase => "Default base",
            ConfigRow.BeforeBranching => "Before branching",
            ConfigRow.DefaultPlace => "Default place",
            ConfigRow.CheckForUpdates => "Check for updates",
            ConfigRow.DiagnosticLog => "Diagnostic log",
            ConfigRow.Setup setup => ConfigSetting(setup.Item),
            ConfigRow.Heading => "Setup",
            _ => row.ToString(),
        };
    }

    /// <summary>What a Config row's setting does, or what its setup is for: the row's hover tooltip only.</summary>
    public static string ConfigDescription(ConfigRow row) => row switch
    {
        ConfigRow.RepositoryRoot => "A folder searched for git repositories; Enter edits every root, one per line.",
        ConfigRow.WorktreeRoot => "Where new worktrees go, each in a folder named <repository>-<slug> on a branch named <slug>.",
        ConfigRow.OpenFolderWith => "The program f opens a row's folder with: one of the known editors, or a program of your own.",
        ConfigRow.DefaultBase => "New worktrees branch from origin's default branch (see Before branching), or from HEAD.",
        ConfigRow.BeforeBranching => "What happens before a new worktree branches from origin: fetch it, fetch and fast-forward, or nothing.",
        ConfigRow.DefaultPlace => "Where the wizard opened with n starts a session: a new worktree, or the repository's current branch.",
        ConfigRow.CheckForUpdates => "Whether to ask the winget community repository once a day for a newer release, which U then installs.",
        ConfigRow.DiagnosticLog => "Writes the diagnostic log to a file, named on the Diagnostic log file row below; Enter turns it on or off.",
        ConfigRow.Setup setup => SetupDescription(setup.Item),
        _ => string.Empty,
    };

    /// <summary>The VALUE cell; <paramref name="now"/> in <paramref name="zone"/> says whether the last update check was today.</summary>
    public static string ConfigValue(ConfigRow row, DateTimeOffset now, TimeZoneInfo zone)
    {
        return row switch
        {
            ConfigRow.RepositoryRoot root => root.Path.Value,
            ConfigRow.WorktreeRoot worktreeRoot => worktreeRoot.Path.Value,
            ConfigRow.OpenFolderWith open => open.Editor.Preset == EditorPreset.Custom ? $"{open.Editor.Executable} {CommandArguments.Join(open.Editor.Arguments)}" : Describe(open.Editor.Preset),
            ConfigRow.DefaultBase defaultBase => Describe(defaultBase.Base),
            ConfigRow.BeforeBranching before => Describe(before.Choice),
            ConfigRow.DefaultPlace place => Describe(place.Place),
            ConfigRow.CheckForUpdates check => check.On ? "on" : "off",
            ConfigRow.DiagnosticLog log => log.On ? "on" : "off",
            ConfigRow.Setup setup => SetupValue(setup.Item, setup.State, now, zone),
            ConfigRow.Heading => string.Empty,
            _ => row.ToString(),
        };
    }

    /// <summary>The title of a setting's dialog.</summary>
    public static string SettingsDialogTitle(SettingsScope scope)
    {
        return scope switch
        {
            SettingsScope.Roots => "Repository roots",
            SettingsScope.WorktreeRoot => "Worktree root",
            SettingsScope.Editor => "Open folder with",
            SettingsScope.Base => "Worktree base",
            SettingsScope.Place => "Default place",
            SettingsScope.BeforeBranching => "Before branching",
            _ => scope.ToString(),
        };
    }

    /// <summary>Where a new worktree's branch starts, as the Default base row, its dialog and the wizard name it: origin's remote-tracking branch, after the update config.json's beforeBranching chooses, or HEAD.</summary>
    public static string Describe(WorktreeBase worktreeBase)
    {
        return worktreeBase switch
        {
            WorktreeBase.RemoteDefault => "origin's default branch",
            WorktreeBase.Head => "the commit checked out now (HEAD)",
        };
    }

    /// <summary>What happens before a new worktree branches, as the Before branching row, its dialog and the wizard name it.</summary>
    public static string Describe(BeforeBranching choice)
    {
        return choice switch
        {
            BeforeBranching.Fetch => "Fetch origin",
            BeforeBranching.Pull => "Pull default branch",
            BeforeBranching.None => "Nothing",
        };
    }

    /// <summary>A place a new session starts, as the Default place row and its dialog name it.</summary>
    public static string Describe(LaunchMode place)
    {
        return place switch
        {
            LaunchMode.CurrentBranch => "Current branch",
            LaunchMode.NewWorktree => "New worktree",
        };
    }

    /// <summary>Enter on a setup row: how that setup is changed, since the tab only reads it.</summary>
    public static string Describe(SetupItem item)
    {
        return item switch
        {
            SetupItem.SettingsFile => "This tab writes the settings file: edit a setting above to change it.",
            SetupItem.DiagnosticLogFile => "Turn the log on or off with Diagnostic log above; f opens this file and c copies its path.",
            SetupItem.TerminalProfiles => "Run cader setup-terminal, then restart Windows Terminal, to add the profiles.",
            SetupItem.ClaudeBell => "Run cader setup-terminal to make Claude ring the terminal bell when a session needs you.",
            SetupItem.LastUpdateCheck => "The winget community repository is asked at most once a day; turn the check on or off with Check for updates.",
        };
    }

    private static string SetupDescription(SetupItem item)
    {
        return item switch
        {
            SetupItem.SettingsFile => "The file the settings above are kept in; f opens it in your editor.",
            SetupItem.DiagnosticLogFile => "Where the diagnostic log is written; it rolls at 10 MiB. f opens it in your editor, c copies its path.",
            SetupItem.TerminalProfiles => "Windows Terminal profiles that give the console's tabs a bell and the icon.",
            SetupItem.ClaudeBell => "Claude's terminal bell, which rings when a session finishes or needs permission while you are away.",
            SetupItem.LastUpdateCheck => "When the winget community repository was last asked for a newer release, and the newest it found.",
        };
    }

    private static string ConfigSetting(SetupItem item)
    {
        return item switch
        {
            SetupItem.SettingsFile => "Settings file",
            SetupItem.DiagnosticLogFile => "Diagnostic log file",
            SetupItem.TerminalProfiles => "Windows Terminal profiles",
            SetupItem.ClaudeBell => "Claude bell",
            SetupItem.LastUpdateCheck => "Last update check",
        };
    }

    private static string SetupValue(SetupItem item, SetupState state, DateTimeOffset now, TimeZoneInfo zone)
    {
        return (item, state) switch
        {
            (_, SetupState.Reading) => "reading…",
            (_, SetupState.Unreadable) => "could not be read",
            (SetupItem.TerminalProfiles, SetupState.At at) => $"installed, {at.Path}",
            (SetupItem.TerminalProfiles, SetupState.NotSetUp) => $"not set up, {SetupTerminal}",
            (SetupItem.TerminalProfiles, SetupState.HiddenByTerminal) => $"hidden by Windows Terminal, {SetupTerminal}",
            (_, SetupState.NotSetUp) => $"not set, {SetupTerminal}",
            (_, SetupState.LogOff off) => $"off, {off.Path}",
            (_, SetupState.LogFile { Exists: true } file) => $"on, {file.Path}",
            (_, SetupState.LogFile file) => $"on, not written yet, {file.Path}",
            (_, SetupState.At at) => at.Path,
            (_, SetupState.Enabled) => "on",
            (_, SetupState.OtherChannel other) => $"{other.Channel}, not the terminal bell",
            (_, SetupState.Disabled) => "off",
            (_, SetupState.Never) => "never",
            (_, SetupState.Checked { Newest: NewestRelease.Found found } check) => $"{When(check.When, now, zone)}, newest {found.Version}",
            (_, SetupState.Checked { Newest: NewestRelease.Unreadable unreadable } check) => $"{When(check.When, now, zone)}, newest {unreadable.Text}",
            (_, SetupState.Checked { Newest: NewestRelease.None } check) => When(check.When, now, zone),
            _ => state.ToString(),
        };
    }

    /// <summary>Each moment takes the offset the zone had then, so a check from before the clocks changed keeps its own.</summary>
    private static string When(DateTimeOffset when, DateTimeOffset now, TimeZoneInfo zone)
    {
        DateTimeOffset at = TimeZoneInfo.ConvertTime(when, zone);

        return IsToday(at, now, zone)
            ? at.ToString("HH:mm 'today'", CultureInfo.InvariantCulture)
            : at.ToString("d MMMM yyyy HH:mm", CultureInfo.InvariantCulture);
    }
}