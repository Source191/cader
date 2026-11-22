using Source191.Cader.Application.Launch;
using Source191.Cader.Application.TerminalSetup;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using System.Diagnostics;

namespace Source191.Cader.Application;

/// <summary>
/// Every sentence the console shows, in one place: status messages, dialog questions, the text beside the busy spinner and the
/// key help. Reducers, dialogs and the screen hand over facts; this class chooses the words. <c>WordingShould</c>
/// holds each sentence verbatim.
/// </summary>
public static partial class Wording
{
    /// <summary>Printed on one line before an attach and erased after it, so it must never wrap.</summary>
    public static string ReturnHint => "Ctrl+Z detaches; the session keeps running.";

    /// <summary>cader here refuses a dash-led name that did not follow --, since it is most likely a mistyped option.</summary>
    public static string NameLooksLikeAnOption(string token) => $"{token} looks like an option, not a session name. To use it as the name, put -- before it: cader here -- {token}";

    /// <summary>The console and cader here print one of the next three and exit 1 when a program they need cannot be run.</summary>
    public static string ClaudeNotFound => "Claude Code was not found. Install it with irm https://claude.ai/install.ps1 | iex, or put claude.exe on the PATH.";

    public static string ClaudeIsABatchFile(string path) => $"Claude Code was found only as the batch file {path}, which Cader does not run: cmd.exe would read session names and prompts as commands. Install the native build with irm https://claude.ai/install.ps1 | iex.";

    public static string GitNotFound => "git was not found on the PATH. Install Git for Windows (winget install --id Git.Git --exact --source winget) and open a new terminal.";

    /// <summary>Where to support the console, printed under its description in <c>cader --help</c>.</summary>
    public static string SupportLine => $"Buy me a sliver of a Claude Max plan: {SupportPage}";

    private static readonly Uri SupportPage = new("https://buymeacoffee.com/simonvane");

    private const string SetupTerminalSyntax = "cader setup-terminal [--sound <wav>] [--session-sound <wav>] [--yes] [--remove]";

    public static string ForceRemoveWarning => "This deletes the folder below and everything in it that is not committed and pushed.\nTo confirm, give its name:";

    /// <summary>The typed confirmation's title: a row with a folder is removed, a branch row's local branch is deleted.</summary>
    public static string ForceTitle(Placement placement) => placement switch
    {
        Placement.Folder => ForceRemoveTitle,
        Placement.NoFolder => "Force delete branch",
    };

    public static string ForceWarning(Placement placement) => placement switch
    {
        Placement.Folder => ForceRemoveWarning,
        Placement.NoFolder => "This deletes the local branch below and every commit on it that is on no remote branch.\nTo confirm, give its name:",
    };

    /// <summary>d on a branch row with nothing at risk: the reducer has already found every commit on a remote branch or inside the merged head.</summary>
    public static string DeleteBranchRowQuestion(string branch) => $"Delete the local branch {branch}?\nEvery commit on it is on a remote branch or inside its merged pull request.";

    /// <summary>The typed-confirmation dialog's line when Enter is pressed on an empty box: the worktree's folder name, or a branch row's branch.</summary>
    public static string NothingTypedYet(string expected) => $"Nothing typed yet; the name to type is {expected}.";

    /// <summary>The wizard's preview for a session that runs in the repository's current branch.</summary>
    public static string PreviewCurrentBranch(string repositoryName) => $"Runs in {repositoryName} on its current branch; the name is only the session's label.";

    /// <summary>The wizard's preview for a folder choice, which starts in the folder whatever Where says.</summary>
    public static string PreviewFolder(string folderName) => $"Runs in {folderName}, which is not a repository, so Where does not apply.";

    /// <summary>The wizard's preview for a worktree whose name has not yielded a folder yet.</summary>
    public static string PreviewBeforeAName(string repositoryName, FolderPath worktreeRoot) => $"The name becomes the worktree folder {repositoryName}-<name> under {worktreeRoot} and its branch.";

    public static string PreviewWorktree(string folderName, FolderPath worktreeRoot, BranchName branch) => $"Worktree {folderName} under {worktreeRoot}, on branch {branch}.";

    /// <summary>The title of the tab that lets a person accept Claude Code's trust prompt for a folder.</summary>
    public static string TrustTabTitle(string folderName) => $"Trust {folderName}";

    public static string TrustTabMessage(string folderName) => $"Cader: accept the trust prompt for {folderName}, then exit Claude and press n in Cader to retry.";

