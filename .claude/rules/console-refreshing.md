---
paths:
  - "src/Source191.Cader.Tui/ConsoleScreen*.cs"
  - "src/Source191.Cader.Tui/TableBody*.cs"
  - "src/Source191.Cader.Tui/TableScroll*.cs"
  - "src/Source191.Cader.Tui/DetailReader*.cs"
  - "src/Source191.Cader/Composition/**"
  - "**/LatestSessions*.cs"
  - "**/RunningLoads*.cs"
  - "**/RefreshSchedule*.cs"
  - "**/RefreshInterval*.cs"
  - "**/LaunchHold*.cs"
  - "**/ChangeWatches*.cs"
  - "**/ChangeFilter*.cs"
  - "**/WatchPlan*.cs"
  - "**/FolderChanges*.cs"
  - "**/ConsoleLoads*.cs"
  - "**/WorktreeInventory*.cs"
  - "**/OriginPruner*.cs"
  - "**/SetupFactsReader*.cs"
  - "**/Config*.cs"
  - "**/StartSessionHandler*.cs"
---

# Refreshing and the Config tab

How the console's tabs refresh, how worktrees are scanned, and the Config tab. Layout, the detail pane, the status line and the dialogs are in `console-screen.md`; the change names and versions that dated each part are in `docs\verified-facts\console-screen.md`.

## Scanning worktrees

