# Spec Delta

## ADDED Requirements

### Requirement: The Config tab lists the remembered accounts
After the setup rows the Config tab SHALL have an Accounts heading, then one row per remembered account in label order: SETTING reads Account and its label; VALUE reads its email and organisation, then logged in now for the current login. The heading SHALL behave as the Setup heading does, and neither it nor the account rows SHALL count as settings or setup to do. With no remembered account there SHALL be neither heading nor rows.

#### Scenario: Two accounts
- **WHEN** Cader remembers work, me@northwind.example in Northwind Traders, logged in now, and personal, me@home.example
- **THEN** after the setup rows the table reads Accounts, then Account personal with me@home.example and its organisation, then Account work with me@northwind.example · Northwind Traders · logged in now

#### Scenario: No account remembered
- **WHEN** Cader has never seen a login
- **THEN** the Config tab has no Accounts heading and no account rows

#### Scenario: The heading is stepped over
- **WHEN** the last setup row is selected and the user presses Down
- **THEN** the first account row is selected

#### Scenario: Not counted
- **WHEN** there is one root, every piece of setup is in place and two accounts are remembered
- **THEN** the counts read 8 settings and the title counts 15 rows

#### Scenario: A filter finds an account
- **WHEN** the user filters by northwind
- **THEN** the table shows only the account rows whose label, email or organisation holds northwind, with no heading

### Requirement: Enter on an account renames its label
Enter on an account row SHALL open a dialog holding the account's label, as renaming a session does. Enter or OK SHALL close it and check the typed label by the label rules: a good one is saved to `accounts.json` and the status line says it was renamed; a refused one leaves the label unchanged and the status line names the problem. Esc SHALL close it unchanged. A rename SHALL show in the ACCOUNT column at the next refresh.

#### Scenario: Rename an account
- **WHEN** the user selects Account 1, presses Enter, types work and presses Enter
- **THEN** the row reads Account work, `accounts.json` holds work, and the status line says account 1 is now work

#### Scenario: A refused label
- **WHEN** the user types a label of nine characters and presses Enter
- **THEN** the dialog closes, the label is unchanged and the status line says a label is at most 8 characters

#### Scenario: Cancel the rename
- **WHEN** the user changes the label and presses Esc
- **THEN** `accounts.json` and the row are unchanged

### Requirement: An account row describes itself
While an account row is selected, and as its tooltip, the Config tab SHALL say that the label names the account in the Sessions tab's ACCOUNT column and that Enter renames it. c and f on an account row SHALL say it holds no path.

#### Scenario: Status line for an account row
- **WHEN** the user selects Account work
- **THEN** the status line says the label names this account in the ACCOUNT column and Enter renames it

#### Scenario: Copy on an account row
- **WHEN** the user presses c on Account work
- **THEN** nothing is copied and the status line says the row holds no path
