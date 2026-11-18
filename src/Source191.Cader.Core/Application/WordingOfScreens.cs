using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using System.Diagnostics;
using System.Globalization;

namespace Source191.Cader.Application;

/// <summary>The words around the table screens: the tab title, the settings row's values, the table title, the key hints and the help.</summary>
public static partial class Wording
{
    private const string UnknownVersion = "unknown";

    private const string UpdateLine = "update in a new tab";
    private const string ReportLine = "report a problem on GitHub";
    private const string ReportTip = "Preview the diagnostics (events, last crash, counts), then open a GitHub issue.";

    /// <summary>What a problem report holds; the user guide says it in the same words.</summary>
    public static string ReportHolds => "A report holds the latest diagnostic events kept in memory (whether or not Diagnostic log is on), the last crash and the child program counts.";

    /// <summary>That the long history stays on the machine unless the user hands it over.</summary>
    public static string ReportNeverAttachesTheLog => "Cader never attaches cader.log to a report.";

    /// <summary>Where the long history is and what to do with it.</summary>
    public static string ReportLogFile => "cader.log is a separate, longer history that rolls at 10 MiB: open it with f on the Diagnostic log file row (Config tab), then give it to Claude or attach it to the issue by hand.";

    /// <summary>The title of the console's own tab outside a dialog; every writer of it uses this one value. The version is in the settings row.</summary>
    public static string TabTitle => "Cader";

    /// <summary>The tab's title while a dialog is open: Cader and the dialog's short name, never the dialog's own title.</summary>
    public static string TabTitleDuring(DialogName dialog) => $"{TabTitle} ({DialogWord(dialog)})";

    private static string DialogWord(DialogName dialog) => dialog switch
    {
        DialogName.Keys => "Keys",
        DialogName.NewSession => "New session",
        DialogName.Rename => "Rename",
        DialogName.Settings => "Settings",
        DialogName.FolderPicker => "Browse",
        DialogName.FilePicker => "Choose editor",
        DialogName.DeleteSession => "Delete session",
        DialogName.DeleteBranch => "Delete branch",
        DialogName.CloseEmpty => "Close empty",
        DialogName.ForceRemove => "Force remove",
        DialogName.ForceDeleteBranch => "Force delete",
        DialogName.OpenSolution => "Open solution",
        DialogName.WhatsNew => "What's new",
        DialogName.Memory => "Memory",
        DialogName.ProblemReport => ProblemReportDialogTitle,
        DialogName.ClearLog => "Clear log",
        DialogName.AccountLabel => "Rename account",
    };

    /// <summary>An amount of memory as the settings row, the detail pane and the breakdown write it: whole megabytes below 1,000 MB, gigabytes to one decimal from there.</summary>
    public static string Memory(MemorySize size)
    {
        double megabytes = (double)size.Bytes / MemorySize.BytesPerMegabyte;
        long whole = (long)Math.Round(megabytes);

        return whole < 1000 ? $"{whole} MB" : $"{(megabytes / 1024).ToString("F1", CultureInfo.InvariantCulture)} GB";
    }

    /// <summary>A file's size as the Config tab's log rows write it: whole kilobytes rounded up below a megabyte, as Explorer shows them, then megabytes to one decimal.</summary>
    public static string FileSize(long bytes)
    {
        const long kilobyte = 1024;
        const long megabyte = kilobyte * kilobyte;

        return bytes < megabyte
            ? $"{((bytes + kilobyte - 1) / kilobyte).ToString(CultureInfo.InvariantCulture)} KB"
            : $"{((double)bytes / megabyte).ToString("F1", CultureInfo.InvariantCulture)} MB";
    }

    /// <summary>The settings row's Claude Code value: the version as the CLI printed it, and the minimum with a prompt to update when it is older.</summary>
    private static string ClaudeVersion(ClaudeCodeVersion claude) => claude switch
    {
        ClaudeCodeVersion.Unknown => UnknownVersion,
        ClaudeCodeVersion.Reported reported => reported.Text,
        ClaudeCodeVersion.BelowMinimum old => $"{old.Installed} (below {old.Minimum}, update Claude Code)",
        _ => throw new UnreachableException(),
    };

