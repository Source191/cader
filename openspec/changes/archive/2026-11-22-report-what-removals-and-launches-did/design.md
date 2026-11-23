## Context

- **The removal path:** `WorktreeRemover.CarryOutAsync(row, decision, createdHere, merged, token)` is the one removal path. Both callers work out `merged` before the removal starts, through `WorktreeInspector.IsMergedAsync`: `RemoveWorktreeHandler` for `d` on Worktrees and `DeleteSessionHandler` for Delete on Sessions. `IsMergedAsync` runs `git merge-base --is-ancestor <branch> <origin's default, else HEAD>`.
- **The deletion:** after `git worktree remove`, `RemovalDecision.BranchActionFor(createdHere, merged)` answers `Delete` or `Ask`. `Delete` runs `IGit.DeleteBranchAsync(..., force: false)`, which is `git branch -d`. A `GitException` from it is not caught, so it reaches `TableBody.RunInBackground` and becomes `StatusMessage.Failed`.
- **Git's own check:** `git branch -d` checks the branch against its upstream when one is set and still exists, otherwise against HEAD of the repository it runs in (the main checkout). A branch made by `git worktree add -b <branch> <path> origin/main` tracks `origin/main`, and `-d` then passes; after `git push -u origin <branch>` it tracks `origin/<branch>`. Once that ref is pruned, `-d` checks HEAD and refuses while the local default branch is behind. Reproduced on 22 November 2026: `merge-base --is-ancestor` exit 0, `git branch -d` exit 1, "error: the branch 'feat2' is not fully merged".
- **The launch:** `LaunchService.StartedAsync` runs after `StartResult.Started`. It calls `recent.RecordRecentRepositoryAsync` and `claude.ListSessionsAsync`, which throws `ClaudeCliException` when the CLI exits with an error; neither call is guarded.

## Goals / Non-Goals

**Goals:**
- A removal that removed the worktree is reported as removed, whatever happens to the branch.
- A branch Cader created and merged is deleted with nothing lost.
- A started session is reported as started.

**Non-Goals:** squash merges, the include copy's failure, changing what is asked about a hand-made branch.

## Decisions

1. **The merge check moves into `WorktreeRemover`, after the worktree is removed.**
   - **How:** `CarryOutAsync` stops taking `merged`. When `createdHere` and the head is a branch, it asks `WorktreeInspector.IsMergedAsync` once git's removal has succeeded (the `Removed` or `FolderLeft` paths), then applies `BranchActionFor`.
   - **Why after:** once its worktree is gone the branch is checked out nowhere, so no session or shell can commit to it between the check and the delete. That makes a forced delete safe, which a check made before the removal (as today) would not.
   - **What follows for the callers:** `RemoveWorktreeHandler` and `DeleteSessionHandler` drop their own `IsMergedAsync` calls, and a `GitException` from the check reads as not merged, so the user is asked.
2. **A merged branch is deleted with `force: true`.** Cader's ancestor check against origin's default branch is the rule the spec states; git's `-d` checks something else. `DeleteBranchHandler` already uses `force: true` after the user's yes.
3. **A delete git still refuses leaves the removal reported.**
   - **How:** `WorktreeRemover.DeleteBranchAsync` catches `GitException` and answers the new `RemoveWorktreeOutcome.RemovedBranchNotDeleted(FolderName, BranchName Branch, string Failure, bool FolderLeft)`, worded "Removed fix-123; branch fix-123 kept: <git's words>" in `WordingOfOutcomes`, toned as a warning.
   - **On the Sessions tab:** `DeleteSessionHandler` passes it through its existing `RemovedWorktree(Name, RemoveWorktreeOutcome, TabClosed)` arm, so it needs no case of its own.
   - **Example:** git refuses when the branch is checked out in another worktree after all, or `.git\packed-refs.lock` is held.
4. **A started launch never throws past `StartedAsync`.**
   - **The recent repository:** writing it catches `IOException` and `UnauthorizedAccessException`, records `Failed(FailureSite.Effect, CodeName.For(typeof(LaunchService)), ExceptionSummary.From(...))` in the diagnostic log (no message text, per the privacy rules), and goes on.
   - **The listing:** a `ClaudeCliException` or `ProcessTimedOutException` answers `LaunchOutcome.NotListedYet(jobId, update)`, which the screen already words as "started; press Enter on it once it appears".
   - **Wiring:** `LaunchService` takes the `DiagnosticLog` in `Composition\LaunchParts`. `FailureSite.Effect` is the site, as for any effect that throws, so no new event case is needed.

## Risks / Trade-offs

- **A forced delete could lose commits** if the branch gained one between the check and the delete → the check runs after the worktree is gone, so the branch is checked out nowhere; a commit made elsewhere by `git update-ref` in that millisecond is not guarded, which is accepted.
- **Reading merged after removal costs one git call later in the flow** → it is the same one call, moved.
- **A listing failure after the start hides a real CLI problem** → the next timed listing shows the same failure on the Sessions tab's banner, as any listing failure does.

## Open Questions

None.
