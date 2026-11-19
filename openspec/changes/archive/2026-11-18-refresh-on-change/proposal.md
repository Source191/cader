# Proposal

## Why

Even with configurable intervals (`configurable-refresh`), the console only learns about a change when a timer fires: a worktree added in a shell, a session started in another terminal or a session that starts waiting shows up to an interval late, and the Worktrees tab is never rescanned while it is hidden, so it is stale the moment it is shown. Shorter intervals hide this at the cost of running `claude agents` (2.3 s a listing, measured 18 November 2026) and git over and over when nothing has changed. Both tools record these changes on disc as they happen, so Cader can refresh when they do.

## What Changes

- **Sessions:** Cader watches Claude Code's `sessions` folder (one `<pid>.json` per running session, rewritten when its status changes) and `jobs` folder (one `state.json` per background job, rewritten when its state changes) in the Claude folder it already uses, and lists the sessions soon after one changes, whichever tab is shown. It only notices that a file changed; it never opens one, and the listing still comes from `claude agents --json`.
- **Worktrees:** Cader watches each listed repository's git folder for worktrees being added or removed and for a checked-out branch changing, and each repository root's top level for repositories appearing or going, and rescans soon after, **whether or not the Worktrees tab is shown**, so it is up to date when the user shows it.
- Changes arriving in a burst start one refresh, at most one a second per tab, never two at once, and a change during a running refresh starts one more after it.
- Nothing to set: the watchers are always on. The timers stay as the safety net for what the watchers cannot see (commits, fetches, a folder deleted by hand) and for a Claude Code that stops writing these files.

Out of scope:
- **Reading Claude Code's session or job files:** they are undocumented and internal; a change is only a hint to run the documented listing.
- **Watching for commits, pushes or file edits inside a worktree:** git touches its index and object store constantly, and Cader's own scan would trigger itself.
- **A setting to turn the watchers off.**
- **Multiple Claude accounts:** a later change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `session-listing`: the list also refreshes when Claude Code records a session starting, ending or changing state.
- `worktree-management`: the worktrees are also rescanned, shown or hidden, when git records a worktree added or removed or a branch checked out, or a repository root gains or loses a folder.

## Impact

- **Implement with:** Claude Opus 5.5 (`claude-opus-5-5`) at **high** effort. File-system events arrive on pool threads in bursts, must be filtered so Cader's own git reads never trigger a rescan, throttled without losing the last change, and the watchers' handles must be released; these are the mistakes a faster pass is likely to make.
- **Ports:** a new `IFolderChanges` (watch a folder, report changed names; nothing read).
- **Adapters:** `Infrastructure\Files\FolderChanges` over `FileSystemWatcher`.
- **Application:** which changes count (`ChangeFilter`), which folders to watch (`WatchPlan`), keeping the watches in step with each scan (`ChangeWatches`), and `RefreshSchedule` learning that a change is pending.
- **Tui and wiring:** the ticker from `configurable-refresh` also starts a hidden Worktrees tab's scan when a change is pending.
- **Diagnostics:** one `DiagnosticEvent` case when a watch cannot start or overflows, naming only its kind.
- **Rules and context:** `.claude\rules\claude-code.md` records the files and when they are written (dated, CLI 2.1.327); `openspec\config.yaml`'s context allows watching the `sessions` and `jobs` folders for change notifications while still forbidding reading them.
- **Docs:** `docs\user-guide.md`, `.claude\rules\console-screen.md`.
- **Version:** a minor raise and an Added entry in `CHANGELOG.md`.
- **Users:** both tabs show changes within about two seconds, the Worktrees tab is current when shown, and longer timer intervals no longer mean stale screens.
