## ADDED Requirements

### Requirement: Rows are grouped by repository
The sessions table SHALL group its rows by where they run: one group per repository, keyed by its main checkout, and one per folder label outside any repository or whose folder has gone. Each group SHALL be headed by a row reading the repository's name, or the folder's label, or folder gone: and the folder's label, in bold orange, with a faint rule across the rest of the row; the group's rows SHALL follow it, their NAME cells indented by two cells.

#### Scenario: A group's heading
- **WHEN** two sessions run in worktrees of Northwind.Orders
- **THEN** one bold orange row reading Northwind.Orders, followed by a faint rule, comes before both rows, whose names start two cells in

### Requirement: Groups run A to Z with attention first within a group
Repository groups SHALL run A to Z by name ignoring case, two of one name in checkout order, then the folder groups A to Z by the folder's path ignoring case, a folder that has gone after a live folder of the same path. Within a group the rows SHALL be ordered by display state: needs input first, then working, idle, completed, failed, stopped, and within a state by most recently started first.

#### Scenario: Two repositories
- **WHEN** the list holds a stopped and a working session in Northwind.Orders and a needs-input session in Northwind.Billing
- **THEN** the table reads a Northwind.Billing heading and its needs-input row, then a Northwind.Orders heading, its working row and its stopped row

#### Scenario: Mixed states within a group
- **WHEN** Northwind.Orders holds one working, one needs-input and one stopped session
- **THEN** under its heading the needs-input row is first and the stopped row is last

#### Scenario: Case is ignored
- **WHEN** sessions run in cader and in Northwind.Orders
- **THEN** the cader group comes first

#### Scenario: Folders follow repositories
- **WHEN** one session runs in Northwind.Orders and one in ~\scratch\notes, outside any repository
- **THEN** the Northwind.Orders group comes first and the ~\scratch\notes group last

### Requirement: A heading is never selected
A heading SHALL never be selected: the arrow keys, a refresh and a filter SHALL step over it as over no row, and the title and counts SHALL count no heading. A filter SHALL keep a heading only while at least one of its rows matches, and SHALL match rows as before. A heading SHALL show no tooltip.

#### Scenario: The selection steps over a heading
- **WHEN** the last row of the Northwind.Billing group is selected and the user presses Down
- **THEN** the first row under the Northwind.Orders heading is selected

#### Scenario: The first load
- **WHEN** the first listing arrives
- **THEN** the first row under the first heading is selected

#### Scenario: A filter hides an empty group
- **WHEN** the user filters by fix-123 and only a Northwind.Orders session matches
- **THEN** the table shows the Northwind.Orders heading and that row alone, and the title counts one session shown, as it would with no headings

#### Scenario: Needs input is counted beside the tabs
- **WHEN** the only needs-input session is in the last group, below the table's visible rows
- **THEN** the counts beside the tabs still read ◐ 1 needs input

## MODIFIED Requirements

### Requirement: Rows show repository, branch and worktree
For each session Cader SHALL derive the repository, the checked-out branch and the folder it runs in. The table SHALL show the repository's name, or the folder-actions label of a folder outside any repository, in the heading of the row's group, not in a column of its own; the worktree's place (its slug, or the path inside the repository, or the main checkout), the branch and the full folder SHALL be shown in the detail pane. The filter SHALL still match the name, the repository and the branch.

#### Scenario: Session in a sibling worktree
- **WHEN** a session's working directory is the linked worktree C:\Users\me\source\worktrees\Northwind.Orders-fix-123 of Northwind.Orders, on branch fix-123
- **THEN** its row is under the Northwind.Orders heading, and the pane reads Worktree fix-123 and Branch fix-123

#### Scenario: Session outside any repository
- **WHEN** a session's working directory is not inside a git repository
- **THEN** its row is under a heading reading the folder's label, such as ~\scratch\notes, and the pane reads Worktree not in a repository

#### Scenario: Session whose folder no longer exists
- **WHEN** a session's working directory has been deleted
- **THEN** the listing still succeeds, its row is under a heading reading folder gone: and the folder's label, and the pane reads Worktree folder gone

