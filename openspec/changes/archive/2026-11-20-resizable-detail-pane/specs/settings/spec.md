## MODIFIED Requirements

### Requirement: Settings live in one file with defaults
Cader SHALL keep its settings in `config.json` in its state folder: the repository roots, the worktree root, the editor, the default worktree base, the default place for a new session, whether the diagnostic log is written to a file, the Sessions and Worktrees refresh intervals, and the heights of the Sessions and Worktrees detail panes.

#### Scenario: The default place by hand
- **WHEN** `config.json` holds `defaultPlace` of `CurrentBranch`
- **THEN** the wizard opened with n starts on Current branch

#### Scenario: The diagnostic log by hand
- **WHEN** `config.json` holds `diagnosticLog` of `true`
- **THEN** Cader writes its diagnostic log to `%LOCALAPPDATA%\Cader\cader.log` from its first event, and the Config tab's Diagnostic log row reads on

### Requirement: The defaults
The defaults SHALL be: roots `<home>\source\repos`, worktree root `worktrees` beside the first repository root, editor VS Code, base the remote default branch, place a new worktree, diagnostic log off, sessions every 5 seconds, worktrees every 15 seconds, and detail panes of six lines on the Sessions tab and ten on the Worktrees tab.

#### Scenario: No settings file
- **WHEN** Cader starts and `config.json` does not exist
- **THEN** it scans `C:\Users\me\source\repos`, puts new worktrees under `C:\Users\me\source\worktrees`, opens folders in VS Code, branches new worktrees from origin's default branch, opens the wizard on New worktree, lists the sessions every 5 seconds, rescans the shown Worktrees tab every 15 seconds, and shows six lines in the Sessions pane and ten in the Worktrees pane

## ADDED Requirements

### Requirement: The detail pane heights are kept in the settings file
`config.json` SHALL hold each pane's lines under its divider as `sessionsDetailRows` (1 to 6) and `worktreesDetailRows` (1 to 200); a file written by Cader SHALL always name both. A missing property, or a whole number outside its range, SHALL read as that pane's default and leave every other setting as the file says; a value that is not a whole number SHALL read as a corrupt file does.

#### Scenario: File from an earlier version
- **WHEN** `config.json` holds only `repositoryRoots` of `D:\work`
- **THEN** Cader scans `D:\work`, the Sessions pane has six lines and the Worktrees pane ten

#### Scenario: Set by hand
- **WHEN** `config.json` holds `worktreesDetailRows` of `20` and the terminal is tall enough
- **THEN** the Worktrees pane opens with twenty lines under its divider

#### Scenario: Out of range
- **WHEN** `config.json` holds `sessionsDetailRows` of `9`, `worktreesDetailRows` of `0` and `repositoryRoots` of `D:\work`
- **THEN** Cader scans `D:\work`, the Sessions pane has six lines and the Worktrees pane ten

#### Scenario: Not a whole number
- **WHEN** `config.json` holds `worktreesDetailRows` of `"tall"` and `repositoryRoots` of `D:\work`
- **THEN** Cader starts with every default, as it does for a corrupt file, and nothing exits

#### Scenario: Written back
- **WHEN** the user saves any setting from the Config tab
- **THEN** `config.json` names `sessionsDetailRows` and `worktreesDetailRows` with the heights the panes have now

### Requirement: A resized pane is saved once it settles
A pane height the user sets by dragging or by keys SHALL be written to `config.json` once that pane has kept the same height for one second, so a drag or a run of key presses writes the file once, and SHALL apply on the next start. A height the pane shows only because the terminal is too short SHALL NOT be saved. A save of the settings dialog SHALL keep both heights as they are at that moment.

#### Scenario: A run of key presses
- **WHEN** the user presses `+` five times on the Worktrees tab, a fifth of a second apart
- **THEN** `config.json` is written once, a second after the last press, with `worktreesDetailRows` of `15`

#### Scenario: The next start
- **WHEN** the user has dragged the Worktrees pane to eighteen lines, waited a second and quit
- **THEN** the next start shows the Worktrees pane with eighteen lines

#### Scenario: A short terminal
- **WHEN** `worktreesDetailRows` is `20` and the terminal is made so short that the pane shows twelve lines
- **THEN** `config.json` still holds `worktreesDetailRows` of `20`

#### Scenario: A settings save after a resize
- **WHEN** the user makes the Sessions pane four lines tall and then saves the editor from the Config tab
- **THEN** `config.json` holds the new editor and `sessionsDetailRows` of `4`
