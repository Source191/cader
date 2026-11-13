# Design

## Context

- **Today:** `TerminalSetupRunner` prints the fragment plan (with the line "Your Windows Terminal settings file is not changed."), asks, writes the fragment through `ITerminalFragment`, then offers Claude Code's bell through `IClaudeSettings`. `SetupFactsReader` reads the Windows Terminal profiles row as `fragment.Exists`, nothing more. `WindowsTerminal` adds `--profile "Cader session"` to every `wt new-tab` while the fragment file exists.
- **Terminal's rule** (issue #218, 4 November 2026, Windows Terminal 1.25.2733.0): `state.json`'s `generatedProfiles` lists every generated profile Terminal has loaded; one listed there with no entry carrying its `guid` in `settings.json` is treated as deleted by the user and stays hidden. Both files live in `%LOCALAPPDATA%\Packages\Microsoft.WindowsTerminal_8wekyb3d8bbwe\LocalState` for the Store package.
- **The id:** a fragment profile's `guid` is a version 5 UUID of its name (UTF-16LE) under a namespace that is itself the version 5 UUID of the fragment's folder name (UTF-16LE) under Terminal's runtime namespace `{f65ddb7e-706b-4499-8a50-40313caf510a}`. Checked in this session: the method gives the issue's observed Vantage pair exactly, and gives Cader's pair as `{3d7a06d2-710f-51d9-90c9-dacae2c3ab46}` (Cader) and `{cdf8708d-a821-5c97-95d0-fea560807dea}` (Cader session). Terminal's own Windows PowerShell id, `{61c54bbd-c2c6-5271-96e7-009a87ff44bf}`, is the same hash of its name directly under the other namespace, `{2bde4a90-d05f-401c-9492-e40884ead1d8}`, with no source step. The Cader pair is derived, not yet observed in a real `state.json`.
- **Observed in the issue:** the hiding rule, that the four-property entry (`guid`, `"hidden": false`, `name`, `source`) added by hand brings a profile back, and that Terminal applies a saved `settings.json` with no restart.
- **Assumed, not yet observed:** that Terminal writes that same entry itself when it first loads a fragment profile, that it appends such entries at the end of the profile list, that it still reads a bare `profiles` array, and that its parser accepts comments. The owner's check (task 7.1) observes each; none changes what is built, only what `windows.md` records.
- **Constraints:** no test or session reads the real Terminal files (`CLAUDE.md`); `System.IO` only in `Adapters`; the shared reflection facts (`NullFreeModelsShould`, `ClosedHierarchiesShould`, `PortParametersShould`, `SentenceLiteralsShould`) hold for every new type.

## Goals / Non-Goals

**Goals:**
- One rule, in one place, decides "hidden by Windows Terminal" for both the command and the Config tab.
- Terminal's settings file is changed only by inserting text, so nothing the user wrote there is reformatted or lost.

**Non-Goals:**
- Changing which profile `wt new-tab` asks for while the session profile is hidden (see Open Questions).
- Reading any Terminal install but the Store package.

## Decisions

### The id is derived in Domain, not stored as a constant

`Domain\TerminalProfileId` holds a `Guid` and is made by `ForFragment(string source, string name)`, which applies the two-step version 5 derivation above. Its `Text` is Terminal's spelling, lower case in braces. Deriving it keeps one home for the profile names and the source: a stored constant would go silently stale if either were renamed, which is the very rename (Vantage to Cader) that produced this issue's ids. The facts use the issue's observed Vantage pair as the known answer and the Cader pair as the derived one; Windows PowerShell's id is recorded in `windows.md` only, because no code derives an id without a source.

- **Alternative:** hard-code the two Cader ids in `TerminalProfiles`. Rejected for the reason above.
- **SHA-1:** version 5 UUIDs are defined over SHA-1 (RFC 9562, section 5.5). If CA5350 or Sonar's S4790 fires, it is suppressed in place with that reason, as the house rule for exemptions says.

### The fragment's source name gets one home

`Ports\TerminalProfiles` gains `Source = "Cader"`. It is the fragment's folder name (`TerminalFragment` uses it instead of its literal), the namespace input for both ids, and the `source` of each entry setup adds. Terminal takes a fragment profile's source from its folder name, so the three must never differ.

### A new port reads Terminal's two files and adds entries

`Ports\ITerminalSettings`, beside `ITerminalFragment` and `IClaudeSettings`:

- `string Location`: the settings file's full path, for the plan and the messages.
- `Task<TerminalProfileRecords> ReadAsync(CancellationToken)`: a closed hierarchy, `Missing` (no state file or no settings file: Terminal has not run, or is not the Store package), `Unreadable(string Reason)` (the parser's reason, shown only to report what failed, as `NotificationChannelState.Unreadable` does) and `Read(IReadOnlySet<TerminalProfileId> Generated, IReadOnlySet<TerminalProfileId> Listed)`, where `Listed` is every id with an entry in the profile list, hidden or not.
- `Task AddProfileEntriesAsync(IReadOnlyList<TerminalProfileEntry> entries, CancellationToken)`: throws `IOException` or `UnauthorizedAccessException` when the file cannot be written, and an `IOException` naming the reason when the file no longer holds a readable profile list, so the runner's existing catch reports every failure the same way.

`TerminalProfileEntry(TerminalProfileId Id, string Name, string Source)` is the entry the issue's workaround showed working: `guid`, `"hidden": false`, `name`, `source`.

### The hiding rule is one Application function

`Application\TerminalSetup\HiddenTerminalProfiles.Of(TerminalProfileRecords)` answers the entries to add, one per Cader profile whose id is in `Generated` and not in `Listed`, in the order the fragment lists the profiles; `Missing` and `Unreadable` answer none. The runner and `SetupFactsReader` both call it, so the command and the row can never disagree. A `"hidden": true` entry is in `Listed`, so a profile the user hid on purpose is never reported.

### The adapter inserts text and never re-serialises the file

`Adapters\Terminal\TerminalSettings(string localStateFolder)`, with `ForCurrentUser()` naming the Store package's `LocalState` folder, reads both files with `JsonDocument` allowing comments and trailing commas, so a file the user has annotated is still read (that Terminal's own parser accepts comments is assumed). The profile list is `profiles.list`, or `profiles` itself when it is a bare array, an older shape Terminal is assumed still to read; reading it costs one branch. An entry whose `guid` does not parse is ignored.

