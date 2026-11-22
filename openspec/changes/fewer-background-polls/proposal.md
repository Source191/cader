## Why

Cader polls Claude Code more often than it needs to now that it hears changes on disc. In an hour of real use it ran `claude agents --json --all` 856 times (median 2.7 s and about 0.6 s of CPU each, so roughly 8 CPU-minutes an hour, whichever tab was shown) and `claude auth status --json` 57 times (2.6 s each). Since refresh-on-change (0.22.0-preview.49) a session's status change, a new session and a job's state reach the console within about two seconds through the `sessions` and `jobs` folder watches, so the five-second timer is only a safety net; and the login it checks every minute changes perhaps once a week.

**Implement with:** Sonnet 5.5 at medium effort: a changed default and two time windows in `AccountCatalogue`, each covered by existing sociable tests with `FakeTimeProvider`; most of the work is the spec scenarios that quote the old default.

## What Changes

- The Sessions refresh default becomes 30 seconds (from 5). A saved `sessionsRefreshSeconds` is kept as it is, so anyone who chose 5 keeps 5. Status changes, new sessions and job states still arrive within about two seconds through the folder watches.
- `claude auth status --json` runs once as the console opens and then no more than once every 15 minutes, except: while a listed session belongs to an organisation Cader does not remember, or after a run that failed, it may run once a minute as today, so a new login is still picked up as soon as one of its sessions is listed.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `settings`: the Sessions refresh default is 30 seconds.
- `config-tab`: the default rows read Sessions refresh every 30 seconds.
- `session-listing`: the list refreshes every 30 seconds by default.
- `session-accounts`: the current login is read every 15 minutes, or every minute while a listed session's organisation is unknown or the last read failed.

## Delivery

- **Order:** step 6 of the stack in #379. Depends on nothing; independent of `lighter-git-reads`.
- **Together:** both parts ship in one pull request, since both cut Claude Code polls and share the release note.

## Out of scope

- Changing the Worktrees refresh default (15 seconds) or the folder watches.
- Pull-request lookups (`pull-requests-per-repository`).
- Multiple Claude accounts beyond naming a session's owner, a later change.

## Impact

- `Domain\Settings.SessionsRefreshWhenUnset` (30).
- `Application\AccountCatalogue.RefreshStaleAsync` takes whether any listed owner is unseen; `SessionRowEnricher` passes it.
- Spec scenarios that quote the default; `README.md` and `docs\user-guide.md` where they quote 5 seconds.
- `CHANGELOG.md` and `Version` in `src\Source191.Cader\Source191.Cader.csproj`.
- **Implement with:** Sonnet 5.5 at medium effort (reason above).
