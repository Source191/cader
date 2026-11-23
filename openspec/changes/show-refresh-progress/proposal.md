## Why

The console gives almost no sign of work in progress after its first load. The Worktrees tab shows "Scanning worktrees" and nothing else until every repository has been read (38 s at the median in the measured hour, though the first repositories are done in a second or two), and once rows are on screen a timed, heard or shown refresh runs with no sign at all, so a table that is about to change looks finished and a slow refresh looks like a hang. Being able to see the work makes the console feel quicker even before the scan changes make it quicker.

**Implement with:** Opus 5.5 at high effort: partial results from one scan must fit the existing ordering rules ("a scan's rows never replace those of a later scan", the selection kept across a refresh, the filter, the orphan merge in name order), and the refresh mark touches the console's layout code, which has known Terminal.Gui pitfalls (`terminal-gui.md`).

## What Changes

- While the Worktrees tab has no rows yet, its scan shows each repository's rows as soon as that repository and every one before it in name order have been read and the first sessions listing has returned, so the table fills from the top; the waiting words count the repositories read ("Scanning worktrees: 23 of 55 repositories") until the last.
- While any later scan of the shown tab is running, its title says how far it has got (`23 of 55 repositories`) when it re-reads more than one repository.
- While a refresh of the shown tab runs in the background (timed, heard, on show, on F5 or after an action), a dim ↻ follows the tab's counts; it goes when the refresh returns. No spinner turns for these refreshes, as today.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `worktree-management`: the first scan fills the table repository by repository and counts the repositories read.
- `console-layout`: a background refresh is marked beside the shown tab's counts.

## Delivery

- **Order:** step 12 of the stack in `openspec\implementation-plan.md`, the last before checkpoint C. Depends on `rescan-only-what-changed` (the per-repository reads it reports on); land it straight after that change. Its rows wait for the first sessions listing, as `parallel-scan-reads` requires (design 1).
- **Together:** the progressive rows and the refresh mark ship in one pull request, as both report a refresh's progress through the same load path.

## Out of scope

- Saving rows to disc so the console can show the last ones at start (considered: after waves 1 and 2 the first scan should take a few seconds and fill from the top, so the extra state file, its strict-JSON shape and acting on stale rows are not yet justified; reconsider with the diagnostic counts from after wave 2).
- Showing a refresh's progress on the Sessions tab, whose listing is one call.
- Multiple Claude accounts beyond naming a session's owner, a later change.

## Impact

- `Application\WorktreeInventory`: an `IProgress` of the rows read so far, in name order, and the count read out of the count to read.
- `Application\ConsoleLoads`, `Application\Views\WorktreesView` and `WorktreesViewState`: a partial rows input that keeps the selection and filter as a full one does, accepted only for the newest load; a scan-progress fact for the waiting words and the title.
- `Tui\TableBody`: partial inputs carry their load's ticket through `LoadSequence`; `Tui\ConsoleScreen.ShowCounts` adds the mark while the shown tab's `RunningLoads` has a load running.
- `Application\Wording`: the waiting words with the count, the title's progress and the mark, with their facts.
- `CHANGELOG.md` and `Version` in `src\Source191.Cader\Source191.Cader.csproj`; `.claude\rules\console-refreshing.md` and `.claude\rules\console-screen.md`, Layout.
- **Implement with:** Opus 5.5 at high effort (reason above).
