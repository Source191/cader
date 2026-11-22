## Why

A session started from the wizard took 7 to 11.5 s from F5 to its tab in the measured hour. Part of that cannot be helped (creating the worktree, starting Claude Code), but about 2.7 s is a full `claude agents --json --all` listing run after `claude --bg` has already answered with the job id, only to learn the full session id that the tab's registration is keyed by. The tab itself needs only the job id (`claude attach <job id>`), the name, the folder and a colour, all known by then. When the listing does not yet hold the new job, no tab opens at all and the user is told to press Enter on the row later.

**Implement with:** Opus 5.5 at high effort: it changes how tab registrations are owned, which the security rules lean on (a process is ended only as a `ConfirmedProcess`, and stop, rename and delete close a session's tabs through its registrations), so the new owner kind and its hand-over to the session id must be reasoned about for every path that finds a session's tabs.

## What Changes

- A session started from the wizard opens in its own tab as soon as `claude --bg` reports its job id, before any listing, and the status line names the tab's colour as today.
- The tab's registration is first owned by the job and is handed to the session once a listing names the session id; until then every key that finds a session's tabs (Enter, stop, rename, delete, the TAB block) finds it by the job id.
- A launch no longer runs a listing of its own; the refresh that follows the launch (the launch hold's release) lists the new session.
- "Started, press Enter on it once it appears" goes: the tab opens whether or not the supervisor lists the job yet, so the requirement that a started session is never reported as failed keeps only its recent-repository part.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `session-launch`: a session started from the wizard opens in its tab as soon as its job id is known, listed or not.

## Delivery

- **Order:** step 15, the last, of the stack in #379. It is late because it is the riskiest of the user-facing changes and saves about 3 s per launch, not per refresh. It builds on `report-what-removals-and-launches-did` (step 1), whose catch around the launch's listing it removes with the listing, and on `guard-overlapping-actions` (step 3), whose registry re-read in `OpenTabHandler` must find a job-owned registration too.
- **Together:** ships alone, in one pull request, with the manual checks `CONTRIBUTING.md` asks of a pull request that touches tabs and attach, and the hand-off check.

## Out of scope

- Making `git worktree add` or `claude --bg` themselves faster (a pool of ready worktrees was considered and is far more machinery than this).
- Multiple Claude accounts beyond naming a session's owner, a later change.

## Impact

- `Domain\TabOwner`: a `ForJob(JobId)` case; `TabRegistrations` finds a background session's tabs by its session id or its job id.
- `Application\Launch\LaunchService`: no listing after the start, and so no catch around it; `LaunchOutcome.Listed` and `NotListedYet` give way to one started outcome carrying the job id; `LaunchSteps` loses its "finding the session" step.
- `Application\Actions\StartSessionHandler` and `OpenTabHandler`: open the tab from the plan and the job id; `TabScripts.Attach` takes what it needs without a listed session.
- `Application\SessionRowEnricher` (live tabs) and the handlers that close a session's tabs (`SessionTabs`): find job-owned registrations; the first listing that names the session re-owns the registration.
- `.claude\rules\security.md` (registrations and their owners) and `.claude\rules\claude-code.md` (the job id is the first 8 characters of the session id, already recorded).
- `CHANGELOG.md` and `Version` in `src\Source191.Cader\Source191.Cader.csproj`.
- **Implement with:** Opus 5.5 at high effort (reason above).
