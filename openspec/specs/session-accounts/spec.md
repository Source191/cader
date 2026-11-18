# session-accounts Specification

## Purpose
Says which Claude account each session belongs to, so someone with more than one account knows which one to log in as before resuming, renaming or adopting a session.

## Requirements

### Requirement: A session's owner comes from its transcript
Cader SHALL take a session's owner from the first `session-owner` line of its transcript, reading only that line's `organizationUuid`. A session whose transcript has no such line, or has no transcript, SHALL have no recorded owner. Once read, an owner SHALL NOT be read again for that session while Cader runs.

#### Scenario: A transcript with an owner line
- **WHEN** a session's transcript holds a `session-owner` line whose `organizationUuid` is the Northwind organisation's
- **THEN** the session's owner is the Northwind organisation

#### Scenario: A transcript from an older Claude Code
- **WHEN** a session's transcript holds user and assistant lines but no `session-owner` line
- **THEN** the session has no recorded owner and every other fact about it is shown as before

#### Scenario: A line that cannot be read
- **WHEN** a transcript's `session-owner` line has no `organizationUuid`, or one that is not text
- **THEN** the session has no recorded owner

#### Scenario: Read once
- **WHEN** a session's owner has been read and the Sessions tab refreshes
- **THEN** the transcript is not read again for its owner

### Requirement: The current login comes from claude auth status
Cader SHALL learn the current login by running `claude auth status --json` once at start and then no more than once a minute, off the screen thread, so that no row waits for it. A login with an email and an organisation id SHALL be the current login; a logged-out answer, an answer missing either, or a failure to run SHALL leave no current login, and the rows SHALL still be shown.

#### Scenario: Logged in
- **WHEN** `claude auth status --json` answers loggedIn true with an email, an orgId and an orgName
- **THEN** that organisation is the current login

#### Scenario: Logged out
- **WHEN** `claude auth status --json` answers loggedIn false
- **THEN** there is no current login and no row is marked as the current login

#### Scenario: Claude cannot be run
- **WHEN** `claude auth status --json` fails or prints something that is not its JSON
- **THEN** there is no current login, the rows are shown without waiting, and the next attempt is a minute later

#### Scenario: The user logs in as another account
- **WHEN** the user runs `/login` as another account while Cader runs
- **THEN** within a minute Cader treats the new account as the current login

### Requirement: Cader remembers the accounts it sees
Each organisation Cader sees as the current login SHALL be remembered in `accounts.json` in Cader's state folder, with its email, organisation name and label, and kept after Cader exits. Seeing it again SHALL update its email and organisation name and keep its label. A missing or unreadable `accounts.json` SHALL read as no accounts and SHALL NOT stop the console.

#### Scenario: The first account
- **WHEN** Cader has remembered no account and sees the Northwind organisation logged in as me@northwind.example
- **THEN** `accounts.json` holds that organisation with that email, its name and the label 1

#### Scenario: The second account
- **WHEN** Cader has remembered one account labelled 1 and sees another organisation logged in
- **THEN** the new account is remembered with the label 2

#### Scenario: A changed email
- **WHEN** a remembered organisation labelled work is seen logged in with another email
- **THEN** its email is updated and its label stays work

#### Scenario: An unreadable file
- **WHEN** `accounts.json` cannot be read
- **THEN** Cader shows the sessions with no remembered accounts and remembers the current login afresh

### Requirement: A new account takes the lowest free number
A newly remembered account's label SHALL be the lowest whole number from 1 that no remembered account uses as its label.

#### Scenario: A renamed first account
- **WHEN** the only remembered account was renamed work and another account is seen
- **THEN** the new account's label is 1

#### Scenario: A gap
- **WHEN** the remembered labels are 1 and 3 and another account is seen
- **THEN** the new account's label is 2

### Requirement: An account's label is short and its own
A label SHALL be 1 to 8 characters after trimming, with no control characters, and SHALL NOT be the same, ignoring case, as another remembered account's label. A label that breaks a rule SHALL be refused with the reason, and the label kept.

#### Scenario: A good label
- **WHEN** the user renames account 1 to work
- **THEN** its label is work and `accounts.json` says so

#### Scenario: Too long
- **WHEN** the user renames an account to northwind-work
- **THEN** the rename is refused, the reason says a label is at most 8 characters, and the label is unchanged

#### Scenario: Blank
- **WHEN** the user renames an account to spaces only
- **THEN** the rename is refused and the reason says a label cannot be blank

#### Scenario: Taken
- **WHEN** one account is labelled work and the user renames another to Work
- **THEN** the rename is refused and the reason says another account already uses that label

#### Scenario: Unchanged
- **WHEN** the user renames account work to work
- **THEN** nothing is written and the status line says the label is unchanged

### Requirement: Accounts are never recorded in diagnostics
No email, organisation name, organisation id or label SHALL appear in the diagnostic log, the crash file or a problem report; `accounts.json` SHALL NOT be attached to a problem report.

#### Scenario: Renaming an account with the log on
- **WHEN** the diagnostic log is on and the user renames account 1 to work
- **THEN** the log records that an effect ran and names neither the label nor the account

#### Scenario: Reading the login fails
- **WHEN** `claude auth status --json` fails with the log on
- **THEN** the log records the child program's run and its exit, and nothing it printed
