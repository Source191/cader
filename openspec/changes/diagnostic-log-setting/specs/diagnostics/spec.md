# Spec Delta

## MODIFIED Requirements

### Requirement: Nothing personal is ever recorded
Every diagnostic record, in memory, in the log file and in the crash file, SHALL hold only facts defined in Vantage's own code (kinds, fixed program and subcommand names, exit codes, durations, counts, versions, exception summaries). No record SHALL hold a path, a repository, branch, worktree or session name, an id, a prompt, pull-request detail, a file name, any program's output, a user or machine name, or anything typed.

#### Scenario: A session is attached
- **WHEN** the user opens the session "Fix flaky tests" (job 81cdb0f9) running in C:\Users\me\source\worktrees\Northwind.Orders-fix-123 in its own tab
- **THEN** the log records that the open-in-tab effect ran with its outcome kind, and neither "Fix flaky tests", "81cdb0f9", "Northwind.Orders" nor "C:\Users\me" appears in it

#### Scenario: git fails with a message naming the repository
- **WHEN** `git worktree remove` fails in C:\Users\me\source\repos\Northwind.Orders with "fatal: 'C:\Users\me\source\worktrees\Northwind.Orders-fix-123' contains modified or untracked files"
- **THEN** the counts hold git `worktree remove` exit 128, the log records that the remove effect failed with a `GitException`, and no part of git's message, the folder or the repository's name appears in either

#### Scenario: Text typed in a dialog
- **WHEN** the user types a session name and an opening prompt in the new session wizard and starts the session
- **THEN** the log records that the wizard opened and that a start effect ran with its outcome kind, and neither the name nor the prompt appears in it

#### Scenario: Terminal.Gui reports input it could not parse
- **WHEN** Terminal.Gui logs a warning that quotes an unrecognised input sequence
- **THEN** the log records a Terminal.Gui warning with no text from Terminal.Gui's message

### Requirement: The log is always on and bounded
Vantage SHALL keep the most recent 500 diagnostic events in memory from start to exit, whether or not the Diagnostic log setting is on, dropping the oldest first. Each event SHALL carry its UTC time. The in-memory log SHALL never be written to disc except as the crash file or as the log file while the Diagnostic log setting is on.

#### Scenario: More events than the log holds
- **WHEN** Vantage records its 501st event
- **THEN** the in-memory log holds the latest 500 and the first is gone

#### Scenario: No log variable
- **WHEN** Vantage runs with the Diagnostic log setting off
- **THEN** it writes no log file and still keeps the in-memory log

### Requirement: Counts are written while Vantage runs
While the Diagnostic log setting is on, Vantage SHALL write the child program, refresh and stall counts to the log file every 10 minutes, and when it exits, each count line carrying the UTC time it was written. The counts SHALL NOT be kept as events in the in-memory log, and nothing SHALL be written while the setting is off.

#### Scenario: Ten minutes pass
- **WHEN** Vantage has run for 10 minutes with the Diagnostic log setting on
- **THEN** the file gains every count line with that moment's UTC time, and the in-memory log holds no count line

#### Scenario: No log variable
- **WHEN** Vantage runs for an hour with the Diagnostic log setting off
- **THEN** no file is written and the counts are still kept for the crash file

#### Scenario: Turned on part way
- **WHEN** Vantage has run for 25 minutes with the setting off and the user then turns it on
- **THEN** the next count lines are written at the next 10-minute mark, and none of the earlier marks is written late

## REMOVED Requirements

### Requirement: VANTAGE_LOG writes the same events to a file
**Reason**: The log file is now switched by the Diagnostic log setting in `config.json` and written to a fixed place, so the environment variable is no longer read.
**Migration**: Turn on Diagnostic log on the Config tab (or set `"diagnosticLog": true` in `config.json`); the lines go to `%LOCALAPPDATA%\Vantage\vantage.log`. A `VANTAGE_LOG` still set in the environment is ignored.

## ADDED Requirements

### Requirement: The Diagnostic log setting writes the same events to a file
While the Diagnostic log setting is on, Vantage SHALL append each diagnostic event to `%LOCALAPPDATA%\Vantage\vantage.log` as one line as it is recorded, with exactly the text the in-memory log holds for that event. Turning the setting on or off SHALL take effect from the next event, without a restart.

#### Scenario: Setting on
- **WHEN** the Diagnostic log setting is on and the user opens a session in its tab and quits
- **THEN** `vantage.log` gains the line naming the open-in-tab effect with its UTC time, and the counts when Vantage exits, each count line with the UTC time it was written

#### Scenario: Turned on while Vantage runs
- **WHEN** Vantage runs with the setting off, and the user turns it on and then stops a session
- **THEN** `vantage.log` gains the line naming the stop effect, and holds no line for anything recorded before the setting was turned on

#### Scenario: Turned off while Vantage runs
- **WHEN** the setting is on, the user turns it off and then stops a session
- **THEN** `vantage.log` gains no line for the stop, and the in-memory log still holds it

#### Scenario: A file that cannot be written
- **WHEN** the setting is on and `vantage.log` is locked by another program
- **THEN** Vantage carries on, the line stays in the in-memory log, and nothing on the screen changes

#### Scenario: VANTAGE_LOG left in the environment
- **WHEN** Vantage starts with `VANTAGE_LOG` set to `%TEMP%\old.log` and the setting off
- **THEN** no file is written, neither `%TEMP%\old.log` nor `vantage.log`

### Requirement: The log file is capped
When appending a line would take `vantage.log` past 10 MB (10,485,760 bytes), Vantage SHALL first rename it to `vantage.log.1`, replacing any earlier `vantage.log.1`, and write the line to a new `vantage.log`, so that no more than the two files are ever kept.

#### Scenario: The file reaches its size
- **WHEN** `vantage.log` holds 10,485,700 bytes and the next line is 100 bytes long
- **THEN** the old file is `vantage.log.1` and `vantage.log` holds only the new line

#### Scenario: An older rollover exists
- **WHEN** `vantage.log.1` already exists and `vantage.log` reaches its size again
- **THEN** the earlier `vantage.log.1` is replaced, and only `vantage.log` and one `vantage.log.1` remain