    public static string UpdateTabTitle => "Update Cader";

    public static string NotTheSame(string typed) => $"Not the same: you typed {typed}.";

    public static string CopiedToClipboard(string expected) => $"Copied {expected} to the clipboard; Ctrl+V pastes it into the box.";

    public static string ClipboardUnavailable => "The clipboard is not available here; Shift+drag selects text in Windows Terminal.";

    public const string ProblemReportDialogTitle = "Report a problem";

    public static string ProblemReportExplanation => "Holds no names, paths or code. Opening the page passes the title and this text to GitHub in its address; nothing is public until you submit it there, where you describe the problem.";

    public static string ProblemReportTitleCaption => "Title (blank for a default)";

    public static string ProblemReportPreviewCaption => "Diagnostics the page will be filled in with";

    /// <summary>The problem report dialog's buttons, in order: open (the default), copy, cancel.</summary>
    /// <summary>Whether a moment falls on the same local date as now in the zone.</summary>
    private static bool IsToday(DateTimeOffset at, DateTimeOffset now, TimeZoneInfo zone) => TimeZoneInfo.ConvertTime(at, zone).Date == TimeZoneInfo.ConvertTime(now, zone).Date;

    public static string KeysTitle => "Keys";

    public static string DeleteBranchTitle => "Delete branch";

    /// <summary>The delete-branch question's buttons, confirm first; the dialog marks each one's hotkey.</summary>
    public static IReadOnlyList<string> DeleteBranchButtons { get; } = ["Delete branch", "Keep"];

    public static string RenameTitle => "Rename session";

    public static string CloseEmptyTitle => "Close empty terminal sessions";

    public static IReadOnlyList<string> CloseEmptyButtons { get; } = ["Close", "Keep"];

    public static string ForceRemoveTitle => "Force remove";

    public static string DeleteSessionTitle => "Delete session";

    public static IReadOnlyList<string> DeleteButtons { get; } = ["Delete", "Cancel"];

    public static string ClearLogTitle => "Clear diagnostic log";

    /// <summary>The clear's question: each log file there with its size as the tab last read it, never one that is absent.</summary>
    public static string ClearLogQuestion(LogFiles files)
    {
        LogFileOnDisc[] both = [files.Current, files.Older];
        IEnumerable<string> written = both.OfType<LogFileOnDisc.Written>().Select(file => $"{Path.GetFileName(file.Path)} ({FileSize(file.Bytes)})");

        return $"Delete {string.Join(" and ", written)}?\nThe events kept for a problem report stay. While Diagnostic log is on, a new cader.log starts with its next event.";
    }

    /// <summary>A dialog's Cancel button with its hotkey marked; Esc is not named because the dialog has no key bar.</summary>
    public static string CancelButton => "_Cancel";

    public static string CancelWithEscButton => "_Cancel (Esc)";

    public static string CopyNameButton => "Cop_y name (F2)";

    public static string StartButton => "_Start (F5)";

    public static string SaveButton => "_Save (F5)";

    public static IReadOnlyList<string> ProblemReportButtons { get; } = ["Open in browser", "Copy text", "Cancel"];

    /// <summary>The title of a report whose title box was left blank.</summary>
    public static string DefaultReportTitle(ToolVersion version) => $"Problem report from Cader {version.Text}";

    public static string Describe(StatusMessage message)
    {
        return message switch
        {
            StatusMessage.Blank => string.Empty,
            StatusMessage.NoSessionSelected => "No session selected.",
            StatusMessage.RunsInOwnTerminal { HasTranscript: true } terminal =>
                $"{Name(terminal.Name)} runs in its own terminal. Press b to move it into the background (its old tab closes), or type /bg there.",
            StatusMessage.RunsInOwnTerminal terminal =>
                $"{Name(terminal.Name)} runs in its own terminal and has no messages yet. d closes its tab, C closes every such tab, or send it something there and press b.",
            StatusMessage.NeverSentAMessage never => $"{Name(never.Name)} has nothing to resume: it was never sent a message. d deletes it.",
            StatusMessage.FolderGone gone => $"{Name(gone.Name)}'s folder no longer exists, so nothing was resumed. d deletes it.",
            StatusMessage.NoEmptyTerminals => "No terminal sessions without messages.",
            StatusMessage.NoMessagesToRename rename => $"{Name(rename.Name)} has no messages yet, so it cannot be renamed; send it something first.",
            StatusMessage.WorkingSoRenameWaits rename => $"{Name(rename.Name)} is working; wait until it is idle or needs input, because renaming restarts it.",
            StatusMessage.RenameCancelled => "Rename cancelled.",
            StatusMessage.InvalidName invalid => Describe(invalid.Problem),
            StatusMessage.InvalidAccountLabel invalid => Describe(invalid.Problem),
            StatusMessage.AlreadyInBackground adopted => $"{Name(adopted.Name)} already runs in the background.",
            StatusMessage.WorkingSoAdoptWaits adopt => $"{Name(adopt.Name)} is working; wait until it is idle or needs input, so nothing in flight is lost.",
            _ => DescribeWorktreesAndScreen(message),
        };
    }

