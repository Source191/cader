# Spec Delta

## MODIFIED Requirements

### Requirement: The list refreshes without losing the selection
Cader SHALL refresh the list automatically every `sessionsRefreshSeconds` (five seconds unless the setting says otherwise) and on demand, SHALL keep the selected session selected across a refresh when it still exists, and SHALL stay consistent when one refresh is still running as the next begins: a refresh SHALL never show rows older than the ones already shown, and rows that arrive while a dialog is open SHALL be kept when it closes. The timed refresh SHALL NOT start a listing while one is running; the refresh after an action SHALL always run. A session folder's branch and place SHALL be reused for `gitCacheSeconds` (30 unless the setting says otherwise) before git is asked again.

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
- **WHEN** a listing takes eight seconds
- **THEN** the five-second timer starts no second `claude agents` process until it returns, and the refresh after a stop or delete still starts its own listing

#### Scenario: A shorter interval is not stacked
- **WHEN** `sessionsRefreshSeconds` is 1 and a listing takes 3 seconds
- **THEN** no second listing starts until the first returns

#### Scenario: A branch switched in a shell
- **WHEN** `gitCacheSeconds` is 5 and the user switches a session folder's branch in a shell
- **THEN** the Sessions row shows the new branch at the first refresh after 5 seconds have passed
