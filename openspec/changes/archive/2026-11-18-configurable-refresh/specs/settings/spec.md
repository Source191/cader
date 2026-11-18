# Spec Delta

## MODIFIED Requirements

### Requirement: Settings live in one file with defaults
Cader SHALL keep its settings in `config.json` in its state folder: the repository roots, the worktree root, the editor, the default worktree base, the default place for a new session, whether the diagnostic log is written to a file, and the Sessions and Worktrees refresh intervals. A missing, corrupt or unreadable file, a number where a base or place name belongs, or a missing property SHALL read as the default: roots `<home>\source\repos`, worktree root `worktrees` beside the first repository root, editor VS Code, base the remote default branch, place a new worktree, diagnostic log off, sessions every 5 seconds and worktrees every 15 seconds.

#### Scenario: No settings file
- **WHEN** Cader starts and `config.json` does not exist
- **THEN** it scans `C:\Users\me\source\repos`, puts new worktrees under `C:\Users\me\source\worktrees`, opens folders in VS Code, branches new worktrees from origin's default branch, opens the wizard on New worktree, lists the sessions every 5 seconds and rescans the shown Worktrees tab every 15 seconds

#### Scenario: File from an earlier version
- **WHEN** `config.json` holds only `repositoryRoots` of `D:\work` and `defaultBase` of `Head`
- **THEN** Cader scans `D:\work`, branches from HEAD, puts new worktrees under `D:\worktrees`, opens folders in VS Code and opens the wizard on New worktree

#### Scenario: Corrupt file
- **WHEN** `config.json` is not valid JSON
- **THEN** Cader starts with every default and does not exit

#### Scenario: A path holding a NUL character
- **WHEN** `config.json` holds `repositoryRoots` of `C:\Users\me\source\repos` and a second root whose text holds a NUL character, and a `worktreeRoot` whose text holds one too
- **THEN** Cader starts, scans `C:\Users\me\source\repos` alone, puts new worktrees under `C:\Users\me\source\worktrees`, and does not exit

#### Scenario: The default place by hand
- **WHEN** `config.json` holds `defaultPlace` of `CurrentBranch`
- **THEN** the wizard opened with n starts on Current branch

#### Scenario: A number where a name belongs
- **WHEN** `config.json` holds `repositoryRoots` of `D:\work` and `defaultBase` of `7`
- **THEN** Cader starts with every default, as it does for a corrupt file, the Config tab draws and the wizard opens, and nothing exits

#### Scenario: A file Cader may not read
- **WHEN** `config.json` exists but the user's account is denied reading its contents
- **THEN** Cader starts with every default and does not exit

#### Scenario: The diagnostic log by hand
- **WHEN** `config.json` holds `diagnosticLog` of `true`
- **THEN** Cader writes its diagnostic log to `%LOCALAPPDATA%\Cader\cader.log` from its first event, and the Config tab's Diagnostic log row reads on

#### Scenario: Not a true or false
- **WHEN** `config.json` holds `diagnosticLog` of `"yes"`
- **THEN** Cader starts with every default, as it does for a corrupt file, and writes no log file

### Requirement: Saved settings apply without a restart
Saving SHALL write `config.json` and SHALL make every later scan, wizard, worktree creation, editor launch, diagnostic event, refresh timer and the Config tab use the saved values, without restarting Cader. Saving SHALL keep the file readable by hand, with the property names `repositoryRoots`, `worktreeRoot`, `editor`, `defaultBase`, `diagnosticLog`, `sessionsRefreshSeconds` and `worktreesRefreshSeconds`.

#### Scenario: Add a root
- **WHEN** the user adds `D:\work` as a second repository root and saves
- **THEN** the Config tab lists both roots at once, and the next worktree scan and the next wizard include repositories under `D:\work`

#### Scenario: Change the worktree root
- **WHEN** the user sets the worktree root to `D:\worktrees` and saves, then starts a session in a new worktree
- **THEN** the worktree is created under `D:\worktrees`

#### Scenario: Turn the diagnostic log on
- **WHEN** the user turns Diagnostic log on and then stops a session
- **THEN** `config.json` holds `"diagnosticLog": true` and `cader.log` gains the line for the stop, without a restart

#### Scenario: Slow the sessions listing
- **WHEN** the sessions are listed every 5 seconds and the user saves Sessions refresh as 30
- **THEN** `config.json` holds `"sessionsRefreshSeconds": 30`, and from then on the timer lists the sessions every 30 seconds, without a restart

## ADDED Requirements

### Requirement: The refresh intervals are kept in the settings file
`config.json` SHALL hold the intervals as `sessionsRefreshSeconds` and `worktreesRefreshSeconds`, whole seconds from 0 to 3600, where 0 means that tab has no refresh timer; a file written by Cader SHALL always name both. A missing property, or a whole number outside 0 to 3600, SHALL read as that interval's default and leave every other setting as the file says; a value that is not a whole number SHALL read as a corrupt file does.

#### Scenario: File from an earlier version
- **WHEN** `config.json` holds only `repositoryRoots` of `D:\work`
- **THEN** the sessions are listed every 5 seconds and the shown Worktrees tab rescans every 15 seconds

#### Scenario: Off by hand
- **WHEN** `config.json` holds `worktreesRefreshSeconds` of `0`
- **THEN** the Worktrees tab rescans when it is shown, after an action and on F5, and never on a timer

#### Scenario: Out of range
- **WHEN** `config.json` holds `sessionsRefreshSeconds` of `-1`, `worktreesRefreshSeconds` of `86400` and `repositoryRoots` of `D:\work`
- **THEN** Cader scans `D:\work`, lists the sessions every 5 seconds and rescans the shown Worktrees tab every 15 seconds

#### Scenario: Not a whole number
- **WHEN** `config.json` holds `sessionsRefreshSeconds` of `"fast"` and `repositoryRoots` of `D:\work`
- **THEN** Cader starts with every default, as it does for a corrupt file, and nothing exits

#### Scenario: Written back
- **WHEN** the user saves any setting from the Config tab
- **THEN** `config.json` names `sessionsRefreshSeconds` and `worktreesRefreshSeconds` with the current values

### Requirement: A refresh interval is checked before it is saved
The Sessions refresh and Worktrees refresh dialogs SHALL each hold one box of seconds. F5 or Save SHALL accept a whole number from 0 to 3600, spaces around it ignored, and SHALL otherwise refuse to save, keep the dialog open and say that the interval is a whole number of seconds from 0 to 3600, where 0 turns the timer off.

#### Scenario: A good interval
- **WHEN** the user types 30 in the Worktrees refresh dialog and presses F5
- **THEN** the setting is saved and the dialog closes

#### Scenario: Off
- **WHEN** the user types 0 in the Sessions refresh dialog and presses F5
- **THEN** the setting is saved and the Sessions refresh row reads off

#### Scenario: Not a number
- **WHEN** the user types 2.5 or ten and presses F5
- **THEN** nothing is saved and the dialog says the interval is a whole number of seconds from 0 to 3600, where 0 turns the timer off

#### Scenario: Too long
- **WHEN** the user types 3601 and presses F5
- **THEN** nothing is saved and the dialog says the same
