## ADDED Requirements

### Requirement: Before branching is a setting row
The Config tab SHALL show a Before branching row directly after Default base, reading Fetch origin, Pull default branch or Nothing; Enter on it SHALL open a dialog to choose one of the three, where F5 or Save writes `config.json` and Esc closes it unchanged. Its tooltip SHALL say that it is what Cader does before a new worktree branches from origin's default branch.

#### Scenario: Default
- **WHEN** `config.json` has no Before branching and the user shows the Config tab
- **THEN** the row after Default base reads Before branching Fetch origin

#### Scenario: Choose Nothing
- **WHEN** the user presses Enter on Before branching, chooses Nothing and presses F5
- **THEN** the row reads Nothing, `config.json` holds `beforeBranching` of `None`, the status line says it was saved, and the next new worktree is branched without a fetch

#### Scenario: Cancel
- **WHEN** the user presses Enter on Before branching, chooses Pull default branch and presses Esc
- **THEN** `config.json` and the row are unchanged

### Requirement: The Default base row says the base is fetched
The Default base row SHALL read origin's default branch or HEAD, and its tooltip SHALL say that a new worktree branches from origin's default branch after the update Before branching chooses, or from HEAD, never that Cader makes no fetch.

#### Scenario: Default base tooltip
- **WHEN** the user hovers over the Default base row
- **THEN** the tooltip names Before branching and does not say that no fetch is made

## MODIFIED Requirements

### Requirement: Default place is a setting row
The Config tab SHALL show a Default place row after Before branching, reading New worktree or Current branch; Enter on it SHALL open a dialog to choose between them, and saving SHALL write `config.json` and apply to the next wizard at once.

#### Scenario: Default
- **WHEN** `config.json` has no default place
- **THEN** the Default place row reads New worktree

#### Scenario: Choose the current branch
- **WHEN** the user presses Enter on Default place, chooses Current branch and presses F5
- **THEN** the row reads Current branch and the next wizard opened with n starts on Current branch
