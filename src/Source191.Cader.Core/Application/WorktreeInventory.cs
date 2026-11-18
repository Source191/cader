using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using System.Diagnostics.CodeAnalysis;
using IOPath = System.IO.Path;

namespace Source191.Cader.Application;

/// <summary>
/// Everything under the repository roots and the worktree root that is, or was, a worktree: git's own listing per repository, plus the
/// leftovers git has forgotten (orphans under .claude\worktrees, folders under a root or the worktree root whose registration is gone, empty folders
/// that worktree tools leave behind, and any empty folder under the worktree root, which git leaves when a program held a worktree's folder as it
/// was removed). Read-only: nothing here prunes, deletes or touches git state.
/// </summary>
public sealed class WorktreeInventory(IGit git, IFolders folders, CurrentSettings settings, PullRequestCatalogue pullRequests, DiagnosticLog log)
{
    private readonly WorktreeInspector inspector = new(git);
    private readonly OncePerFolder unreadableRepositories = new(log, FailureSite.Load, CodeName.For(typeof(WorktreeInventory)));
    private readonly OncePerFolder unreadableWorktrees = new(log, FailureSite.Load, CodeName.For(typeof(WorktreeInventory)));
    private readonly OncePerFolder unreadableBranches = new(log, FailureSite.Load, CodeName.For(typeof(WorktreeInventory)));
    private readonly OncePerFolder uncountableBranches = new(log, FailureSite.Load, CodeName.For(typeof(WorktreeInventory)));

    private readonly SharedSnapshot<IReadOnlyList<FolderPath>> latestRepositories = new([]);

    private const string GitDirPrefix = "gitdir:";

    // A git read is mostly process start-up (about 80 ms); eight at once brings a scan of about 120 reads under the 3-second slow line without crowding the machine.
    private const int ScanSlots = 8;

    /// <summary>What a child of a repository root is: a repository, a worktree whose registration may be gone, an empty folder a tool left behind, or none of these.</summary>
    private enum FolderKind
    {
        Repository,
        Candidate,
        EmptyLeftover,
        Other,
    }

    /// <summary>The repositories under the roots that the latest scan found, in name order; none before the first scan.</summary>
    public IReadOnlyList<FolderPath> Repositories => latestRepositories.Value;

    public async Task<IReadOnlyList<WorktreeRow>> ScanAsync(IReadOnlyList<Session> sessions, CancellationToken cancellationToken)
    {
        Settings snapshot = settings.Value;
        List<WorktreeRow> rows = [];
        List<FolderPath> repositories = [];
        List<FolderPath> candidates = [];

        foreach (FolderPath root in snapshot.RepositoryRoots.Where(root => folders.DirectoryExists(root.Value)))
        {
            foreach (FolderPath child in Paths(folders.Directories(root.Value)).OrderBy(child => child.Key, StringComparer.Ordinal))
            {
                switch (Classify(child))
                {
                    case FolderKind.Repository:
                        repositories.Add(child);
                        break;
                    case FolderKind.Candidate:
                        candidates.Add(child);
                        break;
                    case FolderKind.EmptyLeftover:
                        rows.Add(EmptyLeftover(root, child));
                        break;
                    case FolderKind.Other:
                        break;
                }
            }
        }

        IReadOnlyList<FolderPath> underWorktreeRoot = [.. WorktreeRootChildren(snapshot)];
        candidates.AddRange(underWorktreeRoot.Where(child => folders.FileExists(IOPath.Combine(child.Value, ".git"))));

        // Read together, merged in name order: a repository's orphans are judged against its own registrations and those before it, as a serial scan judged them.
        using SemaphoreSlim slots = new(ScanSlots);
        Registrations[] found = await Task.WhenAll(repositories.Select(repository => RegisteredRowsAsync(repository, slots, cancellationToken)));
        HashSet<FolderPath> registered = [];
        List<WorktreeRow> originCandidates = [];
        Dictionary<(FolderPath, BranchName), Upstream> upstreams = [];
        foreach ((FolderPath repository, Registrations registrations) in repositories.Zip(found))
        {
            registered.UnionWith(registrations.Folders);
            rows.AddRange(registrations.Rows);
            rows.AddRange(OrphanRows(repository, registered));
            rows.AddRange(registrations.Branches.Rows);
            originCandidates.AddRange(registrations.Branches.OriginCandidates);
            foreach (GitBranch.Local local in registrations.Branches.Locals)
            {
                upstreams[(repository, local.Name)] = local.Upstream;
            }
        }

        rows.AddRange(candidates.Where(candidate => !registered.Contains(candidate)).Select(BrokenWorktree));
        rows.AddRange(underWorktreeRoot.Where(child => !registered.Contains(child) && folders.IsEmpty(child.Value)).Select(child => EmptyLeftover(snapshot.WorktreeRoot, child)));

        // Never awaited: the rows show what the catalogue already holds, and the next rescan picks up what lands.
        _ = pullRequests.RefreshStaleAsync(BranchesOf([.. rows, .. originCandidates]));

        return await ShownAsync(rows, originCandidates, upstreams, sessions, cancellationToken);
    }