    /// <summary>Why a typed problem report title is refused, under the dialog's title box.</summary>
    public static string Describe(ReportTitleProblem problem) => problem switch
    {
        ReportTitleProblem.Missing => "Type a title.",
        ReportTitleProblem.TooLong => $"A title can have at most {ReportTitle.MostCharacters} characters.",
    };

    /// <summary>Why a typed session name is refused, in the rename status message, the wizard and cader here alike.</summary>
    public static string Describe(NameProblem problem)
    {
        return problem switch
        {
            NameProblem.Missing => "A session name is required.",
        };
    }

    /// <summary>Why the removal rules said no, or what they did instead; the status messages and the outcomes wrap it in their own sentence.</summary>
    public static string Describe(RemovalReason reason)
    {
        return reason switch
        {
            RemovalReason.LiveSession live => $"session {Name(live.Name)} is using it",
            RemovalReason.Locked => "the worktree is locked",
            RemovalReason.LeftoverFilesWouldBeLost leftover => $"{(leftover.AtLeast ? "at least " : string.Empty)}{leftover.Count} files that git no longer tracks would be lost",
            RemovalReason.ChangesWouldBeLost changes => $"{changes.Count} changed or untracked files would be lost",
            RemovalReason.UnpushedCommits unpushed => $"branch {unpushed.Branch} has {unpushed.Count} unpushed commits",
            RemovalReason.RepositoryOutsideRoots => "its repository is not under the roots; add that root in settings or remove the worktree with git there",
            RemovalReason.RepositoryGone gone => $"its repository {gone.Repository} no longer exists; remove it from the Worktrees tab",
            RemovalReason.GitFailed failed => $"git could not inspect it: {failed.Message}; check it on the Worktrees tab",
            RemovalReason.OutsideTheRoots outside => $"{outside.Folder} is outside the repository roots and the worktree root, so it is not deleted",
            _ => reason.ToString(),
        };
    }

    /// <summary>What is wrong with a worktree; a lock keeps git's own words.</summary>
    public static string Describe(HealthReason reason)
    {
        return reason switch
        {
            HealthReason.None => string.Empty,
            HealthReason.FolderGone => "registered with git but the folder is gone",
            HealthReason.GitFileGone => "registered with git but its .git file is gone",
            HealthReason.Locked locked => locked.LockReason,
            HealthReason.RegistrationGone => "its .git file points at a registration that no longer exists",
            HealthReason.UnderClaudeWorktrees => @"under .claude\worktrees but git no longer lists it",
            HealthReason.EmptyLeftover => "leftover folder with nothing in it",
            HealthReason.NotRegistered => "folder is not registered with git",
            HealthReason.RepositoryOutsideRoots outside => $"its repository {outside.Repository} is not under the roots",
            HealthReason.CheckedOutNowhere => "checked out in no worktree",
            HealthReason.OnlyOnOrigin => "only on origin, as of the last fetch",
            _ => reason.ToString(),
        };
    }

    /// <summary>Why a launch is refused, in the wizard and from cader here; the command line's refusals end with its usage line.</summary>
    public static string Describe(LaunchProblem problem)
    {
        return problem switch
        {
            LaunchProblem.InvalidName invalid => Describe(invalid.Problem),
            LaunchProblem.WorktreeNeedsLetters => "A worktree needs a name with some letters or digits in it.",
            LaunchProblem.PromptLooksLikeAnOption => "A prompt cannot start with a dash: Claude Code would read it as options. Start with a word.",
            _ => problem.ToString(),
        };
    }

