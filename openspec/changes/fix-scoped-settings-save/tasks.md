# Tasks

**Implement with:** Opus 5.5 at medium effort: a pure reducer change in Core with sociable tests, no threading, files or screen layout.

## 1. The scope's fields

- [ ] 1.1 Red: `SettingsViewShould.NameTheFieldsEachDialogShows`: a theory over every scope (the existing `Scopes` data) that `Fields` is `[field]`, except `Editor`, which is Editor, Executable and Arguments, matching `SettingsControls`' dictionary. Verify it fails to compile or fails. Commit.
- [ ] 1.2 Green: `SettingsScope.Fields` (design 1); 1.1 and the existing `SettingsViewShould` facts pass. Commit.

## 2. Saving past what has gone

- [ ] 2.1 Red: `SettingsViewShould.SaveAnotherSettingWhenASavedRootHasGone`: settings with roots `C:\Users\me\source\repos` and `D:\gone` (not in `Exists`), Sessions refresh scope, `SessionsRefreshChanged("2")`, Save answers `SettingsEffect.Save` with Sessions refresh 2 and both roots kept. Verify it fails with `RootMissing`. Commit.
- [ ] 2.2 Red: `SettingsViewShould.SaveAnotherSettingWhenASavedCustomEditorHasGone`: a custom editor `E:\Tools\subl.exe` with `-n {folder}` (not in `Installed`), Place scope, `PlaceChanged`, Save answers `SettingsEffect.Save` with the new place and the editor kept. Verify it fails with `ExecutableMissing`. Commit.
- [ ] 2.3 Green: `Saved` passes the folder check only when the scope shows Roots and the file check only when it shows Executable (design 2); 2.1, 2.2 and the existing `RefuseToSaveWithTheProblemAndItsField` rows (a missing root in the Roots dialog, a missing executable in the editor dialog) pass. Commit.

## 3. Focus stays in the dialog

- [ ] 3.1 Red: `SettingsViewShould.KeepFocusInTheDialogWhenAnotherSettingIsRefused`: settings listing `C:\Users\me\source\repos` twice, Worktrees refresh scope, Save shows `RootAlreadyListed` and keeps `Focus` on WorktreesRefresh; and the same file in the editor dialog with Custom chosen and focus moved to Arguments keeps Arguments. Verify it fails with focus on Roots. Commit.
- [ ] 3.2 Green: `Saved` moves focus to the problem's field only when the scope's `Fields` holds it (design 3); 3.1 and every `SettingsViewShould` fact pass. Commit.
- [ ] 3.3 Refactor: widen 3.1 into a theory over every scope (the `Scopes` data): saving settings that list a root twice answers a `Focus` in `Scope.Fields`, and the existing `RefuseToSaveWithTheProblemAndItsField` rows still answer their own field. Verify `dotnet test test\Source191.Cader.Core.Tests -c Debug --no-build` passes. Commit.

## 4. Finish

- [ ] 4.1 Raise `Version` one preview step above what `main` holds (0.22.0-preview.49, or .50 if `reduce-refresh-allocations` merged first) and add a Fixed entry under Unreleased in `CHANGELOG.md`: saving a setting on the Config tab no longer crashes Cader, or is refused, because a repository root's folder or a custom editor's program has gone (#358). Verify `dotnet build -c Debug`, `dotnet test -c Debug --no-build` at the solution and `dotnet format --verify-no-changes` pass. Commit.
- [ ] 4.2 Run the smoke check over a Release build (`dotnet run -- smoke <exe> <worktree-folder>` from `tools\HeadlessTerminal`) and verify it exits 0, so every settings dialog still opens, saves and closes. Commit any fix.

## Workflow follow-up

- Archive the change in the same pull request (`/opsx:archive`) and verify that `openspec validate` passes.
- The proposal is opened first as a pull request from `c-error`; the implementation lands on the same branch, and its description then gains What changed and closes #358.
