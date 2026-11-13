# Spec Delta

## ADDED Requirements

### Requirement: The Config tab has an Accounts section
Between the settings and the Setup heading, the Config tab SHALL have an Accounts heading row, a row per account naming it and its folder, and an Add account row, in place of the account rows under Setup.

#### Scenario: One account
- **WHEN** no `accounts` list is configured
- **THEN** the Accounts section lists one account at C:\Users\me\.claude and the Add account row

#### Scenario: Two accounts
- **WHEN** work and personal are configured
- **THEN** the section lists work C:\Users\me\.claude and personal C:\Users\me\.claude-personal, then Add account, and the Setup section lists no accounts

### Requirement: Add account runs the setup steps in a dialog
Enter on Add account SHALL open a dialog asking for the name, the start (each of the four with its sentence), the items to share and the roots for the account, then showing the same preview `setup-account` prints; F5 SHALL carry it out as the command does, Esc SHALL close it with nothing written, and a name that is not plain SHALL be named as the problem without closing.

#### Scenario: Add a clean account
- **WHEN** the user presses Enter on Add account, types personal, chooses Clean, ticks the root D:\personal and presses F5
- **THEN** `C:\Users\me\.claude-personal` exists, personal is listed in the section, `D:\personal`'s row names personal, and a login tab opens

#### Scenario: Cancelled
- **WHEN** the user fills the dialog and presses Esc
- **THEN** no folder, file, junction, profile or `config.json` entry is made

### Requirement: An account is renamed or removed in its dialog
Enter on an account row SHALL open a dialog to rename the account, show its folder, change which roots belong to it, and remove it with the same preview and rules as `setup-account --remove`. The account whose folder is `%USERPROFILE%\.claude` SHALL NOT offer removal.

#### Scenario: Rename
- **WHEN** the user opens default's dialog, renames it work and presses F5
- **THEN** `config.json` names the account work everywhere it named default, and the ACCOUNT columns read work

#### Scenario: The default folder's account
- **WHEN** the user opens the dialog of the account at C:\Users\me\.claude
- **THEN** the dialog has no remove button

### Requirement: Removing a root clears its account
When the repository roots dialog saves without a root, its `rootAccounts` entry SHALL be removed too.

#### Scenario: Remove a personal root
- **WHEN** `D:\personal` belongs to personal and the user deletes it from the roots dialog and presses F5
- **THEN** `config.json` has neither the root nor its `rootAccounts` entry
