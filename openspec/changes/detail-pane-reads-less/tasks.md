# Tasks

**Implement with:** Opus 5.5 at medium effort: a short UI-thread delay through Terminal.Gui's timers in `DetailReader`, a one-read-at-a-time rule in `DetailRequests`, and independent reads started together in `DetailLoader`, all inside the existing request-numbering that drops stale answers; contained, but the timer must respect the tab-shown and dispose rules.

Each task is red, green, refactor, with a commit per step.

## 1. Facts together

- [ ] 1.1 `DetailLoaderShould.ReadAWorktreesIndependentFactsTogether` (a runner holding each read: all start before any is released) and `KeepAFailedFactUnavailableOnItsOwn`. Commit.

## 2. Rest before reading, one read at a time

- [ ] 2.1 `DetailRequestsShould` or a new `DetailDelayShould` (the pure part: a selection change asks after 150 ms, a newer change replaces it, a refresh or a show asks at once) over `FakeTimeProvider`. Commit.
- [ ] 2.2 `DetailRequestsShould.StartTheFirstReadAtOnce`, `HoldARequestWhileAReadRuns`, `StartOnlyTheLatestHeldRequestWhenTheReadEnds` and `StartNothingWhenTheSelectionClearedMeanwhile`: the one-at-a-time rule in `DetailRequests`. Commit.
- [ ] 2.3 `DetailReader` applies both: the delay with `app.AddTimeout`, removing a pending timeout when the selection moves again, when the tab is hidden and on `Release`; `Reading` holds a request while a read runs and starts the latest one `DetailRequests` answers when it ends. Verify with the headless smoke check (`dotnet run -- smoke <exe> <worktree-folder>` from `tools\HeadlessTerminal`, exit 0) and, holding Down for two seconds on the Worktrees tab with `Get-Process git` sampled during the run, that no more than one row's git reads (plus a scan's) run at once. Commit.

## 3. Release

- [ ] 3.1 `.claude\rules\console-screen.md`, Detail pane: the 150 ms rest and one read at a time per tab. Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` one preview step above the previous step of #379 and add a CHANGELOG `Unreleased` entry under Changed (the detail pane fills faster, reads nothing for rows the selection only passes, and holding an arrow key no longer floods git). Verify `.github\scripts\Assert-VersionBumped.ps1`, `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes` and `openspec validate detail-pane-reads-less`. Commit.

## Workflow follow-up

- Archive the change in the pull request that completes it.