    /// <summary>The settings row's Cader value: the running version and its own process's memory, then, in amber inside the same brackets, the newer release with the update key when one is known.</summary>
    private static IReadOnlyList<TextPart> OwnVersion(ToolVersion tool, UpdateStatus update, MemorySize memory)
    {
        string version = $"{tool.Text} ({Memory(memory)}";

        return update is UpdateStatus.Available available
            ? [Value($"{version}; "), new TextPart($"{available.Newest} available, U to update", TextTone.Amber), Value(")")]
            : [Value($"{version})")];
    }

    /// <summary>The table's title: "7 sessions, selected: fix-123", or "2 of 7 sessions match "templates", selected: templates-sync" while a filter is set.</summary>
    public static string Describe(TableTitle title)
    {
        string selection = title.Selected is null ? string.Empty : $", selected: {title.Selected}";

        if (string.IsNullOrWhiteSpace(title.Filter))
        {
            return $"{Counted(title)}{selection}";
        }

        string verb = title.Total == 1 ? "matches" : "match";

        return $"{title.Shown} of {Counted(title)} {verb} \"{title.Filter}\"{selection}";
    }

    /// <summary>The rows by kind, leaving out a kind with none: "10 worktrees, 3 branches", "3 branches", or "0 worktrees" when there is nothing.</summary>
    private static string Counted(TableTitle title)
    {
        TitleCount apart = title.Apart;
        int first = title.Total - apart.Count;
        string apartWords = apart.Count == 1 ? $"1 {apart.One}" : $"{apart.Count} {apart.Many}";

        return (first, apart.Count) switch
        {
            (_, 0) => Count(title.Total, title.Noun),
            (0, _) => apartWords,
            _ => $"{Count(first, title.Noun)}, {apartWords}",
        };
    }

    /// <summary>The keys every tab shares for moving between tabs, scrolling a table too wide for the terminal and refreshing now; only the help lists them.</summary>
    private static readonly KeyHelp[] TabKeyHelp =
    [
        KeyHelp.HelpOnly(Key.Left, "the tab to the left", tip: "Show the tab to the left of this one."),
        KeyHelp.HelpOnly(Key.Right, "the tab to the right", tip: "Show the tab to the right of this one."),
        KeyHelp.HelpOnly(Key.Of('<'), "scroll the columns left", tip: "Scroll a table too wide for the terminal to the left."),
        KeyHelp.HelpOnly(Key.Of('>'), "scroll the columns right", tip: "Scroll a table too wide for the terminal to the right."),
        KeyHelp.HelpOnly(Key.F5, "refresh this tab now", tip: "Refresh this tab now, as its timer would; nothing starts while it is refreshing."),
    ];

    public static IReadOnlyList<KeyHelp> SessionsKeys { get; } =
    [
        new(Key.Enter, "Open", "open in its own tab, or switch to it", Tip: "Open this session in its own tab, or switch to that tab if it is open."),
        new(Key.Of('n'), "New session", "new session", Tip: "Start a new session, in a new worktree or on a current branch."),
        new(Key.Of('r'), "Rename", "rename (restarts the session)", Tip: "Give the session a new name; it restarts under it, conversation kept."),
        new(Key.Of('x'), "Stop", "stop", Tip: "Stop the session; it stays listed as stopped and can be resumed later."),
        new(Key.Of('d'), "Delete", "delete, and its worktree if nothing is lost", Tip: "Delete the session, and its worktree too when no work would be lost."),
        new(Key.Of('R'), "Resume", "resume every stopped or failed session shown, each in its own tab", Tip: "Resume each stopped or failed session shown, in its own tab; / filters which."),
        new(Key.Of('C'), "Close empty", "close tabs of unused terminal sessions", Tip: "Close the tabs of terminal sessions that were never sent a message."),
        new(Key.Of('b'), "Adopt", "move a terminal session into the background", Tip: "Move a plain claude session from its own tab into the background."),
        new(Key.Of('/'), "Filter", "filter", Tip: "Type to show only the rows that match; Esc clears the filter."),
        new(Key.Of('?'), "Help", "help", Tip: "Show every key and what each column means."),
        new(Key.Of('q'), "Quit", "quit", Tip: "Quit; your sessions keep running in the background."),
        new(Key.Of('c'), "Copy path", "copy the folder path", Tip: "Copy the row's folder path to the clipboard."),
        new(Key.Of('f'), "Open folder", "open the folder in your editor", Tip: "Open the row's folder in the editor chosen on the Config tab."),
        new(Key.Of('s'), "Open solution", "open its solution", Tip: "Open the row's Visual Studio solution, asking when there are several."),
        new(Key.Of('p'), "Open PR", "open its pull request in the browser", Tip: "Open the row's pull request in your browser."),
        new(Key.Of('m'), "Memory", "what its memory is made of", Tip: "Show what the session's memory is made of; the detail pane shows the total."),
        new(Key.Of('U'), "Update", UpdateLine, Tip: "Update to the newest release, in a tab of its own."),
        new(Key.Of('!'), "Report", ReportLine, Tip: ReportTip),
        .. TabKeyHelp,
    ];

