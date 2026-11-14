## 1. Setup state and row (domain and application first)

- [ ] 1.1 Read `OpenInEditorHandler`, `EditorCommand`, `CopyFolderPath` and the `FolderPath` and `IFolders` types, and settle whether a file path is a `FolderPath`, an existing value, or a new `FilePath` (record the answer in `design.md`); if new, write `FilePathShould` first (From, TryParse, Parent) then the value, as one red-green-refactor cycle
- [ ] 1.2 Red: `ConfigRowsShould` expects a `Setup(SetupItem.DiagnosticLogFile, ...)` row after Settings file; green: add `SetupItem.DiagnosticLogFile`, the state case (path and exists) and its row in `ConfigRows`; refactor
- [ ] 1.3 Red: `SetupFactsReaderShould` reads the log file's path with `exists` true and false from a fake `IFolders` and `Unreadable` when the read throws; green: read it in `SetupFactsReader` off the UI thread
- [ ] 1.4 Red: `ConfigRowsShould` and `ConfigViewShould` expect `ConfigRow.DiagnosticLog` to carry no file and the settings count to stay the same with the title's row count one higher; green: drop `File` from `DiagnosticLog` and the `diagnosticLogFile` parameter's old use, update callers and `ConsoleComposition` and `SceneHost` wiring if a constructor changed

## 2. Words

- [ ] 2.1 Red: `WordingOfConfigShould` for the row's SETTING (Diagnostic log file), the three VALUE sentences (off, on and not written yet, on) and its tooltip (`f` opens it, `c` copies the path, rolls at 10 MiB); green in `WordingOfConfig`
- [ ] 2.2 Red: the Diagnostic log row's tooltip says it names no path and points at the Diagnostic log file row; green: change `ConfigDescription` for `DiagnosticLog`
- [ ] 2.3 Red: `Wording` facts for the `!` key help and tooltip (recent events even when the log is off, last crash, counts, `cader.log` never attached, where to open it) and the Config `f` line; green in `WordingOfScreens`; `KeyHelpParityShould` still passes

## 3. The `f` and `c` keys

- [ ] 3.1 Red: `ConfigViewShould` expects `f` on Settings file and Diagnostic log file to raise `OpenFileInEditor` with that file, and `f` on Windows Terminal profiles and a root to keep raising `OpenFolderInEditor`; green in `ConfigView` (add the effect record, with its marker if `EffectRoutingShould` needs one)
- [ ] 3.2 Red: `ConfigViewShould` expects `c` on Diagnostic log file to raise `CopyFolderPath` with the log's path, with the file absent and the setting off; green (expected to need no code beyond 1.2)
- [ ] 3.3 Handler, after the adapters it uses and before the screen: red `OpenFileInEditorHandlerShould` (opened, file missing, editor not found, editor not started, each asserting the outcome record) using `FakeFolders`, the fake editor locator and the fake desktop; green `OpenFileInEditorHandler`, extracting the editor lookup shared with `OpenInEditorHandler` if it is more than a few lines; register it in `Application\Effects\Composition.Handlers`
- [ ] 3.4 Outcome words and tone: red `WordingOfOutcomesShould` and `StatusTonesShould` lines for each `OpenFileInEditorOutcome` case (the missing log says it does not exist yet and that Diagnostic log turns it on); green in `WordingOfOutcomes` and `Wording.ToneOf`
- [ ] 3.5 Confirm `EffectDispatcherShould`, `EffectRoutingShould`, `HandlersShould` and `ScreenLoopShould` pass; add any line they ask for

## 4. Documentation, rules and release

- [ ] 4.1 Update `docs\user-guide.md`: the Config section's Diagnostic log and new Diagnostic log file entries, the `f` description, and Report a problem (what it holds, that `cader.log` is not attached, how to open it), using the sentences from 2.3
- [ ] 4.2 Update `.claude\rules\console-screen.md` so the Config tab's `DiagnosticLog` no longer carries the file and the new Setup case is listed
- [ ] 4.3 Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` (minor while pre-1.0) and add the changelog line the repository keeps
- [ ] 4.4 Run `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes` and `openspec validate open-the-diagnostic-log`; manually check with `tools\install-local.ps1`, with the log on and off and the file present and absent: path selectable, `f` opens the file in VS Code, `c` copies the path, tooltip has no path (clearing NO_COLOR, CLAUDECODE, CLAUDE_PID and CLAUDE_CODE_* first)
- [ ] 4.5 Archive the change in the same pull request, with a description that opens with Why, Benefit and What changed
