## Why

Nothing is drawn until `claude --version` has answered: the console awaits it before building its first screen (`ConsoleComposition`), and it takes 1.3 to 3.6 s here although it uses about 47 ms of CPU, so most of that is Claude Code waiting on something of its own. Only then does the Sessions tab start its first `claude agents` listing, which takes another 2.7 s. So the first useful screen arrives four to six seconds after `cader` is typed, for a version number that changes only when Claude Code updates itself. And the read is given up after five seconds and never retried, which a cold start exceeded on both of the bug hunt's first runs, so the header then says Claude unknown until the next start.

**Implement with:** Opus 5.5 at medium effort: a small cached value with a state-file record (whose strict-JSON rules `JsonFileStore` enforces), a reordering of the composition's start-up and a header that updates once; the composition is untested wiring, so the logic must move into tested `Application` classes first.

## What Changes

- The console draws its first screen without waiting for Claude Code's version. The version is remembered with the Claude Code executable's size and last write time, and shown at once while they still match; otherwise the header shows the version as being read until `claude --version` answers in the background, however long it takes within the child program time limit, and then shows it; a read that fails shows unknown.
- The first sessions listing starts as soon as Claude Code and git are found, before the screen is built, so it overlaps the start-up work instead of following it.
- The start-up line in the diagnostic log still records the Claude version: the remembered one, the one read if it answers within five seconds, or unknown, so the line is never held back longer than today.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `session-listing`: the header shows the Claude version as being read until it is known, however late it answers, and the console opens without waiting for it.

## Delivery

- **Order:** step 14 of the stack in `openspec\implementation-plan.md`. Depends on nothing in the earlier steps and may be pulled forward; placed here because the scan steps cut more total time. It carries the late version first proposed in `guard-overlapping-actions` (its design 8), since both rework the same start-up code and header.
- **Together:** both parts ship in one pull request; they move the same start-up code.

## Out of scope

- Showing rows from the last run before the first listing (see `show-refresh-progress`, Out of scope).
- `cader here`, which shows no version.
- Multiple Claude accounts beyond naming a session's owner, a later change.

## Impact

- `Application`: a tested `ClaudeVersionMemory` (the executable's stamp and the version) over a new state record, and the start-up order as a tested decision; `Ports\IClaudeVersionRecords` and `Adapters\State\JsonFileStore` for the record (`claude-version.json` in `%LOCALAPPDATA%\Cader`, holding no path).
- `Application\LatestSessions` or `ConsoleLoads`: a first listing that can start before the screen and that the Sessions tab's first load joins.
- `Application\HeaderSettingsRow` and `Wording`: the version as being read, then known.
- `Composition\ConsoleComposition`: starts the listing early and no longer awaits the version before the screen.
- `CHANGELOG.md` and `Version` in `src\Source191.Cader\Source191.Cader.csproj`.
- **Implement with:** Opus 5.5 at medium effort (reason above).
