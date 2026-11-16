## ADDED Requirements

### Requirement: A session whose folder has gone is marked and starts no git
Before asking git about a session's working directory, Cader SHALL check that the folder exists, on every refresh and whenever the detail pane reads the session. For a folder that does not exist it SHALL start no git process, SHALL mark the row in its REPOSITORY cell, its detail pane and a row tooltip, and SHALL keep listing the session. A folder that exists again on a later refresh SHALL be read as usual.

#### Scenario: The row is marked
- **WHEN** a stopped session's working directory C:\Users\me\source\worktrees\Northwind.Orders-fix-123 has been deleted
- **THEN** its REPOSITORY cell reads folder gone: followed by the folder's label ~\source\worktrees\Northwind.Orders-fix-123, cut at its end when the column is narrow, and the pane reads Worktree folder gone and Changes folder gone

#### Scenario: No git process is started
- **WHEN** a session's working directory has been deleted and the console refreshes for ten minutes with that row selected
- **THEN** no git process is started in that folder, and the diagnostic log holds no line about starting git there

#### Scenario: A folder deleted while the console runs
- **WHEN** a session's row was read in its repository and its folder is deleted before the next refresh
- **THEN** the next refresh marks the row as folder gone, without waiting for git's answer to expire

#### Scenario: A folder that comes back
- **WHEN** a marked session's folder is created again
- **THEN** the next refresh reads the folder with git as usual and the mark goes

#### Scenario: Hover over a marked row
- **WHEN** the mouse pointer rests on the row of a background session whose folder has gone
- **THEN** a tooltip says the session's folder no longer exists and that d deletes the session

#### Scenario: The filter still finds it
- **WHEN** the user filters by fix-123 and a session's deleted folder is named Northwind.Orders-fix-123
- **THEN** that session's row stays in the table

## MODIFIED Requirements

### Requirement: Rows show repository, branch and worktree
For each session Cader SHALL derive the repository, the checked-out branch and the folder it runs in. The table SHALL show the repository's name in a REPOSITORY column, or the folder-actions label of a folder outside any repository; the worktree's place (its slug, or the path inside the repository, or the main checkout), the branch and the full folder SHALL be shown in the detail pane. The filter SHALL still match the name, the repository and the branch.

#### Scenario: Session in a sibling worktree
- **WHEN** a session's working directory is the linked worktree C:\Users\me\source\worktrees\Northwind.Orders-fix-123 of Northwind.Orders, on branch fix-123
- **THEN** its REPOSITORY cell reads Northwind.Orders, and the pane reads Worktree fix-123 and Branch fix-123

#### Scenario: Session outside any repository
- **WHEN** a session's working directory is not inside a git repository
- **THEN** its REPOSITORY cell reads the folder's label, such as ~\scratch\notes, and the pane reads Worktree not in a repository

#### Scenario: Session whose folder no longer exists
- **WHEN** a session's working directory has been deleted
- **THEN** the listing still succeeds, its REPOSITORY cell reads folder gone: and the folder's label, and the pane reads Worktree folder gone

#### Scenario: Folder inside the repository
- **WHEN** a session runs in C:\Users\me\source\repos\Northwind.Orders\src\Api on branch main
- **THEN** its REPOSITORY cell reads Northwind.Orders, and the pane reads Worktree src\Api and Branch main

#### Scenario: Filter by branch
- **WHEN** the user filters by hotfix and a session runs on the branch hotfix/orders
- **THEN** that session's row stays in the table though no cell shows its branch

#### Scenario: Branch differs from the slug
- **WHEN** a session runs in the worktree Northwind.Orders-fix-123 on branch hotfix/orders
- **THEN** its REPOSITORY cell reads Northwind.Orders, and the pane reads Worktree fix-123 and Branch hotfix/orders

#### Scenario: Main checkout
- **WHEN** a session runs in C:\Users\me\source\repos\Northwind.Orders on branch main
- **THEN** its REPOSITORY cell reads Northwind.Orders, and the pane reads Worktree main checkout and Branch main

### Requirement: Column headers explain themselves on hover
A table column header SHALL show what the column shows as a tooltip while the mouse pointer rests on it, using the same words as the help key and keeping the help's line breaks so the tooltip never runs off the screen, drawn just under the header over the table's rows and never over the detail pane, and SHALL show no column tooltip while the pointer rests on a row; on the sessions view the only row with a tooltip of its own SHALL be a session whose folder has gone.

#### Scenario: Hover over a column header
- **WHEN** the mouse pointer rests on the STATE header in the sessions view
- **THEN** a tooltip names the column and lists the state words with their colours

#### Scenario: Hover over the PR header
- **WHEN** the mouse pointer rests on the PR header on a 120-column terminal
- **THEN** the tooltip shows the column's meaning on two lines, each within the terminal's width, on the rows just under the header, and the pane is not covered

#### Scenario: Hover over a row
- **WHEN** the mouse pointer rests on a row of the table whose session's folder exists
- **THEN** no tooltip is shown

#### Scenario: The REPOSITORY header names the mark
- **WHEN** the mouse pointer rests on the REPOSITORY header in the sessions view
- **THEN** the tooltip says the column shows the repository, the folder's label outside a repository, or folder gone before the label when the folder no longer exists
