## Context

- **What is in place:** `main` references Terminal.Gui 2.6.0 since PR #386 (0.22.0-preview.54). CI on that pull request (run 45052211647) passed the suite, the smoke check on x64 and Arm and the hand-off check. Nothing has been observed by hand on 2.6.0.
- **The stack:** steps 0 to 4 are open as #380 to #384, all branched before #386 and so all on 2.5.0, with versions 0.22.0-preview.54 to .57; #381's version is the one #386 took. This change is implemented once that stack has been rebased onto `main`.
- **The rule:** `.claude\rules\terminal-gui.md` is headed "Terminal.Gui 2.5.0 facts" and passes `Assert-ClaudeContextBudget.ps1` with little room left (12,295 bytes on disc with CRLF line endings, against a 12,288-byte budget that the script passes). On #383's branch it holds two lock bullets: the original, now marked observed, and a new one saying "no pool thread doing real work calls `app.Invoke`: a launch's step is picked up by the console's ticker". #382 added the wizard's chrome bullet (six rows of `Height` besides the content, observed at 120 x 30, 36 and 40), which `WizardFit` relies on. The evidence is in `docs\verified-facts\terminal-gui.md`.
- **What 2.6.0 changed** that Cader's code or rules lean on, from the release notes, the upstream pull requests and the v2.5.0...v2.6.0 compare:
  - **Timers (tui-cs#5655):** callbacks run outside the queue's lock, one at a time behind a re-entrant gate. `Invoke` from another thread no longer waits for a running callback. A competing `RunTimers` returns at once.
  - **Dispatch (tui-cs#5694):** new `app.InvokeAsync(action, cancellationToken)` and `app.InvokeAsync(SessionToken owner, action, cancellationToken)`. The task completes after the action runs, carries its exception, and is cancelled when the owner session ends. A `SessionToken` comes from `Begin`, `SessionBegun` or `SessionStack`; `app.Run(dialog)` hands none back. The same pull request also changed how nested sessions end and how ended ones are drawn.
  - **Synchronisation context (tui-cs#5722):** `Run` installs it and restores the caller's context; `Begin` alone no longer does.
  - **Tabs (tui-cs#5777):** clean tab pages redraw their shared borders during a partial redraw (`RedrawsCleanSubViewBorders`).
  - **Hotkeys (tui-cs#5761, #5794):** with `AssignHotKeys` on, a title change re-picks the view's hotkey and moves the title's marker to match; `AssignHotKeys` is now `bool?`. `OptionSelector` sets it to true, and `SettingsControls.ShowPresets` retitles the Editor options.
  - **ListView (tui-cs#5745):** setting a new source clears `SelectedItem` before replacing it and clamps the viewport. The wizard's `Push` replaces the source under its `binding` guard, then sets `SelectedItem`.
  - **Not changed:** the compare touches no file under `Views/TableView`, `Views/TextInput`, `Views/Selectors`, `Views/SpinnerView` or `App/ToolTips`, and one file each under `Views/FileDialogs` (the icon provider), `Views/Runnable` and `ViewBase/Adornment`. The last two can affect a dialog's chrome.

## Goals / Non-Goals

**Goals:**
- Every fact in `terminal-gui.md` is true of 2.6.0, or marks the version it was last checked on.
- The wizard's fit rests on chrome measured on 2.6.0.
- The release zip's notices name what it ships.

**Non-Goals:** changing what Cader does, re-checking facts in code 2.6.0 did not touch, and rewriting archived changes.

## Decisions

1. **Probe what the changed code does, and read what is cheaper to read.**
   - **Observed** (a throwaway console program referencing Terminal.Gui 2.6.0, run in `tools\HeadlessTerminal` at 120 x 36 unless named, then deleted):
     - **P1:** a dialog opened inside an `app.Invoke` callback, and another inside a timeout callback. A second thread then calls `app.Invoke` and records how long the call takes to return, and how long until its callback runs.
     - **P2:** a 250 ms timeout counted through a 5-second dialog (the 1 November probe again).
     - **P3:** a `Tabs` strip with a `SpinnerView` turning on the hidden tab, capturing whether the shown tab's top edge under its title closes.
     - **P4:** an `OptionSelector` built like `OptionSelectors.For` with the editor presets' labels, then retitled as `ShowPresets` does (`Rider` to `Rider (not found)`). The capture's escape sequences show which letter of each option is drawn hot before and after.
     - **P5:** #382's chrome measurement again: a Debug build of Cader from the rebased stack opens the wizard at 120 x 30, 36 and 40, and the frame, list rows and form placement are compared with what `WizardFit` expects. Only the wizard is opened, then closed with Esc, so no key saves, stops, deletes or opens a tab.
   - **Read** (the 2.6.0 assembly, decompiled with ilspycmd): R1, that `Run` still installs `MainLoopSyncContext` for the loop, so the `GetResult()` rule and `LaunchWizardDialog`'s pool-thread read stand. R2, that `ListView.SetSource` with a shorter list raises `ValueChanged` while the wizard's `binding` is true, so `Apply` ignores it.
   - **Why:** P1 to P5 are drawing and threading behaviour, where reading has misled before (the Tabs re-measure spike). R1 and R2 are one code path each.
   - **Alternative rejected:** trusting the upstream descriptions alone. They describe the fix, not what Cader's use of it does.
2. **The rule becomes "Terminal.Gui 2.6.0 facts", with dates kept.** The heading line says that each bullet names the version it was checked on, and that a 2.5.0 fact in code the compare shows unchanged still holds. New bullets are one line each in the existing sections; their evidence (timings, captures, the compare's file list) goes in `docs\verified-facts\terminal-gui.md` under a new "2.6.0" heading. #383's two lock bullets become one bullet with P1's result. The ticker is then justified by what it is: one snapshot read on the UI thread per tick, with no listener. The 2.5.0 text moves to the evidence as history. To stay inside the budget, evidence still in the rule (the Tabs re-measuring detail) moves out first.
3. **No workaround is removed unless that is free.**
   - **The hidden tab's spinner:** if P3 shows the edge no longer closes, the fact changes, but `TableBody`'s `isShown &&` stays. A hidden spinner that turns still makes a redraw every frame for a view nobody can see, and the condition costs one term.
   - **The wizard's guard** is kept whatever R2 finds. It exists for every event a `Push` raises, not this one.
   - **#383's ticker** stays whatever P1 finds. Going back to `Follow` and `app.Invoke` would add code to buy a redraw at most 250 ms sooner.
   - **The option hotkeys:** if P4 shows a hot letter moving, that changes what users see, so the change stops before recording it as accepted and asks the owner (proposal, Capabilities).
   - **The wizard's fit:** if P5 differs from #382's numbers, `WizardFitShould` is changed first (red) to the measured heights, then `WizardFit` (green). If the wizard no longer fits 120 x 30, the change stops and asks the owner, because that is a requirement in `session-launch`.
   - **Alternative rejected:** removing workarounds because 2.6.0 might make them unnecessary. Each is a line or two and holds a behaviour the smoke check does not watch.
4. **`app.InvokeAsync` is recorded, not adopted.**
   - **Where it could go:** `SettingsDialog.LookForPresetsAsync` (its `open` check), `DetailReader.Read` (its lifetime check) and `TableBody`'s settle.
   - **Why not now:** a session-owned dispatch needs the dialog's `SessionToken`, which `app.Run` does not return, so reaching it means `SessionBegun` or `SessionStack` plumbing to replace a one-line `if (open)`. Each call also returns a task carrying the callback's exception. Fire-and-forget would leave that exception unobserved, where `Invoke` hands it to the main loop's error handler today. Not one call site waits for the UI callback to finish.
   - **The bullet says:** use `InvokeAsync` where a pool thread must wait for the UI thread's answer, with the task awaited.
5. **Only unbuilt changes are edited.** `name-ui-stalls` (step 8) decompiles 2.6.0. The archived `guard-overlapping-actions` and `fit-narrow-and-short-terminals` stay as written; the corrected facts live in the rule. The pull request says what P1 found, so a reader of #383's archive finds the update.
6. **The notices come from the published build.** The entries come from `dotnet list src\Source191.Cader package --include-transitive` together with the assemblies a `win-x64` publish puts into `cader.exe`. A package whose new version changed its licence (Wcwidth 4 to 5 is a major version) gets its licence text checked against its package's licence file.

## Risks / Trade-offs

- **[A headless probe misses something only Windows Terminal shows]:** each new fact says "observed headless", and checkpoint D's manual tab checks cover the tab strip.
- **[P1 is timing-dependent]:** it reports how long `Invoke` takes to return and how long until its callback runs, separately. A pass is a return in under 100 ms while the dialog stays open for at least 3 s.
- **[P5 drives a real cader exe]:** only the wizard is opened and closed with Esc, and the build runs with a temporary `LOCALAPPDATA` and the probe cleared of Claude Code's variables, as `CLAUDE.md` asks. A headless cader still writes the real config if a saving key is pressed, so no other key is sent.
- **[Trimming the rule to fit loses a fact]:** the slim-claude-context fact check (every date and version in the old rule appears in the new rule or its evidence) runs before commit.
- **[The stack is not yet rebased]:** this change is implemented only after the rebase onto `main`, so its probes run against the code that ships.
