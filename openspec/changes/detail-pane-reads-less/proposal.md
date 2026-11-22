## Why

The detail pane starts its git reads for every row the selection passes over: holding Down across ten worktree rows starts some forty git processes, of which only the last row's answer is ever shown, and nothing waits for the last read to end, so slower stepping still piles reads up (the bug hunt saw twenty sets of git processes at once). A worktree's facts are then read one after another (the created record, origin's default branch, the merge check, then the commits or the changed files), so on a machine where each git run takes 0.5 to 1 s the pane takes two to four seconds to fill.

**Implement with:** Opus 5.5 at medium effort: a short UI-thread delay through Terminal.Gui's timers in `DetailReader`, a one-read-at-a-time rule in `DetailRequests`, and independent reads started together in `DetailLoader`, all inside the existing request-numbering that drops stale answers; contained, but the timer must respect the tab-shown and dispose rules.

## What Changes

- The pane reads a row's detail once the selection has rested on it for 150 ms, so moving through rows starts reads only for the row the selection stops on. A refresh's re-read, and a tab shown again, read at once as today.
- A tab reads one row's detail at a time: a request made while a read runs waits for it, and when it ends only the latest request is started, if its row is still selected. Nothing is cancelled for the selection moving on, so no git process is killed half way through (a killed git can leave `index.lock` behind).
- A worktree's facts that do not depend on each other are read at the same time, so the pane fills in about the time of its slowest fact.
- Every fact still fails on its own as unavailable, and a late answer for a row the selection has left is still dropped.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `detail-pane`: detail is read once the selection rests for 150 ms, one row at a time per tab, and a worktree's facts are read together.

## Delivery

- **Order:** step 13 of the stack in `openspec\implementation-plan.md`. Depends on nothing; after `name-ui-stalls` so the draw counts show the pane's effect. It carries the one-at-a-time rule first proposed in `guard-overlapping-actions` (its design 5), since both rewrite `DetailReader.Reading` and `DetailRequests`.
- **Together:** ships alone, in one pull request.

## Out of scope

- Taking a session's changes from the Worktrees scan instead of reading them (considered; with the Sessions refresh at 30 seconds by `fewer-background-polls` the pane's re-reads are already few).
- Multiple Claude accounts beyond naming a session's owner, a later change.

## Impact

- `Application\Detail\DetailRequests`: the pure rules, tested without Terminal.Gui: a selection change asks after 150 ms and a newer change replaces it; a refresh or a show asks at once; `Begin`, `Ended` and the request to start next, so at most one read runs per tab and only the latest held request starts when it ends.
- `Tui\DetailReader`: a 150 ms `app.AddTimeout` per selection change, replaced by the next and removed when the tab is hidden and on dispose; `Reading` holds a request while a read runs and starts the next one `DetailRequests` answers. Only the tab's disposal cancels a read, as now.
- `Application\Detail\DetailLoader.WorktreeAsync`: the origin record, the merge state and the commits or changed files started together.
- `CHANGELOG.md` and `Version` in `src\Source191.Cader\Source191.Cader.csproj`; `.claude\rules\console-screen.md`, Detail pane.
- **Implement with:** Opus 5.5 at medium effort (reason above).
