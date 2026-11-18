## ADDED Requirements

### Requirement: Rows are grouped by repository
The worktrees table SHALL group its rows by the repository each names (for a leftover, the repository it belonged to; for an empty leftover, the root it sits under), headed by a row reading the repository's name in bold orange, with a faint rule running across the rest of the row; the group's rows SHALL follow it with their WORKTREE cells indented by two cells.

#### Scenario: A group's heading
- **WHEN** Northwind.Orders has the worktrees fix-123 and spike
- **THEN** one bold orange row reading Northwind.Orders, followed by a faint rule, comes before both rows, whose WORKTREE cells start two cells in

### Requirement: Groups run A to Z
Groups SHALL run A to Z by repository name ignoring case, two of one name in folder order. Within a group the worktree folders SHALL come before the branch and origin rows, each A to Z by name ignoring case.

#### Scenario: Two repositories
- **WHEN** Northwind.Orders has the worktree fix-123 and the branch spike-refunds checked out nowhere, and Northwind.Billing has the worktree invoice-pdf
- **THEN** the table reads a Northwind.Billing heading and invoice-pdf, then a Northwind.Orders heading, fix-123 and spike-refunds

#### Scenario: Case is ignored
- **WHEN** the repositories cader and Northwind.Orders each have a worktree
- **THEN** the cader group comes first

#### Scenario: Repositories under two roots
- **WHEN** Northwind.Zeta is under C:\Users\me\source\repos and Northwind.Alpha under D:\work
- **THEN** the Northwind.Alpha group comes first

### Requirement: A heading is never selected
A heading SHALL never be selected: the arrow keys, a scan and a filter SHALL step over it as over no row, and the title and counts SHALL count no heading. A filter SHALL keep a heading only while at least one of its rows matches, and SHALL match rows as before, the repository's name included. A heading SHALL show no tooltip.

#### Scenario: The selection steps over a heading
- **WHEN** invoice-pdf, the last row of the Northwind.Billing group, is selected and the user presses Down
- **THEN** fix-123, the first row under the Northwind.Orders heading, is selected

#### Scenario: A filter by repository
- **WHEN** the user filters by billing
- **THEN** the Northwind.Billing heading and its rows stay, and no other heading shows

## MODIFIED Requirements

### Requirement: Worktrees are listed across all roots
The worktrees view SHALL list every linked worktree of every git repository found directly under the configured roots, wherever the worktree's folder is (under the worktree root, beside the repository or elsewhere), using git's machine-readable worktree listing, each as a row under its repository's heading whose WORKTREE cell names the folder, with its branch in the detail pane and its branch's flag in BRANCH.

#### Scenario: Two repositories with worktrees
- **WHEN** two repositories under the roots each have one linked worktree
- **THEN** the view shows two headings, each naming its repository, and under each one row whose WORKTREE cell reads its folder's slug, with its branch on the pane's Branch line when selected

#### Scenario: Old and new placements together
- **WHEN** Northwind.Orders has one worktree beside it from an earlier version and one under C:\Users\me\source\worktrees
- **THEN** the view shows both rows under the Northwind.Orders heading, each reading its slug

### Requirement: Leftover folders are detected
The view SHALL also show folders that git no longer recognises: a folder under a repository's .claude\worktrees that the listing omits is an orphan; a folder directly under a root or under the worktree root whose .git file points at a worktree registration that no longer exists is broken; an empty folder named with a .worktrees suffix directly under a root, and any empty folder directly under the worktree root, is empty.

#### Scenario: Orphan folder
- **WHEN** a folder exists under .claude\worktrees but is absent from git's listing
- **THEN** the view shows it with health orphan

#### Scenario: Broken registration
- **WHEN** a sibling folder's .git file names a worktree registration whose repository no longer exists
- **THEN** the view shows it with health broken

#### Scenario: Broken registration under the worktree root
- **WHEN** a folder under C:\Users\me\source\worktrees has a .git file naming a worktree registration that no repository under the roots lists
- **THEN** the view shows it with health broken

#### Scenario: Empty leftover
- **WHEN** an empty folder named Northwind.Orders.worktrees sits directly under a root
- **THEN** the view shows it with health empty