    public static IReadOnlyList<KeyHelp> WorktreesKeys { get; } =
    [
        new(Key.Of('d'), "Remove", "remove or delete (refused if work is lost)", Tip: "Remove this worktree or delete a branch row's branch; refused if work is lost."),
        new(Key.Of('D'), "Force remove", "force remove or delete after typing its name", Tip: "Remove this worktree or delete a branch row's branch, whatever it holds."),
        new(Key.Of('P'), "Prune", "prune stale registrations in its repository", Tip: "Prune the stale worktree registrations of this row's repository."),
        new(Key.Of('n'), "New session", "start a session in this worktree", Tip: "Start a new session in this worktree, on the branch checked out there."),
        new(Key.Of('o'), "Open shell", "open a PowerShell tab there", Tip: "Open a PowerShell tab in this worktree's folder."),
        new(Key.Enter, "Open session", "open its session's tab, or switch to it", Tip: "Open this worktree's session in its own tab, or switch to that tab if open."),
        new(Key.Escape, "Sessions", "back to sessions (clears a filter first)", Tip: "Clear the filter, or go back to the sessions view when there is none."),
        new(Key.Of('/'), "Filter", "filter", Tip: "Type to show only the rows that match; Esc clears the filter."),
        new(Key.Of('?'), "Help", "help", Tip: "Show every key and what each column means."),
        new(Key.Of('q'), "Quit", "quit", Tip: "Quit; your sessions keep running in the background."),
        new(Key.Of('c'), "Copy path", "copy the folder path", Tip: "Copy the row's folder path to the clipboard."),
        new(Key.Of('f'), "Open folder", "open the folder in your editor", Tip: "Open the row's folder in the editor chosen on the Config tab."),
        new(Key.Of('s'), "Open solution", "open its solution", Tip: "Open the row's Visual Studio solution, asking when there are several."),
        new(Key.Of('p'), "Open PR", "open its pull request in the browser", Tip: "Open the row's pull request in your browser."),
        new(Key.Of('U'), "Update", UpdateLine, Tip: "Update to the newest release, in a tab of its own."),
        new(Key.Of('!'), "Report", ReportLine, Tip: ReportTip),
        .. TabKeyHelp,
    ];

    public static IReadOnlyList<KeyHelp> ConfigKeys { get; } =
    [
        new(Key.Enter, "Edit", "edit the setting, or say how to set up", Tip: "Edit the setting (all roots, one per line), or say how to change the setup."),
        new(Key.Escape, "Sessions", "back to sessions (clears a filter first)", Tip: "Clear the filter, or go back to the sessions view when there is none."),
        new(Key.Of('/'), "Filter", "filter", Tip: "Type to show only the rows that match; Esc clears the filter."),
        new(Key.Of('?'), "Help", "help", Tip: "Show every key and what each column means."),
        new(Key.Of('q'), "Quit", "quit", Tip: "Quit; your sessions keep running in the background."),
        new(Key.Of('c'), "Copy", "copy the path", Tip: "Copy the row's path to the clipboard."),
        new(Key.Of('f'), "Open folder", "open the folder, or the settings or log file", Tip: "Open the folder, or the settings or log file, in the editor chosen above."),
        new(Key.Of('d'), "Clear log", "clear the diagnostic log, after asking", Tip: "Delete cader.log and cader.log.1 after asking; the report's events stay."),
        new(Key.Of('U'), "Update", UpdateLine, Tip: "Update to the newest release, in a tab of its own."),
        new(Key.Of('!'), "Report", ReportLine, Tip: ReportTip),
        .. TabKeyHelp,
    ];

