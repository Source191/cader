# Proposal

## Why

Starting a session from the new-session wizard takes 8 to 13 seconds from F5 to the tab opening, and for all of it the status line says only `Starting <name>...`, so it is hard to tell a slow launch from a stuck one. The diagnostic log of 17 to 20 November 2026 (24 launches, one machine, 55 repositories) shows where the time goes:

| Step | Command | Typical time |
|---|---|---|
| Read origin's default branch | `git symbolic-ref` | 0.15 s |
| Fetch it | `git fetch` | 0.75 to 1.7 s |
| Create the worktree | `git worktree add` | 1.2 to 2.3 s |
| Start the session | `claude --bg` | 3.7 s median on 19 November, 6 to 7 s on 20 November |
| Read the new session's full id | `claude agents --json --all` | 1.5 s |
| Open its tab | `wt new-tab` | 0.8 s |

Every step needs the one before it: the worktree branches from the fetched base, `claude --bg` runs in the new folder, the tab's registration needs the full session id (which `claude --bg` does not print), and the tab needs its registration. So the chain cannot run in parallel. Two things around it can be improved:

- **The launch competes with Cader's own refreshes.** `git worktree add` writes a worktree record under `.git\worktrees`, which the folder watch counts as a change, so a full worktrees scan (10 to 13 s, up to eight git commands at once) starts mid-launch, followed by its pull-request lookups (`az repos pr list`); the session's new record starts a `claude agents` listing too, and the Sessions timer fires every 5 seconds. On 20 November every launch overlapped a scan, and `claude --bg` took 6 to 7 s against 3.7 s the day before.
- **The fetch waits for F5.** The repository is usually chosen seconds before the name is typed, and the fetch could run in that time.

## Benefit

- **Users:** the status line says which step is running and how many there are (`Starting fix-123: starting Claude Code (3 of 5)...`), so a slow step is visible as itself and a launch never looks stuck; a launch no longer competes with Cader's own scans, which should take seconds off the slowest step; and the fetch is usually finished before F5 is pressed.
- **Maintainers:** the launch's steps are one named list in `Application`, held by tests, and the effect's time in `cader.log` shows whether the held refreshes and the prefetch paid off.

## What Changes

- **Launch progress:** the launch reports each step as it begins, and while a session started from the console is being launched the status line reads `Starting <name>: <step> (<n> of <count>)...`, counting only the steps that launch runs: fetching or pulling origin's default branch, creating the worktree, starting Claude Code, finding the new session and opening its tab.
- **Refreshes wait for the launch:** while a session started from the console is being launched, the Sessions and Worktrees tabs start no refresh of their own, whether on a timer or for a change on disc; a change seen meanwhile is kept, and one refresh of each runs when the launch ends, as the reload after an action already does. F5 and showing a tab still refresh at once.
- **Prefetch in the wizard:** once a repository is chosen in the wizard (Enter on it, or moving on from the list) and the form says a new worktree from origin's default branch with Fetch origin or Pull default branch, Cader starts that branch's fetch at once. F5 joins that fetch when it is still running or finished within the last minute, and fetches again otherwise; Pull's fast-forward still runs only after F5.
- **Version and changelog:** a preview step and a Changed entry.

Out of scope:

- **Making `claude --bg` itself faster:** it is Claude Code's own start of its supervisor.
- **Opening the tab before the session is listed:** the tab's registration is owned by the full session id, which only the listing gives; changing tab ownership to a job id is a larger change.
- **Progress for `cader here`:** it has no status line, and its launch makes no fetch and no worktree.
- **A percentage:** the steps are too uneven (0.15 s to 7 s) for a percentage to mean anything.
- **Prefetching every repository the cursor passes:** arrowing down the list would start a fetch per repository.
- Multiple Claude accounts beyond naming a session's owner, which is a later change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `session-launch`: the status line names the launch's current step; refreshes wait while a session started from the console is launched; the base may be fetched while the wizard is still open, and the rule that nothing is fetched outside a new worktree from origin allows that fetch.
- `session-listing`: the listing that follows a change on disc waits while a session started from the console is launched.
- `worktree-management`: the scan that follows a change on disc waits while a session started from the console is launched.

## Impact

- **Implement with:** Opus 5.5 at high effort: a fetch shared between the wizard and the launch across threads, a hold on the refresh schedules that must always be released, and progress reported from a pool thread to the UI thread.
- **`src` (Core):** `Application\Launch\LaunchService`, `BaseUpdater`, new `LaunchStep`, `LaunchSteps`, `LaunchProgress` and `BaseFetches`; `Application\RefreshSchedule` and a new `Application\LaunchHold`; `Application\Actions\StartSessionHandler`; `Application\Views\WizardView`, `WizardState` and `WizardEffect`; `Application\Views\StatusMessage`; `Application\Wording` and its tests; `Application\Effects\Composition`.
- **`src` (Tui and wiring):** `Tui\ConsoleScreen` (the status line asks `LaunchProgress` for its words and redraws on a step), `Tui\LaunchWizardDialog` (starts the prefetch); `Composition\LaunchParts` and `ConsoleComposition` construct the new classes once; `tools\ReadmeScene\SceneHost` copies the composition change.
- **Tests:** `LaunchServiceShould`, `BaseUpdaterShould`, `StartSessionHandlerShould`, `RefreshScheduleShould`, `WizardViewShould`, `LaunchProgressShould`, `BaseFetchesShould`, `LaunchHoldShould`, the `Wording` facts and `StatusTonesShould`. No new fake: the tests run the real `GitCli` and `ClaudeCli` over `FakeProcessRunner`, whose `WhileRunning` already holds a run open.
- **Version and changelog:** `Version` rises one preview step; `CHANGELOG.md` gets a Changed entry.
- **Docs:** `docs\user-guide.md`'s new-session section names the status line's steps and the early fetch; `.claude\rules\console-screen.md` records the hold on the refresh schedules.
