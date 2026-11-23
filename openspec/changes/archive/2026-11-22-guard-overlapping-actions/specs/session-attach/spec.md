## ADDED Requirements

### Requirement: Enter opens one tab per session
Enter on a session SHALL open at most one Windows Terminal tab for it however often it is pressed: while its tab is being opened, or once it is open but before the next listing shows it, Enter SHALL switch to that tab or say the opening is still running, and SHALL NOT open another.

#### Scenario: Enter pressed twice
- **WHEN** the user presses Enter twice on fix-123, which has no tab
- **THEN** one tab opens for fix-123, and stop and delete later close it

#### Scenario: Enter again before the next listing
- **WHEN** fix-123's tab has just opened and the user presses Enter on fix-123 before the sessions are listed again
- **THEN** Cader switches to that tab and opens no other