To add entries it reads the file's bytes, finds the list with a `Utf8JsonReader` (comments skipped, trailing commas allowed) and inserts the new entries as text right after the last element of the list, or right after `[` when it is empty: each entry is a one-line object serialised by `System.Text.Json` (so names are escaped), on its own line, indented like the list's first element (twelve spaces when it is empty), with the file's own line ending. A byte order mark, if present, is kept. The result is written to a temporary file beside it and moved over the original, so Terminal, which reloads the file when it changes, never sees half a file.

- **Alternative:** parse to `JsonNode`, add, and write it back. Rejected: it drops every comment and reformats the whole file the user owns.
- **Alternative:** a backup copy beside the file. Rejected: the change is insertion only and shown before it is made, so undoing it by hand is deleting at most two lines; a backup would leave Cader's clutter in Terminal's folder.
- **Placement:** after the last element, so the user's own profiles keep their places at the top of the new-tab menu (and, assumed, where Terminal appends the entries it writes itself).

### The runner reads Terminal once and asks a third question

`TerminalSetupRunner` takes `ITerminalSettings`. On a set-up run it reads Terminal's records once, before printing the plan, so the plan's "Your Windows Terminal settings file is not changed." line is printed only when nothing is hidden. The new step runs after the fragment step and before the bell, and only when the fragment exists by then (just written or already there): with entries to add it prints `Wording.HiddenProfilesPlan(location, entries)`, asks `Wording.ShowHiddenProfilesQuestion`, writes, and prints `Wording.HiddenProfilesShown(location)`; with `Unreadable` it prints `Wording.TerminalSettingsUnreadable(location, reason)` and writes nothing. Its step result joins the existing ones: a failed write makes the exit code 1 but still lets the bell be offered, because the bell is a different file; "Nothing was written." is printed only when every step was skipped. A failed fragment write still skips everything after it, as today. `--remove` is untouched.

### The Config row gets its own state

`SetupState.HiddenByTerminal(string Path)` (the fragment's path, so `c` and `f` still work on the row) is answered by `SetupFactsReader` when the fragment exists and `HiddenTerminalProfiles.Of` answers any entry. `WordingOfConfig` words it "hidden by Windows Terminal, run cader setup-terminal", and `WordingOfConsole`'s to-set-up count counts it beside `NotSetUp`. `Missing` and `Unreadable` read as installed, as today, because the fragment is there and nothing says otherwise. The Enter description is unchanged: it already says to run `cader setup-terminal`.

### Wiring

`SharedParts` builds one `TerminalSettings.ForCurrentUser()` for the console and the setup command. `ConsoleComposition` and `TerminalSetupComposition` pass it on. `tools\ReadmeScene\SceneHost` passes a `TerminalSettings` over a folder inside the scene's own temporary folders, where neither file exists, so the screenshot never reads the owner's Terminal. `TestDoubles\FakeTerminalSettings` records what was added and can be told to answer any `TerminalProfileRecords` or to fail the next write.

## Risks / Trade-offs

- **[Terminal changes the rule or the id scheme in a later version]** → The fact is recorded with its version in `windows.md`. An entry is only ever written for an id found in Terminal's own `generatedProfiles`, so a wrongly derived id matches nothing: nothing is reported and nothing is written, which is today's behaviour.
- **[The Cader ids are derived, not observed]** → The derivation reproduces the observed Vantage pair; the owner's manual check after merge (task 7.1) observes the Cader pair in a real `state.json` and records it in `windows.md`.
- **[Terminal writes its settings file at the same moment]** → The write is a single move of a complete file; Terminal reloads on change, and at worst the user runs `setup-terminal` again, which reads afresh.
- **[An unusual layout the inserter has not met, such as a list on one line]** → The inserter works on JSON tokens, not lines, so any valid layout gets valid JSON; only the indentation may look odd. Adapter facts cover a list on one line, an empty list, a bare `profiles` array, comments between and after elements, and a trailing comma.
- **[Only the Store package is read]** → Other installs see no change from today: nothing reported, the row reads installed.

## Migration Plan

Nothing of Cader's own is migrated. A user who already worked around the issue by hand has the entries, so nothing is reported. Rolling back the release leaves any added entries in place, where they are the same entries the issue's workaround adds by hand.

## Open Questions

- **What `wt new-tab --profile "Cader session"` does while that profile is hidden** (already unverified in `windows.md`): the owner's manual check (task 7.2) records it. If it fails to open the tab, Enter on a session opens nothing until `setup-terminal` shows the profile again, and a follow-up change makes `WindowsTerminal` leave `--profile` out while the profile is hidden. That follow-up would not change this change's specs or tasks.
