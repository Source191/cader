# Spec Delta

## ADDED Requirements

### Requirement: Rows show the session's account
The Sessions table SHALL have an ACCOUNT column between PR and AGE showing the label of the session's owner, with ● and a space before it when the owner is the current login. It SHALL be blank for a session with no recorded owner and read ? for an owner Cader has not seen logged in. The column SHALL be as wide as its widest cell or its header, whichever is wider.

#### Scenario: A session under the current login
- **WHEN** fix-123's owner is the account labelled work and work is the current login
- **THEN** its ACCOUNT cell reads ● work

#### Scenario: A session under another account
- **WHEN** fix-123's owner is the account labelled personal and work is the current login
- **THEN** its ACCOUNT cell reads personal

#### Scenario: An owner not seen yet
- **WHEN** fix-123's owner is an organisation Cader has never seen logged in
- **THEN** its ACCOUNT cell reads ?

#### Scenario: No recorded owner
- **WHEN** fix-123's transcript has no `session-owner` line
- **THEN** its ACCOUNT cell is blank

#### Scenario: No current login
- **WHEN** Claude Code is logged out and fix-123's owner is labelled work
- **THEN** its ACCOUNT cell reads work, with no ●

#### Scenario: A renamed label
- **WHEN** the user renames account 1 to work on the Config tab
- **THEN** at the next refresh every row owned by that account reads work

#### Scenario: The help names the column
- **WHEN** the user presses the help key in the sessions view, or rests the pointer on the ACCOUNT header
- **THEN** the text says the column shows the label of the account the session belongs to, that ● marks the account logged in now, that ? is an account not yet seen logged in, and that the Config tab lists and renames the accounts
