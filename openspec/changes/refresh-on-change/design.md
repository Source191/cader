# Design

## Context

This change builds on `configurable-refresh`, which must be merged and archived first: it leaves one 250 ms ticker in `ConsoleScreen` asking each tab's `Application\RefreshSchedule` whether a refresh is due, and the schedule is told `Started()` whenever the console starts that tab's refresh through `ListSessions` or `ScanWorktrees`, which reserve a `RunningLoads` slot and start nothing while one runs.

What the disc shows (checked 18 November 2026, Claude Code 2.1.327, git 2.56, `FileSystemWatcher` in PowerShell 7):
- **`<Claude folder>\sessions\<pid>.json`:** one per running session, background ones included, holding `status` and `statusUpdatedAt`; rewritten when the status changes. Beside it are `<pid>.<hash>.key` files, written when a session starts.
- **`<Claude folder>\jobs\<id>\state.json`:** one per background job, holding `state`; written as `state.json.tmp.<hex>` then renamed over `state.json`, so a watcher sees Created/Changed on the temporary name and Deleted, Renamed on `state.json`. A job's `timeline.jsonl` is written beside it. With the machine quiet, one write in 40 seconds; a `claude agents --json` listing (2.3 s) wrote nothing for any job but the one running it.
- **`git worktree add` and `remove`:** about 90 events for one add and remove, all under `.git\worktrees\<name>\` (lock files, `HEAD`, `index`, `logs`), with `.git\worktrees` itself created for the first worktree and deleted with the last. `git branch -D` touched nothing under `worktrees`.

`openspec\config.yaml` forbids parsing the `sessions` folder; watching for names only stays inside that, and the context line is reworded to say so.

## Goals / Non-Goals

**Goals:**
- Changes from outside Cader reach both tabs within about two seconds, the Worktrees tab while hidden too.
- Every rule (which change counts, which folders to watch, when a refresh is due) is a tested `Application` class; the adapter only reports names and kinds.
- Cader's own git reads and listings never start a refresh.

**Non-Goals:**
- Replacing the timers, or reading any file's contents.

## Decisions

1. **A port that reports names, not contents.** `Ports\IFolderChanges.Watch(FolderPath folder, bool subfolders, Action<FolderChange> changed)` answers an `IDisposable`; `FolderChange` is the path relative to the watched folder and a `FolderChangeKind` (created, changed, deleted, renamed, plus `Overflowed` for a lost buffer; `Domain\ChangeKind` already names something else). `Infrastructure\Files\FolderChanges` wraps one `FileSystemWatcher` per call (`NotifyFilter` file name, directory name and last write; `InternalBufferSize` 64 KB, the maximum) and answers a no-op watch when the folder does not exist. Its `Error` event becomes `Overflowed`, which counts as a change (a rescan is the safe answer to lost events).
2. **`Application\ChangeFilter` decides what counts,** from the relative path alone, per kind of watch:
   - *Claude sessions folder* (top level only): any `*.json`; `.key` files are ignored (a session's own `.json` is written at the same moment).
   - *Claude jobs folder* (with subfolders): `<id>\state.json` created, changed or renamed to; temporary files and `timeline.jsonl` are ignored.
   - *A repository's git folder* (with subfolders): `worktrees` and `worktrees\<name>` created, deleted or renamed; `HEAD` and `worktrees\<name>\HEAD` changed or renamed to. Everything else is ignored: `index` (which Cader's own `git status` may rewrite, so counting it would make every scan start the next), `objects`, `refs`, `logs` and every `*.lock`.
   - *A repository root* (top level only, directories): created, deleted or renamed.
   - *Rejected:* two watchers per repository (`.git` top level, and `.git\worktrees` once it exists) to see fewer events. It needs re-arming when the `worktrees` folder comes and goes and doubles the handles; filtering is a string comparison per event.
3. **`RefreshSchedule` learns `Changed()`.** It is called from pool threads, so the pending time is kept with a `Lock`, as `RunningLoads` keeps its counts. The schedule is due for a change when one is pending, at least 500 ms have passed since the first pending change (so a `git worktree add`'s burst lands as one) and at least one second has passed since the tab's last refresh started. `Started()` clears the pending change, so a change during a running refresh sets it again and is followed by one more. A tick that cannot reserve (a refresh is running) leaves it pending for the next tick. A pending change is acted on whether or not the tab is shown; the timer half keeps its "Worktrees only while shown" rule.
   - *Rejected:* a trailing debounce (wait for quiet). A session working steadily rewrites `state.json` often enough that the list might never refresh.
4. **`Application\WatchPlan` and `ChangeWatches` keep the watches in step.** `WatchPlan.For(settings, claudeFolder, repositories)` is a pure function answering the folders to watch, each with its filter and the schedule it feeds: the Claude `sessions` and `jobs` folders (Sessions), each listed repository's git folder and each repository root (Worktrees). The repositories come from the latest scan, so a root saved in settings or a repository cloned into a root is watched from the scan after it. `ChangeWatches.Follow(plan)` disposes watches no longer planned and starts new ones through `IFolderChanges`, and is called after each scan returns and once as the console opens. A linked worktree's records live in its main repository's git folder, so watching each repository covers its worktrees.
5. **A watch that cannot start is recorded, not shown.** A missing folder is a no-op watch. A watch that throws as it starts, or overflows, adds a `DiagnosticEvent.WatchTrouble` with the watch's kind (Claude sessions, Claude jobs, repository, root) and the trouble (could not start, overflowed); no path, so the privacy rule holds. The timers carry on regardless.

## Adaptations while implementing

- **The Claude folder is never unset:** `Domain\ClaudeFolder` always names a folder (`Unset` is the default `.claude` under the profile), so `WatchPlan` always plans the two Claude watches, and task 2.2's "a Claude folder left unset plans no Claude watch" became "the folder `CLAUDE_CONFIG_DIR` names is the one watched". A Claude folder with no `sessions` or `jobs` folder is the adapter's no-op watch (decision 1).
- **Where the repositories come from:** `WorktreeInventory.Repositories` holds the repositories under the roots that its latest scan found (a `SharedSnapshot`), because a scan's rows leave out a repository with no linked worktree and no free branch, which is exactly the one whose first `git worktree add` must be seen.
- **Who follows the plan:** `ConsoleLoads.FollowChanges()` builds the plan and calls `ChangeWatches.Follow`; `ScanWorktreesAsync` calls it after each scan returns, and the composition (and `SceneHost`, with a watcher that reports nothing) once as the console opens. That keeps the decision in a tested class, and `ConsoleScreen`'s constructor is unchanged: it already holds both schedules.
- **The two halves of a schedule:** `RefreshSchedule.IsDue()` answers for the timer or a change; `IsDueForChange()` for a change alone, which the ticker asks of a hidden Worktrees tab.
- **A refused watch:** the adapter wraps Windows' refusal in `Ports\FolderWatchException`; `ChangeWatches` records it once and keeps the plan entry as no watch, so it is tried again only after it leaves the plan and returns. A folder missing when its watch starts (a Claude folder with no `jobs` yet) is likewise not watched until it leaves the plan; the timers cover both.
- **Overflow:** `WatchTrouble` names the watch's `WatchKind` and a `WatchTroubleKind` (`CouldNotStart`, `Overflowed`), rendered as `watch Repository CouldNotStart`.

## Risks / Trade-offs

- [Claude Code changes where or how it writes these files] → the timers still refresh; the watchers are a hint, `claude-code.md` records what was seen so a re-verification can spot the change, and the contract tests are unaffected because nothing is parsed.
- [A large `git fetch` or `gc` overflows a repository's buffer] → `Overflowed` counts as one change, so the cost is one extra rescan.
- [Many repositories mean many watchers] → one handle and a 64 KB buffer each; `HandleCounts` gains a fact that a hundred watches started and disposed leave the handle count where it began.
- [Cader's own scan triggers itself] → the filter ignores `index`, locks, `refs` and `logs`; an adapter test runs the scan's git commands against a watched repository and asserts no counted change.
- [A watched root holds thousands of folders] → only its top level is watched, for directory names.

## Migration Plan

None. Nothing is stored; an earlier Cader simply has no watchers.
