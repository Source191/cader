## MODIFIED Requirements

### Requirement: Rows can be filtered
The view SHALL filter rows by a text typed after the filter key, matching case-insensitively against name, repository and branch. Whitespace, control and format characters at the start of the filter SHALL be ignored, so a filter holding only them shows every row; characters after the first visible one, spaces included, SHALL still count. The same rule SHALL hold for the Worktrees and Config tabs' filters.

#### Scenario: Filter by repository
- **WHEN** the user types a filter of "templates"
- **THEN** only rows whose name, repository or branch contains "templates" remain visible

#### Scenario: A space before the filter
- **WHEN** the user presses the filter key and types a space followed by "templates"
- **THEN** the same rows remain visible as for "templates"

#### Scenario: Only a space
- **WHEN** the filter holds a single space
- **THEN** every row remains visible

#### Scenario: A space inside the filter still counts
- **WHEN** the filter is "fix login" and one row is named fix login and another fix-login
- **THEN** only the row named fix login remains visible

#### Scenario: The Worktrees tab follows the same rule
- **WHEN** the user filters the Worktrees tab with a space followed by "orders"
- **THEN** the same rows remain visible as for "orders"
