# Tasks

## 1. The settings

- [ ] 1.1 Red: `DelayShould` facts that a whole number of seconds from 1 to 600 builds a `Delay` answering the matching `TimeSpan`, and that 0, 601, a negative number and a missing value are refused. Green: add `Domain\Delay`. Verify the facts pass.
- [ ] 1.2 Red: `SettingsShould` and `SettingsFileShould` facts that each of `sessionsRefreshSeconds`, `worktreesRefreshSeconds`, `gitCacheSeconds` and `pullRequestCacheSeconds` reads its default (5, 15, 30, 60) when absent, not a number or out of range, reads the value when valid, and is written back under its name. Green: add them to `Domain\Settings`, `Ports\UserConfig` and `Application\SettingsFile` (and `SettingsText` if the dialog carries settings whole). Verify the new facts and the existing settings facts pass.

## 2. The caches

- [ ] 2.1 Red: `SessionRowEnricherShould` facts (`FakeTimeProvider`, `TestSettings`) that a folder's location is reused inside `gitCacheSeconds` and asked again after it, and that a settings change applies to the next refresh. Green: read the window from `CurrentSettings` in `SessionRowEnricher` instead of `CacheWindow`. Verify the facts pass.
- [ ] 2.2 Red: `PullRequestCatalogueShould` facts that an answer is reused for `pullRequestCacheSeconds` and a failed one for three times that, and that a change applies to the next lookup. Green: replace `Window` and `UnavailableWindow` with the setting. Verify the facts pass.

## 3. The refresh timers

- [ ] 3.1 Red: `TabRefreshShould` or `ScreenDefinitionsShould` facts that the Sessions and Worktrees definitions answer their interval from `CurrentSettings` each time they are asked, and that a saved change is answered next. Green: make `TabRefresh.Every` carry a `Func<TimeSpan>` and have `SessionsScreenDefinition` and `WorktreesScreenDefinition` read the setting. Verify the facts pass.
- [ ] 3.2 Re-arming (untested Tui wiring, covered by the smoke step in 6.2): `ConsoleScreen.Every` runs the tick, then adds the next timeout with the interval read at that moment, keeps the latest handle in `timers`, and stops when the screen is disposed. The heartbeat keeps its fixed timeout. Verify `dotnet build -c Debug` and the existing `ScreenLoopShould` facts pass.

## 4. The Config tab

- [ ] 4.1 Red: `ConfigRowsShould` facts that four delay rows follow the other settings, in the order Sessions refresh, Worktrees refresh, Git cache, Pull request cache, and `ConfigViewShould` facts that Enter on one raises no effect and Down steps on. Green: the `ConfigRow` cases and their place in `ConfigRows.For`. Verify both test classes and `KeyHelpParityShould` pass.
- [ ] 4.2 Red: `WordingOfConfigShould` facts for each row's label, its VALUE (`5 s`) and its tooltip (property name, default, one sentence of cost, within the description limit). Green: `Wording` and `WordingOfConfig`. Verify `WordingOfConfigShould` and the British-spelling fact pass.

## 5. Wiring and documentation

- [ ] 5.1 Wire it (untested composition): hand `CurrentSettings` to the screen definitions, `SessionRowEnricher` and `PullRequestCatalogue` in `ConsoleComposition` and in `SceneHost` for `tools\ReadmeScene`. Verify `dotnet build -c Debug`, the whole solution's tests and the hand-off check.
- [ ] 5.2 Documentation: `docs\user-guide.md` (the four settings, their range, and lowering `sessionsRefreshSeconds` for a quicker bell), `.claude\rules\console-screen.md` (the refresh section now reads the settings), and the architecture note in `.claude\rules` if it names the 5 and 15 second constants. Verify `git grep "every 15 seconds"` outside the archive and the specs this change modifies finds nothing stale.
- [ ] 5.3 Raise `Version` as CONTRIBUTING's Versions says and add a `CHANGELOG.md` Added line for the delay settings, ending with the pull request number. Verify `Assert-VersionBumped.ps1` and `Assert-ChangelogUpdated.ps1` against `main`.

## 6. Checks

- [ ] 6.1 Run `dotnet format --verify-no-changes`, `openspec validate configurable-delays` and the full test suite.
- [ ] 6.2 Add a step to `SmokeScript` that starts the console with `sessionsRefreshSeconds` of 2 in a temporary `config.json` and sees a changed session appear within 4 seconds. Verify `dotnet run -- smoke` exits 0.
- [ ] 6.3 Manual (owner): set `sessionsRefreshSeconds` to 2, start a background session that asks for permission, and time the bell against the default of 5; read the diagnostic log's refresh durations to see how long a listing takes. Verify by ear and by the log.