    /// <summary>Why the settings dialog will not save.</summary>
    public static string Describe(SettingsProblem problem)
    {
        return problem switch
        {
            SettingsProblem.NoRoots => @"Give a repository root: a full path, such as C:\Users\me\source\repos.",
            SettingsProblem.RootNotFullPath relative => $@"The repository root {relative.Text} is not a full path, such as C:\Users\me\source\repos.",
            SettingsProblem.RootMissing missing => $"{missing.Path} does not exist.",
            SettingsProblem.RootAlreadyListed repeated => $"{repeated.Path} is already a repository root.",
            SettingsProblem.WorktreeRootNotFullPath => @"The worktree root must be a full path, such as C:\Users\me\source\worktrees.",
            SettingsProblem.NoExecutable => "A custom editor needs an executable.",
            SettingsProblem.ExecutableNotFullPath => @"A custom editor's executable must be a full path, such as C:\Tools\subl.exe, so it is never looked for in the folder the console was started from.",
            SettingsProblem.BatchFileExecutable => "A batch file cannot be used as the editor: cmd.exe would read the folder path as commands.",
            SettingsProblem.NoFolderPlaceholder => $"A custom editor's arguments need {EditorChoice.FolderPlaceholder} where the folder goes.",
            SettingsProblem.ExecutableMissing missing => $"{missing.Path} does not exist.",
            SettingsProblem.RefreshIntervalNotWholeSeconds => $"The interval is a whole number of seconds from 0 to {RefreshInterval.MostSeconds}, where 0 turns the timer off.",
            _ => problem.ToString(),
        };
    }

    /// <summary>An editor preset as the settings dialog names it.</summary>
    public static string Describe(EditorPreset preset)
    {
        return preset switch
        {
            EditorPreset.VsCode => "VS Code",
            EditorPreset.VisualStudio => "Visual Studio",
            EditorPreset.Rider => "Rider",
            EditorPreset.Explorer => "Explorer",
            EditorPreset.Custom => "Custom",
        };
    }

    /// <summary>A preset as the Open folder with dialog lists it, marked once the dialog has looked and could not find it; Explorer and Custom are never marked.</summary>
    public static string Describe(EditorPreset preset, PresetSearch search)
    {
        return search switch
        {
            PresetSearch.Looked looked when preset is not (EditorPreset.Explorer or EditorPreset.Custom) && looked.Missing.Contains(preset) => $"{Describe(preset)} (not found)",
            PresetSearch.Looked or PresetSearch.NotYetLooked => Describe(preset),
            _ => throw new UnreachableException(),
        };
    }

    /// <summary>Why the wizard will not go on.</summary>
    public static string Describe(WizardProblem problem)
    {
        return problem switch
        {
            WizardProblem.None => string.Empty,
            WizardProblem.NoRepository => "Choose a repository: type part of its name, then ↑↓.",
            WizardProblem.Refused refused => Describe(refused.Problem),
            _ => problem.ToString(),
        };
    }

    /// <summary>What the status line says while a session started from the console is launched, once its first step has begun.</summary>
    public static string Launching(LaunchStep step) => $"Starting {step.Name.Value}: {StageWords(step)} ({step.Number} of {step.Count})...";

    private static string StageWords(LaunchStep step)
    {
        return step.Stage switch
        {
            LaunchStage.Fetching => $"fetching {BaseWords(step.Base)}",
            LaunchStage.Pulling => $"pulling {BaseWords(step.Base)}",
            LaunchStage.CreatingWorktree => "creating the worktree",
            LaunchStage.StartingClaude => "starting Claude Code",
            LaunchStage.FindingSession => "finding the new session",
            LaunchStage.OpeningTab => "opening its tab",
        };
    }

