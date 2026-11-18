## ADDED Requirements

### Requirement: A repository or a root folder is chosen from configured roots
The wizard SHALL offer every git repository's main checkout found directly under the configured repository roots, and each root that exists as a folder choice shown by its full path, grouped under the headings Recent (the most recently used repositories, most recent first), Folders (no repository) (the roots) and Repositories (the rest, alphabetically), in that order. It SHALL NOT offer a linked worktree directly under a root, and SHALL leave out a group with no choices, heading and all.

#### Scenario: Filtering the picker
- **WHEN** the user types part of a repository name
- **THEN** the picker narrows to repositories whose names contain that text, case-insensitively

#### Scenario: Recently used first
- **WHEN** the picker opens
- **THEN** the ten most recently launched repositories appear under Recent, before the rest

#### Scenario: Old sibling worktree under a root
- **WHEN** the root holds Northwind.Orders and its linked worktree Northwind.Orders-fix-123
- **THEN** the picker offers Northwind.Orders and does not offer Northwind.Orders-fix-123

#### Scenario: Roots are folder choices
- **WHEN** the picker opens with the roots C:\Users\me\source\repos and D:\work, and Northwind.Templates was used recently
- **THEN** it lists Recent with Northwind.Templates, then Folders (no repository) with C:\Users\me\source\repos and D:\work, then Repositories with the other repositories

#### Scenario: A root that does not exist
- **WHEN** a configured root folder does not exist
- **THEN** it is not offered as a folder choice

#### Scenario: A group the filter empties
- **WHEN** the user types "Billing" and only Northwind.Billing matches
- **THEN** the picker shows only the Repositories heading and Northwind.Billing

#### Scenario: Filtering by a root's path
- **WHEN** the user types "work" with the root D:\work
- **THEN** D:\work stays listed under Folders (no repository)

### Requirement: The picker's headings are never selected
The wizard SHALL never rest its selection on a heading: it SHALL open, and reselect after the filter changes, on the first choice, and a move or click that lands on a heading SHALL move on to the nearest choice in the direction of the move, or back to the nearest choice the other way when there is none.

#### Scenario: Opening on the most recent repository
- **WHEN** the wizard opens with Northwind.Templates used recently
- **THEN** Northwind.Templates is selected, not the Recent heading

#### Scenario: Stepping down over a heading
- **WHEN** the last Recent choice is selected and the user presses Down
- **THEN** the first folder choice is selected

#### Scenario: Stepping up over the first heading
- **WHEN** the first choice is selected and the user presses Page Up
- **THEN** the first choice stays selected

#### Scenario: Clicking a heading
- **WHEN** the user clicks the Repositories heading while a Recent choice is selected
- **THEN** the first repository under Repositories is selected

### Requirement: A folder choice starts in the folder whatever Where says
A session started in a folder choice SHALL start in that folder, with no worktree made, whatever Where, the worktree base and Before branching say; they SHALL keep their values for the next choice, and the preview SHALL say the folder is not a repository and that Where does not apply.

#### Scenario: The preview for a folder choice
- **WHEN** the user selects C:\Users\me\source\repos
- **THEN** the preview says it runs in C:\Users\me\source\repos, which is not a repository, so Where does not apply

#### Scenario: Moving back to a repository
- **WHEN** Where reads New worktree, the user selects C:\Users\me\source\repos and then Northwind.Orders
- **THEN** Where still reads New worktree and the preview names the new worktree


## MODIFIED Requirements

### Requirement: A new worktree is the default launch mode
The wizard opened from the overview SHALL start on the default place setting, a new worktree unless the user has chosen the current branch, and SHALL offer the other as the alternative: a new worktree, or the repository's main checkout on whatever branch is checked out. The wizard opened on a fixed folder SHALL offer no choice and start on that folder's current branch, whatever the setting.

#### Scenario: Accepting the defaults
- **WHEN** the default place is New worktree and the user opens the wizard with `n`, chooses Northwind.Orders, names the session "Fix 123" and accepts the other defaults
- **THEN** the session starts in a new worktree Northwind.Orders-fix-123 under the worktree root, on branch fix-123

#### Scenario: Choosing the current branch
- **WHEN** the user opens the wizard with `n`, chooses Northwind.Orders and switches Where to "Current branch"
- **THEN** the session starts in the repository's main checkout and no worktree is created

#### Scenario: Current branch by default
- **WHEN** the default place is Current branch and the user opens the wizard with `n`, chooses Northwind.Orders and names the session
- **THEN** Where reads Current branch and the session starts in the main checkout unless the user switches it

#### Scenario: Default left in place for a folder that is not a repository
- **WHEN** the default place is New worktree and the user chooses C:\Users\me\source\repos, names the session "cross-repo notes" and presses F5 without changing Where
- **THEN** the session starts in C:\Users\me\source\repos and no worktree is created

#### Scenario: Wizard on a fixed folder
- **WHEN** the default place is New worktree and the user starts a session with `n` on a worktree row
- **THEN** the session starts in that worktree on its current branch and no new worktree is created

#### Scenario: Fixed folder without a tab
- **WHEN** the user starts a session with `n` on a worktree row, types a name and presses F5
- **THEN** the session starts in that worktree and opens in its own tab, as on Enter, because the wizard has no way to start without one

#### Scenario: Fixed folder dialog is compact
- **WHEN** the wizard opens from a worktree row
- **THEN** the dialog shows the directory, the name, the preview and the prompt from its left edge, with no empty repository column, and its hint line says Enter or F5 starts and opens in a tab

### Requirement: Nothing is fetched or pulled outside a new worktree from origin
Cader SHALL NOT fetch or pull for a session started on the current branch, in a fixed folder, in a folder choice, with `cader here`, in an existing worktree the wizard offered instead of creating one, or in a new worktree whose base is HEAD.

#### Scenario: Current branch
- **WHEN** Before branching is Pull default branch and the user starts a session on Current branch
- **THEN** no git fetch, merge or branch update is run before the session starts

#### Scenario: HEAD base
- **WHEN** Before branching is Fetch origin and the user starts a session in a new worktree with the base HEAD
- **THEN** no fetch is made and the branch starts at HEAD

#### Scenario: cader here
- **WHEN** Before branching is Pull default branch and the user runs cader here fix-tests
- **THEN** no fetch or pull is made

#### Scenario: A folder choice
- **WHEN** Before branching is Pull default branch and the user starts a session in the folder choice C:\Users\me\source\repos while Where reads New worktree
- **THEN** no fetch or pull is made

## REMOVED Requirements

### Requirement: Repository is chosen from configured roots
**Reason**: Replaced by "A repository or a root folder is chosen from configured roots", which lists the roots as folder choices under grouped headings instead of leaving them out.
**Migration**: None; a session in a root can still be started with cader here, and now also from the wizard.