#### Scenario: Folder inside the repository
- **WHEN** a session runs in C:\Users\me\source\repos\Northwind.Orders\src\Api on branch main
- **THEN** its row is under the Northwind.Orders heading, and the pane reads Worktree src\Api and Branch main

#### Scenario: Filter by branch
- **WHEN** the user filters by hotfix and a session runs on the branch hotfix/orders
- **THEN** that session's row stays in the table though no cell shows its branch

#### Scenario: Filter by repository
- **WHEN** the user filters by orders
- **THEN** the Northwind.Orders heading and every row under it stay in the table, though no row's cell shows the repository

#### Scenario: Branch differs from the slug
- **WHEN** a session runs in the worktree Northwind.Orders-fix-123 on branch hotfix/orders
- **THEN** its row is under the Northwind.Orders heading, and the pane reads Worktree fix-123 and Branch hotfix/orders

#### Scenario: Main checkout
- **WHEN** a session runs in C:\Users\me\source\repos\Northwind.Orders on branch main
- **THEN** its row is under the Northwind.Orders heading, and the pane reads Worktree main checkout and Branch main

### Requirement: Column headers explain themselves on hover
A table column header SHALL show what the column shows as a tooltip while the mouse pointer rests on it, using the same words as the help key and keeping the help's line breaks so the tooltip never runs off the screen, drawn just under the header over the table's rows and never over the detail pane, and SHALL show no column tooltip while the pointer rests on a row or a heading; on the sessions view the only row with a tooltip of its own SHALL be a session whose folder has gone.

#### Scenario: Hover over a column header
- **WHEN** the mouse pointer rests on the STATE header in the sessions view
- **THEN** a tooltip names the column and lists the state words with their colours

#### Scenario: Hover over the PR header
- **WHEN** the mouse pointer rests on the PR header on a 120-column terminal
- **THEN** the tooltip shows the column's meaning on two lines, each within the terminal's width, on the rows just under the header, and the pane is not covered

#### Scenario: Hover over a row
- **WHEN** the mouse pointer rests on a row of the table whose session's folder exists
- **THEN** no tooltip is shown

#### Scenario: Hover over a heading
- **WHEN** the mouse pointer rests on the Northwind.Orders heading
- **THEN** no tooltip is shown

#### Scenario: The REPOSITORY header names the mark
- **WHEN** the mouse pointer rests on the NAME header in the sessions view, which has no REPOSITORY header any more
- **THEN** the tooltip says the column shows the session's name, under a heading naming its repository, the folder's label outside a repository, or folder gone before the label when the folder no longer exists

### Requirement: Long names never hide columns
The sessions table SHALL give NAME the width its other columns leave and SHALL fit every column on a 120-column terminal; a name too long SHALL be cut at its end and a heading too long for the NAME column SHALL lose its middle, each marked with an ellipsis. The row SHALL keep the full values for every key that uses them, < and > SHALL scroll the table on a narrower terminal, and the key bar SHALL show every key that applies within 120 columns, with its words.

#### Scenario: Long repository name in the sessions view
- **WHEN** a session runs in a worktree of Northwind.Integration.CosmosDb.ChangeFeedMonitoring and the terminal is 120 columns wide
- **THEN** every column of the sessions table is on screen without scrolling, and the heading reads Northwind.Integration.CosmosDb.ChangeFeedMonitoring whole

#### Scenario: Long folder in the worktrees view
- **WHEN** a worktree's folder label is 60 characters long and the terminal is 120 columns wide
- **THEN** every column of the worktrees table is on screen without scrolling, and the WORKTREE cell shows it whole

#### Scenario: Copy a capped folder
- **WHEN** the user presses the copy key on a row under a heading that holds an ellipsis
- **THEN** the clipboard holds the full folder path

#### Scenario: Third column at 120 columns
- **WHEN** the terminal is 120 columns wide
- **THEN** the c, e, v and p keys and their words are in the Folder group of the key bar, and their tooltips still explain them

