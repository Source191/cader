# Proposal

## Why

People think about their work by repository (or by folder where there is no repository), but the Sessions tab sorts only by state, so sessions in one repository are scattered through the table and each row repeats its repository's name. The Worktrees tab is already sorted by repository, but every row repeats `repository ›`, so the eye has to read each name to find where one repository ends and the next begins.

## What Changes

- Both tables group their rows by repository: an orange heading row names the repository (or the folder's label outside any repository), followed by a faint rule across the row, and that group's rows sit beneath it, indented. The selection steps over headings; a filter keeps a heading only while one of its rows matches.
- Groups run A to Z by name, ignoring case, with folder groups (outside any repository, or folder gone) after the repositories.
- **Sessions:** within a group, the rows keep today's attention order (needs input first, then working, idle, completed, failed, stopped, most recently started first within a state). A session needing input in a repository late in the alphabet is no longer at the top of the table; the counts beside the tabs still say how many need input. **BREAKING** for anyone relying on needs-input rows always being first.
- **Sessions:** the REPOSITORY column goes, because the heading says it; NAME becomes the column that takes the width the others leave, elided at its end, instead of being cut at 28 cells. A session whose folder has gone is grouped under a heading that reads `folder gone:` and the folder's label.
- **Worktrees:** the WORKTREE cell drops `repository ›` and shows only the folder's part after the repository, or a branch or origin row's branch; within a group the rows keep today's order (worktree folders before branch and origin rows).
- The Config tab is unchanged.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `session-listing`: rows are grouped by repository under headings, attention first within a group; the REPOSITORY column is replaced by the heading; NAME takes the spare width; the folder-gone mark moves to the heading.
- `worktree-management`: rows are grouped by repository under headings; the WORKTREE cell no longer repeats the repository.
- `folder-actions`: where a row's folder is shown moves from the REPOSITORY cell and the `repository ›` prefix to the group heading.

## Out of scope

- Collapsing or expanding groups, and a key to jump between groups.
- A choice between grouped and ungrouped tables, or other groupings (by account, by state).
- Ordering groups by their most urgent session (considered and declined: groups would move about as states change).
- Grouping the Config tab, which keeps its own Setup and Accounts headings.
- Multiple Claude accounts beyond naming a session's owner, which is a later change.

## Impact

- `Application\Views`: `SessionsViewState` and `WorktreesViewState` order rows by group first; a new grouped-lines projection of the visible rows (headings between groups) that `IListViewState` exposes; `ListSelection`'s selection, filter and title logic are unchanged, because headings are never rows.
- `Domain\Sessions`: the attention order is applied within a group.
- `Tui`: `TableSource`, `ColumnSizing`, `TableToolTips` and `TableBody` draw lines rather than rows, so a heading has its own colour, no tooltip and never widens a column; `SessionsScreenDefinition` loses REPOSITORY and makes NAME elastic; `WorktreesScreenDefinition` drops the repository from WORKTREE; each definition names a group's heading.
- `Wording`: the REPOSITORY column's help goes, and the WORKTREE column's help and the folder-gone words change.
- `tools\ReadmeScene` and the README images (`assets\readme\sessions.png`, `new-session.gif`) need remaking once it ships; `docs\user-guide.md` describes the grouping.
- No new dependency, port, setting or state file.
