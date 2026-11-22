## MODIFIED Requirements

### Requirement: A session started from the wizard opens in its own tab
A session started from the wizard SHALL be opened in its own Windows Terminal tab as soon as Claude Code reports the new session's job, whether or not the supervisor lists it yet, and the wizard SHALL have no way to start one without a tab; `cader here` attaches in its own tab instead.

#### Scenario: Start and open in a tab
- **WHEN** the user starts a session from the wizard
- **THEN** Cader starts the session, opens it in a new Windows Terminal tab titled with the session name as the open-in-tab key does, and the sessions view stays on screen with the status line naming the tab's colour

#### Scenario: No start without a tab
- **WHEN** the user presses F6 in the wizard
- **THEN** nothing starts and the wizard stays open

#### Scenario: Started session not listed yet
- **WHEN** the user starts a session from the wizard and the supervisor has not yet listed the new session when the start returns
- **THEN** the tab opens all the same, and the session's row shows that tab's colour once the session is listed

#### Scenario: No listing to open the tab
- **WHEN** the user starts a session from the wizard
- **THEN** no `claude agents` process runs between `claude --bg` answering and the tab opening

### Requirement: A started session is never reported as a failed launch
Once the supervisor has started a session, the launch SHALL be reported as started, and a recent repository that cannot be recorded SHALL NOT stop the launch.

#### Scenario: The listing fails after the start
- **WHEN** the user starts fix-123 from the wizard, the supervisor starts it, and the sessions listing that follows exits with an error
- **THEN** fix-123's tab is open, the status line says fix-123 was started with no "Failed:" line, and the listing's failure shows only as any listing failure does on the Sessions tab

#### Scenario: The recent repository cannot be recorded
- **WHEN** the user starts fix-123 in Northwind.Orders and Cader's state folder refuses the recent-repository record
- **THEN** fix-123 is started and opened in its own tab as usual, and the refusal is recorded only in the diagnostic log

## ADDED Requirements

### Requirement: A new session's tab is found before the session is listed
Until a listing names a session started from the wizard, its tab SHALL be found by the session's job id wherever a session's tab is found (the TAB block, Enter, stop, rename, delete), and once a listing names it the tab SHALL be found by its session id as for any other session.

#### Scenario: Stopped straight after starting
- **WHEN** the user starts fix-123 from the wizard and presses x on its row as soon as it is listed
- **THEN** the session is stopped and its tab closes

#### Scenario: Enter on the new row
- **WHEN** the user presses Enter on fix-123's row while its tab is open
- **THEN** Cader switches to that tab rather than opening a second
