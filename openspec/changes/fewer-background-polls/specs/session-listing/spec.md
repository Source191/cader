## MODIFIED Requirements

### Requirement: The list refreshes without losing the selection
Cader SHALL refresh the list at the Sessions refresh interval (thirty seconds by default, no timer at 0) and on demand, SHALL keep the selected session selected across a refresh while it exists, and SHALL stay consistent when refreshes overlap: a refresh SHALL never show rows older than those shown, and rows that arrive while a dialog is open SHALL be kept when it closes. The timer and F5 SHALL NOT start a listing while one runs; the refresh after an action SHALL always run.

#### Scenario: Selected session survives refresh
- **WHEN** the user has selected a session and a refresh occurs
- **THEN** the same session remains selected even if its row position changed

#### Scenario: Selected session disappears
- **WHEN** the selected session is no longer in the listing after a refresh
- **THEN** the selection moves to the nearest remaining row

#### Scenario: Overlapping refreshes
- **WHEN** a refresh takes longer than the refresh interval and the refresh after an action starts before it finishes
- **THEN** the view shows the complete listing from the refresh that started last, whichever finishes first, and neither refresh fails

#### Scenario: A listing started before a stop lands after it
- **WHEN** the timed listing is running, the user presses x on fix-123, the stop completes and its own listing returns without fix-123, and then the earlier listing returns with fix-123 still running
- **THEN** fix-123 stays gone, no bell rings and no last prompted time is recorded for the reordering

#### Scenario: Rows loaded while a dialog is open
- **WHEN** the user presses m on a session, a timed refresh returns new rows while the memory dialog is open, and the user closes it
- **THEN** the table shows the new rows, not the ones from before the dialog opened

#### Scenario: The timer waits for a running listing
- **WHEN** Sessions refresh is 5 seconds and a listing takes eight seconds
- **THEN** the five-second timer starts no second `claude agents` process until it returns, and the refresh after a stop or delete still starts its own listing

#### Scenario: The default interval
- **WHEN** no Sessions refresh is saved and no change is heard in the Claude folder
- **THEN** the timer starts a listing 30 seconds after the last one started

#### Scenario: A longer interval
- **WHEN** Sessions refresh is 60 seconds
- **THEN** the timer starts a listing 60 seconds after the last one started, not 30

#### Scenario: The timer off
- **WHEN** Sessions refresh is 0 and the console has been open for ten minutes with no key pressed
- **THEN** the sessions were listed once, as the console opened

#### Scenario: F5 lists now
- **WHEN** the Sessions tab is shown, no listing is running and the user presses F5
- **THEN** a listing starts at once, and the rows and the selection follow it as for a timed refresh

#### Scenario: F5 during a listing
- **WHEN** a listing is running and the user presses F5 three times
- **THEN** no second `claude agents` process starts, and the running listing's rows are shown when it returns

#### Scenario: F5 while typing a filter
- **WHEN** the user is typing the filter fix and presses F5
- **THEN** a listing starts, and the filter stays fix and still being typed
