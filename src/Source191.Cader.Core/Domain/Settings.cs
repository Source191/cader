using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Source191.Cader.Domain;

/// <summary>
/// The settings, complete and immutable: where repositories live, where new worktrees go, which editor opens a
/// folder, how new worktrees branch and what is done first, and where the wizard starts a new session. Whatever a file leaves out has been resolved to its default by <see cref="Resolve"/>.
/// </summary>
public sealed record Settings
{
    private Settings(IReadOnlyList<FolderPath> repositoryRoots, FolderPath worktreeRoot, EditorChoice editor, WorktreeBase defaultBase, bool checkForUpdates, LaunchMode defaultPlace, bool diagnosticLog, BeforeBranching beforeBranching, RefreshInterval sessionsRefresh, RefreshInterval worktreesRefresh)
    {
        RepositoryRoots = repositoryRoots;
        WorktreeRoot = worktreeRoot;
        Editor = editor;
        DefaultBase = defaultBase;
        CheckForUpdates = checkForUpdates;
        DefaultPlace = defaultPlace;
        DiagnosticLog = diagnosticLog;
        BeforeBranching = beforeBranching;
        SessionsRefresh = sessionsRefresh;
        WorktreesRefresh = worktreesRefresh;
    }

    public IReadOnlyList<FolderPath> RepositoryRoots { get; }

    public FolderPath WorktreeRoot { get; }

    public EditorChoice Editor { get; }

    public WorktreeBase DefaultBase { get; }

    /// <summary>Whether the console asks the winget community repository, at most once a day, for a newer release of itself.</summary>
    public bool CheckForUpdates { get; }

    /// <summary>Where the wizard opened with n starts a new session: a new worktree, or the repository's current branch.</summary>
    public LaunchMode DefaultPlace { get; }

    /// <summary>Whether every diagnostic event and the counts are appended to the log file in the state folder.</summary>
    public bool DiagnosticLog { get; }

    /// <summary>What is done before a new worktree branches from origin's default branch: fetch it, fetch and fast-forward the local branch too, or nothing.</summary>
    public BeforeBranching BeforeBranching { get; }

    /// <summary>How often the Sessions tab lists the sessions on its own; off for no timer.</summary>
    public RefreshInterval SessionsRefresh { get; }

    /// <summary>How often the Worktrees tab rescans on its own while it is shown; off for no timer.</summary>
    public RefreshInterval WorktreesRefresh { get; }

    /// <summary>The base a file without one branches from: origin's default branch.</summary>
    public const WorktreeBase BaseWhenUnset = WorktreeBase.RemoteDefault;

    /// <summary>A file without the update setting has the check on.</summary>
    public const bool ChecksForUpdatesWhenUnset = true;

    /// <summary>A file without a place starts new sessions in a new worktree.</summary>
    public const LaunchMode PlaceWhenUnset = LaunchMode.NewWorktree;

    /// <summary>A file without the setting writes no log file.</summary>
    public const bool DiagnosticLogWhenUnset = false;

    /// <summary>A file without the setting fetches origin's default branch before a new worktree branches from it.</summary>
    public const BeforeBranching BeforeBranchingWhenUnset = BeforeBranching.Fetch;

    /// <summary>A file without the setting lists the sessions every 5 seconds.</summary>
    public static readonly RefreshInterval SessionsRefreshWhenUnset = RefreshInterval.From(5);

    /// <summary>A file without the setting rescans the shown Worktrees tab every 15 seconds.</summary>
    public static readonly RefreshInterval WorktreesRefreshWhenUnset = RefreshInterval.From(15);

    /// <summary>
    /// Fills in what is missing: no roots means <c>&lt;home&gt;\source\repos</c>; <see cref="WorktreeRootChoice.BesideFirstRoot"/> means <c>worktrees</c>
    /// beside the first root (inside it when it has no parent, such as <c>D:\</c>). A file's missing base, update setting, place, log and before-branching setting are <see cref="BaseWhenUnset"/>, <see cref="ChecksForUpdatesWhenUnset"/>, <see cref="PlaceWhenUnset"/>, <see cref="DiagnosticLogWhenUnset"/> and <see cref="BeforeBranchingWhenUnset"/>.
    /// </summary>
    public static Settings Resolve(IReadOnlyList<FolderPath> roots, WorktreeRootChoice worktreeRoot, EditorChoice editor, WorktreeBase defaultBase, FolderPath home, bool checkForUpdates, LaunchMode defaultPlace, bool diagnosticLog, BeforeBranching beforeBranching, RefreshInterval sessionsRefresh, RefreshInterval worktreesRefresh)
    {
        IReadOnlyList<FolderPath> resolvedRoots = roots is { Count: > 0 } ? roots : [home.Combine(Path.Combine("source", "repos"))];
        FolderPath first = resolvedRoots[0];
        FolderPath resolvedWorktreeRoot = worktreeRoot switch
        {
            WorktreeRootChoice.At chosen => chosen.Folder,
            WorktreeRootChoice.BesideFirstRoot => (first.Parent ?? first).Combine("worktrees"),
            _ => throw new UnreachableException(),
        };

        return new Settings(resolvedRoots, resolvedWorktreeRoot, editor, defaultBase, checkForUpdates, defaultPlace, diagnosticLog, beforeBranching, sessionsRefresh, worktreesRefresh);
    }

