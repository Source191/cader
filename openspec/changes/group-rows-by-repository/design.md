# Design

## Context

See proposal.md for the why, and the delta specs for the behaviour.

Today each table view is a `ListSelection<TRow>` governed by a `ListPolicy<TRow>`: `Visible` is the rows that pass the filter, put in order by the policy's `Order`; `Move`, `Load`, `Select` and the title work over `Visible`, stepping over any row the policy's `Selectable` refuses. `TableBody.Redraw` hands `Visible` to `ColumnSizing.Source`, which wraps it in a `TableSource<TRow>` for Terminal.Gui's `TableView`; `ColumnSizing`'s colour getters and `TableToolTips` read a row back with `TableSource.RowAt(index)`, and `Redraw` puts the table's cursor on `Visible.IndexOf(selected)`. Only keys move the selection: a click on the table never changes the state.

The Config tab already has headings, as a `ConfigRow.Heading` case its policy marks unselectable and its filter fields leave empty. That works because a Config heading is a fixed row. A Sessions or Worktrees heading is not: it exists only while its group has a visible row, and the rows are `SessionRow` and `WorktreeRow`, which every effect, handler and pane read.

`SessionsViewState` orders by `Domain\Sessions.OrderedByAttention`; `WorktreesViewState` by `Repository.Key` (the checkout's folder, so across roots the order follows the path, not the name), then `Placement`, then `FolderName`.

## Goals / Non-Goals

**Goals:**
- Headings that never become rows: no effect, handler, pane, title, count or key ever sees one.
- One grouping mechanism shared by both tabs, tested in Core.
- Keep `ListSelection`'s selection, filter and title logic as it is.

**Non-Goals:**
- Moving Config's headings onto the new mechanism (it works and is fixed; YAGNI).
- Mouse selection, which the console does not have today.

## Decisions

### Headings are a projection of the visible rows, not rows

A new `Application\Views\TableLine<TRow>` (abstract record, cases `Heading(TRow First)` and `Row(TRow Item)`, nested; Core is held at C# 14, so a switch over it has a discard arm throwing `UnreachableException`) and a pure `TableLines.Grouped(IReadOnlyList<TRow> visible, Func<TRow, string> groupKey)` that walks the ordered visible rows and puts a `Heading` before each run of rows sharing a key. Each view state exposes `Lines` (Sessions and Worktrees grouped; Config one `Row` per row), added to `IListViewState<TRow>`.

Because headings are derived from `Visible`, which is already filtered, a filter hides an empty group with no extra rule, and `Move`, `Load`, `Select`, the title and the counts are untouched: they never see a heading. A heading carries its group's first row, so the screen definition names it from facts the row already holds.

Alternatives considered:
- **A heading case in each row type, as Config does:** `SessionRow` and `WorktreeRow` are read by every effect, handler and pane, and each would have to rule the heading out; and the headings would have to be made, filtered and dropped as rows change. Rejected.
- **Headings drawn only in the Tui, with no Application type:** untestable in Core, and `ColumnSizing`, `TableToolTips` and `Redraw` would each map row indices to screen lines by themselves. Rejected.

### Grouping is part of each view's order

Each policy's `Order` sorts by group first, so a group's rows are always one run:
- **Sessions:** a group key and sort label from `SessionLocation`: `InRepository` by name (ignoring case), then its checkout's `FolderPath.Key`; `Outside` and `FolderGone` after every repository, by folder (ignoring case), folder gone kept apart from a live folder of the same path. Within a group, `Sessions.OrderedByAttention` as today (applied to the group's sessions, or as the sort's last keys: the builder's call).
- **Worktrees:** `RepositoryName` ignoring case, then `Repository.Key`, then the existing `Placement`, `FolderName`, `Key`.

`TableLines.Grouped` then needs only the same key: the checkout's key for a repository, the location's folder (with its kind) otherwise, and `Repository.Key` for a worktree.

### The screen draws lines

`ColumnSizing.Source` and `TableSource` take `IReadOnlyList<TableLine<TRow>>`; `TableSource.RowAt` becomes `LineAt`, and the colour getters, the cell getter and `TableToolTips` switch on the line:
- **Heading:** row colour bold orange (`Palette.Orange`, as Config's heading); the first column's cell is the definition's heading text, elided in the middle to the column's width (`TextFit.ElideMiddle`), then the rule; every other cell is the rule in a faint grey. No tooltip. A heading's cells are left out of `WidestCell`, so a heading never widens a column.
- **Row:** as today, with the first column's cell prefixed by two spaces.
- `Redraw` puts the cursor on the selected row's line index.

`IScreenDefinition` gains `string Heading(TRow first)`: Sessions answers the repository's name, the folder's label (`FolderLabels`), or `Wording.FolderGone` and the label, as its REPOSITORY cell does today; Worktrees answers `RepositoryName`. Config's definition is never asked.

How the rule fills the other columns (a cell of `─` the column's width, or an empty cell with a faint fill) is the builder's call, provided it never widens a column and reads as one line in Windows Terminal.

### Sessions' columns

REPOSITORY goes. Every view keeps exactly one elastic column, so NAME changes from `ColumnFit.Capped(28)` to `ColumnFit.Elastic()`, cut at its end; the heading's text lives in that same first column, so the elastic width is what lets a long repository name read whole at 120 columns. `Wording.RepositoryColumn`, its `ColumnHelp` line and its help text go; NAME's help says what the heading names, and WORKTREE's help says the heading names the repository.

Worktrees' WORKTREE cell becomes the folder's part after the repository (`WhereLabel`'s place, or the label alone) and, for a branch or origin row, the branch. If `WhereLabel` is then only used for a label with no repository, the builder may simplify it; nothing else depends on it.

### Implementation model and effort

Implement with **Claude Opus 5.5 (`claude-opus-5-5`) at high effort**. The change crosses Application and the Tui, alters a dozen spec scenarios across three capabilities, and must keep the house's reflection facts green (`ReducersShould`, `KeyHelpParityShould`, `NullFreeModelsShould`, `SealedTypesShould`, `SourceConventionsShould`), while the one part tests cannot fully see (how a heading draws in Terminal.Gui's `TableView`) needs judgement against a real run. The approach is settled and `ListSelection` is untouched, so extra-high or max effort would buy little. A lighter model (Sonnet 5.5) at high effort is a fallback only for the Core tasks (1 to 3 in tasks.md).

## Risks / Trade-offs

- [A needs-input session can sit below the visible rows, in a group late in the alphabet] → The counts beside the tabs still say how many need input, and the bell still rings when a background session starts waiting; ordering groups by urgency was offered and declined.
- [Terminal.Gui's `TableView` has no row span, so a heading is drawn cell by cell] → The heading's text sits in the first column, which is elastic and widest on both tabs; the rule fills the rest. Check it by eye in a real console (the README scene) before the pull request, as the README images are remade anyway.
- [Scrolling with `>` hides the first column, and with it the heading's text] → Accepted: the rule still marks the group, and `<` brings the name back.
- [Two repositories with one name under different roots read as two headings of the same words] → Accepted and rare; they are ordered by folder, and the pane's Repository line tells them apart.
- [An empty leftover's group is its root, so a heading can read a root's folder name, such as worktrees] → Accepted; it is what the WORKTREE cell said before the ›.
- [Tests and fixtures that read the REPOSITORY column or the `repository ›` text] → Updated with their spec scenarios, task by task.

## Migration Plan

None: no state file, setting or port changes. The README images (`tools\New-ReadmeScreenshot.ps1`) and `docs\user-guide.md` are updated in the same pull request; rolling back is reverting it.
