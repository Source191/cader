# Design

## Context

Each tab's refresh is its screen definition's `TabRefresh` (`SessionsScreenDefinition`: `Every(5 s)`, `WorktreesScreenDefinition`: `Every(15 s)`, Config: `Never`). `ConsoleScreen.StartRefreshing` turns each into one `app.AddTimeout` when the console opens and never touches them again, so a saved interval would need a restart, which the settings spec forbids. The timed work goes through `ListSessions` and `ScanWorktrees`, which reserve a `RunningLoads` slot on the UI thread and start nothing when a load is running; the reload after an action calls `TriggerLoad` directly and always runs. Settings are read from `CurrentSettings.Value` at each use.

F5 is already bound inside two dialogs (`LaunchWizardDialog` Start, `SettingsDialog` Save) through each dialog's own `KeyDown`; the console's window never sees a key while a dialog runs. `KeyMapping` knows seven named keys and printable characters, and drops F-keys.

## Goals / Non-Goals

**Goals:**
- A saved interval takes effect on the next tick, with no timer re-created.
- The interval rule ("is a refresh due?") is a tested `Application` class, not untested Tui timer code.
- F5 follows the house route for keys: a reducer case, an effect with a marker, routing that `EffectRoutingShould` holds.

**Non-Goals:**
- Changing how a refresh runs, what it reads, or the reload after an action.
- Any watcher (the `refresh-on-change` change).

## Decisions

1. **One ticker asks a schedule, instead of one timer per interval.** `ConsoleScreen` keeps a single 250 ms `AddTimeout` (the heartbeat's interval) that asks each refreshing tab's `Application\RefreshSchedule` whether it is due. The schedule holds a `Func<RefreshInterval>` (reading `CurrentSettings.Value`) and the `TimeProvider`, answers due when the interval is not off and at least that long has passed since the tab's last refresh started, and is told `Started()` whenever the console starts that tab's refresh (start-up, timer, show, F5). So a refresh from F5 or a show pushes the next timed one back by a whole interval, and a saved interval applies on the next tick. `TabRefresh.Every` carries the schedule rather than a `TimeSpan`.
   - *Rejected:* removing and re-adding the `AddTimeout` on save. The save runs in a handler off the UI thread, so the screen would need a change notification from `CurrentSettings` and a re-arm under the UI thread, all in untested wiring, for no gain.
   - *Rejected:* a ticker every second. With 5 seconds the listing could start almost a second late; 250 ms costs nothing more, and Terminal.Gui already runs the heartbeat at that rate.
2. **`Domain\RefreshInterval` wraps the seconds.** It is a sealed record over a whole number of seconds from 0 to 3600, with `Off`, `From(int)` (throws out of range), `TryFrom(int?, out RefreshInterval)` for the file and `Every` as a `TimeSpan`. It is not an `ITextValue`, as it is not text; what a person types is checked by `Settings.Validate` with the other fields, which gains a `SettingsProblem.RefreshIntervalNotWholeSeconds` case naming the field, so the dialog's problem line and focus work as they do for the worktree root.
3. **The file keeps plain numbers.** `UserConfig` gains `int? SessionsRefreshSeconds = null` and `int? WorktreesRefreshSeconds = null`. `SettingsFile` resolves a missing value, or one outside 0 to 3600, to that interval's default and keeps every other setting; a non-number fails deserialisation as `"yes"` for `diagnosticLog` does today, so the whole file reads as corrupt. That matches the existing rule for a wrong type and needs no new code in `JsonFileStore`.
4. **F5 is a shared key with its own marker.** `KeyName.F5` and `Key.F5` join the named keys and `KeyMapping` maps `GuiKey.F5`. `Application\Views\RefreshKeys.TryFor` (beside `TabKeys`) answers a `RefreshShownTab` effect marked `IRefreshesTab`; each of the three reducers answers it ahead of filter typing, so F5 works while a filter is being typed and leaves the filter alone. `TableBody` routes `IRefreshesTab` to `ITabHost.RefreshShown()`, and `ConsoleScreen` calls `ListSessions`, `ScanWorktrees` or `ReadConfig` for the shown tab (each tells its schedule `Started()`). `EffectRoutingShould` fails if the marker is not routed.
   - *Rejected:* catching F5 in the window's `KeyDown` before the tab. It would be untested Tui code, against "everything a key does is a tested effect".
   - *Rejected:* a do-nothing handler, relying on the reload after an effect. That reload never reserves, so holding F5 down would start a `claude agents` per repeat, and it shows the working spinner.
5. **F5 is listed in the help only.** It joins the shared help-only keys (`KeyHelp.HelpOnly(Key.F5, ...)`, beside ← → < >) on all three tabs. The key bar is full at 120 columns, and adding an item would change every bar scenario.
6. **The two rows sit after Default place.** Check for updates and Diagnostic log stay last, so the "row after Diagnostic log is Setup" rule and the in-place toggles are untouched. Each is a `ConfigRow` case opening a one-value settings dialog with a new `SettingsField` (`SessionsRefresh`, `WorktreesRefresh`), `SettingsText` gaining the two typed strings.

## Risks / Trade-offs

- [Sessions refresh off means a waiting session's bell rings late] → the row's tooltip says so; the `refresh-on-change` change brings it back to near-instant.
- [A 1-second sessions interval runs `claude agents` back to back: a listing took 2.3 s on 18 November 2026, CLI 2.1.327] → `RunningLoads` already keeps it to one at a time; the cost is the user's choice, and the minimum stays 0 or 1 rather than inventing a floor.
- [`SceneHost` in `tools\ReadmeScene` copies `ConsoleComposition`'s wiring] → change it in the same commit; CI builds it, so a missed constructor change fails.
- [The smoke check opens every dialog] → one new step in `SmokeScript` for an interval dialog.

## Migration Plan

None: a `config.json` from an earlier version has neither property and reads today's 5 and 15 seconds. An earlier Cader reading a file this version wrote ignores the two unknown properties (the settings spec already says so).
