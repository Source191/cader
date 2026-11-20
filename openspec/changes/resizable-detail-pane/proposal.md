# Proposal

## Why

The detail pane under the Sessions and Worktrees tables is a fixed height: six lines under its divider on Sessions and ten on Worktrees, whatever the terminal's size. On a tall terminal the Worktrees pane lists only five commits or changed files and then "7 more", with empty table rows above it, and on a short one it takes rows the table needs. The user cannot change either.

## Benefit

- **Users:** drag the pane's divider, or press `+` and `-`, to give the pane or the table the room the moment needs; a taller Worktrees pane lists as many commits or changed files as it holds. The height is remembered per tab in `config.json`, so the console opens the way it was left.
- **Maintainers:** the height rules (limits, defaults, when to save) are tested Application classes, and the drag is Terminal.Gui's own `ViewArrangement.TopResizable`, so the Tui layer gains no mouse handling of its own.

## What Changes

- **Drag:** the pane's divider (its top border) can be dragged with the mouse on the Sessions and Worktrees tabs; the table takes the rows the pane gives up and gives up the rows it takes.
- **Keys:** `+` grows the shown tab's pane by one line and `-` shrinks it by one, except while a filter is being typed; both are listed in the help (`?`) and their tooltips, not in the key bar.
- **Limits:** the pane keeps at least one line under its divider; the table keeps its heading and at least three rows; the Sessions pane grows no taller than its six fact rows, because there is nothing more to show. A terminal too short for the saved height shows less without forgetting it.
- **Saved height:** each tab's height is kept in `config.json` as `sessionsDetailRows` and `worktreesDetailRows`, written once the height has stayed the same for a second, so a drag or a run of key presses writes the file once. A missing or out-of-range value reads as today's height; a value that is not a whole number reads as a corrupt file does, as the refresh intervals do. Saving the settings dialog keeps both.
- **Worktrees list:** the pane lists as many unpushed commits or changed files as its lines hold (after the facts and the "N more" line), not five.
- **Spike first:** confirm in Terminal.Gui 2.5.0 that `TopResizable` works on a view inside a `Tabs` tab, what it does to the pane's `Y` and `Height`, which event reports the new height, and how a resized window behaves after a drag; record the facts in `.claude\rules\terminal-gui.md`. If it falls short, the design's fallback (the pane's own mouse handling on its divider row) is taken instead.
- **Version and changelog:** a preview step and an Added entry.

Out of scope:

- A Config tab row or dialog for the heights: they are set where they are seen, and a hand edit of `config.json` still works.
- A keyboard arrange mode or resizing any other part of the screen (the status line, the tab strip).
- Collapsing the pane altogether (zero lines).
- Multiple Claude accounts beyond naming a session's owner, which is a later change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `detail-pane`: the pane's height is set by the user (drag and keys) within limits, per tab, instead of fixed; the Worktrees pane lists as many items as it holds.
- `settings`: `config.json` keeps the two pane heights, with their defaults, reading rules and write-back.

## Impact

- **Implement with:** Opus 5.5 at high effort: a Terminal.Gui behaviour that is read, not yet observed (the spike), layout that only shows on screen, and a debounced write of `config.json` from the UI thread that must not race the settings dialog's save.
- **Core:** `Domain\Settings` and `Ports\UserConfig` gain the two heights; `Application\SettingsFile` reads and writes them; a new `Application\Detail\PaneHeight` (limits) and `Application\PaneHeights` (pending height and when to save); `Application\Views\PaneKeys` beside `ScrollKeys`; `IDetailSource` reads with the lines it may fill; `WorktreeDetails` asks for as many items as fit; `Wording` gains the two help-only keys and their tips.
- **Tui:** `DetailPane` turns on `TopResizable`; `DetailReader` lays the table out against the pane's top and applies the height; `TableBody` answers `+` and `-`; `ConsoleScreen`'s ticker saves a settled height.
- **Documents:** `docs\user-guide.md` (Detail pane), `CHANGELOG.md`, `.claude\rules\terminal-gui.md` (the spike's facts) and `console-screen.md` (Detail pane).
- **Nothing** changes in the adapters, the command line or what Cader records in its diagnostics, beyond the settings file's two new properties.
