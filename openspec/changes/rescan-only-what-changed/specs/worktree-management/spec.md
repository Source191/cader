## ADDED Requirements

### Requirement: A scan re-reads only the repositories that changed
A worktrees scan SHALL read a repository's worktrees and branches again only when that repository's git folder recorded a change since they were last read, F5 was pressed, an action was taken from the console, or five minutes have passed; otherwise it SHALL use what the last scan read. Every scan SHALL still read each live worktree's changed files and unpushed commits, and its rows SHALL be the same as a full scan's.

#### Scenario: A timed rescan with nothing changed
- **WHEN** the Worktrees tab is shown, 55 repositories are listed and nothing in their git folders has changed for a minute
- **THEN** the 15-second rescan reads no repository's worktree or branch listing and reads each live worktree's changes and unpushed commits

#### Scenario: A worktree added in one repository
- **WHEN** the user runs `git worktree add` in Northwind.Orders
- **THEN** the rescan that follows reads Northwind.Orders' worktrees and branches again and no other repository's, and lists the new worktree

#### Scenario: A file edited in a worktree
- **WHEN** a session edits a file in fix-123 and the Worktrees tab is shown
- **THEN** fix-123's CHANGES cell counts it at the next timed rescan, as today

#### Scenario: F5
- **WHEN** the user presses F5 on the Worktrees tab
- **THEN** every repository's worktrees and branches are read again

#### Scenario: The safety net
- **WHEN** a change in Northwind.Billing's git folder was never reported and five minutes have passed since it was last read
- **THEN** the next scan reads Northwind.Billing's worktrees and branches again

#### Scenario: A watch overflows
- **WHEN** a repository's watch reports that it lost changes
- **THEN** the next scan reads every repository's worktrees and branches again

### Requirement: A reference change is read by the next scan
A change to a repository's references (a commit, a fetch, a branch created, renamed or deleted, references packed) SHALL mark that repository to be read again by the next scan, and SHALL NOT start a scan.

#### Scenario: A commit in a worktree
- **WHEN** a session commits on fix-123 in Northwind.Orders while the Worktrees tab is shown
- **THEN** no scan starts because of it, and the next timed rescan reads Northwind.Orders again and shows fix-123's new last commit

#### Scenario: A branch deleted in a shell
- **WHEN** the user runs `git branch -D spike` in Northwind.Orders while the Worktrees tab is shown
- **THEN** the next timed rescan drops the spike branch row

## MODIFIED Requirements

### Requirement: Git's housekeeping starts no scan
Changes to a repository's index, objects, references, logs or lock files SHALL NOT start a scan, so git's everyday work in a shell and Cader's own git reads never make the worktrees rescan; a reference change SHALL only mark its repository to be read again by the next scan.

#### Scenario: Git's own housekeeping
- **WHEN** the user runs `git status`, `git fetch` or `git commit` in a listed worktree
- **THEN** no scan starts because of it

#### Scenario: Cader's own scan
- **WHEN** a scan reads every listed repository's status
- **THEN** that scan starts no other scan

#### Scenario: Cader's own prune
- **WHEN** a scan prunes origin's deleted branches in Northwind.Orders
- **THEN** no scan starts because of it, and the next scan reads Northwind.Orders again
