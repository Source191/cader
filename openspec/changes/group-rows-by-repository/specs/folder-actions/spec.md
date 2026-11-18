## MODIFIED Requirements

### Requirement: The folder is shown relative to its root
Both table views SHALL show where a row's folder is in the heading of the row's group and, on the Worktrees tab, the WORKTREE column, not a FOLDER column. A session's heading SHALL read its repository's name, or the folder's label outside a repository; a worktree's heading SHALL read its repository's name, and its WORKTREE cell the folder's slug, its inner path, or its label when not named after it. A label SHALL be the path relative to the deepest root holding it, else ~\ and the path under home, else in full. Neither a heading nor a cell SHALL show the branch, except a branch or origin row's WORKTREE cell, which names its branch.

#### Scenario: Worktree under the worktree root
- **WHEN** the worktrees view lists C:\Users\me\source\worktrees\Northwind.Orders-fix-123, a worktree of Northwind.Orders on branch fix-123, and the worktree root is C:\Users\me\source\worktrees
- **THEN** its row is under the Northwind.Orders heading and its WORKTREE cell reads fix-123

#### Scenario: Session in a worktree
- **WHEN** a session runs in C:\Users\me\source\worktrees\Northwind.Orders-fix-123 on branch fix-123
- **THEN** its row is under the Northwind.Orders heading, which has no slug and no branch

#### Scenario: Repository under a repository root
- **WHEN** a session runs in C:\Users\me\source\repos\Northwind.Orders\src\Api and the repository root is C:\Users\me\source\repos
- **THEN** its row is under the Northwind.Orders heading, which has no inner path and no branch

#### Scenario: Session in the main checkout
- **WHEN** a session runs in the main checkout C:\Users\me\source\repos\Northwind.Orders on branch main
- **THEN** its row is under the Northwind.Orders heading, which has no branch

#### Scenario: Folder outside every root
- **WHEN** a session runs in C:\Users\me\scratch\notes, which is not a repository
- **THEN** its row is under a heading reading ~\scratch\notes

#### Scenario: Folder outside home
- **WHEN** a session runs in D:\work\Northwind.Tools, which is not a repository, and no root contains it
- **THEN** its row is under a heading reading D:\work\Northwind.Tools

#### Scenario: Session in a root itself
- **WHEN** a session started with cader here runs in the repository root C:\Users\me\source\repos
- **THEN** its row is under a heading reading ~\source\repos

## REMOVED Requirements

### Requirement: The part after the repository follows ›
**Reason**: No table cell names the repository any more: the group heading does, so a WORKTREE cell holds only the part after the repository and nothing needs the › between them.
**Migration**: None; the repository is read from the row's heading.