    private FolderKind Classify(FolderPath child)
    {
        string gitPath = IOPath.Combine(child.Value, ".git");
        if (folders.DirectoryExists(gitPath))
        {
            return FolderKind.Repository;
        }

        if (folders.FileExists(gitPath))
        {
            return FolderKind.Candidate;
        }

        return IsEmptyLeftover(child) ? FolderKind.EmptyLeftover : FolderKind.Other;
    }

    /// <summary>Each row's repository and branch, for the rows whose head is on one.</summary>
    private static IEnumerable<(FolderPath Checkout, BranchName Branch)> BranchesOf(IEnumerable<WorktreeRow> rows)
    {
        foreach (WorktreeRow row in rows)
        {
            if (row.Head is Head.OnBranch on)
            {
                yield return (row.Repository, on.Branch);
            }
        }
    }

    /// <summary>A leftover's head is Unknown, so the catalogue answers no branch to look up, as the row's default does.</summary>
    private WorktreeRow WithPullRequest(WorktreeRow row) => row with { PullRequest = pullRequests.Find(row.Repository, row.Head) };

    /// <summary>The paths among folder listings that are full paths; anything else is skipped.</summary>
    private static IEnumerable<FolderPath> Paths(IEnumerable<string> texts) => texts.Parsed<FolderPath>();

    /// <summary>
    /// The worktree root's children. Those with a .git file are broken when no repository lists them, or elsewhere when
    /// their gitdir still exists (see <see cref="BrokenWorktree"/>); empty ones are empty leftovers, which is what git
    /// leaves when a program held a worktree's folder as it was removed. A root that is also a repository root was
    /// scanned already.
    /// </summary>
    private IEnumerable<FolderPath> WorktreeRootChildren(Settings snapshot)
    {
        FolderPath worktreeRoot = snapshot.WorktreeRoot;
        if (snapshot.RepositoryRoots.Contains(worktreeRoot) || !folders.DirectoryExists(worktreeRoot.Value))
        {
            return [];
        }

        return Paths(folders.Directories(worktreeRoot.Value)).OrderBy(child => child.Key, StringComparer.Ordinal);
    }

    /// <summary>An empty folder no repository explains, its root standing in for the repository nothing on disc names.</summary>
    private WorktreeRow EmptyLeftover(FolderPath root, FolderPath folder) =>
        Leftover(root, folder, new WorktreeObservation.Unregistered(new GitPointer.NoGitFile(), UnregisteredFolder.EmptyLeftover), ownerExists: false);

