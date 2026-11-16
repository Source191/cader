# Proposal

## Why

The Sessions tab's detail pane packs ten facts into four rows of three columns, so related facts sit apart (Started beside Waiting with Prompted a column further on, Branch beside Worktree rather than under it) and the pane reads as cramped. Facts that belong together should line up in one column.

## What Changes

- The session pane's facts move into five rows of two columns: on the left, two of the pane's three columns wide, Waiting, Folder, Worktree, Branch and Changes; on the right, in the third column, Started, Prompted, Session, Tab and Memory.
- The Sessions pane grows from four fact rows to five, so its table shows one row fewer. The Worktrees pane keeps its four fact rows and its layout.
- No fact's words change; only where each sits.

Out of scope: the Worktrees pane's arrangement, the spacing between columns (`DetailLayout` is unchanged), and multiple Claude accounts, which is a later change.

## Capabilities

### New Capabilities

### Modified Capabilities
- `detail-pane`: the session requirement states the facts' arrangement in two columns of five rows.

## Impact

- `Application\WordingOfDetail.cs`: the session facts' rows, and a fact-row count of five for sessions beside four for worktrees.
- `Application\Detail\SessionDetails.cs`: `Rows` answers five.
- Tests: `WordingOfDetailShould`, `DetailSourcesShould`.
- `.claude\rules\console-screen.md` (the Detail pane section), `docs\user-guide.md` if it describes the arrangement, and the README image if it shows the pane.
- `Version` in `src\Source191.Cader\Source191.Cader.csproj` raised (patch).
