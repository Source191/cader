# Tasks

**Implement with:** Opus 5.5 at high effort: a Terminal.Gui behaviour that is read but not yet observed (the spike), layout that only shows on screen, and a debounced `config.json` write that must not race the settings dialog's save.

Each code task is red, green, refactor with a commit per step; the test is named first.

## 1. Spike

- [ ] 1.1 Throwaway program (not committed) with a `TableView` and a pane of `Arrangement = ViewArrangement.TopResizable` and a top-only border inside a `Tabs` tab, run in the headless terminal (SGR mouse press, motion, release) and in a real Windows Terminal: record whether the drag works, what it leaves in `Y` and `Height`, which event reports it, whether anything is drawn on the border, whether the arrange mode can be entered by a key the console uses, and what a window resize does after a drag (design 1). Verify: the facts are in `.claude\rules\terminal-gui.md` with the date and version, and the choice (`TopResizable` or the fallback) is noted in design.md. Commit.

## 2. Settings

- [ ] 2.1 `PaneHeightsShould` (Domain): a `PaneHeights` record of the Sessions and Worktrees lines; `SettingsShould.KeepPaneHeightsThroughWithPaneHeights` and `ResolveDefaultPaneHeightsOfSixAndTen`. Verify: `dotnet test test\Source191.Cader.Core.Tests -c Debug --no-build`. Commit each step.
- [ ] 2.2 `SettingsFileShould`: reads `sessionsDetailRows` (1 to 6) and `worktreesDetailRows` (1 to 200), a missing or out-of-range value as its default, a non-whole value as a corrupt file; writes both. `UserConfig` gains the two optional members (its `Equals` and `GetHashCode` too) and `UserConfigFileShould` (Infrastructure) round-trips them. Verify: the settings spec's scenarios each have a fact. Commit each step.
- [ ] 2.3 `CurrentSettingsShould.SavePaneHeightsOverTheCurrentSettings` and `WriteTwoSavesOneAfterTheOther` (a `TestSettings` store that holds its first write open): `SavePaneHeightsAsync` and a `SemaphoreSlim` around saves (design 6). Commit each step.
- [ ] 2.4 `SaveSettingsHandlerShould.KeepThePaneHeightsAtTheMomentOfTheSave`: a dialog save keeps the heights `CurrentSettings` holds when it saves, not when the dialog opened. Commit each step.

## 3. Height rules

- [ ] 3.1 `PaneHeightShould` (Application\Detail): at least 1, at most the tab's most, at most the lines the window leaves; a step from the shown height (design 2). Commit each step.
- [ ] 3.2 `PaneHeightsShould` (Application, `FakeTimeProvider`): seeded from the settings; `TakeDue()` answers nothing until a second after the last change, then the heights once, then nothing; a change to one tab keeps the other's height (design 5). Commit each step.
- [ ] 3.3 `PaneKeysShould` (Application\Views): `+` answers 1, `-` answers -1, every other key false. `KeyMappingShouldMap` pins `+` and `-`. Commit each step.

## 4. Worktree list

- [ ] 4.1 `IDetailSource` reads with the lines shown and states its most lines; `WorktreeDetailsShould.ListAsManyItemsAsTheLinesHold` (fifteen lines and twelve commits: ten listed, 2 more; eight commits: all, no more line) and `SessionDetailsShould` unchanged in content. Verify: the detail-pane spec's list scenarios each have a fact. Commit each step.

## 5. Words

- [ ] 5.1 `WordingShould`: `+` and `-` as `KeyHelp.HelpOnly` in `SessionsKeys` and `WorktreesKeys` with their tips, and the help's Pane section (`Wording.SessionsPane`, `WorktreesPane`) says the divider can be dragged; `KeyHelpParityShould` answers them through `PaneKeys`. Verify: `KeyHelpParityShould` and `SpellBritish` pass. Commit each step.

## 6. Screen

- [ ] 6.1 `DetailReader`: one `Apply(lines)` re-anchors the pane and the table (design 3), `Resize(step)` applies `PaneHeight` and tells `PaneHeights`, the tab view's `FrameChanged` re-clamps without changing the wanted height, a resize asks for the detail again, and `None` ignores it. `TableBody.OnKey` answers `PaneKeys` after `ScrollKeys` under `!FilterEditing`. Verify: a headless terminal check presses `+` and `-` on both tabs and on a filter, and the table's row count moves by one each time outside the filter.
- [ ] 6.2 Drag, as the spike chose: `TopResizable` with its frame change read back into `Apply`, or the fallback on the divider row. Verify: the headless terminal drags the Worktrees divider up four rows (SGR press, motion, release) and the pane lists four more commits on a worktree that has them.
- [ ] 6.3 `ConsoleScreen.RefreshWhatIsDue` asks `PaneHeights.TakeDue()` and saves off the UI thread through `Attempt.RunAsync` with a new `FailureSite` (`DiagnosticEventsShould` and `DiagnosticsPrivacyShould` pass); `ConsoleComposition` builds `PaneHeights` from `CurrentSettings` and `SceneHost` copies the wiring. Verify: five `+` presses a fifth of a second apart write `config.json` once in a temporary state folder (`InMemoryState` in a `ScreenLoopShould`-level fact, or the headless terminal against a scratch `%LOCALAPPDATA%`, never the real one).

## 7. Documents and release

- [ ] 7.1 `docs\user-guide.md` (Detail pane: drag, `+` and `-`, saved per tab), `.claude\rules\console-screen.md` (Detail pane: the height rule and its save), `CHANGELOG.md` (Added, under Unreleased), and a preview step of `Version` in `src\Source191.Cader\Source191.Cader.csproj`. Verify: `Assert-VersionBumped.ps1` passes.
- [ ] 7.2 Whole check: `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes`, `openspec validate resizable-detail-pane --strict`, the smoke check of a local install, and by hand in Windows Terminal: drag both dividers, resize the window short and tall, quit and restart.

## Workflow follow-up

- Archive the change in the pull request that completes it, so `openspec\specs` describes the code on `main`.
