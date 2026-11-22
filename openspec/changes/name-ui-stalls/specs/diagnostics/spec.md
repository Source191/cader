## MODIFIED Requirements

### Requirement: A stall line names the work that ran
A stall recorded as a line SHALL also name the longest piece of marked UI-thread work since the beat before (a refresh's or a detail's result being shown, a key, an action's outcome, the console's layout or its drawing) by its code name and duration, or say that no marked work ran, and SHALL give how long the garbage collector paused the process since the beat before.

#### Scenario: A freeze after rows were shown
- **WHEN** the UI thread runs a beat 820 ms late after showing a worktrees scan's rows, which took 610 ms, and a key, which took 5 ms
- **THEN** the stall line says the UI thread stalled for 820 ms and names the worktree rows being shown and 610 ms

#### Scenario: A freeze with no marked work
- **WHEN** the UI thread runs a beat 700 ms late and no marked work ran since the beat before
- **THEN** the stall line says the UI thread stalled for 700 ms and that no marked work ran

#### Scenario: A freeze while drawing
- **WHEN** the UI thread runs a beat 900 ms late and the console's drawing took 840 ms of it
- **THEN** the stall line says the UI thread stalled for 900 ms and names drawing and 840 ms

#### Scenario: A freeze the collector caused
- **WHEN** the UI thread runs a beat 600 ms late and the garbage collector paused the process for 550 ms since the beat before
- **THEN** the stall line says the UI thread stalled for 600 ms and gives the collector's pause as 550 ms

#### Scenario: Work before the previous beat
- **WHEN** the rows were shown before one beat that ran on time, and the next beat runs 600 ms late with no marked work between them
- **THEN** the stall line says no marked work ran and does not name the rows

#### Scenario: A key that opens a dialog
- **WHEN** a key opens a dialog that stays open for 30 seconds, during which the heartbeat runs on time, and the beat after the dialog closes runs 600 ms late
- **THEN** the stall line names the key with at most the time it ran since the beat before, never the 30 seconds the dialog was open

## ADDED Requirements

### Requirement: Drawing is counted
Cader SHALL count each pass in which the console draws, keeping the number of passes, the median and 95th percentile of the latest 1,000 and the longest, and SHALL write them with the other counts.

#### Scenario: Counts written
- **WHEN** the counts are written after the console has drawn 2,000 times
- **THEN** a line says draws since start with 2,000 runs and their median, 95th percentile and longest
