using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.Application;

/// <summary>
/// Keeps one watch per planned folder, telling the schedule each feeds about the changes <see cref="ChangeFilter"/>
/// counts. <see cref="Follow"/> runs as the console opens and after each scan, on whichever thread the scan ended on,
/// and the changes arrive on pool threads, so the watches sit behind a lock; nothing a change does takes it.
/// </summary>
public sealed class ChangeWatches(IFolderChanges changes, RefreshSchedule sessions, RefreshSchedule worktrees, DiagnosticLog log) : IDisposable
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

        try
        {
            return changes.Watch(watch.Folder, watch.Subfolders, change => Heard(watch.Kind, schedule, change));
        }
        catch (FolderWatchException)
        {
            // Kept in the plan as no watch, so it is recorded once and tried again only once it has left the plan and come back.
            log.Record(new WatchTrouble(watch.Kind, WatchTroubleKind.CouldNotStart));

            return new Unwatched();
        }
    }

    private void Heard(WatchKind kind, RefreshSchedule schedule, FolderChange change)
    {
        if (change.Kind is FolderChangeKind.Overflowed)
        {
            log.Record(new WatchTrouble(kind, WatchTroubleKind.Overflowed));
        }

        if (ChangeFilter.Counts(kind, change))
        {
            schedule.Changed();
        }
    }

    /// <summary>A watch Windows refused: nothing to stop.</summary>
    private sealed class Unwatched : IDisposable
    {
        public void Dispose()
        {
            // Nothing was watched, so nothing is released.
        }
    }
}