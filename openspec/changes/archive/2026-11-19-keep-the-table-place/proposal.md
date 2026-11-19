# Proposal

## Why

Partway down a long table, a refresh throws the view back to the top: the Worktrees tab after each rescan, and Sessions every few seconds. Whatever the user was reading disappears, and the selected row can be left off screen below. The cause is in how the table is redrawn. Terminal.Gui 2.5.0 selects the first row and scrolls to the top whenever a table's rows are replaced, and `TableBody.Redraw` replaces them on every refresh and every key, then reselects the right row without scrolling to it. The same reset means Down past the last row on screen moves the selection out of sight instead of scrolling.

## Benefit

- **Users:** a refresh leaves the table exactly where it was, with the same rows on screen and nothing visibly moving, on every tab. Moving the selection scrolls the table just far enough to keep the selected row in view.

## What Changes

- Each table keeps its scroll position across every redraw: the row offset is read before the rows are replaced and put back after, then kept within the rows there are.
- The table scrolls to the selected row only when the selection or the filter has changed since the last redraw, and then only as far as keeps it on screen, so a key that moves the selection scrolls, and a refresh, or a view scrolled with the mouse wheel, stays put.

Out of scope:

- Keeping the selected row at the same height on screen when a refresh inserts rows above it; the rows on screen stay, and the selection keeps its row.
- Multiple Claude accounts beyond naming a session's owner, which is a later change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `console-layout`: a refresh keeps each table's place, and the table scrolls only to keep a newly selected row on screen.

## Impact

- **Implement with:** Claude Opus 5.5 (`claude-opus-5-5`) at medium effort: the cause is found and checked against Terminal.Gui 2.5.0, and the fix is one small class with its facts.
- **Tui:** a new `TableScroll` swaps a table's source keeping its row offset; `TableBody.Redraw` uses it.
- **Tests:** `TableScrollShould` (Core.Tests, Tui) over a real `TableView`.
- **Rules:** `.claude\rules\terminal-gui.md` records the `TableView` facts, dated with 2.5.0.
- **Version:** a preview step and a Fixed entry in `CHANGELOG.md`.