    /// <summary>
    /// The rows with their sessions, pull requests and branch flags, then each origin candidate whose landed lookup flags
    /// it left behind or closed; one still looking, or flagged nothing, waits for a later scan.
    /// </summary>
    private async Task<IReadOnlyList<WorktreeRow>> ShownAsync(IReadOnlyList<WorktreeRow> rows, IReadOnlyList<WorktreeRow> originCandidates, IReadOnlyDictionary<(FolderPath, BranchName), Upstream> upstreams, IReadOnlyList<Session> sessions, CancellationToken cancellationToken)
    {
        List<WorktreeRow> shown = [];
        foreach (WorktreeRow row in rows)
        {
            // A branch row's path is its repository, so a session in the main checkout is not the branch's.
            WorktreeRow withSession = row.Placement is Placement.NoFolder ? row : row with { Session = LiveSessionIn(row.Path, sessions) };
            shown.Add(await FlaggedAsync(WithPullRequest(withSession), upstreams, cancellationToken));
        }

        foreach (WorktreeRow candidate in originCandidates)
        {
            WorktreeRow flagged = await FlaggedAsync(WithPullRequest(candidate), upstreams, cancellationToken);
            if (flagged.Flag is BranchFlagKind.LeftBehind or BranchFlagKind.Closed)
            {
                shown.Add(flagged);
            }
        }

        return shown;
    }

    /// <summary>
    /// The row's branch flag, counting the branch's commits after its pull request's head only when that pull request has
    /// merged with a recorded head. A branch row's commits at risk are none only when that count is exactly zero: they are
    /// inside what merged. A count git could not make leaves them as they were.
    /// </summary>
    private async Task<WorktreeRow> FlaggedAsync(WorktreeRow row, IReadOnlyDictionary<(FolderPath, BranchName), Upstream> upstreams, CancellationToken cancellationToken)
    {
        if (row.Head is not Head.OnBranch on)
        {
            return row;
        }

        int? after = BranchFlags.TryMergedHead(row.PullRequest, out CommitId? head) ? await git.CountCommitsAfterAsync(row.Repository, RevisionOf(row, on.Branch), head, cancellationToken) : null;
        Upstream upstream = upstreams.GetValueOrDefault((row.Repository, on.Branch), new Upstream.None());

        return row with
        {
            Flag = BranchFlags.Of(row.PullRequest, upstream, after),
            CommitsAfterMerge = after ?? 0,
            UnpushedCommits = row.Health.Kind is WorktreeHealthKind.Branch && after == 0 ? 0 : row.UnpushedCommits,
        };
    }

    /// <summary>An origin row's branch is origin's copy; every other row's is the local branch.</summary>
    private static string RevisionOf(WorktreeRow row, BranchName branch) => row.Health.Kind is WorktreeHealthKind.Origin ? $"refs/remotes/origin/{branch.Value}" : $"refs/heads/{branch.Value}";

    /// <summary>
    /// One branch listing per repository. An empty worktree listing is one git could not give (a real one always has the
    /// main checkout), so nothing says which branches are free, and no branch is listed.
    /// </summary>
    private async Task<RepositoryBranches> BranchesAsync(FolderPath repository, IReadOnlyList<GitWorktreeEntry> entries, SemaphoreSlim slots, CancellationToken cancellationToken)
    {
        if (entries.Count == 0)
        {
            return RepositoryBranches.None;
        }

        HashSet<BranchName> checkedOut = [.. entries.Select(entry => entry.Head).OfType<Head.OnBranch>().Select(on => on.Branch)];

        return await unreadableBranches.RunAsync(repository, () => ListedBranchesAsync(repository, checkedOut, slots, cancellationToken), IsGitFailure, _ => RepositoryBranches.None);
    }

