## Context

- **Terminal.Gui's timer lock:** it holds a lock while it runs each timeout and each `app.Invoke` callback, and `app.Invoke` from another thread takes that lock to queue its callback (`.claude\rules\terminal-gui.md`).
  - **The probe:** a throwaway program run in the headless terminal on 22 November 2026, Terminal.Gui 2.5.0 on .NET 11, opened a dialog inside an `app.Invoke` callback. A second thread's `app.Invoke` call did not return until the dialog closed 4.1 s later.
  - **Where Cader does it:** `TableBody.RunInBackground` settles every action inside `app.Invoke`, and `DialogEffects` opens four dialogs from that settle: `AskAboutBranch`, `AskAboutReport`, `ShowBreakdown` and `ChooseSolution`.
  - **Why it stalls a launch:** `ConsoleScreen` registers `launchProgress.Follow(() => app.Invoke(FrameChanged))`, and `LaunchProgress.Report` calls the listener on the launch's own thread, so each step waits for any such dialog.
- **Double presses:** `TableBody.Dispatch` and `RunInBackground` start every effect they are given. `inFlight` counts them but refuses nothing. `SessionsView` decides `OpenTabForSession(session, row.Tab)` from the row, whose `Tab` changes only on the next listing, and `OpenTabHandler.OpenAsync` removes every earlier registration for the session before writing its own pending one.
- **Quit:** `SharedKeys` maps q to `Quit` for every reducer, and `ConsoleScreen.Leave` stops the application at once. `ScreenLoop` then returns and the process exits, ending every pool thread.
- **Toggles:** `ToggleUpdateCheckHandler` and `ToggleDiagnosticLogHandler` read `settings.Value`, change one value and call `CurrentSettings.SaveAsync`, which writes and then replaces the snapshot, with no ordering between two saves.

## Goals / Non-Goals

**Goals:**
- No pool thread doing real work ever waits on the UI thread.
- One running copy of an action at a time.
- No silent quit mid-action.
- Settings changes applied in order.

**Non-Goals:** moving the post-action dialogs, a queue of actions, and the three overlaps other changes in #379 fix (detail reads in `detail-pane-reads-less`, git's optional locks in `lighter-git-reads`, the late Claude version in `faster-start`).

## Decisions

1. **A launch's step is redrawn by the ticker, not by `Invoke`.**
   - **How:** `RefreshWhatIsDue` (every 250 ms on the UI thread) compares `launchProgress.Latest` with the step it last drew and calls `FrameChanged` when it differs. `LaunchProgress.Follow` and the listener are removed, so `Report` only replaces the snapshot.
   - **Why:** this deletes code, and the ticker already runs under dialogs; a step shows at most 250 ms late.
   - **Alternative rejected:** handing the `Invoke` to a throwaway task, which keeps a blocked thread per step.
2. **`Application\RunningEffects`, one per console, refuses an equal effect already running.**
   - **Its API:** `TryBegin(ViewEffect)` answers false when an equal one (record equality) is running, and `End(ViewEffect)` releases it. Both are called on the UI thread only (a settle runs inside `app.Invoke`), so it needs no lock. It is handed to each `TableBody` through `ScreenContext`, so the Worktrees tab's Enter on a worktree and the Sessions tab's Enter on its session share one guard.
   - **How `TableBody` uses it:** `Dispatch` and `RunInBackground` (for a `Working(effect)`) call `TryBegin` first. A refusal shows `StatusMessage.StillRunning(effect)`, worded from the effect's working words in `Wording` with a refusal's tone. `End` runs in the settle.
   - **Why record equality:** effects are records of the row they act on, so two Enters on fix-123 are equal and Enter on fix-123 then on fix-456 are not.
3. **`OpenTabHandler` re-reads the registry before opening.** For `LiveTab.None` it checks for a live registration for the session (a pending one counts for its 60 s grace) and switches to it when found, as `LiveTab.Open` does. This covers the Enter pressed after the first opening has finished but before the next listing.
4. **`ConsoleScreen.Leave` asks before quitting while any tab is running.**
   - **How:** when `sessions.Running || worktrees.Running || config.Running`, `Confirm.Ask` with `DialogName.Quit` shows `Wording.QuitWhileRunningTitle`, `QuitWhileRunningQuestion` and `QuitWhileRunningButtons` (Quit, Keep open, with Esc keeping it open), and only Quit stops. This holds for the `Quit` that `UpdateTabOpened` raises too.
   - **Why in the screen:** it runs from the key handler, never inside `Invoke`, and the decision is one boolean the screen already has.
5. **`CurrentSettings.UpdateAsync(Func<Settings, Settings>)` applies changes one at a time.**
   - **How:** a `SemaphoreSlim(1, 1)` around read, change, write and replace (a lock cannot be held across the write's `await`). The toggles pass their change, and `SaveSettings` from a dialog passes `_ => saved`.
   - **Why the dialog saves a whole snapshot:** a dialog's save is modal and replaces what its scope edits. A toggle saved while a dialog was open would be overwritten by that dialog's snapshot, which is accepted, since a dialog blocks the toggle's key.

## Risks / Trade-offs

- **Refusing equal effects could refuse a wanted repeat,** for example copying the same path twice quickly → the second copy is a no-op anyway, and once the first ends the key works again.
- **The quit question appears when the user means it** → Esc keeps the console open and Enter on Quit quits, one key more than today and only while something runs.

## Open Questions

None.
