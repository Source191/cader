using Source191.Cader.Application.Views;
using Source191.Cader.Domain;

namespace Source191.Cader.Application.Detail;

/// <summary>
/// The Worktrees tab's pane: a worktree's facts and as many of its unpushed commits or changed files as the pane's lines hold
/// after the facts and the line saying how many more, keyed by the row's key.
/// </summary>
public sealed class WorktreeDetails(DetailLoader loader, TimeProvider clock) : IDetailSource<WorktreeRow>
{
    public int MostRows => PaneHeights.WorktreesMost;

    public object KeyOf(WorktreeRow row) => row.Key;

    public async Task<DetailContent> ReadAsync(WorktreeRow row, int lines, CancellationToken cancellationToken) =>
        Wording.Detail(await loader.WorktreeAsync(row, Math.Max(lines - Wording.WorktreeFactRows - 1, 0), cancellationToken), clock.GetUtcNow(), clock.LocalTimeZone);
}