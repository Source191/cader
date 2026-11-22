# Tasks

**Implement with:** Opus 5.5 at high effort: it changes how tab registrations are owned, which the security rules lean on (a process is ended only as a `ConfirmedProcess`, and stop, rename and delete close a session's tabs through its registrations), so the new owner kind and its hand-over to the session id must be reasoned about for every path that finds a session's tabs.

Each task is red, green, refactor, with a commit per step. Tab facts use `InMemoryState`, `FakeProcessRegistry` and `FakeWindowsTerminal`.

## 1. Domain

- [ ] 1.1 `TabRegistrationsShould.FindABackgroundSessionsTabByItsJobId` and `FindNoInteractiveSessionsTabByAJobId`: `TabOwner.ForJob`; fix every switch the closed hierarchy now fails (`ClosedHierarchiesShould`). Commit.
- [ ] 1.2 `TabRegistrationsShould.ReOwnAJobsTabToItsSession` (same id, colour and claim). Commit.

## 2. Launch

- [ ] 2.1 `LaunchServiceShould.AnswerStartedWithoutListing` (the scripted CLI sees no `agents` call after `--bg`) and `LaunchStepsShould.HaveNoFindingStep`. Commit.
- [ ] 2.2 Check in the headless terminal, with a throwaway session in a scratch linked worktree under `%TEMP%`, whether `claude attach <job id>` run straight after `claude --bg` answers attaches before the supervisor lists the job; record the answer with the date and CLI version in `.claude\rules\claude-code.md`, then `claude rm` the session. If it refuses, add a short retry to `TabScripts.Attach` with `TabScriptsShould.RetryAnAttachTheSupervisorRefusesAtFirst`. Commit.
- [ ] 2.3 `StartSessionHandlerShould.OpenTheTabFromTheJobId` (a job-owned pending registration, `claude attach <job id>` in the script) and `OpenTheTabWhenTheJobIsNotListedYet`; `OpenTabHandlerShould` keeps its listed-row facts. Wording for the removed "press Enter once it appears" outcome goes with its fact. Commit.

## 3. Finding and re-owning

- [ ] 3.1 `SessionRowEnricherShould.ShowAJobOwnedTabsColour` and `ReOwnAJobOwnedTabOnceListed`. Commit.
- [ ] 3.2 `SessionTabsShould.CloseAJobOwnedTab` (stop, rename, delete) and `StopHandlerShould.CloseTheTabOfASessionStartedMomentsAgo`; run the re-own and a close in both orders. Commit.
- [ ] 3.3 `.claude\rules\security.md`, Processes and identities: who may own a registration and that the owner never confirms a shell. Commit.

## 4. Checks and release

- [ ] 4.1 The hand-off check (`dotnet build src\Source191.Cader -c Release`, then `dotnet run -- check` from `tools\HeadlessTerminal`, exit 0) and the manual checks in `CONTRIBUTING.md` for tabs and attach in a real Windows Terminal: start from the wizard, see the tab open before the row is listed, then Enter, x and d on the new row. Record the results in the pull request. Commit nothing for this task unless a fact was added.
- [ ] 4.2 Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` one preview step above what `main` holds when this merges and add a CHANGELOG `Unreleased` entry under Changed (a new session's tab opens about three seconds sooner, and opens even before the session is listed). Verify `.github\scripts\Assert-VersionBumped.ps1`, `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes` and `openspec validate open-new-session-tab-sooner`. Commit.

## Workflow follow-up

- Archive the change in the pull request that completes it.
