# Tasks

**Implement with:** Opus 5.5 at medium effort: a small cached value with a state-file record (whose strict-JSON rules `JsonFileStore` enforces), a reordering of the composition's start-up and a header that updates once; the composition is untested wiring, so the logic must move into tested `Application` classes first.

Each task is red, green, refactor, with a commit per step. State facts use `InMemoryState`; adapter facts a temporary folder.

## 1. Remembered version

- [ ] 1.1 `JsonFileStoreShould.RoundTripTheClaudeVersionRecord` and `ReadAVersionRecordMissingAValueAsCorrupt`: the record (length, last write time, version), `Ports\IClaudeVersionRecords`, and `InMemoryState` answering it. Commit.
- [ ] 1.2 `ClaudeVersionMemoryShould.AnswerTheRememberedVersionForAnUnchangedExecutable`, `AnswerBeingReadForAChangedOne` and `RememberAVersionOnceRead`, over a fake file stamp and a scripted `IClaudeCli`. Commit.

## 2. Header and start record

- [ ] 2.1 `HeaderSettingsRowShould.ShowTheVersionAsBeingRead` and `ShowTheVersionOnceKnown`, and the words in `Wording` with a `WordingShould` fact. Commit.
- [ ] 2.2 `ClaudeVersionWatchShould.AnswerBeingReadUntilTheVersionArrives`, `AnswerTheVersionOnceItArrives` (after more than five seconds), `AnswerUnknownWhenTheReadFails`, `JudgeItAgainstTheMinimum` and `WaitAtMostTheStartUpTime` (`FakeTimeProvider`): `Application\ClaudeVersionWatch` (design 2). Commit.
- [ ] 2.3 `StartedShould.RecordTheRememberedVersionAtOnce` and `RecordUnknownAfterFiveSecondsWithoutAnAnswer` (or the decision class the composition uses): `Started` is recorded once, with the remembered version, the version read within five seconds, or unknown. Commit.

## 3. First listing early

- [ ] 3.1 `LatestSessionsShould.ShareAListingStartedFirstWithTheFirstLoad` and `StartNoSecondListingForTheFirstLoad`. Commit.
- [ ] 3.2 `ConsoleComposition` starts the first listing after `LaunchParts`, starts `VersionAsync` with no five-second cancellation, no longer awaits it, and hands the header the `ClaudeVersionWatch`. The owner checks by hand in a scratch build started from a clean environment that the screen appears before `claude --version` returns on a first start and with the version at once on the second (the implementation plan, step 14); the agent verifies with the headless smoke check (`dotnet run -- smoke <exe> <worktree-folder>` from `tools\HeadlessTerminal`, exit 0). Commit.

## 4. Release

- [ ] 4.1 Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` one preview step above the previous step of the implementation plan and add a CHANGELOG `Unreleased` entry under Changed (the console opens without waiting for Claude Code's version, and a slow version still reaches the header; sessions appear sooner after start). Verify `.github\scripts\Assert-VersionBumped.ps1`, `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes` and `openspec validate faster-start`. Commit.

## Workflow follow-up

- Archive the change in the pull request that completes it.
