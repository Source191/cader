# Design

## Context

- `WorktreeInventory.ScanAsync` builds `WorktreeRow`s from git's worktree listing (skipping the main checkout's entry, so the main checkout has no row) and the leftover folders, then `WithPullRequest` fills each row from `PullRequestCatalogue.Find` and `RefreshStaleAsync` starts the lookups that are due. A row never waits for a lookup: the answer lands on a later scan (every 15 seconds while the tab shows).
- Rows are identified by folder: `WorktreesViewState`'s `ListPolicy.KeyOf` is `row.Path.Key` and `WorktreeDetails.KeyOf` is `row.Path`. Sessions are matched to rows by folder.
- `Find` shows one pull request per branch through `Domain\PullRequests.ForBranch`: the open one, otherwise the newest. A missing one carries a `PullRequestGap`: `NoPullRequest` when the host listed none, and `NoBranch`, `NoRemote`, `UnknownHost`, `CliMissing`, `NotSignedIn`, `Looking`, `Failed` or `BranchNotGivenToAz` when it could not say.
- The lookups ask `gh pr list --json number,url,state,isDraft,headRefName,createdAt` and `az repos pr list`; neither reads a head commit today.
- `IGit` has `IsMergedAsync` (false when git fails), `CountUnpushedCommitsAsync` (for a worktree's HEAD) and `DeleteBranchAsync(force)`. `PortParametersShould` wants Domain values, not strings, on new port parameters.
- Branch deletion already exists for a branch a removal leaves behind: `DialogEffects.AskAboutBranch` asks `Wording.DeleteBranchQuestion` with the Delete branch and Keep buttons, and a yes raises `DeleteBranch(BranchToConfirm)`, whose `DeleteBranchHandler` runs `git branch -D`. Cader records the branches it created in `ICreatedWorktrees` and forgets each record when its worktree is removed.

## Goals / Non-Goals

**Goals:**
- The flags are right for merge commits and squash merges alike, on GitHub and Azure DevOps.
- A scan stays offline and starts no more git processes per repository than it must: one branch listing, one count per branch whose shown pull request has merged, and one per branch row for its commits at risk.
- Nothing personal leaves the git adapter: the user's email is compared there and never returned or logged.
- The existing branch question and `DeleteBranch` handler are reused, not copied.

**Non-Goals:**
- Showing the BRANCH flag on the Sessions tab.
- Tracking branches of repositories outside the roots (`elsewhere` worktrees keep their row and get no branch rows).

## Decisions

### 1. Branch rows are `WorktreeRow`s with their own key and a placement

A branch row and an origin row are `WorktreeRow`s whose `Health.Kind` is `Branch` or `Origin`, whose `Path` is the repository (so the sort, the filter's repository field, `c` and `P` work unchanged), whose `Head` is `OnBranch`, and whose `Session` is none (a session in the main checkout is not the branch's). Two new members:

- **`Key`:** the row's identity, `Path.Key` for a folder row as today and `<repository key>|branch|<name>` for a branch or origin row. `ListPolicy.KeyOf` and `WorktreeDetails.KeyOf` use it, so two branch rows of one repository never share a selection or a pane answer.
- **`Placement`:** `Folder` or `NoFolder`, which the reducer reads to decide what `n`, `o`, `f`, `s`, Enter, `d` and `D` do. `FolderExists` stays a folder fact.

Alternative considered: a separate `BranchRow` in a `closed` row hierarchy. It is the cleaner model, but every column, the filter, the pane and the title would need a case per row type for three columns' worth of difference.

### 2. One `for-each-ref` per repository lists every branch

`IGit.ListBranchesAsync(FolderPath repository)` runs `git for-each-ref refs/heads refs/remotes/origin` with a NUL-separated format of the ref name, `%(upstream)`, `%(upstream:track)` (whose `[gone]` is the gone flag), `%(committerdate:iso-strict)` and `%(authoremail)`, and reads `git config user.email` once. It answers `GitBranch` records: `Local(BranchName, Upstream)` where `Upstream` is `None`, `Tracking(RemoteBranch)` or `Gone`, and `OriginOnly(BranchName)`, each with its tip time and `Mine` (the author email equals the user email, ordinal ignoring case). The email is compared inside the adapter and never leaves it. Branches checked out somewhere come from the worktree listing already read, which includes the main checkout's entry. Alternative: `git branch -vv` parsing. Rejected: its layout is for people and changes with config.

### 3. Left behind compares the branch with the shown pull request's head commit

`PullRequest` gains `Head`, a `HeadCommit` of `Known(CommitId)` or `NotRecorded`; `CommitId` is a text value of 40 or 64 hex digits. The adapters read `headRefOid` and `lastMergeSourceCommit.commitId` through `CommitId.TryParse`. When a branch's shown pull request has merged and its head is `Known`, the scan calls `IGit.CountCommitsAfterAsync(FolderPath repository, string revision, CommitId head)` (`git rev-list --count <revision> --not <head>`: commits on the branch that the head does not contain), which answers null when git fails, as it does for a head it has never fetched. More than zero is left behind; null is not judged. Checked on 15 November 2026 against this repository: #298's head `afdacd8b` and `origin/c-new-bug` count 1, the stranded archive commit. Alternative: `IsMergedAsync(branch, origin's default)`. Rejected: after a squash merge none of the branch's commits is on main, so every merged branch would read left behind.

### 4. The flag is a pure Domain rule

`Domain\BranchFlagKind` (`LeftBehind`, `Gone`, `Closed`, `NoPullRequest`, `None`) and `BranchFlags.Of(PullRequestLookup, Upstream, int? commitsAfterMergedHead)` decide it in the spec's order: left behind, gone, closed, no PR. Only `PullRequestGap.NoPullRequest` gives `NoPullRequest`; every other gap gives neither `Closed` nor `NoPullRequest`, so a signed-out CLI or a repository with no origin never paints rows `· no PR`. Each flag's glyph, word, tone and pane reason live in `Wording` (`WordingOfBranches`).

### 5. Origin rows need a lookup first, so they appear a scan later

An origin-only branch is a candidate when it is `Mine` and is not origin's default. Candidates go to `RefreshStaleAsync` with the local branches; a candidate becomes a row only once its lookup has landed and the rule says `LeftBehind` or `Closed`. Repositories that squash-merge keep merged branches on origin until they are deleted there, so this can mean a lookup per such branch; the catalogue's four slots and its one-minute cache bound it. Alternative: list every origin-only branch. Rejected by the owner as noise.

### 6. `d` and `D` on a branch row reuse the branch question and `DeleteBranch`

The scan stores each branch row's commits at risk in `UnpushedCommits`: `IGit.CountCommitsOnNoRemoteAsync(repository, branch)` (`git rev-list --count refs/heads/<name> --not --remotes`), set to zero when the shown pull request has merged and `CountCommitsAfterAsync` answers zero, because those commits are inside what merged. The reducer then decides without git, as it does for a worktree:

- **`d`, nothing at risk:** a new `ConfirmDeleteBranch(BranchToConfirm)` dialog effect; `DialogEffects` asks with the existing title and buttons and a question for a branch with no worktree, and a yes raises the existing `DeleteBranch`. Cader always asks: a branch row is never one it still records creating, because that record goes with the worktree.
- **`d`, commits at risk:** the `NotRemoved`-style refusal naming the count and offering `D`.
- **`D`:** the existing typed confirmation with the branch name as the text to type, then `DeleteBranch`.
- **Origin row:** a status saying Cader never deletes a branch on origin.

`DeleteBranchHandler` is unchanged (`git branch -D`, because git's own `-d` judges against the upstream or HEAD, which says nothing about a squash merge). `WorktreeRemover` and `IFolderDeleter` are not involved: no folder is touched. The count is as old as the last scan (at most 15 seconds); a branch checked out nowhere gains commits only from a git command the user runs, which is accepted.

### 7. The Azure DevOps head commit is the last merge source

Azure DevOps documents a pull request's `lastMergeSourceCommit` as "the commit at the head of the source branch at the time of the last pull request merge" ([GitPullRequest](https://learn.microsoft.com/javascript/api/azure-devops-extension-api/gitpullrequest), checked 15 November 2026), and `az repos pr list --output json` returns the REST shape. For a completed pull request that is the head that merged. The fact is recorded in `.claude\rules` with its date and link. Where it is missing (a pull request that never merged, or an older server), the lookup records `NotRecorded`, and the branch is never flagged left behind; the owner's manual check (task 6.3) sees it on a real repository when one is to hand.

### 8. Glyphs

`⑂` (U+2442) and `○` (U+25CB) for the two healths, `▲`, `✗` and `·` for the flags: all single-cell, none with emoji presentation. A task checks `⑂` in Windows Terminal with Cascadia Mono at 120 x 36; if it falls back to a wide glyph, `⎇` (U+2387) replaces it in the spec before the change completes.

### 9. Choices made while building the rows and keys

- **`Placement` is read from the health:** `Branch` and `Origin` are `NoFolder`, every other health `Folder`, so the two can never disagree. `Key` is `Path.Key` for a folder row and `<repository key>|branch|<name>` otherwise; a repository's folder rows come before its branch and origin rows, so the tab still opens on a worktree, and rows of one folder are ordered by the key after the folder name. A message about a row with no folder, such as p's missing pull request, names its branch.
- **The WORKTREE cell names the branch:** a row with no folder shows `Northwind.Orders › spike-refunds` in the slug's place. It is built with the rows rather than with group 5, because without it two branch rows of one repository read the same.
- **A scan never guesses that nothing is at risk:**
  - When git cannot list a repository's worktrees, it gets no branch rows, because nothing says which branches are free.
  - A branch row whose commits on no remote cannot be counted is left out of that scan.
  - `UnpushedCommits` drops to zero only when `CountCommitsAfterAsync` answers exactly 0 for the head `BranchFlags.TryMergedHead` gives. That is the one test of a merged, recorded head, which `BranchFlags.Of` uses too.
- **Origin's default is asked for only when needed:** that is when a repository has a free local branch or an origin candidate. It is one `symbolic-ref` beside the listing.
- **No session on a branch row:** sessions are matched to rows by folder, and a branch row's folder is its repository, so a session there is skipped.
- **`d` and `D` reuse what exists:**
  - `d` with commits at risk answers `StatusMessage.NotRemoved` with `RemovalReason.UnpushedCommits` and offers `D`.
  - `D` raises the existing `ConfirmForceRemove` with the branch name as the text to type. The dialog takes its title and warning from `Wording.ForceTitle` and `ForceWarning` by placement.
  - `ForceConfirmed` on a branch row raises `DeleteBranch`, never `RemoveWorktree`.
  - Keep on the branch question shows `Kept.`, as closing empty tabs does.
  - `Wording.NothingTypedYet` now says "the name to type", as the same dialog asks for a branch.
- **The dialog flow has no behavioural fact:** `DialogEffects` flows run Terminal.Gui dialogs and have no test harness. `DialogEffectsShould` holds only that `ConfirmDeleteBranch` has a flow, and task 6.3's manual check sees `d` ask and delete.

### 10. Choices made while building the column, the pane and the title

- **BRANCH sits after HEALTH:** both describe the row, and LAST COMMIT and SESSION stay at the right. It is `ColumnFit.Natural`, so it is as wide as `▲ left behind` when a row reads that and no wider when none does; a blank cell takes the row's colour, a flag its own through `CellPaint.Text`.
- **The left behind count rides on the row:** `WorktreeRow.CommitsAfterMerge` keeps the count the scan already made, so the pane's `1 commit after #298 merged` costs no second git call.
- **The pane's reasons:** left behind `N commits after #298 merged` (or `!12 completed` on Azure DevOps), gone `origin no longer has it, as of the last fetch`, closed `#301 closed without merging`, no PR `no pull request on its host`. A flagged worktree's Branch line keeps its merge words and adds the reason after ` · `.
- **A row with no folder reads no git for its pane:** its `Path` is the main checkout, whose changes and unpushed commits are not the branch's, and an origin row has no local ref to judge a merge on. Its four fact rows are Health, Branch with the reason and PR, Repository in place of Folder and Created, and Changes: the commits at risk that `d` weighs on a branch row, `no local branch` on an origin row. It lists nothing beneath, and its title is the branch.
- **One name per row:** `WorktreeRow.Name` (the folder, or a row with no folder's branch) is what the table title, the pane's title and p's message use.
- **The title counts branches apart:** `TableTitle.Apart`, an init-only `TitleCount` that is `None` for the other tables, so their titles and facts are unchanged. Branch and origin rows are both counted as branches. A count of none is left out (`2 branches`), except that an empty table still reads `0 worktrees`. Under a filter the matches are one number over both counts: `1 of 10 worktrees, 3 branches match "spik"`.
- **A forced branch deletion names its own dialog:** `DialogName.ForceDeleteBranch` (`Cader (Force delete)` in the tab title), chosen by placement beside `Wording.ForceTitle`, so diagnostics and the tab title no longer call it a removal. The smoke script still opens only `ForceRemove`, on a worktree row.
- **The README scene is unchanged:** its images show the Sessions tab and the new-session wizard, never the Worktrees tab, so a branch row in `scene.json` would show nowhere and would need the scene script to make a branch.

## Risks / Trade-offs

- [Gone and origin rows are as stale as the last fetch] → the pane says so; the console never fetches. A user who never fetches sees no `gone`.
- [Lookups for many origin-only branches in a squash-merging repository] → only your own branches are candidates, the catalogue bounds concurrency and caches answers; the first scan after opening shows origin rows a scan late.
- [A tip authored with a different email than `user.email` (another machine, a web edit)] → such a branch gets no origin row; its local copy, if any, still gets a branch row. Accepted.
- [The pull request's head commit was force-pushed over after merge] → `CountCommitsAfterAsync` counts commits not in the old head, so rewritten history reads left behind. That is right: the rewritten commits are not in what merged.
- [A later closed pull request hides an earlier merged one] → the shown pull request is the newest, so the branch reads `✗ closed` and is not judged left behind. Accepted: the closed one is the latest word on the branch.
- [A branch name that starts with `-` reaching `git branch -D` or `git rev-list`] → `BranchName` only refuses blanks and whitespace, and a ref made with `git update-ref` can start with `-`. `DeleteBranchAsync` passes the name after `--` and the new queries name `refs/heads/<name>`, with an adapter test for a branch named `-x`.