    /// <summary>
    /// A row for each local branch checked out nowhere, leaving out the one that tracks origin's default branch, and an
    /// origin candidate for each branch of yours only on origin but its default; origin's default is asked for only when
    /// there is either. A branch row whose commits at risk git cannot count is left out: nothing at risk is never guessed.
    /// </summary>
    private async Task<RepositoryBranches> ListedBranchesAsync(FolderPath repository, HashSet<BranchName> checkedOut, SemaphoreSlim slots, CancellationToken cancellationToken)
    {
        IReadOnlyList<GitBranch> listed = await InSlotAsync(slots, () => git.ListBranchesAsync(repository, cancellationToken), cancellationToken);
        IReadOnlyList<GitBranch.Local> locals = [.. listed.OfType<GitBranch.Local>()];
        IReadOnlyList<GitBranch.Local> free = [.. locals.Where(branch => !checkedOut.Contains(branch.Name))];
        IReadOnlyList<GitBranch.OriginOnly> yours = [.. listed.OfType<GitBranch.OriginOnly>().Where(branch => branch.Mine)];
        if (free.Count == 0 && yours.Count == 0)
        {
            return new RepositoryBranches([], [], locals);
        }

        RemoteBranch? originDefault = RemoteBranch.TryParse(await InSlotAsync(slots, () => git.DefaultRemoteBranchAsync(repository.Value, cancellationToken), cancellationToken), out RemoteBranch? parsed) ? parsed : null;
        List<WorktreeRow> rows = [];
        foreach (GitBranch.Local branch in free.Where(branch => !(branch.Upstream is Upstream.Tracking tracking && tracking.Branch == originDefault)))
        {
            int? atRisk = await uncountableBranches.RunAsync<int?>(repository, async () => await InSlotAsync(slots, () => git.CountCommitsOnNoRemoteAsync(repository, branch.Name, cancellationToken), cancellationToken), IsGitFailure, _ => null);
            rows.AddRange(atRisk is int count ? [BranchRow(repository, branch, WorktreeHealth.CheckedOutNowhere) with { UnpushedCommits = count }] : []);
        }

        return new RepositoryBranches(rows, [.. yours.Where(branch => branch.Name != originDefault?.Branch).Select(branch => BranchRow(repository, branch, WorktreeHealth.OnlyOnOrigin))], locals);
    }

    private static WorktreeRow BranchRow(FolderPath repository, GitBranch branch, WorktreeHealth health) =>
        new(repository, repository, new Head.OnBranch(branch.Name), health, 0, 0, new LastCommit.Made(branch.TipAt), new WorktreeSession.None(), 0, OwnerExists: true, FolderExists: true);

    /// <summary>One repository's branches as the scan uses them: the branch rows, the origin rows that wait on their lookup, and every local branch for its upstream.</summary>
    private sealed record RepositoryBranches(IReadOnlyList<WorktreeRow> Rows, IReadOnlyList<WorktreeRow> OriginCandidates, IReadOnlyList<GitBranch.Local> Locals)
    {
        public static readonly RepositoryBranches None = new([], [], []);
    }

    private async Task<Registrations> RegisteredRowsAsync(FolderPath repository, SemaphoreSlim slots, CancellationToken cancellationToken)
    {
        // One unusable folder must not blank the whole view.
        IReadOnlyList<GitWorktreeEntry> entries = await unreadableRepositories.RunAsync<IReadOnlyList<GitWorktreeEntry>>(repository, () => InSlotAsync(slots, () => git.ListWorktreesAsync(repository.Value, cancellationToken), cancellationToken), IsGitFailure, _ => []);

        List<FolderPath> paths = [];
        List<Task<WorktreeRow>> rows = [];
        foreach (GitWorktreeEntry entry in entries.Where(entry => !entry.IsMain))
        {
            if (!FolderPath.TryParse(entry.Path, out FolderPath? path))
            {
                continue;
            }

            paths.Add(path);
            rows.Add(RegisteredRowAsync(repository, path, entry, slots, cancellationToken));
        }

        Task<RepositoryBranches> branches = BranchesAsync(repository, entries, slots, cancellationToken);

        return new Registrations(paths, await Task.WhenAll(rows), await branches);
    }

    private async Task<WorktreeRow> RegisteredRowAsync(FolderPath repository, FolderPath path, GitWorktreeEntry entry, SemaphoreSlim slots, CancellationToken cancellationToken)
    {
        // The disc decides whether the folder exists; git's prunable says only that its .git file is gone, and such a
        // folder may still hold work, so it is counted like any leftover.
        bool folderExists = folders.DirectoryExists(path.Value);
        WorktreeHealth health = WorktreeHealth.From(new WorktreeObservation.Registered(folderExists, entry.Lock, entry.IsPrunable));

        return folderExists && !entry.IsPrunable
            ? await InspectedAsync(repository, path, entry, health, slots, cancellationToken)
            : new WorktreeRow(repository, path, entry.Head, health, 0, 0, new LastCommit.Unknown(), new WorktreeSession.None(), CountLeftoverFiles(path), OwnerExists: true, FolderExists: folderExists);
    }