#### Scenario: Scroll on a narrow terminal
- **WHEN** the terminal is 100 columns wide and the user presses > on the Sessions tab
- **THEN** the table scrolls one column to the right and the Sessions tab stays shown

#### Scenario: Third column of key hints
- **WHEN** the terminal is 126 columns wide
- **THEN** the c, e, v and p keys and their words are in the Folder group of the key bar on both tabs

#### Scenario: Wide terminal
- **WHEN** the terminal is 127 columns or wider
- **THEN** the heading for Northwind.Integration.CosmosDb.ChangeFeedMonitoring and every NAME cell are shown whole

#### Scenario: Long session name
- **WHEN** a session is named northwind-orders-cosmos-change-feed-monitoring-spike and the terminal is 120 columns wide
- **THEN** every column of the sessions table is on screen without scrolling, and the NAME cell is cut at its end with an ellipsis only if the column is narrower than the name

### Requirement: Each column keeps its own fit
How a sessions column fits SHALL be a property of that column and never decided from its header's words: NAME SHALL take the width the other columns leave, cut at its end with an ellipsis, AGE SHALL be right-aligned so its durations line up on their last character, and every other column SHALL take its natural width. A heading SHALL never widen a column. Rewording a header SHALL change the header alone.

#### Scenario: Ages line up at the right
- **WHEN** fix-123 started five minutes ago, templates-sync two hours and ten minutes ago and a stopped session three days and four hours ago
- **THEN** the AGE cells 5m, 2h10m and 3d4h each end at the column's right edge, and the NAME cells start two cells in from its left edge, under their heading

#### Scenario: A long name
- **WHEN** a session is named northwind-orders-cosmos-change-feed-monitoring-spike on a terminal too narrow for it
- **THEN** its NAME cell is cut at its end with an ellipsis, the rename and open keys use the full name, and the other columns keep their widths

#### Scenario: A reworded header
- **WHEN** the AGE header is reworded in a later version
- **THEN** that column's cells are still right-aligned and NAME still takes the width the others leave

### Requirement: A session whose folder has gone is marked and starts no git
Before asking git about a session's working directory, Cader SHALL check that the folder exists, on every refresh and whenever the detail pane reads the session. For a folder that does not exist it SHALL start no git process, SHALL mark the row by grouping it under a folder gone heading, in its detail pane and in a row tooltip, and SHALL keep listing the session. A folder that exists again on a later refresh SHALL be read as usual.

#### Scenario: The row is marked
- **WHEN** a stopped session's working directory C:\Users\me\source\worktrees\Northwind.Orders-fix-123 has been deleted
- **THEN** its row is under a heading reading folder gone: followed by the folder's label ~\source\worktrees\Northwind.Orders-fix-123, after the repository groups, and the pane reads Worktree folder gone and Changes folder gone

#### Scenario: No git process is started
- **WHEN** a session's working directory has been deleted and the console refreshes for ten minutes with that row selected
- **THEN** no git process is started in that folder, and the diagnostic log holds no line about starting git there

#### Scenario: A folder deleted while the console runs
- **WHEN** a session's row was read in its repository and its folder is deleted before the next refresh
- **THEN** the next refresh moves the row under a folder gone heading, without waiting for git's answer to expire, and the row stays selected

#### Scenario: A folder that comes back
- **WHEN** a marked session's folder is created again
- **THEN** the next refresh reads the folder with git as usual and the row moves back under its repository's heading

#### Scenario: Hover over a marked row
- **WHEN** the mouse pointer rests on the row of a background session whose folder has gone
- **THEN** a tooltip says the session's folder no longer exists and that d deletes the session

#### Scenario: The filter still finds it
- **WHEN** the user filters by fix-123 and a session's deleted folder is named Northwind.Orders-fix-123
- **THEN** that session's row stays in the table, under its folder gone heading

## REMOVED Requirements

### Requirement: Rows are ordered by attention needed
**Reason**: Replaced by "Rows are grouped by repository", which keeps the attention order within each group but no longer puts every needs-input row at the top of the table.
**Migration**: The counts beside the tabs still show how many sessions need input; the bell still rings when a background session starts waiting.