    private static string BaseWords(LaunchBase remoteBase)
    {
        return remoteBase switch
        {
            LaunchBase.Remote remote => remote.Branch.Value,
            LaunchBase.None => "origin's default branch",
            _ => throw new UnreachableException(),
        };
    }
    /// <summary>What the status line says, beside the busy spinner, while an effect runs off the UI thread.</summary>
    public static string Working(ViewEffect effect)
    {
        return effect switch
        {
            OpenTabForSession open => $"Opening a tab for {Name(open.Session.Name)}...",
            StopSession => "Stopping...",
            AdoptSession adopt => $"Moving {Name(adopt.Session.Name)} into the background...",
            RenameSession rename => $"Renaming {Name(rename.Session.Name)} to {rename.NewName.Value}...",
            CloseEmptyTerminals => "Closing...",
            ResumeSessions resume => $"Resuming {resume.Sessions.Count}...",
            DeleteSession deletion => $"Deleting {Name(deletion.Session.Name)}...",
            DeleteBranch branch => $"Deleting branch {branch.Branch.Branch}...",
            RemoveWorktree remove => $"Removing {remove.Row.FolderName}...",
            PruneRepository => "Pruning...",
            OpenShellTab shell => $"Opening a shell in {shell.Row.FolderName}...",
            StartSession start => $"Starting {start.Plan.Name.Value}...",
            CopyFolderPath => "Copying...",
            OpenFolderInEditor open => $"Opening {open.Folder.Name} in the editor...",
            OpenFileInEditor file => $"Opening {file.File.Name} in the editor...",
            FindSolution find => $"Looking for a solution in {find.Folder.Name}...",
            ShowMemory show => $"Measuring {Name(show.Session.Name)}...",
            OpenSolution solution => $"Opening {Path.GetFileName(solution.File)}...",
            OpenPullRequest open => $"Opening {PullRequestNumber(open.PullRequest.Host, open.PullRequest.Number)}...",
            OpenReleasePage page => $"Opening the {page.Version} release page...",
            SaveSettings => "Saving settings...",
            StartUpdate update => $"Opening a tab to update to {update.Newest}...",
            ShowProblemReport => "Gathering the diagnostics...",
            OpenProblemReport => "Opening the problem report in your browser...",
            CopyProblemReport => "Copying the problem report...",
            _ => "Working...",
        };
    }

    /// <summary>What the update tab says when winget upgrade fails, under winget's own message.</summary>
    public static string UpdateFailedHint => "Cader: the upgrade failed. If winget says a file is in use, quit every Cader that is still open; if it found no newer package, winget may not have this release yet. Then press U in Cader again.";

    public static string WizardTitle(WizardState wizard) => wizard.Origin switch
    {
        WizardOrigin.Anywhere => "New session",
        WizardOrigin.Fixed pinned => $"New session in {pinned.Choice.Name}",
        _ => throw new UnreachableException(),
    };

    public static string WizardChosenCaption(WizardState wizard) => wizard.ShowsChooser ? "Repository" : "Directory";

    /// <summary>A row of the wizard's list: a group's heading, or a choice indented under it.</summary>
    public static string WizardListRow(WizardRow row) => row switch
    {
        WizardRow.Heading heading => WizardHeading(heading.Group),
        WizardRow.Offered offered => $"  {offered.Choice.Name}",
        _ => throw new UnreachableException(),
    };

    public static string WizardHeading(ChoiceGroup group) => group switch
    {
        ChoiceGroup.Recent => "Recent",
        ChoiceGroup.Folders => "Folders (no repository)",
        ChoiceGroup.Repositories => "Repositories",
    };

    public static string WizardChosen(WizardState wizard) => wizard.Chosen switch
    {
        RepositoryPick.Picked picked => picked.Choice.Name,
        RepositoryPick.None => "(none)",
        _ => throw new UnreachableException(),
    };

    public static string WizardHint(WizardState wizard)
    {
        const string Fields = "Enter or Tab next field   F5 start and open in tab   Esc cancel";

        return wizard.ShowsChooser ? $"Type to filter, ↑↓ to choose, Enter confirms the repository   {Fields}" : "Enter or F5 starts and opens in tab   Esc cancel";
    }

    public static string MemoryDialogTitle(ListedName sessionName) => $"Memory: {Name(sessionName)}";

    /// <summary>The breakdown dialog's text: Claude Code, each program with its subtree's memory, then the total with the process count, figures right-aligned.</summary>
    public static string MemoryBreakdownLines(MemoryBreakdown breakdown)
    {
        const string claudeCode = "Claude Code";
        const string total = "Total";
        const int figureWidth = 7;
        IReadOnlyList<MemoryShare> shares = [new MemoryShare(claudeCode, breakdown.ClaudeCode), .. breakdown.Programs];
        int labelWidth = Math.Max(shares.Max(share => share.Label.Length), total.Length) + 2;
        IEnumerable<string> lines = shares.Select(share => $"{share.Label.PadRight(labelWidth)}{Memory(share.Memory).PadLeft(figureWidth)}");
        string processes = breakdown.ProcessCount == 1 ? "process" : "processes";

        return string.Join('\n', [.. lines, string.Empty, $"{total.PadRight(labelWidth)}{Memory(breakdown.Total).PadLeft(figureWidth)} in {breakdown.ProcessCount} {processes}"]);
    }

    public static string ChooseSolutionTitle => "Open solution";

