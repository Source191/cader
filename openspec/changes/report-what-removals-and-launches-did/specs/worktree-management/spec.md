## ADDED Requirements

### Requirement: A merged branch is judged once its worktree is gone
When Cader removes a worktree it created, it SHALL judge whether the branch is merged into the remote default branch after the worktree has been removed, and SHALL then delete a merged branch whatever git's own merged check says, from the Worktrees tab and from a session's Delete alike.

#### Scenario: Pushed, merged and pruned
- **WHEN** the user removes the clean Cader-created worktree Northwind.Orders-fix-123, whose branch fix-123 was pushed with an upstream, merged into origin's main, deleted on origin and pruned, while the local main is behind origin's
- **THEN** the worktree and the branch fix-123 are removed and the status line says so, with no "Failed:" line

#### Scenario: Deleted with its session
- **WHEN** the user deletes the session fix-123 in that same worktree and accepts its removal
- **THEN** the session, the worktree and the branch fix-123 are removed and the status line says so

#### Scenario: A commit before the removal
- **WHEN** the session in a Cader-created worktree commits to its branch after the worktree row was last scanned and before the user removes the worktree
- **THEN** that commit is part of what is judged, so a branch it leaves unmerged is asked about, not deleted

### Requirement: A branch that cannot be deleted leaves the removal reported
When git removed a worktree but refused to delete its branch, Cader SHALL report the worktree as removed and the branch as kept, giving git's reason, and SHALL NOT report the removal as failed.

#### Scenario: Git refuses the branch
- **WHEN** the user removes the clean Cader-created worktree Northwind.Orders-fix-123 whose merged branch fix-123 git refuses to delete
- **THEN** the worktree is removed, the branch fix-123 is kept, and the status line says the worktree was removed and the branch kept with git's reason, with no "Failed:" line
