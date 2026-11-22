## Why

Cader runs its actions and loads on pool threads while the user keeps pressing keys, and several of those overlaps go wrong:

- **A dialog can stall a launch.** The delete-branch question after a removal, the problem report, the memory breakdown and the solution chooser all open inside the `app.Invoke` callback that settles a background action. While such a dialog is open, every other thread's `app.Invoke` call waits for it to close (observed on 22 November 2026: 4.1 s, the time the probe's dialog stayed open). A launch reports each step through `app.Invoke`, so a session being started stops at its next step until the user closes the dialog.
- **Pressing a key twice runs the action twice.** Nothing refuses a second Enter, x or toggle while the first is still running. Two Enters on a session with no tab open two Windows Terminal tabs on it, and the second opening forgets the first tab's registration, so Stop and Delete never close it.
- **`q` quits mid-action without a word.** A launch or a removal still running is cut off part-way: a session started with no tab, or a folder half deleted.
- **Two quick settings toggles can undo each other.** Each toggle reads the settings, changes one value and saves the whole, on its own pool thread.

Three more overlaps the bug hunt found are fixed by other changes in the stack (#379): detail reads piling up by `detail-pane-reads-less`, Cader's git status holding `index.lock` by `lighter-git-reads`, and a late Claude version by `faster-start`.

**Implement with:** Opus 5.5 at high effort: thread-ordering fixes across the UI thread, pool threads and Terminal.Gui's timer lock, each pinned by a sociable test where the logic can live in Application and checked in the headless terminal where it cannot.

## What Changes

- **A launch never waits on the UI thread:** its progress is picked up by the console's existing 250 ms ticker instead of an `app.Invoke` per step.
- **An action already running is not started again:** the same action on the same row while the first is still running is refused, and the status line says it is still running. Enter on a session also opens no second tab while its first tab is still being opened or is open.
- **`q` with an action running asks first:** "An action is still running; quit anyway?", with Quit and Keep open (Esc) buttons.
- **Settings changes are applied one at a time,** each to the settings the last one saved.
- **The facts file:** `.claude\rules\terminal-gui.md` records the dialog-inside-`Invoke` fact as observed.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `session-launch`: a launch carries on while a dialog is open.
- `console-layout`: an action already running is not started twice, and quit asks while an action runs.
- `session-attach`: Enter opens one tab per session however often it is pressed.
- `settings`: settings changes never undo one another.

## Delivery

- **Order:** step 3 of the stack in #379, after `fit-narrow-and-short-terminals`. `open-new-session-tab-sooner` (step 15) builds on its registry re-read in `OpenTabHandler`.

## Out of scope

- Moving the post-action dialogs out of the `Invoke` callback. Background threads other than the launch still wait while such a dialog is open, but they only queue redraws behind a modal dialog, which the user cannot see past anyway.
- Moving a selection clearing a running action's words (the reducers' existing rule).
- Cancelling or killing processes Cader has started.
- Multiple Claude accounts beyond naming a session's owner, a later change.

## Impact

- `Application`: a console-wide `RunningEffects` (a running action's identity, with its `StatusMessage.StillRunning` and words), `CurrentSettings.UpdateAsync`, the toggles' handlers, `OpenTabHandler` (re-reads the registry before opening), `Wording` for the quit question and the still-running words.
- `Tui`: `TableBody` (asks `RunningEffects`), `ConsoleScreen` (`Leave` asks, the ticker redraws a new launch step, `Follow` removed).
- `.claude\rules\terminal-gui.md`, `.claude\rules\console-screen.md`, `CHANGELOG.md` and the version in `src\Source191.Cader\Source191.Cader.csproj`.
