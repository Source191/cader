# Spec Delta

## ADDED Requirements

### Requirement: A branch checked out nowhere has a row
The worktrees view SHALL list, for each repository under the roots, every local branch that is checked out neither in the main checkout nor in any linked worktree, as a row with the health ⑂ branch whose WORKTREE cell names the repository and the branch. The local branch whose upstream is origin's default branch SHALL NOT have such a row. Each such row SHALL be one row of its own, never merged with another row of the same repository.

#### Scenario: A branch with no worktree
- **WHEN** Northwind.Orders's main checkout is on main, its one linked worktree is on fix-123, and it also has the local branch spike-refunds
- **THEN** the view shows a row reading Northwind.Orders › spike-refunds with ⑂ branch in HEALTH, beside the fix-123 worktree's row

#### Scenario: The default branch checked out nowhere
- **WHEN** Northwind.Orders's main checkout is on fix-123 and its local main tracks origin/main, origin's default branch
- **THEN** no row is shown for main, and none for fix-123, which the main checkout has

#### Scenario: Two branch rows in one repository
- **WHEN** Northwind.Orders has the branches spike-refunds and old-idea checked out nowhere, and the user selects old-idea
- **THEN** both rows are shown, the selection stays on old-idea after the next scan, and the pane describes old-idea

### Requirement: A flagged branch only on origin has a row
The worktrees view SHALL list a branch that exists on origin, as of the last fetch, with no local branch of that name, only when its tip was authored with the repository's git user email and it is flagged left behind or closed. Its row SHALL read ○ origin in HEALTH. Origin's default branch SHALL never have such a row.

#### Scenario: Left behind on origin
- **WHEN** origin/c-new-bug has no local branch, its tip is yours, and its merged pull request #298's head is one commit behind it
- **THEN** the view shows a row reading Northwind.Orders › c-new-bug with ○ origin in HEALTH and ▲ left behind in BRANCH

#### Scenario: Someone else's branch
- **WHEN** origin/dependabot/nuget/bump has no local branch, its only pull request was closed, and its tip was authored by another email
- **THEN** no row is shown for it

#### Scenario: Merged with nothing left behind
- **WHEN** origin/fix-88 has no local branch, its tip is yours, and its merged pull request's head is its tip
- **THEN** no row is shown for it

### Requirement: Every row shows its branch's flag
The worktrees table SHALL show a BRANCH column on every row that names a branch, with the most urgent flag that applies as a glyph and a word in the flag's colour: ▲ left behind (red), ✗ gone (amber), ✗ closed (amber), · no PR (dim). It SHALL be blank when none applies, while the branch's pull request has not been looked up, when it could not be looked up, and on a row that names no branch.

#### Scenario: Left behind wins over gone
- **WHEN** a worktree's branch has a merged pull request whose head is two commits behind the branch, and its upstream has been deleted on origin
- **THEN** its BRANCH cell reads ▲ left behind

#### Scenario: Gone after a merge
- **WHEN** a branch row's upstream has been deleted on origin and its merged pull request's head is its tip
- **THEN** its BRANCH cell reads ✗ gone

#### Scenario: Closed without merging
- **WHEN** a worktree's branch has only an abandoned Azure DevOps pull request and its upstream still exists
- **THEN** its BRANCH cell reads ✗ closed

#### Scenario: No pull request
- **WHEN** a branch row's branch has never been pushed and the host lists no pull request for it
- **THEN** its BRANCH cell reads · no PR

#### Scenario: An open pull request
- **WHEN** a worktree's branch has an open pull request and its upstream exists
- **THEN** its BRANCH cell is blank

#### Scenario: The lookup is unavailable
- **WHEN** the GitHub CLI is not signed in
- **THEN** no BRANCH cell reads · no PR or ✗ closed, and a gone upstream still reads ✗ gone

#### Scenario: No origin
- **WHEN** a repository has no origin remote
- **THEN** none of its rows reads · no PR

### Requirement: Left behind is judged against the merged head
A branch SHALL be left behind when the pull request its rows show (the open one, otherwise the newest) has merged, records a head commit, and the branch has commits that head does not contain. A branch whose merged pull request's head commit is unknown to git, or not recorded, SHALL NOT be flagged left behind.

#### Scenario: A squash merge
- **WHEN** a branch's pull request was squash-merged, so none of its commits is on main, and its tip is the pull request's head
- **THEN** it is not flagged left behind

#### Scenario: A head git has not fetched
- **WHEN** a merged pull request's head commit is not in the repository
- **THEN** the branch is not flagged left behind

### Requirement: Branches are judged without reaching the network
A worktrees scan SHALL judge origin's branches and each upstream's state from git's own records as of the last fetch, and SHALL NOT fetch or prune.

#### Scenario: Deleted on origin since the last fetch
- **WHEN** a branch's upstream was deleted on GitHub and no fetch has run since
- **THEN** its BRANCH cell does not read ✗ gone, and no git process that reaches the network is started

### Requirement: The pane explains the branch's flag
For a row whose branch is flagged, the detail pane's Branch line SHALL follow the branch with the flag's reason in full: the number of commits after the pull request merged, that the upstream is gone as of the last fetch, which pull request was closed, or that there is none.

