using Source191.Cader.Domain;

namespace Source191.Cader.Application;

/// <summary>
/// The folders whose changes refresh a tab: the Claude folder's <c>sessions</c> and <c>jobs</c> folders for the Sessions
/// tab, and each listed repository's git folder and each repository root for the Worktrees tab. A linked worktree's
/// records live in its main repository's git folder, so watching each repository covers its worktrees. The repositories
/// are the latest scan's, so a repository cloned into a root, or a root saved in the settings, is watched from the scan after.
/// </summary>
public static class WatchPlan
{
    public static IReadOnlyList<PlannedWatch> For(Settings settings, ClaudeFolder claudeFolder, IReadOnlyList<FolderPath> repositories) =>
    [
        new PlannedWatch(WatchKind.ClaudeSessions, claudeFolder.Folder.Combine("sessions")),
        new PlannedWatch(WatchKind.ClaudeJobs, claudeFolder.Folder.Combine("jobs")),
        .. repositories.Distinct().Select(repository => new PlannedWatch(WatchKind.Repository, repository.Combine(".git"))),
        .. settings.RepositoryRoots.Distinct().Select(root => new PlannedWatch(WatchKind.Root, root)),
    ];
}