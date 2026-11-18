using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.Application;

/// <summary>
/// Keeps one watch per planned folder, telling the schedule each feeds about the changes <see cref="ChangeFilter"/>
/// counts. <see cref="Follow"/> runs as the console opens and after each scan, on whichever thread the scan ended on,
/// and the changes arrive on pool threads, so the watches sit behind a lock; nothing a change does takes it.
/// </summary>
public sealed class ChangeWatches(IFolderChanges changes, RefreshSchedule sessions, RefreshSchedule worktrees) : IDisposable
{
    private readonly Lock gate = new();
    private readonly Dictionary<PlannedWatch, IDisposable> live = [];

    /// <summary>Stops the watches no longer planned and starts the newly planned ones, leaving the rest running.</summary>
    public void Follow(IReadOnlyList<PlannedWatch> plan)
    {
        lock (gate)
        {
            foreach (PlannedWatch gone in live.Keys.Except(plan).ToList())
            {
                live[gone].Dispose();
                live.Remove(gone);
            }

            foreach (PlannedWatch added in plan.Where(watch => !live.ContainsKey(watch)).ToList())
            {
                live[added] = Start(added);
            }
        }
    }

    public void Dispose()
    {
        lock (gate)
        {
            foreach (IDisposable watch in live.Values)
            {
                watch.Dispose();
            }

            live.Clear();
        }
    }

    private IDisposable Start(PlannedWatch watch)
    {
        RefreshSchedule schedule = watch.Feeds switch
        {
            RefreshedTab.Sessions => sessions,
            RefreshedTab.Worktrees => worktrees,
        };

        return changes.Watch(watch.Folder, watch.Subfolders, change => Heard(watch.Kind, schedule, change));
    }

    private static void Heard(WatchKind kind, RefreshSchedule schedule, FolderChange change)
    {
        if (ChangeFilter.Counts(kind, change))
        {
            schedule.Changed();
        }
    }
}