#### Scenario: Left behind
- **WHEN** the c-new-bug row is selected and its branch has one commit after pull request #298 merged
- **THEN** the pane reads Branch c-new-bug · 1 commit after #298 merged

#### Scenario: Gone
- **WHEN** a row whose upstream is gone is selected
- **THEN** the pane's Branch line says origin no longer has the branch as of the last fetch

### Requirement: A branch row is deleted only when no work is lost
On a branch row, d SHALL ask before deleting the local branch when every commit on it is on a remote branch or inside its merged pull request's head, and SHALL otherwise refuse and say how many commits would be lost. D SHALL delete the local branch whatever it holds after the user types its name. Both SHALL refuse on an origin row, and nothing SHALL be deleted on origin.

#### Scenario: A pushed branch
- **WHEN** the user presses d on a branch row whose every commit is on origin and answers Delete branch
- **THEN** the local branch is deleted, the row goes on the next scan, and the status line says the branch was deleted

#### Scenario: Kept
- **WHEN** the user presses d on that row and answers Keep
- **THEN** nothing is deleted

#### Scenario: A squash-merged branch gone from origin
- **WHEN** the user presses d on a branch row whose upstream is gone and whose tip is its merged pull request's head
- **THEN** Cader asks, and deletes the local branch on Delete branch

#### Scenario: Unpushed commits
- **WHEN** the user presses d on a branch row with two commits on no remote branch and no merged pull request containing them
- **THEN** nothing is asked or deleted and the status line says two commits would be lost and D forces it

#### Scenario: Forced
- **WHEN** the user presses D on that row and types spike-refunds
- **THEN** the local branch is deleted

#### Scenario: An origin row
- **WHEN** the user presses d or D on an origin row
- **THEN** nothing is deleted and the status line says Cader never deletes a branch on origin

### Requirement: Keys on a row with no folder
On a branch row or an origin row, p SHALL open its pull request, c SHALL copy the repository's folder and P SHALL prune the repository, as on any row; n, o, f, s and Enter SHALL do nothing but say the branch has no worktree.

#### Scenario: New session on a branch row
- **WHEN** the user presses n on the spike-refunds branch row
- **THEN** no session starts and the status line says spike-refunds has no worktree

#### Scenario: Pull request of an origin row
- **WHEN** the user presses p on the c-new-bug origin row
- **THEN** pull request #298 opens in the browser

## MODIFIED Requirements

### Requirement: Worktrees are listed across all roots
The worktrees view SHALL list every linked worktree of every git repository found directly under the configured roots, wherever the worktree's folder is (under the worktree root, beside the repository or elsewhere), using git's machine-readable worktree listing, each as a row whose WORKTREE cell names the repository and the folder, with its branch in the detail pane and its branch's flag in BRANCH.

#### Scenario: Two repositories with worktrees
- **WHEN** two repositories under the roots each have one linked worktree
- **THEN** the view shows two rows, each with a WORKTREE cell reading its repository › its folder's slug, and its branch on the pane's Branch line when selected

#### Scenario: Old and new placements together
- **WHEN** Northwind.Orders has one worktree beside it from an earlier version and one under C:\Users\me\source\worktrees
- **THEN** the view shows both rows, each reading Northwind.Orders › and its slug

### Requirement: A worktree's health reads without colour
The worktrees table SHALL show each row's health in a HEALTH column as a glyph and the health's word, in the row's colour: ✓ ok, ◆ locked, △ orphan, ✗ broken, □ empty, ↗ elsewhere, ⑂ branch (dim), ○ origin (amber); no two healths SHALL share a glyph. The help and the HEALTH header's tooltip SHALL list every glyph with its word.

#### Scenario: Every health in the table
- **WHEN** the table lists one worktree of each health, one branch row and one origin row
- **THEN** their HEALTH cells read ✓ ok, ◆ locked, △ orphan, ✗ broken, □ empty, ↗ elsewhere, ⑂ branch and ○ origin

#### Scenario: Two amber healths
- **WHEN** a locked worktree and an elsewhere worktree are listed
- **THEN** both rows are amber, one HEALTH cell reads ◆ locked and the other ↗ elsewhere

#### Scenario: The help lists the glyphs
- **WHEN** the help is shown on the worktrees view
- **THEN** its HEALTH line lists ✓ ok, ◆ locked, △ orphan, ✗ broken, □ empty, ↗ elsewhere, ⑂ branch and ○ origin, and the header's tooltip shows the same words

### Requirement: The worktrees table title counts rows and names the selection
The worktrees view's table title SHALL count the worktree rows and the branch and origin rows apart, leaving out a count of none, name the filter when one is set, and name the selected row's folder, or its branch when it has no folder.

#### Scenario: No filter
- **WHEN** twelve worktrees are listed with no filter and Northwind.Orders-fix-123 is selected
- **THEN** the table title reads 12 worktrees, selected: Northwind.Orders-fix-123

#### Scenario: With a filter
- **WHEN** a filter of "orp" matches one of twelve rows, the folder Northwind.Orders-orphaned
- **THEN** the table title reads 1 of 12 worktrees match "orp", selected: Northwind.Orders-orphaned

#### Scenario: Worktrees and branches
- **WHEN** ten worktrees, two branch rows and one origin row are listed with no filter and the branch row spike-refunds is selected
- **THEN** the table title reads 10 worktrees, 3 branches, selected: spike-refunds
