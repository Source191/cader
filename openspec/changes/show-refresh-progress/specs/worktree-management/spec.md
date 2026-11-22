## MODIFIED Requirements

### Requirement: Scanning is visible
The worktrees view SHALL show a spinner with the words "Scanning worktrees" and how many of the repositories have been read in the middle of the table while it is shown and no rows have yet arrived, and SHALL show a spinner beside the status line's working text while an action started from the view runs; neither spinner SHALL appear for the rescan when the tab is shown again or for the timed rescans. Typing a filter, Backspace or Escape before the first scan SHALL NOT end the waiting spinner.

#### Scenario: First scan pending
- **WHEN** the user opens the worktrees view and no repository has yet been read
- **THEN** the table area shows a spinner and "Scanning worktrees: 0 of 55 repositories", which disappear when the first rows arrive

#### Scenario: A filter typed before the first scan
- **WHEN** the worktrees view is shown, its first scan has not yet returned any rows and the user presses / and types n
- **THEN** the table area still shows the spinner and "Scanning worktrees", and the title shows the filter being typed

#### Scenario: Rescan on show
- **WHEN** the user opens the worktrees view after its first scan has returned
- **THEN** the earlier rows show at once with no spinner while the rescan runs

#### Scenario: Action running
- **WHEN** the user removes a worktree
- **THEN** the status line shows a spinner before "Removing name" until the removal reports its outcome

## ADDED Requirements

### Requirement: The first scan fills the table from the top
While the Worktrees tab has no rows yet, its scan SHALL show each repository's rows as soon as that repository and every repository before it in name order have been read, with the rows the finished scan will show for them, and SHALL count the repositories read in the table's title until the scan returns. Rows already shown SHALL NOT be replaced by a later scan's partial rows.

#### Scenario: The first repositories read
- **WHEN** the first scan has read Northwind.Billing and Northwind.Orders, the first two of 55 repositories by name, and is still reading the rest
- **THEN** their rows are listed under their headings, and the title says 2 of 55 repositories

#### Scenario: A later repository read first
- **WHEN** the first scan has read Northwind.Orders but not Northwind.Billing, which comes before it
- **THEN** no row is listed yet

#### Scenario: A rescan with rows on screen
- **WHEN** the user presses F5 on the Worktrees tab with its rows shown
- **THEN** the rows stay as they are until the rescan returns, and the title counts the repositories read meanwhile
