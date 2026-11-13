# Spec Delta

## ADDED Requirements

### Requirement: The refresh and cache delays are settings
Cader SHALL keep four delays in `config.json` as whole seconds: `sessionsRefreshSeconds` (the time between timed Sessions listings), `worktreesRefreshSeconds` (between worktree rescans while that tab is shown), `gitCacheSeconds` (how long a session folder's branch and place are reused) and `pullRequestCacheSeconds` (how long a branch's pull request answer is reused). A missing property, a value that is not a whole number, or one outside 1 to 600 SHALL read as the default: 5, 15, 30 and 60. A saved settings file SHALL name all four. A save SHALL apply to the caches at their next use, and to the two refresh timers at their next tick, without a restart.

#### Scenario: No delays in the file
- **WHEN** `config.json` holds none of the four properties
- **THEN** the Sessions list refreshes every 5 seconds, the worktrees every 15, a folder's branch is reused for 30 and a pull request answer for 60

#### Scenario: A shorter sessions refresh by hand
- **WHEN** `config.json` holds `sessionsRefreshSeconds` of 2 and Cader starts
- **THEN** the Sessions list refreshes every 2 seconds

#### Scenario: A value outside the range
- **WHEN** `config.json` holds `sessionsRefreshSeconds` of 0, `worktreesRefreshSeconds` of 5000 and `gitCacheSeconds` of "soon"
- **THEN** Cader starts, those three read as 5, 15 and 30, and nothing exits

#### Scenario: A change applies on the next tick
- **WHEN** the settings are saved with `sessionsRefreshSeconds` of 10 while the Sessions list refreshes every 5 seconds
- **THEN** the refresh already due runs, and the ones after it come every 10 seconds, with no restart
