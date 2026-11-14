# Spec Delta

## MODIFIED Requirements

### Requirement: Delete offers to remove the session's worktree
Deleting a session SHALL first end its Cader-opened tab and wait for the tab's shell to exit. When the session's working directory is a worktree Cader created, delete SHALL then offer to remove that worktree under the worktree-management safety rules. When that worktree's repository no longer exists, or git cannot inspect the worktree, the session SHALL still be deleted, the worktree SHALL be kept untouched and the status line SHALL say why, in Cader's words.

#### Scenario: Clean worktree
- **WHEN** the user deletes a session in a Cader-created worktree that has no changes and no unpushed commits
- **THEN** Cader offers removal and, if accepted, removes the worktree and its branch

#### Scenario: Worktree with changes
- **WHEN** the worktree has uncommitted changes
- **THEN** the session is deleted but the worktree is kept and the row explains why

#### Scenario: Tab closed before the worktree goes
- **WHEN** the user deletes a session that was started and opened in its own tab
- **THEN** the tab closes before the worktree is removed, the worktree is removed, and the status line says the session was deleted and its tab closed

#### Scenario: Worktree without its .git
- **WHEN** the user deletes a session whose Cader-created worktree no longer has a `.git` file and whose folder holds no files
- **THEN** the session is deleted, the folder is deleted, the stale registration is pruned from the repository, and the status line says so

#### Scenario: Folder still in use
- **WHEN** the user deletes a session while a PowerShell tab opened from the worktrees view sits in its worktree under the worktree root
- **THEN** the session is deleted, the worktree is removed and its branch dealt with, the status line says the worktree's folder was left because a tab or program still has it open, and pressing remove on its empty row in the worktrees view after closing that tab deletes it

#### Scenario: The worktree's repository has gone
- **WHEN** the user deletes a session in the Cader-created worktree C:\Users\me\source\worktrees\Northwind.Orders-fix-123 and its repository C:\Users\me\source\repos\Northwind.Orders has been deleted since the worktree was created
- **THEN** the session is deleted, git is not asked anything, the folder and its files are kept, and the status line says the worktree was kept because its repository C:\Users\me\source\repos\Northwind.Orders no longer exists and that the Worktrees tab removes it

#### Scenario: Git fails while the worktree is inspected
- **WHEN** the user deletes a session in a Cader-created worktree and git fails while listing the repository's worktrees
- **THEN** the session is deleted, nothing is removed or deleted, and the status line says the worktree was kept because git could not inspect it, giving git's reason, with no "Failed:" line

### Requirement: Delete leaves a folder in use for a later remove
A worktree folder git cannot delete because a tab or program still uses it SHALL be waited for as the worktree-management removal rules say; when it is still held, the session SHALL be deleted, the worktree removed and its branch dealt with, and the folder left for a later remove, which the status line SHALL say.

#### Scenario: A program holds the folder
- **WHEN** the user deletes a session in the Cader-created worktree C:\Users\me\source\worktrees\Northwind.Orders-fix-123 while a program still uses that folder for longer than Cader waits, so it cannot be deleted
- **THEN** the session is deleted, the status line says the worktree's folder was left because a tab or program still has it open, the worktree is no longer registered with its repository, and the worktrees view lists the empty folder Northwind.Orders-fix-123 with health empty

#### Scenario: The program lets go while Cader waits
- **WHEN** the user deletes a session in a Cader-created worktree whose folder the session's own ending process releases a second after git fails to delete it
- **THEN** the session is deleted, the folder is gone, and the status line says the worktree was removed without mentioning a folder in use
