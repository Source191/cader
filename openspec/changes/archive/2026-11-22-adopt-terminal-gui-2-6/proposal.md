## Why

Terminal.Gui 2.6.0 was merged in PR #386 (0.22.0-preview.54), but everything that describes it still describes 2.5.0, and some of it is now wrong or unchecked:

- **The timer lock fact is out of date.** `.claude\rules\terminal-gui.md` says a dialog opened inside a timeout or `app.Invoke` callback blocks every other thread's `app.Invoke` until it closes. 2.6.0 runs callbacks outside the queue's lock (tui-cs#5655), so that is probably no longer true. `guard-overlapping-actions` (#383, built on 2.5.0) records the 2.5.0 probe as a fact and gives it as the reason launch steps are redrawn by the ticker. Once the stack is on 2.6.0, the fact is wrong and the reason no longer holds. The ticker stays, because it deleted code.
- **A fact from #382 was observed on 2.5.0 only.** `fit-narrow-and-short-terminals` (#382) measured the wizard's dialog chrome on 2.5.0 (six rows of `Height` besides its content), and `WizardFit` pins the heights it chose. 2.6.0 changed a file under `Views/Runnable` and one under `ViewBase/Adornment`, so the chrome needs re-measuring before the fit is trusted.
- **Other facts the console relies on changed underneath it:** tab borders are redrawn differently (tui-cs#5777, which may fix the hidden tab's spinner closing the shown tab's edge), options re-pick their hotkeys when retitled (tui-cs#5761, which the Open folder with dialog does), replacing a `ListView`'s source now clears its selection and raises `ValueChanged` (tui-cs#5745, which the wizard does), and `Run` alone now owns the synchronisation context (tui-cs#5722). Nobody has checked these on 2.6.0, so the rules may send a later change after a problem that is gone, or past one that is new.
- **The shipped licence notices are stale.** `THIRD-PARTY-NOTICES.txt`, which goes into every release zip, names Terminal.Gui 2.5.0 and older versions of six other dependencies (Markdig, Microsoft.Extensions.Configuration, Microsoft.Extensions.FileProviders.Physical, TextMateSharp, TextMateSharp.Grammars, Wcwidth) than the ones shipped.
- **2.6.0 adds an API** worth knowing about: `app.InvokeAsync`, awaitable and optionally owned by a session. The rules should say what it offers and when Cader should use it, so it is not adopted by habit or missed where it helps.

Doing this before checkpoint A means the owner reviews steps 1 to 4 on the Terminal.Gui they will ship with, and `name-ui-stalls` (step 8) decompiles the right version.

## What Changes

- **Re-verify the facts 2.6.0 touched** with throwaway probes in the headless terminal and by reading the 2.6.0 assembly: a dialog inside `Invoke`, timers under a dialog, the hidden tab's spinner, option hotkeys after a retitle, the wizard's dialog chrome, a `ListView` source swap, and the synchronisation context. Each result is one dated bullet in `terminal-gui.md`, and its evidence goes in `docs\verified-facts\terminal-gui.md`.
- **Re-head the rule as Terminal.Gui 2.6.0 facts.** Each fact keeps its own date and version. A fact in code that 2.6.0 did not change (checked against the v2.5.0...v2.6.0 source compare) says so. The two lock bullets #383 left are replaced by one, and the ticker's reason is restated.
- **Record `InvokeAsync`** and why Cader keeps `app.Invoke` for now (design 4).
- **Follow the probes where they lead:** if the wizard's chrome changed, correct `WizardFit`'s heights test-first; `name-ui-stalls`' spike decompiles 2.6.0, not 2.5.0.
- **Correct the references:** `CLAUDE.md`'s rule index, `docs\verified-facts\README.md`, and `THIRD-PARTY-NOTICES.txt` with every shipped dependency at its shipped version.
- **Simplifications are taken only where a probe allows one without a cost** (design 3). None is expected: the spinner stilling also saves redraws, and the wizard's binding guard already absorbs the new `ValueChanged`.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None: Cader's behaviour does not change, so the change declares `skip_specs: true`. If a probe finds a behaviour that does change (for example, an option's hot letter moving, or the wizard no longer fitting 120 x 30), the change stops and the owner decides whether it becomes a requirement here or a change of its own.

## Delivery

- **Proposal:** this pull request adds the change alone, to `main`, as #376 to #378 did.
- **Implementation:** a step of its own in `openspec\implementation-plan.md`, stacked on #384 (step 4) and before checkpoint A, once the stack has been rebased onto `main` (which now holds 2.6.0 and 0.22.0-preview.54). The owner may place it elsewhere.
- **Together:** one pull request. It raises `Version` one preview step above the step below it and adds a `CHANGELOG.md` line, which ships the corrected notices.

## Out of scope

- **Adopting `app.InvokeAsync`** in `SettingsDialog`, `DetailReader` or `TableBody` (design 4 says why).
- **Undoing #383's ticker.** It is correct on 2.6.0 too and deleted code; only its stated reason changes.
- **Editing archived changes.** `guard-overlapping-actions` and `fit-narrow-and-short-terminals` are archived in #383 and #382 as the history of what was decided on 2.5.0.
- **New features 2.6.0 enables:** custom `FileDialog` icons, opening a menu at an item, inline mode.
- **A check that keeps the notices in step with the packages.** It would fail every Dependabot pull request until someone edits the notices, which needs the owner's decision on how Dependabot's follow-up commit handles it. Noted for a later change.
- **The keys spike** (`Key.TryGetPrintableRune` against `AsRune`), which needs a person at a real keyboard.
- **Re-observing in Windows Terminal** what the probes see headless. Checkpoint D's manual checks of tabs and attach cover the tab strip.
- **Multiple Claude accounts** beyond naming a session's owner, a later change.

## Impact

- `.claude\rules\terminal-gui.md` and `docs\verified-facts\terminal-gui.md`; `docs\verified-facts\README.md`; `CLAUDE.md` (one line).
- `openspec\changes\name-ui-stalls\` (proposal and tasks); `openspec\implementation-plan.md` (the step's tick).
- `src\Source191.Cader.Core\Application\Views\WizardFit.cs` (from #382) and its tests, only if the chrome probe shows the heights changed.
- `THIRD-PARTY-NOTICES.txt`, `CHANGELOG.md` and the version in `src\Source191.Cader\Source191.Cader.csproj`.
- The probes are throwaway programs, deleted before commit.
- **Implement with:** Opus 5.5 at high effort: little is edited, but the probes test threading and drawing (the timer gate, a dialog inside `Invoke`, nested `Run`, dialog chrome), and reading a result wrongly would mislead every later step.
