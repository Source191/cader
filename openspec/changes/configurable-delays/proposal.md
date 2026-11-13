# Proposal

## Why

The bell and some changes feel slow to arrive. Today every delay is a constant in the code, so nobody can trade freshness against the cost of the work behind it. What the code does now:

- **Bell:** it rings from `StateTransitionNotifier` when one Sessions listing shows a background row in needs input that the previous listing did not. So it can never ring sooner than the next Sessions refresh, which runs every 5 seconds (`SessionsScreenDefinition`) and takes as long as `claude agents --json` takes. Claude Code's own reporting of the state comes on top; Cader cannot shorten that.
- **Worktrees:** rescanned on show and every 15 seconds while shown (`WorktreesScreenDefinition`).
- **Branch and place of a session's folder:** cached for 30 seconds (`SessionRowEnricher.CacheWindow`), so a branch switched in a shell takes up to that long to reach the Sessions rows.
- **Pull requests:** an answer is kept for 60 seconds, a failed one for 3 minutes (`PullRequestCatalogue`).

I have not measured how much of the felt delay is the 5-second poll and how much is the listing's own duration or Claude Code's. The change therefore makes the delays settings first; the diagnostic log, which already counts refresh durations, shows what is worth lowering.

## What Changes

- Four new settings in `config.json`, in whole seconds: `sessionsRefreshSeconds` (default 5), `worktreesRefreshSeconds` (15), `gitCacheSeconds` (30) and `pullRequestCacheSeconds` (60). The failed-lookup window stays three times the pull request one.
- A missing property, a non-number, or a value outside 1 to 600 reads as its default, as other unusable values do.
- The Sessions refresh sets how soon the bell can ring, because the bell is raised by that refresh. The needs-input requirement says so.
- Saving applies without a restart: the caches read their window per use, and the two refresh timers re-arm after every tick and read the setting then.
- The Config tab shows the four as setting rows (for example `Sessions refresh 5 s`), each with a tooltip naming its `config.json` property and what it costs. They are edited in `config.json`; Enter on one does nothing yet (see Out of scope).

## Capabilities

### New Capabilities

None.

### Modified Capabilities
- `settings`: the four delay settings, their defaults and range, and that they apply without a restart.
- `session-listing`: the timed refresh follows `sessionsRefreshSeconds` instead of "at least every five seconds", and a folder's branch is reused for `gitCacheSeconds`.
- `worktree-management`: the rescan while shown follows `worktreesRefreshSeconds`.
- `pull-requests`: the cache windows follow `pullRequestCacheSeconds`.
- `needs-input-notification`: the bell's latency is bounded by the Sessions refresh interval, and a shorter interval brings it forward.
- `config-tab`: the four rows and their tooltips.

## Impact

- **Code:**
  - Core: `Domain\Settings`, `Ports\UserConfig` and `Application\SettingsFile` (four properties, with a `Delay` value object in Domain wrapping the seconds and the range); `Application\SessionRowEnricher` and `PullRequestCatalogue` (read the window from `CurrentSettings` per use instead of a `static readonly`); `Views\ConfigRow`, `ConfigRows`, `Wording` and `WordingOfConfig`.
  - Tui: `TabRefresh.Every` takes a function of the current delay, and `ConsoleScreen.Every` becomes a re-arming one-shot timeout (`app.AddTimeout` has a fixed interval); `SessionsScreenDefinition` and `WorktreesScreenDefinition` pass the setting.
  - The composition root hands `CurrentSettings` to the definitions and the catalogue.
- **Tests:** `SettingsShould`, `SettingsFileShould`, `ConfigRowsShould`, `WordingOfConfigShould`, `SessionRowEnricherShould` and `PullRequestCatalogueShould` with a `FakeTimeProvider` and `TestSettings`; a smoke step or `ScreenLoopShould` fact for a changed interval.
- **Docs:** `docs\user-guide.md` (the settings and the bell), `.claude\rules\console-screen.md` (the refresh section), `CHANGELOG.md` and `Version`.
- **Overlap:** `diagnostic-log-setting` also adds a `config.json` property and a Config tab row, and modifies "Settings live in one file with defaults". This change adds a new settings requirement instead of modifying that one, so the two archive in either order; only the Config tab row order and `UserConfig`'s constructor need rebasing.
- **Cost:** a short Sessions interval runs `claude agents --json` more often. The existing no-overlap guard (`RunningLoads.TryReserve`) stops a slow listing stacking up, and the floor of 1 second bounds the rate.

## Out of scope

- Multiple Claude accounts, which is a later change.
- Editing the delays inside the console (an Enter dialog). They are edited in `config.json` until it is clear which people change often.
- Making the bell independent of the Sessions refresh (a faster dedicated poll for needs input). That is a second listing and a larger design; revisit if lowering the interval is not enough.
- The heartbeat (250 ms), the success message's 10 seconds, the update check's daily interval, the 60-second child time limit and other timings, which are not freshness trade-offs.
- Any change to what a refresh reads or to the bell's rules (once per transition, background rows only).