    public static string ChooseSolutionQuestion(string folderName) => $"{folderName} holds more than one solution. Which one?";

    /// <summary>A session's name as shown anywhere: its value, or nothing for a session the listing gave none.</summary>
    public static string Name(ListedName name) => name switch
    {
        ListedName.Named named => named.Name.Value,
        ListedName.Unnamed => string.Empty,
        _ => throw new UnreachableException(),
    };

    public static string RenamePrompt(ListedName name) => $"Rename {Name(name)}. The session restarts under the new name and keeps its conversation.";

    public static string DeleteSessionQuestion(ListedName name) => $"Delete {Name(name)}?\nIts worktree is removed only when nothing in it would be lost.";

    public static string CloseEmptyQuestion(IReadOnlyList<ListedName> names)
    {
        string listed = string.Join(", ", names.Select(Name));
        string what = names.Count == 1 ? $"Close the tab of {listed}?" : $"Close {names.Count} tabs: {listed}?";

        return $"{what}\nNone of them has been sent a message, so nothing is lost.";
    }

    public static string DeleteBranchQuestion(string branch, bool createdHere)
    {
        return createdHere
            ? $"Branch {branch} is not merged. Delete it anyway?"
            : $"Branch {branch} was not created here. Delete it anyway?";
    }

    // cader setup-terminal.

    public static string SetupTerminalUsage => $"Usage: {SetupTerminalSyntax}";

    public static string WriteProfilesQuestion => "Write the Windows Terminal profiles? [y/N] ";

    public static string TurnOnClaudeBellQuestion => "Turn on Claude Code's terminal bell? [y/N] ";

    public static string ClaudeBellAlreadyOn => "Claude Code's terminal bell is already on.";

    public static string NothingWritten => "Nothing was written.";

    public static string NothingToRemove => "There is nothing to remove.";

    /// <summary>Why setup-terminal will not run; every refusal ends with the usage line.</summary>
    public static string DescribeSetupProblem(TerminalSetupProblem problem)
    {
        return problem switch
        {
            TerminalSetupProblem.RemoveWithSound => $"--remove cannot be combined with --sound or --session-sound. {SetupTerminalUsage}",
            TerminalSetupProblem.SoundNotFound missing => $"The sound file {missing.Path} does not exist. {SetupTerminalUsage}",
            TerminalSetupProblem.SoundNotWav notWav => $"The sound file {notWav.Path} is not a .wav file. {SetupTerminalUsage}",
            _ => problem.ToString(),
        };
    }

    public static string TerminalSetupPlan(string fragmentPath, TerminalProfiles profiles, bool terminalHidesAProfile)
    {
        return string.Join(Environment.NewLine, [
            $"Two Windows Terminal profiles will be written to {fragmentPath}",
            .. terminalHidesAProfile ? Array.Empty<string>() : ["Your Windows Terminal settings file is not changed."],
            $"  {TerminalProfiles.ConsoleProfileName,-13}  runs {profiles.ConsoleExecutable}; bell sound {SoundText(profiles.ConsoleSound)}",
            $"  {TerminalProfiles.SessionProfileName,-13}  runs PowerShell; bell sound {SoundText(profiles.SessionSound)}",
            "Both ring the bell and flash the window and the taskbar.",
        ]);
    }

    public static string ProfilesWritten(string fragmentPath) => $"Wrote {fragmentPath}. Restart Windows Terminal to see the Cader and Cader session profiles.";

    public static string DefaultSoundMissing(string profileName, string path) => $"{path} was not found, so the {profileName} profile uses Windows Terminal's own bell sound.";

    /// <summary>What turning the bell on will do to the settings file; <paramref name="replacing"/> is the other channel it displaces, if any.</summary>
    public static string ClaudeBellPlan(string settingsPath, string? replacing)
    {
        string change = replacing is null
            ? $"This sets \"preferredNotifChannel\": \"{NotificationChannelState.TerminalBell}\""
            : $"This replaces \"preferredNotifChannel\": \"{replacing}\" with \"{NotificationChannelState.TerminalBell}\"";

        return string.Join(Environment.NewLine,
            "Claude Code can ring the terminal bell when a session finishes or needs permission.",
            $"{change} in {settingsPath} and keeps every other setting.");
    }

    public static string ClaudeBellEnabled(string settingsPath) => $"Set preferredNotifChannel to {NotificationChannelState.TerminalBell} in {settingsPath}. It applies to sessions started from now on.";

