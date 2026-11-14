# Tasks

## 1. Git's behaviour, pinned

- [x] 1.1 Red-green: `GitWorktreeRemoveShould.DropTheRegistrationAndEmptyTheFolderWhenAProgramHoldsIt` (Infrastructure, real git over `TemporaryGitRoot`): add a worktree, start a child process (`pwsh -NoProfile -Command Start-Sleep 30`) with the worktree as its working directory, call `GitCli.RemoveWorktreeAsync`, and assert the `GitException` has `InUse` true, the folder exists and is empty, and `ListWorktreesAsync` no longer lists it; the child is killed in a `finally`. It passes against today's code (it pins git, not Cader). Verify with `dotnet test test\Source191.Cader.Infrastructure.Tests -c Debug --no-build --filter-class *GitWorktreeRemoveShould`.

## 2. The fake a folder that is let go

- [x] 2.1 Add `FakeFolders.HeldOpenFor(string path, int attempts)`: `DeleteTree` of that folder throws `IOException` for that many calls, then deletes as usual (`HeldOpen` stays "always"). It is a test double, so it is covered by the remover's tests in group 3. Verify `dotnet build -c Debug`.

## 3. The remover finishes a held removal

- [x] 3.1 Red: `RemoveWorktreeHandlerShould.WaitForAFolderThatIsLetGoAndRemoveTheWorktreeAsUsual`: git answers "Permission denied", the worktree's folder is declared and `HeldOpenFor` two attempts, and the clock (`FakeTimeProvider`, advanced in a bounded loop until the task completes) moves on. Expect `BranchKept(..., FolderLeft: false)`, the folder gone and no "remove" asked of git twice. Green: give `WorktreeRemover` a `TimeProvider` and, on `GitException.InUse`, try `IFolderDeleter.DeleteTree` up to 20 times 250 ms apart with `Task.Delay(interval, timeProvider, cancellationToken)`, only when `InsideTheRoots`; update the constructions in `RemoveWorktreeHandlerShould`, `DeleteSessionHandlerShould`, `ConsoleComposition` and `tools\ReadmeScene\SceneHost` (`TimeProvider.System` in the last two). Verify the fact passes.
- [x] 3.2 Red: `RemoveWorktreeHandlerShould.FinishTheRemovalAndSayTheFolderWasLeftWhenItStaysHeld` (replacing `ReportAWorktreeStillInUse`): the folder is `HeldOpen`, the branch was created here and is merged. Expect `RemovedAndDeletedBranch("Northwind.Orders-fix-123", fix-123, FolderLeft: true)`, `git branch -d fix-123` asked, and the created-worktree record forgotten. Green: add `bool FolderLeft = false` to `RemoveWorktreeOutcome.Removed`, `RemovedAndDeletedBranch` and `BranchKept`, and run the record and branch step after the wait whatever its result. Verify the fact passes.
- [x] 3.3 Red-green: `RemoveWorktreeHandlerShould.LeaveAHeldFolderOutsideTheRootsUntouchedAndStillFinish`: a registered worktree at `D:\Northwind.Archive` that git cannot delete is never handed to `DeleteTree` (`FakeFolders` would throw on it) and the outcome has `FolderLeft: true`. Verify the fact passes.
- [x] 3.4 Red-green: `RemoveWorktreeHandlerShould.AskAboutAHandMadeBranchWhenTheFolderStaysHeld`: expect `BranchKept(..., CreatedHere: false, FolderLeft: true)`, so the screen still asks about the branch. Verify the fact passes.
- [x] 3.5 Refactor: correct the comment at the in-use catch in `WorktreeRemover` (git has already dropped the registration and the contents) and the class and `RemoveWorktreeOutcome.InUse` summaries (now only a leftover's folder that could not be deleted). Verify `dotnet build -c Debug` and the `RemoveWorktreeHandlerShould` class pass.

## 4. Delete carries the fact

- [x] 4.1 Red: `DeleteSessionHandlerShould.FinishTheWorktreeAndSayItsFolderWasLeftWhenItStaysHeld` (replacing `ReportAWorktreeStillInUse`): a merged branch created here gives `RemovedWorktreeAndBranch(Name, fix-123, TabClosed, FolderLeft: true)`; an unmerged one gives `BranchNotMerged(..., FolderLeft: true)` (a `Theory` or two facts). Green: add `bool FolderLeft = false` to those two `DeleteSessionOutcome` cases and copy it across in `DeleteSessionHandler`. Verify the facts pass and `DeleteSessionHandlerShould` stays green.

## 5. Words and tones

- [x] 5.1 Red: `WordingOfOutcomesShould.SayTheFolderWasLeftAfterARemoval` covering `Removed`, `RemovedAndDeletedBranch`, both `BranchKept` forms and the two delete cases with `FolderLeft: true`, each ending "The worktree's folder was left because a tab or program still has it open; close that and remove it from the worktrees view."; the existing in-use facts keep their words. Green: one shared sentence in `WordingOfOutcomes`, appended where `FolderLeft` is true. Verify the facts pass and `SentenceLiteralsShould` and `SpellBritish` stay green.
- [x] 5.2 Red: `StatusTonesShould` lines giving each flagged case `StatusTone.Notice` (the unflagged ones keep `Success`). Green: the `ToneOf` arms for both outcome families. Verify `StatusTonesShould` passes.

## 6. Empty folders under the worktree root are listed

- [x] 6.1 Red: `WorktreeInventoryLeftoversShould.ListAnEmptyFolderUnderTheWorktreeRootAsEmpty`: an empty `C:\Users\me\source\worktrees\Northwind.Orders-fix-123` gives one row with health empty, repository `C:\Users\me\source\worktrees`, `OwnerExists` false and no files. Green: the worktree root's children pass splits into `.git`-file candidates and empty folders, the latter made into rows as an empty `.worktrees` folder is. Verify the fact passes.
- [x] 6.2 Red-green: `WorktreeInventoryLeftoversShould.SkipAFolderUnderTheWorktreeRootThatHoldsFilesButNoGitFile` and `LeaveAnEmptyFolderGitStillRegistersToItsRegisteredRow` (git lists it as prunable: one row, health broken, not a second empty row). Verify both pass.
- [ ] 6.3 Red-green: `WorktreeInventoryLeftoversShould.ListNoEmptyRowsWhenTheWorktreeRootIsARepositoryRoot` (settings whose worktree root is also a root: an empty child without the `.worktrees` suffix is not listed). Verify the fact passes, then update `WorktreeInventory`'s class summary to name the new leftover.

## 7. Docs and release

- [ ] 7.1 Update `.claude\rules\windows.md`'s line on git's "Permission denied" with the verified fact (13 November 2026, git 2.56.0.windows.4: git deletes the contents, the `.git` file and the registration, then fails on the held folder, so `WorktreeRemover` waits and finishes the removal), and add a **Folder in use** bullet to the Worktrees section of `docs\user-guide.md`. Verify by reading both.
- [ ] 7.2 Raise `Version` with `.github\scripts\Step-Version.ps1` and add a `### Fixed` line under Unreleased in `CHANGELOG.md` ("Removing a worktree whose folder a tab or program still had open no longer leaves an empty folder Cader cannot see: Cader waits a few seconds, finishes the removal and its branch either way, and the Worktrees tab lists any empty folder under the worktree root as empty, which d deletes"). Verify `Assert-VersionBumped.ps1` and `Assert-ChangelogUpdated.ps1` against `main`.

## 8. Integration

- [ ] 8.1 Run `dotnet build -c Debug`, `dotnet test -c Debug --no-build` at the solution and `dotnet format --verify-no-changes`. Verify all pass, with the counts reported.
- [ ] 8.2 Manual (owner): on a local build (`tools\install-local.ps1`), open a worktree's shell with `o`, press `d` on it, and check the status line says the folder was left and the empty row appears; close the tab and press `d` on the empty row. Then check the 34 existing empty folders are listed as empty. Verify by eye.

## Workflow follow-up

- Archive the change in the pull request that completes it, so `openspec\specs` describes the code on `main`.
