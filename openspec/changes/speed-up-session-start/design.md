# Design

## Context

F5 in the wizard dispatches `StartSession`; `TableBody.Dispatch` sets `StatusMessage.Working(effect)` (words `Wording.Working`: `Starting <name>...`), starts the spinner and runs `StartSessionHandler` on a pool thread. The handler calls `LaunchService.LaunchAsync` (read origin's default branch, `BaseUpdater` fetch or pull, `git worktree add`, record, copy includes, `claude --bg`, record the repository as recent, `claude agents` for the full session id), then `OpenTabHandler.OpenAsync` (`wt new-tab`). Nothing reports from inside the handler until its outcome settles the status line.

Meanwhile `ConsoleScreen`'s ticker asks each tab's `RefreshSchedule.IsDue()` (and `IsDueForChange()` for a hidden Worktrees tab) once per heartbeat. `git worktree add` writes `.git\worktrees\<name>`, which `ChangeFilter` counts, and the session's start writes its `sessions\<pid>.json`, so both schedules go due mid-launch; the Sessions timer (5 s) fires as well. A worktrees scan then runs up to eight git commands at once across every repository, and the pull-request lookups it starts run `az` four at a time.

`LaunchService` is built once in `Composition\LaunchParts` and shared by the console and `cader here`.

## Goals / Non-Goals

**Goals:**

- Show the step a console launch is on, and how many steps it has, without the handler touching the screen.
- Keep Cader's own refreshes off the machine while a console launch runs, and catch up once afterwards.
- Overlap the fetch with the time the user spends filling in the wizard.

**Non-Goals:**

- Faster `claude --bg`, `git worktree add` or `wt new-tab`; opening the tab before the listing; progress for `cader here`; a percentage (proposal, Out of scope).
- Cancelling a refresh already running when a launch starts: it runs to its end.

## Decisions

### 1. The steps are a value built once per launch

`Application\Launch\LaunchStage` is a closed enum: `Fetching`, `Pulling`, `CreatingWorktree`, `StartingClaude`, `FindingSession`, `OpeningTab`. `LaunchSteps` is the ordered list of stages one launch runs, built by `LaunchService` as soon as it knows them, which is after `git symbolic-ref` for a new worktree from origin's default branch (no `origin/HEAD` means no fetch step) and at once otherwise:

| Launch | Stages | Count |
|---|---|---|
| New worktree from origin's default branch, Fetch origin | Fetching, CreatingWorktree, StartingClaude, FindingSession, OpeningTab | 5 |
| The same with Pull default branch | Pulling, then as above | 5 |
| New worktree with base HEAD, Before branching Nothing, or no `origin/HEAD` | CreatingWorktree, StartingClaude, FindingSession, OpeningTab | 4 |
| Current branch, folder choice, existing worktree | StartingClaude, FindingSession, OpeningTab | 3 |

`LaunchSteps.Step(stage)` answers a `LaunchStep(SessionName Name, LaunchStage Stage, int Number, int Count)`. Copying includes and recording the worktree stay inside CreatingWorktree (milliseconds each), and recording the repository as recent stays inside FindingSession.

`LaunchOutcome.Listed` carries its `LaunchSteps`, so `StartSessionHandler` reports `OpeningTab` with the same count. A launch that ends early (not trusted, failed, not listed yet) never reports a later step; its outcome replaces the words as today.

*Alternative:* counting the steps in the screen from the plan. Rejected: only the service knows whether `origin/HEAD` exists, and the count must match the steps actually reported.

### 2. Progress goes through one holder, and the screen asks it for words

`Application\Launch\LaunchProgress` implements `IProgress<LaunchStep>` and holds the latest step in a `SharedSnapshot<LaunchStep>` (a `LaunchStep.None` null object to start, never null). It is built once in `LaunchParts` and handed to `LaunchService` and `StartSessionHandler` (the writers) and to `ConsoleScreen` (the reader). `Report` replaces the snapshot and calls a listener the screen registers once through `Follow(Action)`, a no-op until then, not a nullable event.

`LaunchProgress.Shown(StatusMessage message)` answers `StatusMessage.Launching(step)` when `message` is `Working(StartSession start)` and the latest step's `Name` equals `start.Plan.Name`, and `message` otherwise. `ConsoleScreen.ShowStatus` passes every message through it, so a step only ever decorates the launch it belongs to: two launches at once (possible, since a second F5 can follow before the first ends) each show plain `Starting <name>...` unless they are the latest, and a stale step from an earlier launch with another name never shows. The listener calls `app.Invoke` to redraw the status line on the UI thread; the spinner is unaffected because `running` still comes from the tab's in-flight count.

`StatusMessage.Launching` is a new case with its words in `Wording` (`Starting fix-123: starting Claude Code (3 of 5)...`, the stage words in one switch with no discard arm) and its tone in `Wording.ToneOf` (the same tone as `Working`), so the reflection facts and `StatusTonesShould` cover it. `cader here` reports into the same holder with no listener, which is harmless and keeps `LaunchService` with one constructor.

*Alternatives:* an `IProgress` parameter on `IEffectHandler.HandleAsync` (every handler would change for one); BCL `Progress<T>` created on the UI thread (it would need the dispatcher to pass it through, the same signature change); having `TableBody` replace its own message (the Application layer would need to know which tab raised the launch). The holder keeps the handler free of the screen and the screen free of rules.

### 3. A hold on the refresh schedules, taken by the handler

`Application\LaunchHold` counts launches in flight behind a `Lock`; `Hold()` answers a disposable that releases once (a second dispose does nothing). Each `RefreshSchedule` is given the hold and answers false from `IsDue()` and `IsDueForChange()` while it is held. `Changed()` still records a pending change, and `lastStarted` is untouched, so once the hold ends the next tick finds the timer overdue or the change due and starts a refresh for each tab; `Started()` then clears both. The tab that dispatched the launch also reloads after its outcome as it does for every action (`TriggerLoad`, which tells no schedule and is never skipped), so that tab may refresh twice in a row after a launch; that is today's behaviour after any action and is left alone.

`StartSessionHandler` takes the hold in a `using` around its whole body, so a failure or an exception releases it before the outcome settles. F5 (`ITabHost.RefreshShown`) and showing a tab call `ListSessions` and `ScanWorktrees` directly and never ask the schedule, so they still refresh at once. `cader here` never takes the hold: it has no refreshes.

*Alternative:* pausing the folder watches. Rejected: a change would be lost rather than kept, and the timers would still fire.

### 4. The early fetch is a shared task the launch can join

`Application\Launch\BaseFetches` keeps, per repository (`FolderPath`), the latest early fetch: its `RemoteBranch`, when it started (`TimeProvider`) and its `Task<bool>` (true when it succeeded; `GitException` and `ProcessTimedOutException` are caught inside and answer false, so the task never faults and nothing goes unobserved). `Begin(FolderPath repository)` reads origin's default branch and fetches it with the same `IGit` calls the launch uses, on a pool thread with no cancellation (the git time limit still applies), and does nothing when one is already running for that repository.

`BaseUpdater.FetchAsync` asks `BaseFetches.FetchAsync(repository, remoteBranch, cancellationToken)`, which awaits the early fetch when it is for the same remote branch, started less than a minute ago, and is running or succeeded; otherwise it runs `git fetch` itself and lets its exceptions reach `BaseUpdater` as today, so a failed early fetch is followed by the launch's own, whose reason the status line gives. Pull's fast-forward is never started early.

The wizard decides when: `WizardState` gains a `RepositoryConfirmed` flag (set by `ConfirmRepository`, by `NextField` from the list and by `FieldFocused` on any field after the list; cleared by `ListMoved`, `FilterChanged` and `ChoiceSelected`) and the repository it last asked to fetch. After each input, when the flag is set, the choice is a repository, Where is New worktree, the base is origin's default branch, Before branching is Fetch origin or Pull default branch and that repository is not the last one asked, the reducer answers `WizardEffect.Prefetch(FolderPath)` and records it. `LaunchWizardDialog` handles it by calling `BaseFetches.Begin` and then pushes the state as for `None`. A fixed-folder wizard has no Where, so it never asks.

*Alternatives:* fetching when the wizard opens, before anything is chosen (it opens on the most recent repository, which is often not the one wanted), or on every cursor move (a fetch per repository passed). Leaving a finished fetch valid for as long as the wizard is open was rejected: a wizard left open over lunch would branch from a stale base.

### 5. Measuring the result

No new diagnostic event. `cader.log` already records `effect StartSession -> ... in <ms>`, every slow child, and the 10-minute counts per command; after the change, `claude --bg`'s median and the effect's time against the figures in the proposal say whether the hold paid off, and fewer `git fetch` runs than launches would mean a launch joined an early fetch that had already failed. The owner compares them after a day's use and records the result in the pull request that archives the change.

## Risks / Trade-offs

- **The bell waits for a launch** → a session elsewhere that starts waiting during a launch is listed, and its bell rung, up to one launch later (about 10 s) instead of within two seconds. Accepted: it happens only while the user is starting a session at the console, so they are looking at it.
- **A hold that is never released would stop every timed refresh** → the hold is taken only in `StartSessionHandler`'s `using`, and `LaunchHoldShould` and `StartSessionHandlerShould` cover release after success, failure and an exception.
- **The gain from the hold is a hypothesis** → the proposal's figures came from one machine on two days; Decision 5 says how it is checked. If it turns out small, the hold still costs nothing but the bell delay above.
- **An early fetch for a launch that never happens** → it updates only the remote-tracking branch, which any later fetch would; the modified requirement says so.
- **Two launches at once** → both are held under one count, and only the latest one's steps are shown (Decision 2).
- **The early fetch runs beside whatever the wizard and the console read** → it is one git process, held to the git time limit, and it writes only the remote-tracking ref, which no wizard rule reads.
