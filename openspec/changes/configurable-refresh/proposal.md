# Proposal

## Why

How often the console refreshes is fixed in the code: the Sessions tab lists sessions every 5 seconds and the Worktrees tab rescans every 15 seconds while it is shown. Someone with many repositories, a slow disc or a busy machine cannot slow the scans down, and someone waiting on a session cannot speed the listing up. There is also no way to ask for a refresh now: after work done outside Cader (a `git worktree add` in a shell, a session started in another terminal) the only options are to wait for the timer or to switch tabs and back.

## What Changes

- Two new settings, **Sessions refresh** and **Worktrees refresh**, each a whole number of seconds from 0 to 3600, kept in `config.json` as `sessionsRefreshSeconds` (default 5) and `worktreesRefreshSeconds` (default 15). 0 turns that tab's timer off; the tab still refreshes when it is shown, after an action and on F5.
- The Config tab shows both as rows after Default place, each edited in a one-value dialog like Worktree root, checked before it is saved, and applied at once without a restart.
- **F5** refreshes the shown tab now: Sessions lists the sessions, Worktrees rescans and Config rereads the setup. Like the timer, it starts nothing while that tab's refresh is already running. The help lists it on every tab; the key bar does not, as it is full.
- F5 keeps its meaning inside the new-session wizard (Start) and the settings dialogs (Save): a dialog takes every key while it is open, so the two never meet.

Out of scope:
- **Refreshing when something changes on disc:** the `refresh-on-change` change, applied after this one.
- **A refresh interval for the Config tab:** it has no timer and keeps none.
- **Sub-second intervals, or one interval for both tabs.**
- **Multiple Claude accounts:** a later change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `settings`: the settings file holds the two intervals with their defaults and rules, the interval dialog checks what was typed, and a saved interval applies at once.
- `config-tab`: the two new rows, their dialog and descriptions, and the counts that grow by two.
- `session-listing`: the list refreshes at the configured interval and on F5.
- `worktree-management`: the worktrees rescan at the configured interval while shown and on F5.
- `console-layout`: the help names F5 on every tab.

## Impact

- **Implement with:** Claude Opus 5.5 (`claude-opus-5-5`) at **medium** effort. The work is mostly settings plumbing through every layer, and the build's convention tests catch most slips; the one decision with any subtlety (a timer that follows a saved setting) is settled in the design.
- **Core:** `Domain\Settings` and `RefreshInterval` (new); `Ports\UserConfig`; `Application\SettingsFile`, `ConfigRows`, `SettingsView` and `SettingsField`, a shared F5 key (`RefreshKeys`) with a `RefreshShownTab` effect, `Wording` and its facts; a new `KeyName.F5`.
- **Tui:** `KeyMapping` maps F5; `ConsoleScreen` drives each tab's timer from the current settings and routes `RefreshShownTab`; the screen definitions stop holding fixed intervals.
- **Tests:** the reducers', settings file's, wording and routing facts; `KeyHelpParityShould` presses F5.
- **Docs:** `docs\user-guide.md` (the settings and the key), `.claude\rules\console-screen.md` (Refreshing).
- **Version:** `src\Source191.Cader\Source191.Cader.csproj` raises its minor `Version`; `CHANGELOG.md` gets an Added entry.
- **Users:** the refresh rates suit their machine, and F5 shows the latest state at once.