    /// <summary>The TAB cell of a plain claude in its own terminal; a session shown in a session tab has its cell filled with that tab's colour instead.</summary>
    public const string TerminalSwatch = "▯";

    /// <summary>The block a session tab's colour is drawn with: two cells, leaving the column's third as space before the state.</summary>
    public const string TabBlock = "██";

    /// <summary>The words while a tab's first load runs; a closed enum, so a new tab fails the build here.</summary>
    public static string WaitingText(ConsoleTab tab) => tab switch
    {
        ConsoleTab.Sessions => "Listing sessions",
        ConsoleTab.Worktrees => "Scanning worktrees",
        ConsoleTab.Config => "Reading the settings",
    };

    /// <summary>The words before the banner's failure message; a closed enum, so a new tab fails the build here.</summary>
    public static string LoadFailurePrefix(ConsoleTab tab) => tab switch
    {
        ConsoleTab.Sessions => "listing failed",
        ConsoleTab.Worktrees => "scan failed",
        ConsoleTab.Config => "reading failed",
    };

    /// <summary>The table's title while a filter is being typed: "filter: tem_  2 of 7 sessions match "tem"".</summary>
    public static string FilterBeingTyped(string text, TableTitle title) => $"filter: {text}_  {Describe(title)}";

    /// <summary>The glyph before a state's word: a closed enum, so a new kind fails the build here.</summary>
    public static string StateGlyph(DisplayStateKind kind) => kind switch
    {
        DisplayStateKind.NeedsInput => "◐",
        DisplayStateKind.Working => "●",
        DisplayStateKind.Idle => "○",
        DisplayStateKind.Completed => "✓",
        DisplayStateKind.Failed => "✗",
        DisplayStateKind.Stopped => "■",
        DisplayStateKind.Unknown => "?",
    };

    /// <summary>The word in the STATE column: a known state's own word, "starting" for a job with no status yet, and an unknown combination's raw words so it is noticed.</summary>
    public static string StateWord(DisplayState state) => state switch
    {
        DisplayState.Starting => "starting",
        DisplayState.Unknown unknown => unknown.RawWords,
        DisplayState.Known known => KnownStateWord(known.Kind),
        _ => throw new UnreachableException(),
    };

    private static string KnownStateWord(DisplayStateKind kind) => kind switch
    {
        DisplayStateKind.NeedsInput => "needs input",
        DisplayStateKind.Working => "working",
        DisplayStateKind.Idle => "idle",
        DisplayStateKind.Completed => "completed",
        DisplayStateKind.Failed => "failed",
        DisplayStateKind.Stopped => "stopped",
        DisplayStateKind.Unknown => throw new UnreachableException(),
    };

    /// <summary>The glyph before a worktree health's word, so the health reads without its colour: a closed enum, so a new kind fails the build here.</summary>
    public static string HealthGlyph(WorktreeHealthKind kind) => kind switch
    {
        WorktreeHealthKind.Ok => "✓",
        WorktreeHealthKind.Locked => "◆",
        WorktreeHealthKind.Orphan => "△",
        WorktreeHealthKind.Broken => "✗",
        WorktreeHealthKind.Empty => "□",
        WorktreeHealthKind.Elsewhere => "↗",
        WorktreeHealthKind.Branch => "⑂",
        WorktreeHealthKind.Origin => "○",
    };

    /// <summary>The HEALTH cell: the health's glyph and its word, in the row's colour.</summary>
    public static string HealthCell(WorktreeHealthKind kind) => $"{HealthGlyph(kind)} {HealthWord(kind)}";

    /// <summary>A health's word, the one the HEALTH cell, the counts and the pane's Health line all use.</summary>
    private static string HealthWord(WorktreeHealthKind kind) => kind.ToString().ToLowerInvariant();

