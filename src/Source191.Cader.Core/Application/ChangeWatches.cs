using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.Application;

/// <summary>Keeps one watch per planned folder, telling the schedule each feeds about the changes that count.</summary>
public sealed class ChangeWatches(IFolderChanges changes, RefreshSchedule sessions, RefreshSchedule worktrees) : IDisposable
{
    private readonly List<IDisposable> live = [];

    public void Follow(IReadOnlyList<PlannedWatch> plan) => live.AddRange(plan.Select(watch => changes.Watch(watch.Folder, watch.Subfolders, _ => (watch.Feeds is RefreshedTab.Sessions ? sessions : worktrees).Started())));

    public void Dispose() => live.Clear();
}