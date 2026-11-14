# Spec Delta

## MODIFIED Requirements

### Requirement: Leftover folders are detected
The view SHALL also show folders that git no longer recognises: a folder under a repository's .claude\worktrees that the listing omits is an orphan; a folder directly under a root or under the worktree root whose .git file points at a worktree registration that no longer exists is broken; an empty folder named with a .worktrees suffix directly under a root, and any empty folder directly under the worktree root, is empty.

#### Scenario: Orphan folder
- **WHEN** a folder exists under .claude\worktrees but is absent from git's listing
- **THEN** the view shows it with health orphan

#### Scenario: Broken registration
- **WHEN** a sibling folder's .git file names a worktree registration whose repository no longer exists
- **THEN** the view shows it with health broken

#### Scenario: Broken registration under the worktree root
- **WHEN** a folder under C:\Users\me\source\worktrees has a .git file naming a worktree registration that no repository under the roots lists
- **THEN** the view shows it with health broken

#### Scenario: Empty leftover
- **WHEN** an empty folder named Northwind.Orders.worktrees sits directly under a root
- **THEN** the view shows it with health empty

#### Scenario: Empty folder under the worktree root
- **WHEN** an empty folder C:\Users\me\source\worktrees\Northwind.Orders-fix-123 sits directly under the worktree root C:\Users\me\source\worktrees, which is not a repository root
- **THEN** the view shows it with health empty, its WORKTREE cell naming the worktree root and the folder, and the remove key deletes it without asking

#### Scenario: A folder under the worktree root that holds files but no .git file
- **WHEN** C:\Users\me\source\worktrees\Northwind.Notes holds a file and no .git file
- **THEN** the view does not show it

#### Scenario: Worktree of a repository outside the roots
- **WHEN** a folder under C:\Users\me\source\worktrees has a .git file naming the registration D:\other\Northwind.Ledger\.git\worktrees\fix, that registration exists, D:\other\Northwind.Ledger\.git exists, and D:\other is not a root
- **THEN** the view shows it with health elsewhere, its repository as Northwind.Ledger, and the status line says its repository D:\other\Northwind.Ledger is not under the roots

### Requirement: Removal refuses to destroy work
Removal SHALL be refused when the worktree has changed or untracked files, unpushed commits, a lock, or a live session inside it, and the refusal SHALL name the reason. When git cannot delete the folder because a tab or program still has it as its working directory, git has already removed the worktree's registration and contents, so Cader SHALL try to delete what is left for about five seconds, then finish the removal as for any other (the branch dealt with as the branch rules say) whether or not the folder went, and SHALL name a folder it had to leave.

#### Scenario: Unpushed commits
- **WHEN** the user presses remove on a worktree with unpushed commits
- **THEN** nothing is removed and the status line names the branch and the number of unpushed commits

#### Scenario: Clean worktree
- **WHEN** the user presses remove on a worktree with no changes, no unpushed commits, no lock and no session
- **THEN** the worktree is removed from git and from disc

#### Scenario: Folder released while Cader waits
- **WHEN** the user presses remove on a clean worktree whose folder a closing program releases two seconds after git fails to delete it
- **THEN** the folder is gone, the branch is dealt with as for any removal, and the status line says the worktree was removed and does not mention a folder in use

#### Scenario: Folder in use by a shell tab
- **WHEN** the user presses remove on a clean worktree Northwind.Orders-fix-123 under the worktree root while a PowerShell tab opened from the worktrees view sits in it
- **THEN** after about five seconds the status line says Northwind.Orders-fix-123 was removed but its folder was left because a tab or program still has it open, the branch is dealt with as for any removal, and the worktrees view lists the empty folder with health empty, which the remove key deletes once that tab is closed

#### Scenario: Branch of a worktree whose folder stayed in use
- **WHEN** the worktree whose folder stayed in use was created by Cader on the branch fix-123, which is merged
- **THEN** the branch fix-123 is deleted, and the status line says so as well as naming the folder that was left
