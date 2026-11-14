# Proposal

## Why

The worktree root on the owner's machine holds 34 empty folders that Cader neither lists nor can remove. When a tab or program still has a worktree's folder open, `git worktree remove` deletes the folder's contents, its `.git` file and its registration, then fails on the folder itself with "Permission denied" (reproduced with git 2.56.0.windows.4). Cader assumes the registration survives for a later remove. In fact the folder becomes invisible: the worktrees view lists children of the worktree root only when they hold a `.git` file. The status line then tells the user to remove it again from a row that never appears, and the branch is left behind (seven `dx-*` branches in one repository).

## What Changes

- **In-use folder is waited for:** when git reports a worktree's folder in use, Cader tries to delete what git left for a few seconds, because the holder is usually a shell or `claude` process that is just ending (delete closes the session's tab moments before).
- **The removal is finished either way:** git has already dropped the registration, so Cader forgets the created-worktree record and deals with the branch as after any removal (deleted when created here and merged, otherwise asked about).
- **Honest words:** when the folder is still held after the wait, the status line says the worktree was removed but its folder was left because a tab or program still has it open, and to remove it from the worktrees view once that is closed. The in-use wording no longer claims a registration remains.
- **Empty folders under the worktree root are listed:** an empty folder directly under the worktree root is shown with health empty, as an empty `.worktrees` folder under a root already is, so the remove key deletes it without a prompt. This also surfaces the empty folders already on disc.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `worktree-management`: the leftover detection requirement adds empty folders directly under the worktree root; the removal requirement replaces "leave the registration for a later remove or prune" with the wait, the finished removal and the folder left as an empty leftover.
- `session-attach`: delete's in-use requirement and scenarios say the worktree is removed, its branch dealt with, and its folder left as an empty leftover in the worktrees view, instead of the worktree still being listed.

## Impact

- **Code:** `Application\Actions\WorktreeRemover` (the wait, then the record and branch on the in-use path; gains a `TimeProvider`), `Application\WorktreeInventory` (empty children of the worktree root), `RemoveWorktreeOutcome` and `DeleteSessionOutcome` (the removal cases say whether the folder was left), `WordingOfOutcomes` (the words and tones), `DeleteSessionHandler` (carries the fact through). The tool project's `ConsoleComposition` and `tools\ReadmeScene\SceneHost` pass the clock.
- **Tests:** `RemoveWorktreeHandlerShould`, `DeleteSessionHandlerShould`, `WorktreeInventoryLeftoversShould`, `WordingOfOutcomesShould`, `StatusTonesShould`; an Infrastructure fact against real git and a folder held open by a child process pins git's behaviour.
- **Docs:** the comment in `WorktreeRemover`, `.claude\rules\windows.md`'s line on git's "Permission denied", the user guide's worktrees section where it describes a folder in use, and the changelog.
- **Cost:** a removal whose folder stays held takes about five seconds longer before it reports.

## Out of scope

- Multiple Claude accounts, which is a later change.
- Listing folders under the worktree root that hold files but no `.git` file (a removal git half-finished because a file inside was locked). The wait usually clears those, and listing them would also list anything else kept in the worktree root.
- Empty folders directly under a repository root other than the `.worktrees` ones already listed (worktrees placed beside their repository by versions before 0.8.0), which could be a new repository the user is about to clone into.
- Guessing which repository an empty folder belonged to from its name; its row names the worktree root, as an empty `.worktrees` row names its root.
- Waiting before a leftover folder is deleted with the remove key; a held leftover is still reported at once.
- Deleting the 34 existing empty folders; once listed, the remove key deletes each.