- **The first scan** runs once as the console opens, whichever tab is shown, and waits for the Sessions tab's first listing (succeeded or failed) through `Application\LatestSessions`, which holds the latest listing for every scan, so a scan runs no `claude agents` of its own.
- **After it** the Worktrees tab rescans on every show, on F5, at the Worktrees refresh interval while shown (never on its timer while hidden) and for a change a folder watch heard, shown or not.
- **One at a time:** `Application\RunningLoads` lets one scan run; the console reserves one on the UI thread before `TriggerLoad` and starts nothing when it cannot, and the scan function is wrapped in its `Track`, so the reload after an action is counted but never skipped.
- **Inside a scan** `WorktreeInventory` lists the repositories and inspects live worktrees at most eight git reads at once (a slot pool per scan, held only around one listing or one worktree's counts), then merges each repository's results in name order, so rows and orphans are what a serial scan gives.
- **Pruning:** a scan that shows a ○ origin row starts `Application\OriginPruner` for that repository, never awaited: `git remote prune origin` with the fetch's no-prompt settings, at most once an hour per repository from the attempt, a failure recorded once and never shown. A reference change starts no rescan (`ChangeFilter`).

## Timers and schedules

- **`TableBody`** keeps a tab's state, reducer loop and effect routing; `ColumnSizing` its column styles, widths and offset; `TableToolTips` its tooltips; `TableScroll` its place (every redraw swaps the source, so it keeps the row offset and scrolls to the cursor only when the selected key or filter changed); `DetailReader` the pane's requests.
- **A tab's refresh** is its definition's `TabRefresh`: `Never`, or `Every` an `Application\RefreshSchedule` the composition builds over `CurrentSettings.Value.SessionsRefresh` or `WorktreesRefresh` and the `TimeProvider`.
- **One ticker:** `ConsoleScreen` keeps one at the heartbeat's interval (`RefreshWhatIsDue`) that asks each schedule `IsDue()` (the sessions' whether shown or not, the worktrees' only while shown), reading the interval afresh each time, so a saved setting applies on the next tick with no timer re-created. The heartbeat keeps its own timer.
- **Pushing back:** `ListSessions` and `ScanWorktrees` tell the schedule `Started()` whenever they reserve a load (start-up, the ticker, a show, F5), so any of those pushes the next timed refresh back a whole interval. An interval of 0 (`RefreshInterval.Off`) is never due. The reload after an action calls `TriggerLoad` directly and tells no schedule.
- **A launch holds the schedules:** `StartSessionHandler` takes `Application\LaunchHold.Hold()` in a `using` around its whole body, and every `RefreshSchedule` answers false from `IsDue()` and `IsDueForChange()` while any hold is open (counted, so two launches share it).
- **During the hold** `Changed()` still records a change and `lastStarted` is untouched, so the first tick after the launch refreshes each tab once; F5, a show and the reload after an action never ask a schedule. `cader here` takes no hold.
- **`DetailReader`** asks only while its tab is shown and never makes a refresh wait; the waiting spinner centres on the table. Its words are `Wording.WaitingText(ConsoleTab)` and a failed load's `Wording.LoadFailurePrefix(ConsoleTab)`.

## Refreshing on change

- **The plan:** `Application\WatchPlan.For` plans the folders to watch: the Claude folder's `sessions` and `jobs` for Sessions; for Worktrees each repository the latest scan found (its `.git` folder with subfolders, from `WorktreeInventory.Repositories`) and each repository root's top level.
- **The watches:** `Application\ChangeWatches.Follow` keeps one `Ports\IFolderChanges` watch per planned folder (`Infrastructure\Files\FolderChanges` over `FileSystemWatcher`, a no-op for a missing folder) and stops those no longer planned; `ConsoleLoads.FollowChanges` runs it as the console opens and after each scan.
- **What counts:** `Application\ChangeFilter` decides from the changed name alone: a session's `.json`, a job's `state.json`, a worktree's record coming or going under `.git\worktrees`, a `HEAD` written, a root's folder coming or going, and every overflow. Never the index, objects, references, logs or lock files, so git's everyday work and Cader's own scan start nothing.
- **Timing:** a counted change calls `RefreshSchedule.Changed()` from the watcher's pool thread. It makes the schedule due half a second after the first change and at least a second after the tab's last start (`IsDue()`, or `IsDueForChange()` for a change alone, which the ticker asks of a hidden Worktrees tab); `Started()` clears it, so a change during a refresh is followed by one more.
- **Trouble:** a watch Windows refuses, or one that overflows, is recorded as `DiagnosticEvent` `WatchTrouble` naming only its `WatchKind`; nothing is read from any watched file.
- **F5** is `RefreshKeys.TryFor`'s `RefreshShownTab`, marked `IRefreshesTab` and answered by every reducer ahead of filter typing; `TableBody` routes it (`EffectRoutes.Of`, `EffectRoutingShould`) to `ITabHost.RefreshShown`, which calls `ListSessions`, `ScanWorktrees` or `ReadConfig` for the shown tab.

## The Config tab

- **Rows:** `ConfigView` over `ConfigRow`s (`ConfigRows.For(settings, SetupFacts)`), a case per setting; then one `Heading` the selection steps over (`ListPolicy.Selectable`) and any filter hides (`ConfigSection.Setup`); then one `Setup` case per piece of setup, its key stable across a save.
- **Settings rows:** shown at once. `CheckForUpdates` and `DiagnosticLog` (which carries only whether it is on) are toggled in place by Enter. Sessions refresh and Worktrees refresh sit after Default place, each opening a one-box dialog (`SettingsScope.SessionsRefresh`, `WorktreesRefresh`).
- **Accounts:** a second heading, `ConfigSection.Accounts`, heads the `Account` rows `AccountCatalogue` remembers, shown only when there is one; Enter renames a label through `AskForAccountLabel` and `NameDialog`.
- **The Claude folder row** (directly after Settings file) is `SetupState.Folder` over `Domain\ClaudeFolder`, read once at start by `SharedParts` from `CLAUDE_CONFIG_DIR` and handed to `ClaudeTranscripts`, `ClaudeSettings` and `SetupFactsReader`; `c` and `f` act on the folder itself.
- **The log rows:** the Diagnostic log file row is `SetupState.LogFile` while the setting is on and `SetupState.LogOff` while off, both carrying the `LogFiles` (`cader.log` and `cader.log.1`, each `Written` with its size or `Absent`) that `SetupFactsReader` reads through `IDiagnosticLogFiles`.
- **The older log:** `ConfigRows` adds a `SetupItem.OlderDiagnosticLogFile` row (`SetupState.OlderLog`) only once the files are read and `cader.log.1` is `Written`.
- **Keys on those rows:** `f` on either log row and on Settings file raises `OpenFileInEditor` (every other path row `OpenFolderInEditor`); `d` on either log row raises `ClearDiagnosticLog` (a dialog effect, confirmed in `DialogEffects`) or explains `NoLogToClear` or `LogFilesNotRead`, and on any other row `ClearsOnlyTheLog`.
- **Reading setup:** `SetupFactsReader` reads off the UI thread each time the tab is shown, after every action its keys dispatch (`TableBody` reloads a tab when an effect settles, so a save or a clear shows at once) and on F5. It has no refresh timer.
