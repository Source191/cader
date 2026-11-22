## Why

The detail pane starts its git reads for every row the selection passes over: holding Down across ten worktree rows starts some forty git processes, of which only the last row's answer is ever shown. A worktree's facts are then read one after another (the created record, origin's default branch, the merge check, then the commits or the changed files), so on a machine where each git run takes 0.5 to 1 s the pane takes two to four seconds to fill.

**Implement with:** Opus 5.5 at medium effort: a short UI-thread delay through Terminal.Gui's timers in `DetailReader` and independent reads started together in `DetailLoader`, both inside the existing request-numbering that drops stale answers; contained, but the timer must respect the tab-shown and dispose rules.

## What Changes

- The pane reads a row's detail once the selection has rested on it for 150 ms, so moving through rows starts reads only for the row the selection stops on. A refresh's re-read, and a tab shown again, read at once as today.
- A worktree's facts that do not depend on each other are read at the same time, so the pane fills in about the time of its slowest fact.
- Every fact still fails on its own as unavailable, and a late answer for a row the selection has left is still dropped.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `detail-pane`: detail is read once the selection rests for 150 ms, and a worktree's facts are read together.

## Delivery

- **Order:** 9 of 11, wave 3 (how it feels). Depends on nothing; best after `name-ui-stalls` so the draw counts show the pane's effect.
- **Together:** ships alone, in one pull request.

## Out of scope

- Taking a session's changes from the Worktrees scan instead of reading them (considered; with the Sessions refresh at 30 seconds by `fewer-background-polls` the pane's re-reads are already few).
- Multiple Claude accounts beyond naming a session's owner, a later change.

## Impact

- `Tui\DetailReader`: a 150 ms `app.AddTimeout` per selection change, replaced by the next and removed on dispose; refreshes and shows bypass it.
- `Application\Detail\DetailLoader.WorktreeAsync`: the origin record, the merge state and the commits or changed files started together.
- `CHANGELOG.md` and `Version` in `src\Source191.Cader\Source191.Cader.csproj`; `.claude\rules\console-screen.md`, Detail pane.
- **Implement with:** Opus 5.5 at medium effort (reason above).
