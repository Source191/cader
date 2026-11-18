# Tasks

**Implement with:** Claude Opus 5.5 (`claude-opus-5-5`) at **high** effort, in one session, one task at a time. Each task is red, green, refactor with a commit per step (CLAUDE.md). Start only once `configurable-refresh` is merged and archived; first run `/opsx:update refresh-on-change` if its archive changed anything this change's design relies on (`RefreshSchedule`, the single ticker).

## 1. Record the facts

- [x] 1.1 Add the files, their fields and when they are written (design, Context) to `.claude\rules\claude-code.md`, dated 18 November 2026 with CLI 2.1.327, and reword `openspec\config.yaml`'s context line to allow watching the `sessions` and `jobs` folders for change notifications while still forbidding reading them. Verify `openspec validate --all` passes.

## 2. What counts and what is watched

- [x] 2.1 Red then green: `ChangeFilterShould` (Core.Tests, Application) states, for each kind of watch, the counted and ignored paths of design decision 2, including `jobs\aba4b38e\state.json.tmp.4f0d6ab5` ignored and renamed to `state.json` counted, `worktrees\feature\index.lock` and `index` ignored, `worktrees\feature` deleted counted, and `Overflowed` always counted; add `Ports\FolderChange`, `FolderChangeKind` and `Application\ChangeFilter`.
- [x] 2.2 Red then green: `WatchPlanShould` states the plan for two roots, three repositories and a Claude folder (the two Claude folders for Sessions, each repository's git folder and each root for Worktrees, with their filters), and that a Claude folder left unset plans no Claude watch; add `Application\WatchPlan` (decision 4).

## 3. The schedule hears changes

- [x] 3.1 Red then green: `RefreshScheduleShould` (`FakeTimeProvider`) states a pending change is due 500 ms after the first change and at least one second after the last start, is due while the interval is `Off`, is cleared by `Started()`, is set again by a change after `Started()`, and that changes every 200 ms for a minute make it due no more than once a second; add `Changed()` behind a `Lock` (decision 3).

## 4. The adapter

- [x] 4.1 Add `Ports\IFolderChanges` and its hand-written fake in `TestDoubles` (which raises changes on demand).
- [x] 4.2 Red then green: `FolderChangesShould` (Infrastructure.Tests, a temporary folder as the root) states a created file and a created subfolder are reported with their relative paths, a missing folder answers a watch that reports nothing and throws nothing, and nothing is reported after the watch is disposed; add `Infrastructure\Files\FolderChanges` over `FileSystemWatcher` (decision 1).
- [x] 4.3 Red then green: with a real repository in a temporary folder, `FolderChangesShould` states `git worktree add` and `remove` produce a change `ChangeFilter` counts, and that `git status`, `git fetch` from a local remote and `git commit` produce none it counts.
- [x] 4.4 Red then green: `HandleCounts` gains a fact that starting and disposing a hundred watches leaves the process's handle count where it began.

## 5. Keeping watches in step

- [x] 5.1 Red then green: `ChangeWatchesShould` (with the fake) states `Follow` starts a watch per planned folder, disposes one no longer planned, leaves an unchanged one alone, and routes a counted change to the schedule it feeds and an ignored one nowhere; add `Application\ChangeWatches`.
- [x] 5.2 Red then green: `DiagnosticEventsShould` and `DiagnosticsPrivacyShould` cover a new `DiagnosticEvent.WatchTrouble` (watch kind and trouble, no path), and `ChangeWatchesShould` states a watch that throws as it starts, or overflows, records it; give it its line in `DiagnosticLine` (decision 5).

## 6. The screen

- [x] 6.1 Wiring: `ConsoleComposition` (and `tools\ReadmeScene\SceneHost`, with a fake that reports nothing) builds `FolderChanges`, `ChangeWatches` and the plan; `ChangeWatches.Follow` runs as the console opens and after each scan returns; the ticker starts a hidden Worktrees tab's scan when its schedule is due for a change. Untested wiring under the Tui rule; verify with `dotnet build -c Debug` and the full suite.
- [ ] 6.2 Manual, in a local install (`tools\install-local.ps1`, Claude Code variables cleared), with both intervals set to 0: on the Sessions tab, `git worktree add` in a shell, wait three seconds and show the Worktrees tab (listed at once); `git worktree remove` with the tab shown (row goes within about two seconds); start `claude` in another terminal (row appears); prompt a background session to ask for permission while the Worktrees tab is shown (bell rings); run `git fetch` and `git status` in a worktree and see no scan in the diagnostic log's counts. Record what was seen, the date and the build here.

## 7. Documents and release

- [x] 7.1 `docs\user-guide.md` says the tabs refresh when sessions or worktrees change and that the timers are a safety net; `.claude\rules\console-screen.md`'s Refreshing section names `ChangeFilter`, `WatchPlan`, `ChangeWatches` and `RefreshSchedule.Changed`. Verify by reading them against the specs.
- [ ] 7.2 Raise the minor `Version` with `.github\scripts\Step-Version.ps1` and add an Added entry under Unreleased in `CHANGELOG.md`. Verify `dotnet build -c Debug`, `dotnet test -c Debug --no-build` at the solution and `dotnet format --verify-no-changes` pass.

## Workflow follow-up

- Archive the change in the same pull request (`/opsx:archive`) and verify that `openspec validate` passes.
- Open the pull request with Why, Benefit and What changed.
