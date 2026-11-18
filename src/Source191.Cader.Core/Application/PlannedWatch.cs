using Source191.Cader.Domain;

namespace Source191.Cader.Application;

/// <summary>A folder to watch and what it is, which says whether its subfolders are watched too and which tab its counted changes refresh.</summary>
public sealed record PlannedWatch(WatchKind Kind, FolderPath Folder)
{
    public bool Subfolders => Kind is WatchKind.ClaudeJobs or WatchKind.Repository;

    public RefreshedTab Feeds => Kind is WatchKind.ClaudeSessions or WatchKind.ClaudeJobs ? RefreshedTab.Sessions : RefreshedTab.Worktrees;
}