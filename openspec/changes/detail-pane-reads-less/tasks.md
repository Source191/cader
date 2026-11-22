# Tasks

**Implement with:** Opus 5.5 at medium effort: a short UI-thread delay through Terminal.Gui's timers in `DetailReader` and independent reads started together in `DetailLoader`, both inside the existing request-numbering that drops stale answers; contained, but the timer must respect the tab-shown and dispose rules.

Each task is red, green, refactor, with a commit per step.

## 1. Facts together

- [ ] 1.1 `DetailLoaderShould.ReadAWorktreesIndependentFactsTogether` (a runner holding each read: all start before any is released) and `KeepAFailedFactUnavailableOnItsOwn`. Commit.

## 2. Rest before reading

- [ ] 2.1 `DetailRequestsShould` or a new `DetailDelayShould` (the pure part: a selection change asks after 150 ms, a newer change replaces it, a refresh or a show asks at once) over `FakeTimeProvider`. Commit.
- [ ] 2.2 `DetailReader` applies it with `app.AddTimeout`, removing a pending timeout when the selection moves again, when the tab is hidden and on `Release`. Verify with the headless smoke check (`dotnet run -- smoke <exe> <worktree-folder>` from `tools\HeadlessTerminal`, exit 0) and, with the diagnostic log on in a scratch build, that holding Down across ten rows records git runs for the last row only. Commit.

## 3. Release

- [ ] 3.1 `.claude\rules\console-screen.md`, Detail pane. Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` one preview step above what `main` holds when this merges and add a CHANGELOG `Unreleased` entry under Changed (the detail pane fills faster and reads nothing for rows the selection only passes). Verify `.github\scripts\Assert-VersionBumped.ps1`, `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes` and `openspec validate detail-pane-reads-less`. Commit.

## Workflow follow-up

- Archive the change in the pull request that completes it.