#### Scenario: Empty folder under the worktree root
- **WHEN** an empty folder C:\Users\me\source\worktrees\Northwind.Orders-fix-123 sits directly under the worktree root C:\Users\me\source\worktrees, which is not a repository root
- **THEN** the view shows it with health empty, under a heading naming the worktree root, its WORKTREE cell naming the folder, and the remove key deletes it without asking

#### Scenario: A folder under the worktree root that holds files but no .git file
- **WHEN** C:\Users\me\source\worktrees\Northwind.Notes holds a file and no .git file
- **THEN** the view does not show it

#### Scenario: Worktree of a repository outside the roots
- **WHEN** a folder under C:\Users\me\source\worktrees has a .git file naming the registration D:\other\Northwind.Ledger\.git\worktrees\fix, that registration exists, D:\other\Northwind.Ledger\.git exists, and D:\other is not a root
- **THEN** the view shows it with health elsewhere under the Northwind.Ledger heading, and the status line says its repository D:\other\Northwind.Ledger is not under the roots

### Requirement: One WORKTREE column names the worktree
The worktrees table SHALL show the folder in one WORKTREE column, under its repository's heading, as the folder's part after the repository (the slug of a folder named `<repository>-<slug>`, else the folder's label) without the repository's name, elastic in width and cut at its end when too long, followed by HEALTH, LAST COMMIT and SESSION; a heading too long for the column SHALL lose its middle, marked with an ellipsis, and SHALL never widen a column. LAST COMMIT SHALL be right-aligned, as AGE is on the Sessions tab, so its durations line up on their last character.

#### Scenario: A worktree Cader made
- **WHEN** Northwind.Billing has the worktree C:\Users\me\source\worktrees\Northwind.Billing-invoice-pdf on branch invoice-pdf
- **THEN** its row is under the Northwind.Billing heading, its WORKTREE cell reads invoice-pdf, and the pane reads Branch invoice-pdf

#### Scenario: A leftover folder
- **WHEN** a broken leftover C:\Users\me\source\worktrees\Northwind.Sales-republish once belonged to Northwind.Sales
- **THEN** its row is under the Northwind.Sales heading, its WORKTREE cell reads republish, its HEALTH cell reads ✗ broken and its row is red

#### Scenario: A folder that does not follow the naming
- **WHEN** Northwind.Orders has a hand-made worktree at C:\Users\me\source\worktrees\spike on branch spike
- **THEN** its row is under the Northwind.Orders heading and its WORKTREE cell reads spike

#### Scenario: Filter by health
- **WHEN** the user filters by broken
- **THEN** the broken rows stay in the table under their headings, each with its HEALTH cell reading ✗ broken

#### Scenario: The help names the column
- **WHEN** the help is shown on the worktrees view
- **THEN** it lists WORKTREE with its meaning, saying the row's repository is its heading, and the header shows the same words on hover

#### Scenario: Last commits line up at the right
- **WHEN** Northwind.Billing-invoice-pdf last committed five minutes ago, Northwind.Orders-fix-123 three days and four hours ago, and an empty leftover folder has no commit
- **THEN** the LAST COMMIT cells 5m, 3d4h and - each end at the column's right edge, and the WORKTREE cells start two cells in from its left edge, under their headings

### Requirement: A branch checked out nowhere has a row
The worktrees view SHALL list, for each repository under the roots, every local branch that is checked out neither in the main checkout nor in any linked worktree, as a row with the health ⑂ branch under the repository's heading, whose WORKTREE cell names the branch. The local branch whose upstream is origin's default branch SHALL NOT have such a row. Each such row SHALL be one row of its own, never merged with another row of the same repository.

#### Scenario: A branch with no worktree
- **WHEN** Northwind.Orders's main checkout is on main, its one linked worktree is on fix-123, and it also has the local branch spike-refunds
- **THEN** the view shows a row reading spike-refunds with ⑂ branch in HEALTH under the Northwind.Orders heading, after the fix-123 worktree's row

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
- **THEN** the view shows a row reading c-new-bug with ○ origin in HEALTH and ▲ left behind in BRANCH, under the Northwind.Orders heading

#### Scenario: Someone else's branch
- **WHEN** origin/dependabot/nuget/bump has no local branch, its only pull request was closed, and its tip was authored by another email
- **THEN** no row is shown for it

#### Scenario: Merged with nothing left behind
- **WHEN** origin/fix-88 has no local branch, its tip is yours, and its merged pull request's head is its tip
- **THEN** no row is shown for it