    /// <summary>
    /// Checks what the settings dialog holds, in the order its fields read: the roots (one per line, each a full path
    /// that exists, none listed twice), the worktree root (a full path, created when first used), then a custom editor's
    /// command, whose executable must exist by <paramref name="fileExists"/>.
    /// </summary>
    public static SettingsValidation Validate(SettingsText text, Func<string, bool> folderExists, Func<string, bool> fileExists)
    {
        List<string> lines = [.. text.Roots.Split('\n').Select(line => line.Trim()).Where(line => line.Length > 0)];
        List<FolderPath> roots = [];
        foreach (string line in lines)
        {
            if (!FolderPath.TryParse(line, out FolderPath? root))
            {
                return Refused(new SettingsProblem.RootNotFullPath(line));
            }

            roots.Add(root);
        }

        return roots switch
        {
            [] => Refused(new SettingsProblem.NoRoots()),
            _ when roots.FirstOrDefault(root => !folderExists(root.Value)) is FolderPath missing => Refused(new SettingsProblem.RootMissing(missing)),
            _ when roots.Where((root, index) => roots.Take(index).Contains(root)).FirstOrDefault() is FolderPath repeated => Refused(new SettingsProblem.RootAlreadyListed(repeated)),
            _ => Validated(text, roots, fileExists),
        };
    }

    /// <summary>The settings as the dialog shows them: roots one per line and the custom arguments on one line.</summary>
    public SettingsText ToText()
    {
        return new SettingsText(
            string.Join('\n', RepositoryRoots.Select(root => root.Value)),
            WorktreeRoot.Value,
            Editor.Preset,
            Editor.Executable,
            CommandArguments.Join(Editor.Arguments),
            DefaultBase,
            CheckForUpdates,
            DefaultPlace,
            DiagnosticLog,
            BeforeBranching,
            SessionsRefresh.Seconds.ToString(CultureInfo.InvariantCulture),
            WorktreesRefresh.Seconds.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>These settings with the update check turned the other way.</summary>
    public Settings WithUpdateCheckToggled() => new(RepositoryRoots, WorktreeRoot, Editor, DefaultBase, !CheckForUpdates, DefaultPlace, DiagnosticLog, BeforeBranching, SessionsRefresh, WorktreesRefresh);

    /// <summary>These settings with the diagnostic log turned the other way.</summary>
    public Settings WithDiagnosticLogToggled() => new(RepositoryRoots, WorktreeRoot, Editor, DefaultBase, CheckForUpdates, DefaultPlace, !DiagnosticLog, BeforeBranching, SessionsRefresh, WorktreesRefresh);

    public bool Equals(Settings? other)
    {
        return other is not null
            && RepositoryRoots.SequenceEqual(other.RepositoryRoots)
            && WorktreeRoot.Equals(other.WorktreeRoot)
            && Editor.Equals(other.Editor)
            && DefaultBase == other.DefaultBase
            && CheckForUpdates == other.CheckForUpdates
            && DefaultPlace == other.DefaultPlace
            && DiagnosticLog == other.DiagnosticLog
            && BeforeBranching == other.BeforeBranching
            && SessionsRefresh.Equals(other.SessionsRefresh)
            && WorktreesRefresh.Equals(other.WorktreesRefresh);
    }

    public override int GetHashCode() => HashCode.Combine(RepositoryRoots.Count, WorktreeRoot, Editor, DefaultBase);

    private static SettingsValidation Validated(SettingsText text, IReadOnlyList<FolderPath> roots, Func<string, bool> fileExists)
    {
        if (!FolderPath.TryParse(text.WorktreeRoot.Trim(), out FolderPath? worktreeRoot))
        {
            return Refused(new SettingsProblem.WorktreeRootNotFullPath());
        }

        if (!TypedInterval(text.SessionsRefresh, out RefreshInterval? sessionsRefresh))
        {
            return Refused(new SettingsProblem.RefreshIntervalNotWholeSeconds(RefreshedTab.Sessions));
        }

        if (!TypedInterval(text.WorktreesRefresh, out RefreshInterval? worktreesRefresh))
        {
            return Refused(new SettingsProblem.RefreshIntervalNotWholeSeconds(RefreshedTab.Worktrees));
        }

        if (text.Preset != EditorPreset.Custom)
        {
            return new SettingsValidation.Valid(new Settings(roots, worktreeRoot, EditorChoice.Of(text.Preset), text.Base, text.CheckForUpdates, text.Place, text.DiagnosticLog, text.BeforeBranching, sessionsRefresh, worktreesRefresh));
        }

        IReadOnlyList<string> arguments = CommandArguments.Split(text.Arguments);

        return EditorChoice.Validate(text.Executable, arguments, fileExists) switch
        {
            EditorChoiceValidation.Valid valid => new SettingsValidation.Valid(new Settings(roots, worktreeRoot, valid.Editor, text.Base, text.CheckForUpdates, text.Place, text.DiagnosticLog, text.BeforeBranching, sessionsRefresh, worktreesRefresh)),
            EditorChoiceValidation.Refused refused => Refused(refused.Problem),
            _ => throw new UnreachableException(),
        };
    }

    /// <summary>An interval as typed: a whole number of seconds from 0 to 3600, with spaces around it ignored.</summary>
    private static bool TypedInterval(string typed, [MaybeNullWhen(false)] out RefreshInterval interval) =>
        RefreshInterval.TryFrom(int.TryParse(typed.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out int seconds) ? seconds : null, out interval);

    private static SettingsValidation.Refused Refused(SettingsProblem problem) => new(problem);
}