## MODIFIED Requirements

### Requirement: Detail never delays the listing
Reading a row's detail SHALL run off the UI thread, for the selected row only and only while its tab is shown, once the selection has rested on the row for 150 ms and after each refresh, and SHALL NOT delay a refresh of the table; a detail that arrives after the selection has moved on, or after a newer detail has been shown, SHALL be dropped. A worktree's facts that do not depend on each other SHALL be read at the same time.

#### Scenario: Slow git
- **WHEN** the merge check for the selected worktree takes four seconds
- **THEN** the table refreshes on time and the pane's Branch line fills in when the answer arrives

#### Scenario: Selection moves while loading
- **WHEN** the user moves the selection before the previous row's detail has arrived
- **THEN** the pane shows the new row's detail only

#### Scenario: Moving through rows
- **WHEN** the user holds Down across ten worktree rows, resting under 150 ms on each, and stops on spike
- **THEN** detail is read for spike only

#### Scenario: Facts read together
- **WHEN** the selected worktree's merge check takes two seconds and its unpushed commits take one second
- **THEN** the pane has both about two seconds after the selection rests on it
