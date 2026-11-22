## MODIFIED Requirements

### Requirement: The defaults
The defaults SHALL be: roots `<home>\source\repos`, worktree root `worktrees` beside the first repository root, editor VS Code, base the remote default branch, place a new worktree, diagnostic log off, sessions every 30 seconds and worktrees every 15 seconds.

#### Scenario: No settings file
- **WHEN** Cader starts and `config.json` does not exist
- **THEN** it scans `C:\Users\me\source\repos`, puts new worktrees under `C:\Users\me\source\worktrees`, opens folders in VS Code, branches new worktrees from origin's default branch, opens the wizard on New worktree, lists the sessions every 30 seconds and rescans the shown Worktrees tab every 15 seconds

### Requirement: The refresh intervals are kept in the settings file
`config.json` SHALL hold the intervals as `sessionsRefreshSeconds` and `worktreesRefreshSeconds`, whole seconds from 0 to 3600, where 0 means that tab has no refresh timer; a file written by Cader SHALL always name both. A missing property, or a whole number outside 0 to 3600, SHALL read as that interval's default and leave every other setting as the file says; a value that is not a whole number SHALL read as a corrupt file does.

#### Scenario: File from an earlier version
- **WHEN** `config.json` holds only `repositoryRoots` of `D:\work`
- **THEN** the sessions are listed every 30 seconds and the shown Worktrees tab rescans every 15 seconds

#### Scenario: A saved five seconds is kept
- **WHEN** `config.json` holds `sessionsRefreshSeconds` of `5`
- **THEN** the sessions are listed every 5 seconds

#### Scenario: Off by hand
- **WHEN** `config.json` holds `worktreesRefreshSeconds` of `0`
- **THEN** the Worktrees tab rescans when it is shown, after an action and on F5, and never on a timer

#### Scenario: Out of range
- **WHEN** `config.json` holds `sessionsRefreshSeconds` of `-1`, `worktreesRefreshSeconds` of `86400` and `repositoryRoots` of `D:\work`
- **THEN** Cader scans `D:\work`, lists the sessions every 30 seconds and rescans the shown Worktrees tab every 15 seconds

#### Scenario: Not a whole number
- **WHEN** `config.json` holds `sessionsRefreshSeconds` of `"fast"` and `repositoryRoots` of `D:\work`
- **THEN** Cader starts with every default, as it does for a corrupt file, and nothing exits

#### Scenario: Written back
- **WHEN** the user saves any setting from the Config tab
- **THEN** `config.json` names `sessionsRefreshSeconds` and `worktreesRefreshSeconds` with the current values
