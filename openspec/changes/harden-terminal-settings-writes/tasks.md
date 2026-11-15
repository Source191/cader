# Tasks

Every fact runs over a temporary folder or the fakes, never Windows Terminal's real files.

## 1. The result of a write

- [x] 1.1 **Refactor:** `ITerminalSettings.AddProfileEntriesAsync` answers a `Ports\TerminalSettingsEdit` (`Written`, `IsALink`); `SettingsFileIsALinkException` becomes the `IsALink` case; `TerminalSettings` takes a `TimeProvider` (wiring in `SharedParts` and `SceneHost`); the runner switches over the result. **Verify:** every existing fact passes with the same words.

## 2. Backup (gap 2)

- [x] 2.1 **Red:** `TerminalSettingsShould.KeepABackupNamedWithTheUtcTime` and `KeepTheFilesCreationTimeAndPermissions` (the `File.Replace` fact); `TerminalSetupRunnerShould` and `WordingShould` facts that the shown sentence names the backup. **Green:** `File.Replace` with the backup; `Written(Backup)`; `Wording.HiddenProfilesShown(path, backup)`. **Verify:** the facts pass; record the fact in `windows.md`.

## 3. Race with Terminal (gap 1)

- [x] 3.1 **Red:** `TerminalSettingsShould.WriteNothingWhenTheFileChangesWhileItIsWritten`, with a clock that rewrites the file when read; runner and `Wording` facts for `ChangedMeanwhile`. **Green:** read the file again before replacing it. **Verify:** the facts pass.

## 4. Verify the output (gap 3)

- [x] 4.1 **Red:** `TerminalSettingsShould.WriteNothingWhenTheNewFileWouldNotHoldTheListExpected` (a file naming `profiles` twice); runner and `Wording` facts for `NotVerified`. **Green:** parse the new bytes and compare the list with `JsonElement.DeepEquals`. **Verify:** the facts pass.

## 5. Read-only file (gap 4)

- [x] 5.1 **Red:** `TerminalSettingsShould.RefuseAReadOnlyFileAndLeaveItAlone` answers `IsReadOnly` (it threw before); runner and `Wording` facts give the entries to add by hand. **Green:** check the attribute before reading. **Verify:** the facts pass.

## 6. `--remove` takes out what setup added (gap 5)

- [x] 6.1 **Red:** `TerminalSettingsShould` facts that `ReadAsync` lists in `Added` only the entries exactly as setup adds them (any letter case of the guid; not one with a fifth property, `"hidden": true`, another name or another source). **Green:** `TerminalProfileRecords.Read.Added`. **Verify:** the facts pass.
- [x] 6.2 **Red:** `TerminalSettingsShould` facts for `RemoveProfileEntriesAsync`: add then remove gives back the original bytes for every add shape; the first, a middle, the last and the only element, a trailing comma, a comma in a comment, a customised entry kept, nothing to remove answers `Unchanged`; a link, a read-only file, a change meanwhile and a failed check write nothing. **Green:** the removal through the guarded write. **Verify:** the facts pass.
- [x] 6.3 **Red:** `TerminalSetupRunnerShould` facts for `--remove`: shows the entries and asks, `--yes` answers, declined writes nothing, nothing to remove only with neither fragment nor entry, each refusal exits 1 with its words, unreadable files reported. `WordingShould` facts for the plan, question, done sentence and by-hand words; `CommandLineShould` for the `--remove` description. **Green:** the runner, `Wording`, `CommandLine`. **Verify:** the facts pass.

## 7. Docs and release

- [ ] 7.1 Update `.claude\rules\security.md` (the `settings.json` bullet), `.claude\rules\windows.md` (the `File.Replace` facts, dated) and `docs\user-guide.md` (the backup, the refusals, what `--remove` takes out). **Verify:** read through.
- [ ] 7.2 `Step-Version.ps1` once and a `CHANGELOG.md` line. **Verify:** `Assert-VersionBumped.ps1` and `Assert-ChangelogUpdated.ps1` against `stack-17-winget-banner`.
- [ ] 7.3 **Owner, after merge:** with Windows Terminal open, run `cader setup-terminal` against a hidden profile and confirm Terminal shows it again after the `File.Replace` write with no restart, and that `--remove` takes the entry out; look at the backup beside `settings.json`.
