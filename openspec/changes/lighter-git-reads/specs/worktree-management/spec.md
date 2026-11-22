## ADDED Requirements

### Requirement: Cader's git reads never write the index
Every git command Cader runs SHALL run with git's optional locks turned off, so that reading a worktree's status never rewrites its index or takes its index lock, and a session committing in that worktree at the same moment is never refused because of Cader.

#### Scenario: A scan while a session commits
- **WHEN** a worktrees scan reads the status of fix-123 while a session in fix-123 runs `git commit`
- **THEN** the commit is not refused with an index lock error, and fix-123's index file is not rewritten by the scan

#### Scenario: A touched file that did not change
- **WHEN** a file in fix-123 has its write time changed but not its content, and the worktrees are scanned
- **THEN** fix-123's CHANGES cell does not count it

### Requirement: A scan reuses answers it already holds
A worktrees scan SHALL read a repository's user email at most once every ten minutes, and SHALL take the last commit time of a worktree on a branch from that repository's branch listing, running a separate read only for a worktree whose head the listing does not cover.

#### Scenario: Repeated scans
- **WHEN** the shown Worktrees tab rescans Northwind.Orders every 15 seconds for five minutes
- **THEN** git is asked for Northwind.Orders' user email once

#### Scenario: The email changes
- **WHEN** the user changes `user.email` in Northwind.Orders
- **THEN** within ten minutes the origin rows of Northwind.Orders are judged yours or not by the new email

#### Scenario: A worktree on a branch
- **WHEN** fix-123 is on branch fix-123, whose tip was committed at 10:42
- **THEN** its LAST COMMIT cell reads as before and no separate last-commit read runs for it

#### Scenario: A detached head
- **WHEN** spike has a detached head
- **THEN** its last commit is read on its own, as before