    public static string ClaudeSettingsUnreadable(string settingsPath, string reason) => $"Could not read {settingsPath} ({reason}), so it was left alone.";

    public static string ShowHiddenProfilesQuestion => "Show the hidden profiles again? [y/N] ";

    /// <summary>Which profiles Windows Terminal hides, why, and the entry setup would add for each, as it would write it.</summary>
    public static string HiddenProfilesPlan(string settingsPath, IReadOnlyList<TerminalProfileEntry> entries)
    {
        string names = string.Join(" and ", entries.Select(entry => entry.Name));
        string which = entries.Count == 1 ? $"the {names} profile" : $"the {names} profiles";
        string adds = entries.Count == 1 ? "an entry" : "an entry for each";

        return string.Join(Environment.NewLine, [
            $"Windows Terminal is hiding {which}: it keeps a profile deleted from its settings hidden, however often the fragment is written.",
            $"This adds {adds} to the profile list in {settingsPath} and keeps everything else in the file as it is:",
            .. entries.Select(entry => $"  {entry.Json}"),
        ]);
    }

    public static string HiddenProfilesShown(string settingsPath, string backup) => $"Added the entries to {settingsPath}. Windows Terminal shows the profiles again once it reads the file, with no restart. The file as it was is kept as {backup}.";

    /// <summary>Why nothing was written to a linked settings file, and the entries to add by hand.</summary>
    public static string TerminalSettingsLinked(string settingsPath, IReadOnlyList<TerminalProfileEntry> entries)
    {
        return string.Join(Environment.NewLine, [
            $"Could not add the entries: {settingsPath} is a link, and writing it would replace the link with a file, so nothing was written.",
            "Add each of these to the profile list in the file it links to:",
            .. entries.Select(entry => $"  {entry.Json}"),
        ]);
    }

    /// <summary>Why nothing was written to a read-only settings file, and the entries to add by hand.</summary>
    public static string TerminalSettingsReadOnly(string settingsPath, IReadOnlyList<TerminalProfileEntry> entries)
    {
        return string.Join(Environment.NewLine, [
            $"Could not add the entries: {settingsPath} is read-only, so nothing was written.",
            "Add each of these to its profile list by hand, or clear its read-only attribute and run the command again:",
            .. entries.Select(entry => $"  {entry.Json}"),
        ]);
    }

    public static string TerminalSettingsChanged(string settingsPath) => $"{settingsPath} changed while setup was writing it, most likely saved by Windows Terminal, so nothing was written. Run the command again.";

    public static string TerminalSettingsNotVerified(string settingsPath) => $"{settingsPath} would not have held exactly the profile list it should once changed, so nothing was written. A file that names \"profiles\" twice does this.";

    public static string TerminalSettingsUnreadable(string settingsPath, string reason) => $"Could not read Windows Terminal's settings or state ({reason}), so {settingsPath} was left alone.";

    public static string RemoveEntriesQuestion => "Take them out? [y/N] ";

    /// <summary>The entries setup added to Windows Terminal's settings file, which <c>--remove</c> would take out, as they are written.</summary>
    public static string EntriesToRemovePlan(string settingsPath, IReadOnlyList<TerminalProfileEntry> entries)
    {
        string names = string.Join(" and ", entries.Select(entry => entry.Name));
        (string which, string them, string profiles) = entries.Count == 1 ? ("the entry", "it", $"the {names} profile") : ("the entries", "them", $"the {names} profiles");

        return string.Join(Environment.NewLine, [
            $"{settingsPath} holds {which} setup added to show {profiles} again.",
            $"This takes {them} out of its profile list and keeps everything else in the file as it is:",
            .. entries.Select(entry => $"  {entry.Json}"),
        ]);
    }

    public static string EntriesRemoved(string settingsPath, string backup) => $"Took the entries out of {settingsPath}. The file as it was is kept as {backup}.";

    /// <summary>Why nothing was taken out of a linked settings file, and the entries to take out by hand.</summary>
    public static string TerminalSettingsLinkedForRemoval(string settingsPath, IReadOnlyList<TerminalProfileEntry> entries)
    {
        return string.Join(Environment.NewLine, [
            $"Could not take the entries out: {settingsPath} is a link, and writing it would replace the link with a file, so nothing was written.",
            "Take each of these out of the profile list in the file it links to:",
            .. entries.Select(entry => $"  {entry.Json}"),
        ]);
    }

