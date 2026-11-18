# Tasks

**Implement with:** Claude Opus 5.5 (`claude-opus-5-5`) at **medium** effort, in one session, one task at a time. Each task is red, green, refactor with a commit per step (CLAUDE.md). Apply this change, merge it and archive it before `refresh-on-change` starts.

## 1. The interval value and the settings

- [x] 1.1 Red then green: `RefreshIntervalShould` (Core.Tests, Domain) states `From(0)` is `Off`, `From(3600)` is an hour, `From(-1)` and `From(3601)` throw, `TryFrom(null)` and `TryFrom(86400)` answer false, and `Every` is the seconds as a `TimeSpan`; add `Domain\RefreshInterval` (design decision 2).
- [x] 1.2 Red then green: `SettingsShould` states `Resolve` carries both intervals, `ToText` shows them as their numbers, `Validate` accepts ` 30 ` and `0` and refuses `2.5`, `ten` and `3601` with `SettingsProblem.RefreshIntervalNotWholeSeconds` naming the field, and `Equals` compares them; extend `Settings`, `SettingsText` and `SettingsProblem` (`SealedTypesShould` and `ClosedHierarchiesShould` stay green).
- [x] 1.3 Red then green: `SettingsFileShould` states a file without the properties reads 5 and 15 seconds, `-1` and `86400` read as their defaults with `repositoryRoots` kept, and `ToConfig` always writes both; add the two `int?` members to `Ports\UserConfig` (`= null`, as its record allows) and resolve them in `SettingsFile` (decision 3). `JsonFileStoreShould` (Infrastructure.Tests) states `"sessionsRefreshSeconds": "fast"` reads as a corrupt file.

## 2. The schedule

- [x] 2.1 Red then green: `RefreshScheduleShould` (Core.Tests, Application, `FakeTimeProvider`) states a schedule is due once its interval has passed since `Started()`, never while the interval is `Off`, picks up a new interval from its function on the next ask, and that `Started()` from any cause pushes the next due time back a whole interval; add `Application\RefreshSchedule` (decision 1).

## 3. F5

- [x] 3.1 Red then green: `KeyMappingShould` states `GuiKey.F5` maps to `Key.F5`; add `KeyName.F5`, `Key.F5` and the mapping.
- [x] 3.2 Red then green: `SessionsViewShould`, `WorktreesViewShould` and `ConfigViewShould` each state F5 answers `RefreshShownTab`, including while a filter is being typed, with the filter unchanged; add `RefreshShownTab` with its `IRefreshesTab` marker, `Application\Views\RefreshKeys`, and the arm in each reducer ahead of filter typing (decision 4).
- [x] 3.3 Red then green: `EffectRoutingShould` fails for a marked effect nothing routes until `TableBody` routes `IRefreshesTab` to `ITabHost.RefreshShown()`; add the member to `ITabHost` and its fakes.
- [ ] 3.4 Red then green: `WordingShould` states the help-only F5 line ("F5 refresh this tab now") and its tip are in `SessionsKeys`, `WorktreesKeys` and `ConfigKeys` and in no key bar; `KeyHelpParityShould` presses F5 on each loaded table and gets an answer (decision 5).

## 4. The Config rows and their dialog

- [ ] 4.1 Red then green: `ConfigRowsShould` states the rows read Default place, Sessions refresh, Worktrees refresh, Check for updates in that order, with values every 5 seconds, every 15 seconds, every second for 1 and off for 0, and the counts read 10 settings for one root; add the two `ConfigRow` cases and their `Wording` (values, labels, tooltips with the bell sentence on Sessions) with `WordingShould` facts.
- [ ] 4.2 Red then green: `ConfigViewShould` states Enter on either row opens the settings dialog on its field, and c and f say the value is not a path; `SettingsViewShould` states typing `60` and Save in the Worktrees refresh dialog saves 60, typing `ten` keeps the dialog open with the problem on that field, and the dialog opens on the saved value; add the two `SettingsField` members and the reducer cases, with the problem's words in `Wording`.
- [ ] 4.3 `Tui\SettingsDialog` shows the one box for an interval field, bound to the reducer as the worktree root's box is; `SmokeScript` gains a step that opens Sessions refresh and closes it with Esc (its marker from `Wording`), and `SmokeScriptShould` stays green. Verify by `dotnet run -- smoke` from `tools\HeadlessTerminal` against a local install.

## 5. The screen follows the schedule

- [ ] 5.1 `TabRefresh.Every` carries a `RefreshSchedule`; `SessionsScreenDefinition` and `WorktreesScreenDefinition` take theirs from the composition (reading `CurrentSettings.Value.SessionsRefresh` and `WorktreesRefresh`), and `ConsoleComposition` and `tools\ReadmeScene\SceneHost` build them. `ConsoleScreen` replaces the per-tab timers with one 250 ms ticker that asks each schedule, tells it `Started()` wherever it starts that tab's refresh, keeps the Worktrees timer's "only while shown" rule, and implements `RefreshShown()`. Untested wiring under the Tui rule; verify with `dotnet build -c Debug` and the full suite.
- [ ] 5.2 Manual, in a local install (`tools\install-local.ps1`, Claude Code variables cleared): set Worktrees refresh to 5, add a worktree in a shell and see it listed within about 5 seconds with the tab shown; set it to 0 and see it only on F5 or a show; press F5 on Sessions and see a session started elsewhere at once; press F5 in the wizard and see the session start. Record what was seen, the date and the build here.

## 6. Documents and release

- [ ] 6.1 `docs\user-guide.md` describes the two settings and F5; `.claude\rules\console-screen.md`'s Refreshing section names `RefreshSchedule`, the single ticker and `IRefreshesTab`. Verify by reading them against the specs.
- [ ] 6.2 Raise the minor `Version` with `.github\scripts\Step-Version.ps1` and add an Added entry under Unreleased in `CHANGELOG.md`. Verify `dotnet build -c Debug`, `dotnet test -c Debug --no-build` at the solution and `dotnet format --verify-no-changes` pass.

## Workflow follow-up

- Archive the change in the same pull request (`/opsx:archive`) and verify that `openspec validate` passes.
- Open the pull request with Why, Benefit and What changed.
