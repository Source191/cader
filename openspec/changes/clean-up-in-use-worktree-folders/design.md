# Design

## Context

See proposal.md (Why). The code paths involved:

- **`WorktreeRemover.RemoveThroughGitAsync`** runs `git worktree remove`. On a `GitException` with `InUse` (git's "Permission denied" or "Directory not empty", read in `GitCli`) it returns `RemoveWorktreeOutcome.InUse` at once. That skips `ForgetCreatedWorktreeAsync` and the branch step. Its comment says the registration stays.
- **What git does:** reproduced on 13 November 2026 with git 2.56.0.windows.4 against a worktree that another process had as its working directory. Git deletes the contents, the `.git` file and the `.git\worktrees\<name>` registration, then fails on the folder itself and exits 255 with `error: failed to delete '<path>': Permission denied`. Git's `remove_worktree` deletes the registration even when deleting the work tree failed ("there's no going back from here"), so this is not specific to the version.
- **`WorktreeInventory.ScanAsync`** lists the worktree root's children only when they hold a `.git` file (`WorktreeRootCandidates`). Its empty-leftover rule (`IsEmptyLeftover`) covers only `*.worktrees` children of a repository root.
- **`DeleteSessionHandler`** maps the remover's outcomes to `DeleteSessionOutcome`. `DialogEffects` asks about the branch for `RemoveWorktreeOutcome.BranchKept` and `DeleteSessionOutcome.BranchNotMerged`.
- **`IFolderDeleter`** is held only by `WorktreeRemover`. It deletes strictly inside a root or the worktree root, never follows links, and throws `IOException` or `UnauthorizedAccessException` for a folder in use.

## Goals / Non-Goals

**Goals:**
- A removal git half-finishes because of a held folder ends up finished: no registration, no record, the branch dealt with, and the folder either gone or visible as an empty row.
- The common case, a holder that is just ending, finishes with no in-use message at all.
- Every word on the status line is true.

**Non-Goals:**
- Finding out which process holds the folder.
- A diagnostic event for the wait; nothing about it needs measuring yet.
- Any change to deleting a leftover row with the remove key, including a held one.

## Decisions

### Wait inside `WorktreeRemover`, after git reports the folder in use
After `GitException.InUse`, the remover calls `IFolderDeleter.DeleteTree` on the worktree's folder up to 20 times, 250 milliseconds apart (about five seconds), awaiting `Task.Delay(interval, timeProvider, cancellationToken)` between tries. It stops at the first success. A folder outside the roots is never tried (`InsideTheRoots`, as for any leftover); it is simply left. `WorktreeRemover` gains a `TimeProvider`, passed by `ConsoleComposition` and `SceneHost` as `TimeProvider.System`.
- **Why here:** the remover is the one class allowed to delete a folder, and the one place both the Worktrees tab's remove and the session delete pass through. Only this path has a holder that is probably ending, because delete has just ended the tab's shell and `claude rm` the session's process.
- **Why a count of tries, not a deadline:** with `FakeTimeProvider` a test advances the clock until the task finishes. With a fixed number of tries, an advance that lands before the next delay is created only costs the test another loop; it cannot shorten the wait, so the tests stay deterministic.
- **Why `DeleteTree` and not a second `git worktree remove`:** git has already forgotten the worktree and would answer "is not a working tree". What is left is a plain folder. The removal rules already allowed git to delete its contents (clean, or forced), so deleting the rest does what the user asked for.
- **Alternative: wait before running git.** Rejected: nothing tells Cader what to wait for, and most removals are not held.
- **Alternative: retry inside the `Folders` adapter.** Rejected: it would also slow every held leftover delete, and the logic could not be tested in the in-memory suite.

### Check git really dropped the registration before deleting anything
`GitCli` marks any failure whose words hold "Permission denied" or "Directory not empty" as `InUse`. Before this change that chose only a message; now it leads to a delete. So after `InUse` the remover first asks `ListWorktreesAsync` whether the repository still lists the worktree's path. Only when it no longer does (git removed the registration, as `GitWorktreeRemoveShould` shows for a held folder) does the wait and the finished removal follow. When the worktree is still listed, or the listing itself fails, the remover returns `RemoveWorktreeOutcome.InUse` as before and touches nothing: no folder, record or branch. A later remove then sees the same worktree and tries again.
- **Why:** git may print those words for a failure that happens before it removes anything, such as a file it cannot read. Deleting a folder that git still treats as a worktree would destroy what the removal rules protect.
- **Found during implementation** (review of the first pass); it adds one git call to a removal that fails as in use, and none to any other.

### Finish the removal whether or not the folder went
After the wait, the remover runs what follows a successful `git worktree remove` today: forget the created-worktree record, then delete the branch, ask about it, or do neither, as `RemovalDecision.Branch` and the head say. The branch is no longer checked out anywhere, so `git branch -d` works. The outcome says whether the folder was left:
- `RemoveWorktreeOutcome.Removed`, `RemovedAndDeletedBranch` and `BranchKept` gain `bool FolderLeft = false`.
- `DeleteSessionOutcome.RemovedWorktreeAndBranch` and `BranchNotMerged` gain `bool FolderLeft = false`, which `DeleteSessionHandler` copies across. A plain `Removed` (detached or unknown head) already reaches the delete as the `RemovedWorktree` wrapper, which describes it in the remover's own words.
- `RemoveWorktreeOutcome.InUse` and `DeleteSessionOutcome.WorktreeInUse` stay. They are now raised when deleting a leftover row's folder fails, or when git reports a folder in use but still lists the worktree. Their words ("close it and remove again", "remove the folder from the worktrees view") are true in both cases, because the row stays listed.
- **Why a flag and not new cases:** the flag rides on the cases the screens already pattern-match (`BranchKept` and `BranchNotMerged` open the branch question), so `DialogEffects` needs no change. `CreatedHere` and `TabClosed` set the precedent for a bool on an outcome.
- **Alternative: wrap the outcome in a "folder left" case.** Rejected: `DialogEffects` would have to look inside the wrapper to ask about the branch.

### Words and tones
`WordingOfOutcomes` appends one sentence to each flagged case: "The worktree's folder was left because a tab or program still has it open; close that and remove it from the worktrees view." For example: "Removed Northwind.Orders-fix-123 and deleted its merged branch fix-123. The worktree's folder was left because …". It says "the worktree's folder" rather than "its folder", which after "its merged branch" could be read as the branch's. The removal sentences already name the folder; the session-delete sentences say "the worktree", as they do today. A flagged case's tone is Notice instead of Success, in `ToneOf` for both outcome families, with a line each in `StatusTonesShould`. The wording drops the old claim that git keeps the registration.

### List empty children of the worktree root
`WorktreeRootCandidates` becomes a pass over the worktree root's children, when the worktree root exists and is not also a repository root, as it is now:
- A child with a `.git` file is a candidate, as now.
- An empty child (`IFolders.IsEmpty`, whatever its name) becomes a leftover row: `WorktreeObservation.Unregistered(GitPointer.NoGitFile, UnderClaudeWorktrees: false, EmptyLeftover: true)`, with the worktree root as its repository and `ownerExists: false`, exactly as an empty `.worktrees` folder is with its root.
- Anything else is skipped.

An empty child that some repository still registers (git lists it as prunable) is left to the registered row, health broken with "its .git file is gone", which prunes as it deletes. The empty rows are therefore added only for folders not in `registered`, the same filter the broken candidates pass through.

`IFolders.IsEmpty` now runs on every child of the worktree root, after the registered rows' git calls, and a folder can be deleted in that gap. So the `Folders` adapter answers false for a folder that has gone or cannot be read, as `FilesIn` already does for a missing one. One such folder is then skipped rather than failing the whole scan.
- **Why the worktree root as repository:** nothing on disc says which repository an empty folder belonged to, and guessing from the name is unreliable (`Northwind` and `Northwind.Orders` both prefix `Northwind.Orders-fix`). `ownerExists: false` keeps the prune key and pull request lookups away from it, and `RemovalDecision` already deletes an empty row without a prompt and without pruning.

### Pin git's behaviour with a real-git fact
`Infrastructure.Tests` gains a fact over `TemporaryGitRoot`. It starts a child process whose working directory is a fresh worktree, calls `GitCli.RemoveWorktreeAsync`, and asserts the `GitException` is `InUse`, the folder exists and is empty, and `ListWorktreesAsync` no longer lists it. The design rests on this behaviour, so a future git that keeps the registration fails the fact instead of quietly bringing back invisible folders.

## Risks / Trade-offs

- **[A held removal takes about five seconds longer]** → The status line shows its waiting spinner meanwhile, and a holder that is ending usually lets go well within the five seconds.
- **[A half-emptied folder (a locked file inside) can stay hidden]** if it is still locked after the wait and git had already deleted its `.git` file. → Out of scope (proposal); the wait deletes it in the usual case. The status line still says the folder was left, so the user knows to look.
- **[An empty folder the user made on purpose under the worktree root is listed as empty]** → Deleting an empty folder loses nothing, and the worktree root is Cader's own folder for worktrees.
- **[The folder left may sit outside the worktree root]**, for example a worktree beside its repository from before 0.8.0, and so is not listed. → The status line still names it, and it is empty; the user can delete it by hand. Cader has created worktrees under the worktree root since 0.8.0.

## Migration Plan

Nothing to migrate. On the first scan after upgrading, the empty folders already under the worktree root (34 on the owner's machine) appear as empty rows, and the remove key deletes each one.
