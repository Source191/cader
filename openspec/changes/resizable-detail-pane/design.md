# Design

## Context

- `DetailReader<TRow>.Reading` builds the `DetailPane` with `source.Rows` lines (`SessionDetails` 6, `WorktreeDetails` 4 + 5 + 1), places it at `Pos.AnchorEnd(pane.Rows)` and sets `table.Height = Dim.Fill(pane.Rows)`, once; nothing changes either afterwards.
- `DetailLayout.Rows(content, width, height)` already lays facts and then the lines below into whatever height it is given, so the pane draws correctly at any height. `WorktreeDetails` asks `DetailLoader.WorktreeAsync` for `Wording.DetailListedItems` (5) commits or changed files.
- `TableBody.OnKey` answers `<` and `>` through `ScrollKeys.TryFor` before the reducer, and not while a filter is being typed; that is the model for `+` and `-`, which also only change the screen.
- `CurrentSettings.SaveAsync(Settings)` writes the whole `config.json` and swaps the snapshot. The settings dialog builds its `Settings` from `SettingsText` through `Settings.Validate`, so it knows nothing about values it has no field for.
- `ConsoleScreen` runs one ticker at `Heartbeat.Interval` (250 ms), `RefreshWhatIsDue`, which keeps running under a modal dialog (`terminal-gui.md`).
- Terminal.Gui 2.5.0 documents `View.Arrangement` with `ViewArrangement.TopResizable` ("the top edge of the view can be resized", only when the view's position and size are independent of other SubViews). That is read from the XML docs; nothing in the code base uses it yet.

## Goals / Non-Goals

**Goals:** one height rule for both inputs (drag and keys); the saved height and the shown height kept apart, so a short terminal never overwrites the saved height; one write of `config.json` per settled resize.

**Non-Goals:** resizing the pane on Config (it has none); a generic splitter for other views.

## Decisions

### 1. The spike decides how the drag is done

Before any production code, a throwaway program in `tools\HeadlessTerminal` (or a scratch console project run in a real Windows Terminal, as earlier spikes were) puts a `TableView` and a pane with `Arrangement = ViewArrangement.TopResizable` and a top-only border inside a `Tabs` tab, and records:
- whether dragging the top border resizes it inside `Tabs`, and whether `Movable` or arrange buttons appear on the border;
- what the drag leaves in `Y` and `Height` (expected absolute values in place of `AnchorEnd`), and which event reports it (`FrameChanged` expected);
- whether the arrange mode's keyboard entry (`Arranger.EnterArrangeMode`) can be reached by a key the console uses;
- how the pane behaves when the window is resized after a drag.

The facts go into `terminal-gui.md` with the date and version. **Fallback**, if `TopResizable` does not work inside `Tabs` or draws something the console cannot hide: the pane handles its own mouse on its divider row (press on row 0 grabs the mouse, position reports while pressed give the new top, release ends it), using the same height rule below. Either way only the Tui layer differs.

### 2. The height rule is one Application value

`Application\Detail\PaneHeight` (pure, tested) answers the lines to show from the wanted lines, the tab's most (`IDetailSource.MostRows`: 6 for Sessions, 200 for Worktrees, the saved range) and the lines the window leaves (the tab view's height minus the table's heading and three rows). It clamps to at least 1. The wanted lines are what the user set; the shown lines are the clamp. A drag reports a shown height, which becomes the wanted height (it already fits the window); `+` and `-` step the wanted height from the shown one, so a press on a short terminal does not grow an invisible reserve.

*Alternative:* keep a single height and clamp it in place. Rejected: a terminal made short and tall again would lose the user's height, which the spec forbids.

### 3. The pane re-anchors itself after every change

`DetailReader` gains `Resize(int step)` and keeps the shown height. Every change (key, drag, window resize) goes through one `Apply(lines)` that sets `pane.Y = Pos.AnchorEnd(lines + 1)`, `pane.Height = lines + 1` and `table.Height = Dim.Fill(lines + 1)`, so after a drag the pane is anchored to the bottom again and a later window resize behaves as it does today. The window resize is heard from the tab view's `FrameChanged`. A `None` reader (Config) ignores `Resize`.

*Alternative:* `table.Height = Dim.Fill(Dim.Func(...pane.Frame.Height))`. Rejected unless the spike shows re-setting `Height` misbehaves: re-applying values is simpler to reason about and to test in the headless terminal.

### 4. `+` and `-` are screen keys like `<` and `>`

`Application\Views\PaneKeys.TryFor(Key, out int step)` answers `+` as 1 and `-` as -1; `TableBody.OnKey` asks it after `ScrollKeys`, under the same `!FilterEditing` guard, and calls `detail.Resize(step)`. No effect and no handler, because nothing leaves the screen and nothing is written at once (decision 5). `Wording` lists both as `KeyHelp.HelpOnly` in `SessionsKeys` and `WorktreesKeys` with tips, and `KeyHelpParityShould` learns that they answer through `PaneKeys`, as `<` and `>` answer through `ScrollKeys`. `KeyMappingShould` pins `+` and `-`.

*Alternative:* `[` and `]`, or Ctrl+Up and Ctrl+Down. `+` and `-` read as grow and shrink; Ctrl chords do not reach the reducers (`KeyMapping`) and were not verified in Windows Terminal.

### 5. Saving waits for the height to settle

`Application\PaneHeights` (tested with `FakeTimeProvider`) holds each tab's wanted lines, seeded from `CurrentSettings.Value`, and the time of the last change. `Changed(tab, lines)` records it; `TakeDue()` answers the heights to save once a second has passed with no change, and only once. `ConsoleScreen.RefreshWhatIsDue` asks it on every tick and, when due, saves off the UI thread through `CurrentSettings.SavePaneHeightsAsync`, which saves `Value.WithPaneHeights(heights)`. A failed write is recorded through `Attempt.RunAsync` with a new `FailureSite` (an `ExceptionSummary`, nothing personal) and shows nothing; the pane keeps its height and the next settled change tries again.

*Alternative:* save on each key press and at the end of a drag. Rejected: a run of presses would write the file many times, and the end of a Terminal.Gui drag may not be an event we can hear (spike).

### 6. The settings dialog cannot drop the heights

`Domain\Settings` gains `PaneHeights` (a record of the two line counts, resolved by `SettingsFile` with the ranges in the spec) and `WithPaneHeights`. The dialog's `Settings` comes from `Validate`, which has no heights, so `SaveSettingsHandler` saves `effect.Settings.WithPaneHeights(current.Value.PaneHeights)`: the heights at the moment of the save, not when the dialog opened. `CurrentSettings` serialises its saves with a `SemaphoreSlim`, so a settled pane save and a dialog save that land together are written one after the other, each from the latest value.

### 7. The Worktrees pane lists what fits

`IDetailSource.ReadAsync` takes the lines the pane shows; `WorktreeDetails` asks the loader for `lines - Wording.WorktreeFactRows - 1` items (at least 0) and `SessionDetails` ignores it. A resize asks for the selected row's detail again (`DetailReader.AskAgain`), so growing the pane fills it; the `DetailRequests` numbering already drops a stale answer.

## Risks / Trade-offs

- [`TopResizable` misbehaves inside `Tabs` or draws arrange buttons] → the spike runs first and the fallback (decision 1) keeps the rest of the design unchanged.
- [A drag re-reads git for every row step on a large worktree] → the read is off the UI thread and only the newest answer is shown; if the spike shows a drag reports every row, the re-read waits for the drag to end or a quarter-second pause.
- [Two Cader processes save `config.json`] → unchanged from today: the file is replaced whole and the last writer wins; `cader here` never writes pane heights.
- [`+` on a UK keyboard needs Shift] → `KeyMapping` reads the character, so Shift+= arrives as `+`; `=` is not bound, to keep the key list short.

## Migration Plan

A `config.json` from an earlier version has no heights and reads as today's heights; the first save writes both. An earlier version reading a newer file ignores the two properties (`settings`: a property Cader does not know is ignored), and a save from it drops them, so the panes read as their defaults again. No rollback steps.