    /// <summary>Why nothing was taken out of a read-only settings file, and the entries to take out by hand.</summary>
    public static string TerminalSettingsReadOnlyForRemoval(string settingsPath, IReadOnlyList<TerminalProfileEntry> entries)
    {
        return string.Join(Environment.NewLine, [
            $"Could not take the entries out: {settingsPath} is read-only, so nothing was written.",
            "Take each of these out of its profile list by hand, or clear its read-only attribute and run the command again:",
            .. entries.Select(entry => $"  {entry.Json}"),
        ]);
    }

    public static string ProfilesRemoved(string fragmentPath) => $"Removed {fragmentPath}. Restart Windows Terminal to drop the profiles.";

    /// <summary>An exception's message reports what failed, here and nowhere as a reason.</summary>
    public static string TerminalSetupFailed(string path, string reason) => $"Could not write {path}: {reason}";

    private static string SoundText(BellSound sound) => sound switch
    {
        BellSound.File file => file.Path,
        BellSound.TerminalDefault => "is Windows Terminal's own",
        _ => throw new UnreachableException(),
    };

    private static string DescribeWorktreesAndScreen(StatusMessage message)
    {
        return message switch
        {
            StatusMessage.NoWorktreeSelected => "No worktree selected.",
            StatusMessage.SetupAdvice advice => Describe(advice.Item),
            StatusMessage.NoPathHere => "This row's value is not a path.",
            StatusMessage.NoSettingSelected => "No setting selected.",
            StatusMessage.NoLogToClear => "There is no log file to delete.",
            StatusMessage.LogFilesNotRead => "The log files have not been read yet; press d again in a moment.",
            StatusMessage.ClearsOnlyTheLog => "d clears the diagnostic log on the Diagnostic log file row, and changes nothing here.",
            StatusMessage.NotRemoved { OfferForce: true } refused => $"Not removed: {Describe(refused.Reason)}. D forces after a typed confirmation.",
            StatusMessage.NotRemoved refused => $"Not removed: {Describe(refused.Reason)}.",
            StatusMessage.ForceCancelled => "Cancelled; nothing removed.",
            StatusMessage.NothingTyped => "Nothing was typed; nothing removed.",
            StatusMessage.TypedWrongName wrong => $"You typed {wrong.Typed}, not {wrong.Expected}; nothing removed.",
            StatusMessage.NothingToPrune prune => $"Nothing to prune: the repository {prune.RepositoryName} no longer exists.",
            StatusMessage.LeftoverFolder leftover => $"{leftover.FolderName} is a leftover folder, not a worktree.",
            StatusMessage.NoSessionIn none => $"No session is running in {none.FolderName}.",
            StatusMessage.NoWorktree none => $"{none.Branch} has no worktree; nothing to open or start there.",
            StatusMessage.NotDeletedOnOrigin origin => $"{origin.Branch} is only on origin, and Cader never deletes a branch on origin.",
            StatusMessage.SessionNotListed gone => $"{Name(gone.Name)} is not in the sessions list any more; it may have ended. Try again after the next refresh.",
            StatusMessage.NoSolution none => $"No solution found in {none.FolderName} or its src folder.",
            StatusMessage.WizardTooSmall small => $"The new-session window needs at least {small.NeededColumns} x {small.NeededRows}; this terminal is {small.Columns} x {small.Rows}.",
            StatusMessage.NothingToMeasure stopped => $"{Name(stopped.Name)} has no process to measure; it is stopped.",
            StatusMessage.NoPullRequest none => Describe(none),
            StatusMessage.UpToDate => "This is the newest release.",
            StatusMessage.NewerReleaseFound found => $"Version {found.Version} is available: press U to see what changed.",
            StatusMessage.UpdateCheckOff => "The update check is off, or this is a development build; nothing to update to.",
            StatusMessage.Kept => "Kept.",
            StatusMessage.NothingToResume => "Nothing to resume.",
            StatusMessage.NoDialogFor missing => $"No dialog is defined for {missing.EffectName}.",
            StatusMessage.Working working => Working(working.Effect),
            StatusMessage.Launching launching => Launching(launching.Step),
            StatusMessage.Reported reported => Describe(reported.Outcome),
            StatusMessage.BranchKept kept => $"{Describe(kept.Removal)} Branch kept.",
            StatusMessage.Followed followed => $"{Describe(followed.First)} {Describe(followed.Then)}",
            StatusMessage.Failed failed => $"Failed: {failed.Message}",
            _ => message.ToString(),
        };
    }
}