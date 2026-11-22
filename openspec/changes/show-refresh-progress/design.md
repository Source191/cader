## Context

See proposal.md, Why. `TableBody.TriggerLoad` runs the tab's load function on a pool thread and hands its one `ViewInput` to the reducer through `app.Invoke`, accepted only if `LoadSequence` says no later load has landed. `WorktreeInventory.ScanAsync` merges each repository's results in name order once every repository's reads have returned (after `rescan-only-what-changed`, many come from its cache). The waiting indicator shows `Wording.WaitingText(ConsoleTab)` while the tab is shown, not loaded and with no banner. `RunningLoads` knows whether a tab's load is running; the counts row is rebuilt only when its parts change.

## Decisions

### 1. Partial rows as a prefix in name order

The scan reports progress through an `IProgress<ScanProgress>`: the rows of the longest run of repositories, in name order, whose reads have all returned, with the number read and the number to read. Only a prefix is reported because a repository's orphans are judged against its own registrations and those before it, so a prefix's rows are exactly what the full scan will show for those repositories; the worktree-root leftovers and broken worktrees, which need every registration, come only with the final rows. Pull requests and branch flags are filled for a prefix's rows as for the final rows.

### 2. Partial rows only while the table is empty

`WorktreesView` takes a `WorktreeRowsPartial` input only while the list is not yet loaded; once rows are shown, a later scan's progress changes only the title's count, never the rows, so a rescan never makes rows vanish and reappear. The final `WorktreeRowsLoaded` replaces the partial rows as today, keeping the selection by key. `LoadSequence` gains acceptance of a partial input for the newest ticket only, and never after that ticket's final input.

Alternative considered: merge a later scan's fresh prefix over the old rows; rejected as flicker-prone for little gain once rescans take a second or two.

### 3. The mark reads the loads already running

`ConsoleScreen.FrameChanged` asks the shown tab's `RunningLoads` whether a load is running and appends `Wording.Refreshing` (a dim ↻ part) to the counts; a load's start and end call `FrameChanged`, so the mark appears and goes without a timer. The spinners are untouched, so the existing "no spinner for timed refreshes" requirements hold.

## Risks / Trade-offs

- [More redraws during the first scan] → one per repository read; with about 55 repositories that is 55 redraws in a few seconds, each measured by `name-ui-stalls`' draw counts.
- [A partial input arriving after a reload's final rows] → `LoadSequence` drops it; a fact orders them that way on purpose.
