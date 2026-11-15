# Tasks

Each constructor change lands with every call site it breaks (`src`, `test` and `tools\ReadmeScene\SceneHost`, including target-typed `new(`), so the solution builds and `dotnet test -c Debug --no-build` passes after every commit.

## 1. Pure rules: sizes and the clear's event

- [x] 1.1 **Red:** `WordingOfConfigShould` facts for `Wording.FileSize`: 0 bytes reads 0 KB, 300 bytes 1 KB, 512,000 bytes 500 KB, 1,048,575 bytes 1024 KB, 1,048,576 bytes 1.0 MB, 2,516,582 bytes 2.4 MB, 10,485,700 bytes 10.0 MB. **Green:** `Wording.FileSize` (invariant culture). **Refactor:** keep it beside `Wording.Memory`. **Verify:** the facts pass.
- [x] 1.2 **Red:** `DiagnosticLineShould` fact that `DiagnosticLogCleared(2, 13002282, 0)` reads `diagnostic log cleared: 2 files, 13002282 bytes, 0 kept`. **Green:** the `Application\Diagnostics\DiagnosticLogCleared` event and its `DiagnosticLine` arm. **Verify:** the fact, `DiagnosticEventsShould` and `SealedTypesShould` pass.

## 2. The port and its adapter

- [x] 2.1 **Red:** `Infrastructure.Tests\DiagnosticLogFilesShould` over a temporary folder: `Read` answers both files' sizes and paths, and `Absent` for a missing file and for a missing folder. **Green:** `Ports\IDiagnosticLogFiles` (`Read`, `Clear`, no parameters), the records `LogFiles`, `LogFileOnDisc` (`Absent`, `Written`, cases inside the base) and `LogFilesCleared`, `Adapters\Diagnostics\DiagnosticLogFiles(string path)`, and `DiagnosticFile.OlderPath(path)` as the one home of the `.1` name, used by `DiagnosticFile`'s rollover too. **Verify:** the facts, every `DiagnosticFileShould` fact, `ClosedHierarchiesShould`, `NullFreeModelsShould` and `LayeringShould` pass.
- [x] 2.2 **Red:** `DiagnosticLogFilesShould` facts for `Clear`: both files deleted with their bytes counted; one file missing; neither file nor the folder there answers 0, 0, 0; a file held open by a `FileStream` with `FileShare.Read` is kept and counted while the other is still deleted; `last-crash.log` and any other file in the folder are untouched. **Green:** `Clear`, deleting `cader.log` before `cader.log.1` and catching `IOException` and `UnauthorizedAccessException` per file. **Verify:** the facts pass.
- [ ] 2.3 **Red:** `DiagnosticLogFilesShould` fact that a `DiagnosticFile` over the same path writes a new `cader.log` holding only its next line after a clear. **Green:** nothing new expected; fix whatever the fact finds. **Verify:** the fact passes.
- [ ] 2.4 Add `TestDoubles\FakeDiagnosticLogFiles` (sizes in memory, a file can be marked held open, counts its clears), with a line in the TestDoubles `README.md`. **Verify:** the solution builds; the fake is exercised from 3.1 on.

## 3. The rows read the files

- [ ] 3.1 **Red:** `SetupFactsReaderShould` facts that the Diagnostic log file state carries both files from `FakeDiagnosticLogFiles`. **Green:** `SetupFactsReader` takes `IDiagnosticLogFiles` in place of the log path and of `IFolders`, which it used only for the log; `SetupState.LogFile(LogFiles)` and `LogOff(LogFiles)` replace the old shapes; `ConfigRows.LogFileAs` keeps the files when the setting is off; `SharedParts` builds `DiagnosticLogFiles` over the sink's `logFile` and `ConsoleComposition`, `SceneHost`, `ConsoleLoadsShould` and the other call sites pass it. **Verify:** the facts, the updated existing facts and `dotnet build -c Debug` (with `tools\ReadmeScene`) pass.
- [ ] 3.2 **Red:** `ConfigRowsShould` facts: the older row follows the log row only while `cader.log.1` is `Written`, on or off; none while `Reading` or `Unreadable`; its key is stable. **Green:** `SetupItem.OlderDiagnosticLogFile`, `SetupState.OlderLog(LogFiles)`, `ConfigRows`. **Verify:** the facts pass, and the counts still read 8 settings with the older row present.
- [ ] 3.3 **Red:** `WordingOfConfigShould` facts for the values (on, 2.4 MB, path; off, 500 KB, path; off, path; on, not written yet, path; 10.0 MB, path of `cader.log.1`), the older row's name Diagnostic log file (older), its tooltip, the log row's tooltip naming `d` and 10 MB, and the Enter advice on both rows naming `d`. **Green:** `WordingOfConfig`. **Verify:** the facts and `SpellBritish` pass.
- [ ] 3.4 **Red:** `ConfigViewShould` facts: `c` on the older row copies the path of `cader.log.1`; `f` on it raises `OpenFileInEditor`, not `OpenFolderInEditor`. **Green:** `ConfigView.TryPathOf` and `OpenEffect`. **Verify:** the facts pass.

## 4. The handler

