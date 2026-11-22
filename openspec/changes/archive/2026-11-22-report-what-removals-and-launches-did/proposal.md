## Why

Two actions can finish their main work and then tell the user they failed, so the status line contradicts what has happened on disc.

- **A merged worktree's removal:** Cader judges a branch merged when it is an ancestor of origin's default branch, then deletes it with `git branch -d`. But git judges `-d` against the branch's upstream, or against HEAD of the main checkout once that upstream has gone. Take the everyday flow: the branch is pushed with `-u`, its pull request is merged, origin deletes the branch, and the deletion is pruned (Cader's own origin pruner does that). Then `git branch -d` answers "the branch is not fully merged" whenever the local default branch is behind.
  - **The effect:** the worktree has already been removed and forgotten, and the branch is kept, yet the status line shows the removal as a failure, with git's words. Reproduced in scratch repositories on 22 November 2026 (git refused, exit 1). It happens from `d` on the Worktrees tab and from Delete on the Sessions tab.
- **A started session:** after the supervisor has started it, `LaunchService` lists the sessions to find the new one and records the recent repository. If either throws (the CLI refusing the listing, the state folder refusing the write), the exception escapes the launch. The status line then says the launch failed and no tab opens, though the session is running.

**Implement with:** Sonnet 5.5 at medium effort: two contained fixes in Application handlers with sociable tests over the existing fakes, plus one real-git adapter fact.

## What Changes

- **The merge check:** a branch Cader created is judged merged after its worktree has been removed, when it is checked out nowhere and so cannot gain a commit, and is then deleted with git's force, since Cader's own check against origin's default branch is the one that counts.
- **When the branch still cannot be deleted** (git refuses for another reason), the outcome says the worktree was removed and the branch was kept, with git's words, never that the removal failed.
- **A session the supervisor has started** is never reported as a failed launch: a listing that fails afterwards is reported as started and not listed yet (press Enter on it once it appears), and a recent repository that cannot be recorded is logged and the launch carries on.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `worktree-management`: the merge state that decides a branch's automatic deletion is read once its worktree is gone, the deletion follows that check rather than git's, and a branch that cannot be deleted leaves the removal reported as done.
- `session-launch`: a session the supervisor started is never reported as a failed launch.

## Out of scope

- A failure of the `.worktreeinclude` copy after the worktree was made; that is still reported as a failed launch, as it is today.
- Squash-merged branches, which are not ancestors of origin's default branch and are still asked about.
- Multiple Claude accounts beyond naming a session's owner, a later change.

## Impact

- `Application\Actions\WorktreeRemover` (merge check after removal, force delete, a new `RemoveWorktreeOutcome` case), `RemoveWorktreeHandler` and `DeleteSessionHandler` (they stop computing `merged` before removal), `WordingOfOutcomes` and `Wording.ToneOf` for the new case.
- `Application\Launch\LaunchService` (the listing and the recent record after the start), its `DiagnosticLog` from `Composition\LaunchParts`.
- `CHANGELOG.md` and the version in `src\Source191.Cader\Source191.Cader.csproj`.
