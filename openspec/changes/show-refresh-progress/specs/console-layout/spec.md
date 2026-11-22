## ADDED Requirements

### Requirement: A background refresh is marked
While a refresh of the shown tab runs (timed, started by a change on disc, by showing the tab, by F5 or after an action), the console SHALL show a dim ↻ after the tab's counts, and SHALL take it away when the refresh returns. No spinner SHALL turn for it.

#### Scenario: A timed listing
- **WHEN** the Sessions tab is shown and its timed listing is running
- **THEN** a dim ↻ follows the session counts until the listing returns

#### Scenario: Nothing running
- **WHEN** no refresh of the shown tab is running
- **THEN** no ↻ follows its counts

#### Scenario: A hidden tab's refresh
- **WHEN** the Sessions tab is shown and a heard change starts a worktrees scan
- **THEN** no ↻ follows the session counts because of it
