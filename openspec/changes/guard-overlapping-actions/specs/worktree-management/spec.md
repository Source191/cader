## ADDED Requirements

### Requirement: Cader's reads never hold a worktree's index lock
Cader's git calls that only read a repository or worktree (its status, counts, changed files, unpushed commits, worktree listing and merge checks) SHALL take no optional lock, so they never hold a worktree's index lock while the user runs git there.

#### Scenario: A commit during a scan
- **WHEN** the user commits in Northwind.Orders-fix-123 while Cader is scanning the worktrees
- **THEN** the commit never fails because of an index lock Cader holds

#### Scenario: Writes still lock
- **WHEN** Cader removes a worktree or deletes a branch
- **THEN** git takes the locks those commands always take