    public static ColumnHelp NameColumn { get; } = new("NAME", $"the session name given at launch, or the folder name for an unnamed one, under a heading\nnaming its repository, or the folder's label outside a repository, after {FolderGoneWords}:\nwhen the folder no longer exists");

    public static ColumnHelp TabColumn { get; } = new("TAB", $"a block in the tab's colour, faded as the tab looks from the console's own tab,\nwhile a tab opened here shows the session; {TerminalSwatch} grey for a plain claude in its own terminal");

    public static ColumnHelp StateColumn { get; } = new("STATE", "◐ needs input (amber), ● working (cyan), ○ idle (white),\n✓ completed (green), ✗ failed (red), ■ stopped (grey), ? unknown (grey)");

    /// <summary>The heading of a session whose folder has gone: the mark first, so a narrow column elides the label and keeps it.</summary>
    public static string FolderGone(string label) => $"{FolderGoneWords}: {label}";

    /// <summary>A Sessions row's hover tip: only a session whose folder has gone has one, and only a background one can be deleted with d.</summary>
    public static string SessionRowTip(SessionRow row) => row switch
    {
        { Location: SessionLocation.FolderGone, Session: Session.Background } => "This session's folder no longer exists; d deletes the session.",
        { Location: SessionLocation.FolderGone } => "This session's folder no longer exists.",
        _ => string.Empty,
    };

    public static ColumnHelp PullRequestColumn { get; } = new("PR", "its pull request: #123 on GitHub (open, draft, merged, closed),\n!123 on Azure DevOps (active, draft, completed, abandoned); blank if none or not yet known");

    public static ColumnHelp AccountColumn { get; } = new("ACCOUNT", "the account it belongs to: ● logged in now, ? not yet seen logged in; Config lists and renames them");

    public static ColumnHelp AgeColumn { get; } = new("AGE", "time since the session started");

    public static ColumnHelp WorktreeColumn { get; } = new("WORKTREE", "the folder's slug, or its label when not named after its repository, or a branch row's branch,\nunder a heading naming the repository");

    public static ColumnHelp HealthColumn { get; } = new("HEALTH", "✓ ok, ◆ locked (by another tool), △ orphan (a folder git has forgotten),\n✗ broken (registration or owner gone), □ empty (a leftover folder),\n↗ elsewhere (a worktree of a repository outside the roots),\n⑂ branch (no worktree), ○ origin (only on origin); the pane gives the reason");

    // Beside HealthColumn, because WorktreesColumns below reads it as the type initialises, in this file's order.
    public static ColumnHelp BranchColumn { get; } = new("BRANCH", "▲ left behind (commits after its pull request merged), ✗ gone (deleted on origin),\n✗ closed (its pull request closed unmerged), · no PR (none on its host);\nblank when none applies or not yet known; the pane gives the reason");

    public static ColumnHelp LastCommitColumn { get; } = new("LAST COMMIT", "time since the last commit");

    public static ColumnHelp SettingColumn { get; } = new("SETTING", "a setting kept in config.json, or, under the Setup heading, setup it reads but does not change");

    public static ColumnHelp ValueColumn { get; } = new("VALUE", "its value; a setup row that needs attention says what to run");

    public static ColumnHelp SessionColumn { get; } = new("SESSION", "the session running in it, if any");

    public static IReadOnlyList<ColumnHelp> SessionsColumns { get; } = [NameColumn, TabColumn, StateColumn, PullRequestColumn, AccountColumn, AgeColumn];

    public static IReadOnlyList<ColumnHelp> WorktreesColumns { get; } = [WorktreeColumn, HealthColumn, BranchColumn, LastCommitColumn, SessionColumn];

    public static IReadOnlyList<ColumnHelp> ConfigColumns { get; } = [SettingColumn, ValueColumn];