    private async Task<WorktreeRow> InspectedAsync(FolderPath repository, FolderPath path, GitWorktreeEntry entry, WorktreeHealth health, SemaphoreSlim slots, CancellationToken cancellationToken)
    {
        return await unreadableWorktrees.RunAsync(
            path,
            async () =>
            {
                WorktreeCounts counts = await InSlotAsync(slots, () => inspector.CountAsync(path, cancellationToken), cancellationToken);

                return new WorktreeRow(
                    repository,
                    path,
                    entry.Head,
                    health,
                    counts.DirtyFiles,
                    counts.UnpushedCommits,
                    counts.LastCommitAt,
                    new WorktreeSession.None(),
                    LeftoverFileCount: 0,
                    OwnerExists: true,
                    FolderExists: true);
            },
            IsGitFailure,
            _ => new WorktreeRow(repository, path, entry.Head, health, 0, 0, new LastCommit.Unknown(), new WorktreeSession.None(), 0, OwnerExists: true, FolderExists: true));
    }

    private static bool IsGitFailure(Exception exception) => exception is GitException;

    /// <summary>One git read once a slot is free, the slot held only while it runs, so no read waits on another for a slot; a read still waiting when the scan is cancelled never starts.</summary>
    private static async Task<T> InSlotAsync<T>(SemaphoreSlim slots, Func<Task<T>> read, CancellationToken cancellationToken)
    {
        // Checked once the slot is held, not while waiting: a slot freed as the scan is cancelled can reach a waiter before its cancellation does.
        await slots.WaitAsync(CancellationToken.None);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await read();
        }
        finally
        {
            slots.Release();
        }
    }

    private IEnumerable<WorktreeRow> OrphanRows(FolderPath repository, HashSet<FolderPath> registered)
    {
        FolderPath claudeWorktrees = repository.Combine(IOPath.Combine(".claude", "worktrees"));
        if (!folders.DirectoryExists(claudeWorktrees.Value))
        {
            yield break;
        }

        foreach (FolderPath folder in Paths(folders.Directories(claudeWorktrees.Value)).OrderBy(folder => folder.Key, StringComparer.Ordinal))
        {
            if (registered.Contains(folder))
            {
                continue;
            }

            string gitFile = IOPath.Combine(folder.Value, ".git");
            GitPointer pointer = folders.FileExists(gitFile) ? TargetOf(folder) : new GitPointer.NoGitFile();
            WorktreeObservation observation = new WorktreeObservation.Unregistered(pointer, UnregisteredFolder.UnderClaudeWorktrees);
            yield return Leftover(repository, folder, observation, ownerExists: true);
        }
    }

    /// <summary>
    /// A direct child of a root or the worktree root with a .git file that no repository under the roots lists. Its
    /// .git file names a folder that exists: a live checkout of a repository outside the roots (a linked worktree, one
    /// of a bare repository, or a checkout made with a separate git directory), so elsewhere, never a leftover. It names
    /// a folder that is gone, or none: broken, its registration gone.
    /// </summary>
    private WorktreeRow BrokenWorktree(FolderPath folder)
    {
        if (TryGitDirOf(folder, out FolderPath? gitDir) && folders.DirectoryExists(gitDir.Value))
        {
            FolderPath repository = RepositoryOf(gitDir);
            WorktreeObservation elsewhere = new WorktreeObservation.Unregistered(new GitPointer.RegisteredElsewhere(repository), UnregisteredFolder.Other);

            return Leftover(repository, folder, elsewhere, ownerExists: true);
        }

        FolderPath owner = gitDir is not null && IsRegistration(gitDir) ? RepositoryOf(gitDir) : folder.Parent ?? folder;
        bool ownerExists = folders.DirectoryExists(IOPath.Combine(owner.Value, ".git"));
        WorktreeObservation observation = new WorktreeObservation.Unregistered(new GitPointer.TargetGone(), UnregisteredFolder.Other);

        return Leftover(owner, folder, observation, ownerExists);
    }

    /// <summary>
    /// The folder the .git file's gitdir line names, a relative one read from the .git file's own folder as git does;
    /// false when the file names none or the text is no path.
    /// </summary>
    private bool TryGitDirOf(FolderPath folder, [MaybeNullWhen(false)] out FolderPath gitDir)
    {
        string? text = folders.Lines(IOPath.Combine(folder.Value, ".git"))
            .Where(candidate => candidate.StartsWith(GitDirPrefix, StringComparison.Ordinal))
            .Select(line => line[GitDirPrefix.Length..].Trim().Replace(IOPath.AltDirectorySeparatorChar, IOPath.DirectorySeparatorChar))
            .FirstOrDefault();

        return FolderPath.TryParse(text is null || IOPath.IsPathFullyQualified(text) ? text : IOPath.Combine(folder.Value, text), out gitDir);
    }

    /// <summary>A registration sits at <c>&lt;git directory&gt;\worktrees\&lt;name&gt;</c>.</summary>
    private static bool IsRegistration(FolderPath gitDir) => gitDir.Parent is FolderPath worktrees && string.Equals(worktrees.Name, "worktrees", StringComparison.OrdinalIgnoreCase) && worktrees.Parent is not null;

    /// <summary>
    /// The repository a gitdir belongs to, read from its shape: a registration's git directory is two levels up, and is
    /// the repository itself when it is a bare one or the parent of a folder named .git; a gitdir that is no
    /// registration is a git directory of its own (a checkout made with <c>--separate-git-dir</c>).
    /// </summary>
    private static FolderPath RepositoryOf(FolderPath gitDir)
    {
        FolderPath gitDirectory = IsRegistration(gitDir) && gitDir.Parent?.Parent is FolderPath twoUp ? twoUp : gitDir;

        return string.Equals(gitDirectory.Name, ".git", StringComparison.OrdinalIgnoreCase) && gitDirectory.Parent is FolderPath repository ? repository : gitDirectory;
    }

    private WorktreeRow Leftover(FolderPath repository, FolderPath folder, WorktreeObservation observation, bool ownerExists) => new(repository, folder, new Head.Unknown(), WorktreeHealth.From(observation), 0, 0, new LastCommit.Unknown(), new WorktreeSession.None(), CountLeftoverFiles(folder), ownerExists, FolderExists: folders.DirectoryExists(folder.Value));

    /// <summary>Whether the registration a folder's .git file points at still exists; never names an owner.</summary>
    private GitPointer TargetOf(FolderPath folder) => TryGitDirOf(folder, out FolderPath? gitDir) && folders.DirectoryExists(gitDir.Value) ? new GitPointer.TargetExists() : new GitPointer.TargetGone();

    private bool IsEmptyLeftover(FolderPath folder)
    {
        bool leftoverName = folder.Name.EndsWith(".worktrees", StringComparison.OrdinalIgnoreCase);

        return leftoverName && folders.IsEmpty(folder.Value);
    }

    private int CountLeftoverFiles(FolderPath folder) => LeftoverFiles.Count(folders, folder);

    /// <summary>The first session with a process working in the worktree or a folder inside it.</summary>
    private static WorktreeSession LiveSessionIn(FolderPath worktree, IReadOnlyList<Session> sessions)
    {
        return sessions
            .Where(session => session is Session.Interactive or Session.Background { Process: JobProcess.Running })
            .Where(session => session.WorkingDirectory.IsUnder(worktree))
            .Select(WorktreeSession (session) => new WorktreeSession.Running(session.SessionId, session.Name))
            .FirstOrDefault(new WorktreeSession.None());
    }

    /// <summary>One repository's registered worktree folders and its rows, both in git's order, and its branches, merged into the scan in repository order.</summary>
    private sealed record Registrations(IReadOnlyList<FolderPath> Folders, IReadOnlyList<WorktreeRow> Rows, RepositoryBranches Branches);
}