- [ ] 4.1 **Red:** `ClearDiagnosticLogHandlerShould` facts over `FakeDiagnosticLogFiles` and a real `DiagnosticLog` with a `RecordingSink`: both cleared answers `Cleared(2, bytes)`; none there answers `NothingToClear`; one held open answers `PartlyCleared`; both held open answers `NoneCleared`; each records one `DiagnosticLogCleared` with the same numbers before it returns, and the in-memory events recorded before it are still there. **Green:** the `ClearDiagnosticLog(LogFiles Files)` effect record (a `ViewEffect`; its `IDialogEffect` marker comes with its flow in 5.2, and no reducer raises it before 6.1), `ClearDiagnosticLogHandler`, `ClearDiagnosticLogOutcome` in its own file, `EffectPorts.LogFiles` passed at every call site (`ConsoleComposition`, `SceneHost`, `EffectDispatcherShould`, `DiagnosticsPrivacyShould`) and its line in `Composition.Handlers`. **Verify:** the facts, `EffectDispatcherShould` and `HandlersShould` pass.
- [ ] 4.2 **Red:** `WordingOfOutcomesShould` facts for each case's words (2 files (12.4 MB) deleted, the events kept for a problem report unchanged; no log file to delete; one could not be deleted, another program may have it open, try again; none could be deleted, try again) and `StatusTonesShould` lines (Success, Hint, Warning, Warning). **Green:** `WordingOfOutcomes` and `Wording.ToneOf`. **Verify:** the facts pass.
- [ ] 4.3 **Red:** a `DiagnosticsPrivacyShould` flow that clears a log under a Northwind fixture folder and finds no fixture value in the output. **Green:** nothing new expected. **Verify:** the fact passes.

## 5. The dialog

- [ ] 5.1 **Red:** `WordingShould` facts for `ClearLogTitle` and `ClearLogQuestion` (names each existing file with its size, never an absent one, says the events kept for a problem report stay and that a log that is on starts again with its next event), and for `DialogName.ClearLog`'s tab title `Cader (Clear log)`. **Green:** the words and the `DialogName` member with its `DialogWord` arm. **Verify:** the facts pass.
- [ ] 5.2 **Red:** mark `ClearDiagnosticLog` as an `IDialogEffect`; `DialogEffectsShould` fails for it with no flow. **Green:** `ConfirmClearLog` in `Tui\DialogEffects`: `Confirm.Ask` with `Wording.DeleteButtons`; Cancel shows `StatusMessage.Kept`; Delete dispatches the effect. **Verify:** `DialogEffectsShould`, `EffectRoutingShould` and `ScreenLoopShould` pass.

## 6. The key

- [ ] 6.1 **Red:** `ConfigViewShould` facts: `d` on the log row or the older row with a file `Written` raises `ClearDiagnosticLog` carrying the files; with both `Absent` it explains `NoLogToClear`; while `Reading` it explains `LogFilesNotRead`; on Worktree root and on Diagnostic log it explains `ClearsOnlyTheLog` and changes no setting. **Green:** the `d` case in `ConfigView.Act`, the three `StatusMessage` cases, their `Wording` words and `ToneOf` lines (Hint). **Verify:** the facts and `StatusTonesShould` pass.
- [ ] 6.2 **Red:** `WordingOfScreensShould` fact that the Config bar's second row reads c, f, d, /, ?, !, q, with d labelled Clear log. **Green:** the `KeyHelp` line in `ConfigKeys` and `SecondBarRowForConfig`. **Verify:** the fact and `KeyHelpParityShould` (which presses `d` on a root row, answered by the explanation) pass.

## 7. Checks and docs

- [ ] 7.1 Build the README scene tool and run its tests (`dotnet build tools\ReadmeScene` and `dotnet test tools\ReadmeScene.Tests`), never `New-ReadmeScreenshot.ps1`, which rewrites the images. **Verify:** both pass.
- [ ] 7.2 By hand on a local install (`tools\install-local.ps1`, run with NO_COLOR, CLAUDECODE, CLAUDE_PID and CLAUDE_CODE_* cleared): with the log on and a `cader.log.1` made by copying a file, the older row shows, `d` and Cancel keep both, `d` and Delete clear both and the log row reappears small with the clear's line near its start; with a second Cader running, both carry on. **Verify:** each step seen as described.
- [ ] 7.3 Update `docs\user-guide.md` (the Config section's Diagnostic log file paragraph and the diagnostic log bullet: the sizes, the older row, `d`), `.claude\rules\console-screen.md` (the Config paragraph: `LogFile`/`LogOff` carry `LogFiles`, the older row, `d`) and `.claude\rules\security.md` (the log files are deleted only by `DiagnosticLogFiles.Clear`, at the two paths it was built with, taking no path). **Verify:** the guide's text matches `Wording` where facts compare them, and `dotnet test -c Debug --no-build` passes.
- [ ] 7.4 Add an Added entry under Unreleased in `CHANGELOG.md` with the pull request's number, and raise `Version` with `.github\scripts\Step-Version.ps1` (0.22.0-preview.20 to 0.22.0-preview.21, a preview of the minor release already under way). **Verify:** `.github\scripts\Assert-VersionBumped.ps1 -Base main -Head HEAD` passes and `dotnet format --verify-no-changes` is clean.

## Workflow follow-up

- Archive the change in the same pull request (`/opsx:archive`), so `openspec\specs\config-tab` and `diagnostics` describe `main`, then run `openspec validate --strict` on the specs.