    /// <summary>The sessions pane's lines, for the help.</summary>
    public static IReadOnlyList<ColumnHelp> SessionsPane { get; } =
    [
        new("Waiting", "what it waits for: Claude Code's reason, a first prompt, or nothing"),
        new("Started", "when the session started"),
        new("Prompted", "when it was last seen going from waiting or idle to working"),
        new("Folder", "the folder it runs in"),
        new("Session", "the session id"),
        new("Worktree", "the worktree's slug, the path inside the repository, or the main checkout"),
        new("Branch", "the branch checked out there"),
        new("Tab", "the tab opened for it and the tab's colour, or its own terminal"),
        new("Changes", "changed files, unpushed commits and the last commit there"),
        new("Memory", "what the session and everything it started use; m for the breakdown"),
        new("Account", "its account's label, email and organisation, and whether it is logged in now or which to log in as"),
    ];

    /// <summary>The worktrees pane's lines, for the help.</summary>
    public static IReadOnlyList<ColumnHelp> WorktreesPane { get; } =
    [
        new("Health", "ok; locked (by another tool); orphan (a folder git has forgotten);\nbroken (registration or owner gone); empty (a leftover folder);\nelsewhere (a worktree of a repository outside the roots);\nbranch (checked out in no worktree); origin (only on origin); then the reason in full"),
        new("Branch", "the branch and whether a worktree's is merged into origin's default branch,\nor, when BRANCH flags it, the flag's reason in full"),
        new("PR", "its pull request, as the sessions table shows it, or none; p opens it"),
        new("Folder", "the worktree's folder"),
        new("Created", "when Cader created it, or not by Cader"),
        new("Repository", "a branch or origin row's repository, in place of Folder and Created"),
        new("Changes", "changed or untracked files, and commits on its branch that no remote has"),
        new("Unpushed", "the newest five unpushed commits, or the changed files when none, and how many more"),
    ];

    /// <summary>The sessions view's help: its keys and columns, then the Claude Code executable the console drives.</summary>
    public static string SessionsHelp(string claudeExecutable) => $"{Help(SessionsKeys, SessionsColumns, SessionsPane)}\n\nClaude Code: {claudeExecutable}";

    public static string WorktreesHelp => Help(WorktreesKeys, WorktreesColumns, WorktreesPane);

    public static string ConfigHelp => Help(ConfigKeys, ConfigColumns, pane: []);

    /// <summary>The keys two to a line, so the dialog stays short enough for a small terminal.</summary>
    private static string KeyColumns(IReadOnlyList<KeyHelp> keys)
    {
        // No key name is longer than six characters (WordingOfScreensShould holds it), so seven leaves a space after each.
        const int keyWidth = 7;
        static string Entry(KeyHelp key) => $"{key.HelpKey.PadRight(keyWidth)}{key.Line}";
        int width = keys.Where((_, index) => index % 2 == 0).Max(key => Entry(key).Length) + 3;

        return string.Join('\n', keys.Chunk(2).Select(pair => pair.Length == 2 ? Entry(pair[0]).PadRight(width) + Entry(pair[1]) : Entry(pair[0])));
    }

    /// <summary>The keys, the columns, and the pane's lines when the view has a pane.</summary>
    private static string Help(IReadOnlyList<KeyHelp> keys, IReadOnlyList<ColumnHelp> columns, IReadOnlyList<ColumnHelp> pane)
    {
        IEnumerable<string> paneSection = pane.Count == 0 ? [] : [string.Empty, "Pane", .. Explained(pane)];

        return string.Join('\n', ["Keys", KeyColumns(keys), string.Empty, "Columns", .. Explained(columns), .. paneSection, string.Empty, "Report a problem (!)", .. ReportParagraphs.SelectMany(paragraph => StatusLayout.Rows(paragraph, HelpWidth))]);
    }

    private const int HelpWidth = 100;

    private static IEnumerable<string> ReportParagraphs => [ReportHolds, ReportNeverAttachesTheLog, ReportLogFile];

    /// <summary>Each name padded to the longest, its meaning beside it and its further lines indented to match.</summary>
    private static IEnumerable<string> Explained(IReadOnlyList<ColumnHelp> entries)
    {
        int width = entries.Max(entry => entry.Name.Length) + 2;
        string indent = new(' ', width);

        return entries.Select(entry => $"{entry.Name.PadRight(width)}{entry.Meaning.Replace("\n", "\n" + indent, StringComparison.Ordinal)}");
    }
}