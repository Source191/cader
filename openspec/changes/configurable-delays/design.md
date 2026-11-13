# Design

## Context

Delays are constants today. The Sessions and Worktrees refresh intervals are `TabRefresh.Every(TimeSpan)` values on the screen definitions, started by `ConsoleScreen.Frame.StartRefreshing` through `app.AddTimeout`, which fixes its interval when it is added. The git location cache (`SessionRowEnricher.CacheWindow`, 30 s) and the pull request windows (`PullRequestCatalogue.Window` 60 s and `UnavailableWindow` 3 min) are `static readonly`. Settings reach the code through `CurrentSettings.Value`, read per use, which is how a save applies without a restart. A hand edit of `config.json` applies on the next start.

The bell has no delay of its own: `StateTransitionNotifier.Observe` compares two consecutive Sessions listings. Its latency is the wait for the next tick, plus the listing's duration, plus how long Claude Code takes to report the needs-input state.

## Goals / Non-Goals

**Goals:**
- The four delays can be lowered or raised in `config.json` and are visible on the Config tab.
- A lower Sessions interval brings the bell forward, with no new mechanism.
- No restart for a saved change, and no stacking of listings.

**Non-Goals:**
- An in-console editor for the delays, a dedicated needs-input poll, or any other timing (see the proposal).

## Decisions

### Four flat whole-second properties, one value object
`config.json` gains `sessionsRefreshSeconds`, `worktreesRefreshSeconds`, `gitCacheSeconds` and `pullRequestCacheSeconds`, flat like the existing properties, nullable in `UserConfig` and resolved to the defaults in `SettingsFile`. A `Delay` value object in Domain holds the seconds, answers a `TimeSpan`, and refuses anything outside 1 to 600, which then reads as the default. Whole seconds because that is the granularity anyone wants; the range because 0 would spin and hours would look like a hung console.
- **Alternative: a nested `delays` object.** Tidier JSON, but the other settings are flat and `UserConfig` is a flat record. Rejected.
- **Alternative: milliseconds or `TimeSpan` strings.** More to get wrong by hand for no gain. Rejected.
- **Alternative: one setting for the Sessions refresh and derive the rest.** The worktree scan is heavier than a listing and the caches protect git and `gh`, so they want separate dials. Rejected.

### Timers re-arm instead of repeating
`app.AddTimeout(interval, callback)` cannot change its interval. `ConsoleScreen.Every` becomes a one-shot that runs the tick, then adds the next timeout with the delay read from `CurrentSettings` at that moment. `TabRefresh.Every` carries a `Func<TimeSpan>` (so a definition stays a description) and `Frame` calls it each time it re-arms. The tracked-timers list holds the latest handle so disposal still stops the chain; `ClearLater` already shows the pattern.
- **Effect:** a saved change reaches the timers on their next tick, at most one old interval later.
- **Alternative: cancel and recreate timers on save.** More wiring, and settings reach the screen only through reads today. Rejected.
- The heartbeat keeps its fixed repeating timeout, untouched.

### The caches read the window per use
`SessionRowEnricher` and `PullRequestCatalogue` take `CurrentSettings` and read their window when they test an entry's age, so a save applies to the next refresh. The pull request failed-lookup window stays three times the configured one rather than becoming a fifth setting.

### Config tab rows, edited by file
Four `ConfigRow` cases follow the existing settings, labelled Sessions refresh, Worktrees refresh, Git cache and Pull request cache, with a VALUE like `5 s`. `Wording.ConfigDescription` gives each tooltip: the property name, the default, and the cost in one sentence ("Lower brings the bell forward; runs claude agents more often."). Enter on these rows is not bound in this change, so `KeyHelpParityShould` needs no new key. The Settings file row's `f` already opens the folder holding `config.json`.

### The bell is documented, not changed
`needs-input-notification` gains a requirement that the bell rings within one Sessions refresh (plus the listing's duration) of the transition, which is already true; the user guide says to lower `sessionsRefreshSeconds` for a quicker bell. The diagnostic log already records refresh durations, so a user can see whether the interval or the listing is the larger part.

## Risks / Trade-offs

- **A very short interval:** `claude agents --json` every second costs CPU and may strain the supervisor. The floor and `RunningLoads.TryReserve` bound it, and the tooltip says so. If it proves a problem, raise the floor for the Sessions setting alone.
- **No in-console edit:** a hand edit applies on the next start per `CurrentSettings`' contract, which is slower to try than a dialog. Accepted for the first version.
- **The felt delay may be mostly Claude Code's:** then lowering the interval helps less than hoped. The proposal says so rather than promising it.

## Open Questions

- **Floor of 1 second for the Sessions refresh?** Recommended as is; raise to 2 if the supervisor misbehaves.
- **Should a hand edit apply live (watching the file)?** Not in this change; the settings contract says next start.
- **Is the git cache (30 s) worth a setting?** Included because a branch change is one of the "changes that appear late"; drop it if you would rather keep the first version to three.
