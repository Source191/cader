# Proposal

## Why

The Worktrees tab only sees a branch while a worktree has it checked out, so the branches that most need attention are invisible. On 15 November 2026 a branch review of this repository found two of yours that Cader could not show: `c-new-bug`, whose pull request had merged but which then gained a commit that never reached `main` (only origin had it, as the local branch was gone), and `rewrite-static-initialisers`, a live branch with no worktree. Finding them took a round of `git` and `gh` commands by hand. Cader already reads each branch's pull request and whether it is merged, so it can say which of your branches are dead, which are live, and which hold work that would otherwise be lost.

## What Changes

- **Branch rows:** each repository's local branches that are checked out nowhere (not in the main checkout or any linked worktree) get a row on the Worktrees tab with the health `⑂ branch`. The local branch that tracks origin's default branch is left out, as every repository has one.
- **Origin rows, only when they need you:** a branch that exists only on origin (as of the last fetch), whose tip you authored, gets a row with the health `○ origin` only when it is flagged left behind or closed. Origin rows are view-only.
- **A BRANCH column** on every row of the Worktrees tab, worktree rows included, with one glyph and word, most urgent first:
  - **`▲ left behind`:** the branch's pull request has merged, but the branch has commits after the merged head (the `c-new-bug` case). Measured against the pull request's own head commit, so squash merges are judged correctly.
  - **`✗ gone`:** the local branch's upstream has been deleted on origin.
  - **`✗ closed`:** the branch's pull request was closed or abandoned without merging.
  - **`· no PR`:** the branch has no pull request.
  - Blank when none applies, or while the lookup has not answered.
- **The detail pane** gives the flag's reason in full on the Branch line (for example, `1 commit after #298 merged`), and says that `gone` is as of the last fetch.
- **Deleting a branch row:** `d` asks before deleting the local branch, as Cader already asks about a branch a removal leaves behind, and refuses when it holds commits that would be lost (on no remote branch and not inside its merged pull request's head); `D` deletes it whatever it holds after you type its name. Both refuse on an origin row, which Cader never deletes.
- **Other keys on a branch or origin row:** `p` opens its pull request, `c` copies the repository's folder and `P` prunes the repository, as on any row; `n`, `o`, `f`, `s` and Enter say the branch has no worktree.
- **The pull request lookups read the head commit** (`headRefOid` from `gh`, `lastMergeSourceCommit` from `az`, the latter checked and recorded first), which the left-behind flag needs.
- **No fetching:** origin's state is git's own as of your last fetch; a scan never reaches the network for branches.
- **The table title** counts worktrees and branches apart: `10 worktrees, 3 branches`.

## Out of scope

- Fetching or pruning origin from Cader, and any key that writes to origin (deleting an origin branch, closing a pull request).
- A separate Branches tab.
- Opening a worktree on an existing branch from a branch row (the wizard has no existing-branch mode).
- The branch the main checkout is on: it is in use, the main checkout has no row today, and it gets no row or flag.
- Branches on remotes other than origin, and branches only on origin whose tip someone else authored (Dependabot's, a colleague's), which would fill the table and need a lookup each.
- Multiple Claude accounts, which is a later change.

## Capabilities

### New Capabilities

(none)

### Modified Capabilities

- `worktree-management`: the Worktrees tab lists branch rows and flagged origin rows beside the worktrees, shows a BRANCH column of flags, explains the flag in the pane, deletes a branch row by the work-at-risk rules, and counts worktrees and branches apart in its title; the health glyph list gains `⑂ branch` and `○ origin`.
- `pull-requests`: a lookup records the pull request's head commit.
- `detail-pane`: a flagged branch's reason takes the merge state's place on the Branch line, and a branch or origin row's pane is titled with the branch, reads no git and shows its repository in place of the folder.

## Impact

- **Domain:** `WorktreeHealthKind` gains `Branch` and `Origin`; new `BranchFlagKind` and its rule; new `CommitId` text value; `PullRequest` gains its head commit.
- **Ports:** `IGit` gains a branch listing (local and origin branches, each upstream's state, whether the tip is yours), a count of a branch's commits after a given commit, and a count of a branch's commits on no remote branch.
- **Adapters:** `GitCli` for the three queries and `--` before branch names; `GitHubPullRequests` and `AzureDevOpsPullRequests` read the head commit.
- **Application:** `WorktreeRow` gains its own key, placement and flag; `WorktreeInventory` adds branch and origin rows; `WorktreesView` routes `d` and `D` on a branch row to the existing branch question and `DeleteBranch` handler; `Wording` gains the BRANCH column, the flag words and the title count.
- **Docs:** `docs\user-guide.md`'s Worktrees section, `.claude\rules\console-screen.md`, `CHANGELOG.md`, and a minor version bump.
