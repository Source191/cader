# Tasks

**Implement with:** Opus 5.5 at high effort: little is edited, but the probes test threading and drawing (the timer gate, a dialog inside `Invoke`, nested `Run`, dialog chrome), and reading a result wrongly would mislead every later step.

**Before starting:** the stack (#380 to #384) has been rebased onto `main`, so it builds against Terminal.Gui 2.6.0. Branch from step 4's branch (`c-reused-branch-prs`). Each probe is a throwaway console program outside the solution (in the scratchpad, referencing Terminal.Gui 2.6.0), run in `tools\HeadlessTerminal` at 120 x 36 unless named, and deleted after its result is written down. Only P5 runs Cader itself, and only as design 1 and its risk say. Code changes are red, green, refactor; every other task is one commit.

## 1. Probes and reads

- [x] 1.1 **P1** (design 1): open a dialog inside an `app.Invoke` callback for 4 s while a second thread calls `app.Invoke`, and repeat with the dialog opened inside a timeout callback. Record, for each, how long the call takes to return and how long until its callback runs. It passes if the call returns in under 100 ms while the dialog is open. Write the times in `docs\verified-facts\terminal-gui.md` under a new "Terminal.Gui 2.6.0" heading. Commit.
- [x] 1.2 **P2**: a 250 ms timeout through a 5-second dialog opened by a key. Verify it beats about 18 times, as on 2.5.0, and write the count in the evidence. Commit.
- [ ] 1.3 **P3**: `Tabs` with Sessions shown and a `SpinnerView` turning on the hidden Worktrees tab. Capture whether the shown tab's top edge under its title closes, and write it in the evidence with the capture's relevant row. Commit.
- [ ] 1.4 **P4**: an `OptionSelector` built as `OptionSelectors.For<EditorPreset>` does, then each option's `Title` set as `SettingsControls.ShowPresets` does (`Rider` to `Rider (not found)`, and back). Write the hot letter of each option before and after, read from the capture's escape sequences, in the evidence. **If any letter moves, stop and tell the owner** before task 2.1 (proposal, Capabilities). Commit.
- [ ] 1.5 **P5**: a Debug build of the branch opens the wizard (`n` on Sessions) at 120 x 30, 36 and 40, with a temporary `LOCALAPPDATA` and Claude Code's variables cleared, then closes it with Esc; no other key is sent. Compare the drawn frame, list rows and form placement with #382's bullet and `WizardFit.For`'s answers. Write both in the evidence. **If the wizard no longer fits 120 x 30, stop and tell the owner.** Commit.
- [ ] 1.6 **R1 and R2** (design 1): from the 2.6.0 assembly decompiled with ilspycmd, verify that `ApplicationImpl.Run` installs `MainLoopSyncContext` for the loop, and that `ListView`'s `Source` setter raises `ValueChanged` when it clears the selection. Write both in the evidence, naming the methods read. Commit.
- [ ] 1.7 Save the v2.5.0...v2.6.0 compare's list of changed files under `Terminal.Gui/` (`gh api repos/tui-cs/Terminal.Gui/compare/v2.5.0...v2.6.0`) in the evidence. Note the areas the rule's facts depend on that it leaves untouched: TableView, TextInput, Selectors, SpinnerView and ToolTips. Commit.

## 2. The wizard's fit, only if P5 differed

- [ ] 2.1 `WizardFitShould`: change the expected heights and list rows to P5's measurements (red, failing on the old numbers). Commit.
- [ ] 2.2 `Application\Views\WizardFit`: the chrome constant or heights that make 2.1 pass (green); then refactor. Verify `dotnet test test\Source191.Cader.Core.Tests -c Debug --no-build` and the smoke check of a `tools\install-local.ps1` build (`dotnet run -- smoke <exe> <worktree-folder>` from `tools\HeadlessTerminal`). Commit each step.

## 3. The rule and its references

- [ ] 3.1 `.claude\rules\terminal-gui.md`: head it "Terminal.Gui 2.6.0 facts", with the heading line from design 2. First move the Tabs re-measuring detail and any other evidence still in the rule to `docs\verified-facts\terminal-gui.md`. Then write:
  - one bullet replacing #383's two lock bullets, with P1's result and the ticker's own reason (design 2); the 2.5.0 text moves to the evidence as history;
  - #382's chrome bullet re-dated with P5's result;
  - one dated bullet each for P2, P3 (with the reason the spinner stilling stays: design 3), P4, R1 and R2 (under the wizard's guard), and `app.InvokeAsync` (design 4);
  - the 2.6.0 CI run (45052211647) in Basics, beside the .NET 11 bullet.

  Verify the slim-claude-context fact check (every date and version in the rule as it was on the branch appears in the new rule or its evidence) and that `pwsh .github\scripts\Assert-ClaudeContextBudget.ps1 -Root .` passes. Commit.
- [ ] 3.2 `docs\verified-facts\terminal-gui.md`'s title, and its row in `docs\verified-facts\README.md`, read "Terminal.Gui 2.5.0 and 2.6.0". `CLAUDE.md`'s rule index says Terminal.Gui 2.6.0. Verify the budget script still passes. Commit.
- [ ] 3.3 `.claude\rules\console-screen.md`: verify that nothing in it rests on a fact task 3.1 changed, including the ticker line #383 added. The re-measure pointer on the tab strip line still holds, because the compare leaves `TitleView` alone. Change only what does not hold, and say so in the commit. Commit.

## 4. The unbuilt changes and the plan

- [ ] 4.1 `name-ui-stalls`: its proposal's and tasks' "Implement with" lines and task 1.1 say Terminal.Gui 2.6.0. Verify `openspec validate name-ui-stalls --strict`. Commit.
- [ ] 4.2 `openspec\implementation-plan.md`: add and tick this change as its own step after step 4 and before checkpoint A (Opus 5.5, high), unless the owner has placed it elsewhere. Commit.

## 5. The notices and the release

- [ ] 5.1 `THIRD-PARTY-NOTICES.txt` (design 6): build the list from `dotnet list src\Source191.Cader package --include-transitive` and a `win-x64` publish into `artifacts\publish`. Update every entry whose version differs (Terminal.Gui 2.6.0, Markdig 1.4.2, Microsoft.Extensions.Configuration and FileProviders.Physical 11.0.0, TextMateSharp and TextMateSharp.Grammars 2.0.5, Wcwidth 5.0.2, and any other the list shows). Add or remove entries to match what ships, and check each changed major version's licence text against its package's licence file. Verify that every shipped third-party assembly has an entry and no entry names something that does not ship. Commit.
- [ ] 5.2 Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` one preview step above step 4's and add the `CHANGELOG.md` line ("The licence notices name the versions shipped."). Verify `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes`, `Invoke-Pester test\Scripts` and `openspec validate adopt-terminal-gui-2-6 --strict`. Commit.

## Workflow follow-up

- Archive the change in the same pull request (`/opsx:archive`), so it never reaches `openspec\specs`; it declares `skip_specs: true`.
- The pull request says "Stacked on #384", opens with Why, Benefit and What changed, and says what P1 and P5 found.
