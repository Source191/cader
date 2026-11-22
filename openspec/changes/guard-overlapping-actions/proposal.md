## Why

Cader runs its actions, loads and detail reads on pool threads while the user keeps pressing keys, and several of those overlaps go wrong:

- **A dialog can stall a launch.** The delete-branch question after a removal, the problem report, the memory breakdown and the solution chooser all open inside the `app.Invoke` callback that settles a background action. While such a dialog is open, every other thread's `app.Invoke` call waits for it to close (observed on 22 November 2026: 4.1 s, the time the probe's dialog stayed open). A launch reports each step through `app.Invoke`, so a session being started stops at its next step until the user closes the dialog.
- **Pressing a key twice runs the action twice.** Nothing refuses a second Enter, x or toggle while the first is still running. Two Enters on a session with no tab open two Windows Terminal tabs on it, and the second opening forgets the first tab's registration, so Stop and Delete never close it.
- **`q` quits mid-action without a word.** A launch or a removal still running is cut off part-way: a session started with no tab, or a folder half deleted.
- **Detail reads pile up.** Every selection move starts its row's detail read, and nothing waits for the last one, so holding Down over twenty worktrees starts twenty sets of git processes at once.
- **Cader's git status can block the user's own git.** `git status` refreshes the index and takes `index.lock` to write it back unless told not to (git's `--no-optional-locks`). Cader runs it in every worktree on every scan and detail read, so the user's own commit in that worktree can fail with "index.lock exists".
- **Two quick settings toggles can undo each other.** Each toggle reads the settings, changes one value and saves the whole, on its own pool thread.
- **A slow first answer from Claude leaves the header saying "Claude unknown" until the next start.** The version read is given 5 s and never retried, which a cold start exceeded on both of the bug hunt's first runs.

**Implement with:** Opus 5.5 at high effort: thread-ordering fixes across the UI thread, pool threads and Terminal.Gui's timer lock, each pinned by a sociable test where the logic can live in Application and checked in the headless terminal where it cannot.

## What Changes

- **A launch never waits on the UI thread:** its progress is picked up by the console's existing 250 ms ticker instead of an `app.Invoke` per step.
- **An action already running is not started again:** the same action on the same row while the first is still running is refused, and the status line says it is still running. Enter on a session also opens no second tab while its first tab is still being opened or is open.
- **`q` with an action running asks first:** "An action is still running; quit anyway?", with Quit and Keep open (Esc) buttons.
- **A tab reads one row's detail at a time:** a selection move while a read runs waits for it, then reads only the row selected by then. Nothing is killed, so no git process is left half way through.
- **Cader's read-only git calls take no optional locks,** so they never hold a worktree's `index.lock`.
- **Settings changes are applied one at a time,** each to the settings the last one saved.
- **The Claude version:** a read that takes longer than start-up's 5 s carries on, and the header shows the version once it arrives.
- **The facts file:** `.claude\rules\terminal-gui.md` records the dialog-inside-`Invoke` fact as observed.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `session-launch`: a launch carries on while a dialog is open.
- `console-layout`: an action already running is not started twice, and quit asks while an action runs.
- `session-attach`: Enter opens one tab per session however often it is pressed.
- `detail-pane`: a tab reads one row's detail at a time.
- `worktree-management`: Cader's reads never hold a worktree's index lock.
- `settings`: settings changes never undo one another.
- `session-listing`: the Claude version reaches the header even when it answers late.

## Out of scope

- Moving the post-action dialogs out of the `Invoke` callback. Background threads other than the launch still wait while such a dialog is open, but they only queue redraws behind a modal dialog, which the user cannot see past anyway.
- Moving a selection clearing a running action's words, which `scope-status-to-its-tab` accepts in its design.
- Cancelling or killing git processes Cader has started.
- Multiple Claude accounts beyond naming a session's owner, a later change.

## Impact

- `Application`: a console-wide `RunningEffects` (a running action's identity, with its `StatusMessage.StillRunning` and words), `CurrentSettings.UpdateAsync`, the toggles' handlers, `OpenTabHandler` (re-reads the registry before opening), `DetailRequests` or a new one-at-a-time rule for the pane, `Wording` for the quit question and the still-running words.
- `Adapters\Git\GitCli`: `--no-optional-locks` on its read-only calls.
- `Tui`: `TableBody` (asks `RunningEffects`), `ConsoleScreen` (`Leave` asks, the ticker redraws a new launch step, `Follow` removed), `DetailReader` (one read at a time).
- `Composition\ConsoleComposition`: the version read without its 5 s cancellation, the header fed from its answer.
- `.claude\rules\terminal-gui.md`, `.claude\rules\console-screen.md`, `CHANGELOG.md` and the version in `src\Source191.Cader\Source191.Cader.csproj`.
