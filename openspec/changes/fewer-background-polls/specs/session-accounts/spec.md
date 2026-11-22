## MODIFIED Requirements

### Requirement: The current login comes from claude auth status
Cader SHALL learn the current login by running `claude auth status --json` once at start and then as often as the next requirement allows, off the screen thread, so that no row waits for it. A login with an email and an organisation id SHALL be the current login; a logged-out answer, an answer missing either, or a failure to run SHALL leave no current login, and the rows SHALL still be shown.

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
- **WHEN** the user runs `/login` as another account while Cader runs and starts a session under it
- **THEN** within a minute of that session being listed Cader treats the new account as the current login

## ADDED Requirements

### Requirement: The login is read again only when it may have changed
After its first run, Cader SHALL run `claude auth status --json` no more than once every 15 minutes, and no more than once a minute while a listed session belongs to an organisation Cader does not remember or the last run failed.

#### Scenario: Nothing new
- **WHEN** every listed session belongs to a remembered organisation and the last run answered
- **THEN** `claude auth status --json` runs again only once 15 minutes have passed

#### Scenario: Logged in again with no new session
- **WHEN** the user runs `/login` as another account while Cader runs and starts no session
- **THEN** within 15 minutes Cader treats the new account as the current login
