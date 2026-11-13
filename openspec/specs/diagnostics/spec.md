# diagnostics Specification

## Purpose
Records what Cader itself did, so a maintainer can diagnose a problem, while guaranteeing that nothing about the user, their machine or the code in their repositories is ever recorded.

## Requirements

### Requirement: Nothing personal is ever recorded
Every diagnostic record, in memory, in the log file and in the crash file, SHALL hold only facts defined in Cader's own code (kinds, fixed program and subcommand names, exit codes, durations, counts, versions, exception summaries). No record SHALL hold a path, a repository, branch, worktree or session name, an id, a prompt, pull-request detail, a file name, any program's output, a user or machine name, or anything typed.

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

### Requirement: Cader records its own behaviour
Cader SHALL record a diagnostic event when it starts (its version, the Claude Code version, the Windows version, the .NET runtime and the terminal's size), when the user switches view or opens a dialog, and when an effect runs (its kind, the kind of outcome and how long it took). A refresh that succeeds within 3 seconds SHALL NOT be recorded as an event; it is only counted.

#### Scenario: An effect runs
- **WHEN** the user stops a session with `x`
- **THEN** the log has a line naming the stop effect, the kind of its outcome and its duration

#### Scenario: The sessions view refreshes
- **WHEN** the sessions view refreshes every 5 seconds for an hour without a failure and each part of each refresh takes under 3 seconds
- **THEN** the log gains no event for those refreshes, and the refresh counts hold about 720 runs of the sessions listing and of the sessions enrichment

### Requirement: Child programs are counted
Cader SHALL count every child program it runs by program (git, claude, gh, az, pwsh, wt, vswhere, dotnet, or another program), its subcommand when the leading arguments are words on Cader's fixed list for that program, and its exit code, keeping the number of runs, the median and 95th percentile duration of the latest 1,000 runs, and the longest duration. A run that fails to start, takes longer than 5 seconds or is ended at its time limit SHALL also be recorded as a line.

#### Scenario: Worktree lists are read
- **WHEN** a scan runs `git worktree list --porcelain` in three repositories and each exits 0
- **THEN** the counts hold git `worktree list` exit 0 three times with the median, the 95th percentile and the longest duration in milliseconds, and the log gains no line for them

#### Scenario: Typical and worst runs differ
- **WHEN** `claude agents --json` exits 0 one hundred times, ninety-nine of them in 1,600 ms and one in 4,900 ms
- **THEN** the counts hold claude `agents` exit 0 with 100 runs, median 1600 ms, 95th percentile 1600 ms and longest 4900 ms

#### Scenario: More runs than the sample holds
- **WHEN** git `status` exits 0 for the 1,001st time
- **THEN** its count says 1001 runs, its longest is the longest of all 1,001, and its median and 95th percentile are taken over the latest 1,000

#### Scenario: A slow child
- **WHEN** `claude agents --json` takes 6.2 seconds
- **THEN** the log has a line naming claude, `agents`, exit code 0 and 6200 ms

#### Scenario: A child ended at its limit
- **WHEN** `claude agents --json` is ended after 60 seconds without exiting
- **THEN** the log has a line saying claude `agents` timed out after 60000 ms, with no exit code and nothing of its output, it is not recorded as a program that failed to start, and the counts gain no run for it

#### Scenario: A program Cader has no word for
- **WHEN** Cader runs C:\Users\me\tools\MyTool.exe with the argument C:\Users\me\source\repos\Northwind.Orders
- **THEN** the counts name it as another program with no subcommand, and no part of its path or arguments is recorded

#### Scenario: An argument that is not on the list
- **WHEN** Cader runs `git branch -d fix-123`
- **THEN** the counts name git `branch` and record neither `-d` nor `fix-123`

### Requirement: Failures are recorded with a summary
When an effect, a view's load, a dialog flow, the update check or a background task fails, Cader SHALL record the failure with an exception summary: the type of the exception and of each inner exception, and the stack frames as method names with the source file's name and line where known, without the exception's message, any folder in a file path or any value. The screen SHALL show the failure as it does today.

#### Scenario: A load fails
- **WHEN** the sessions view's load throws an `InvalidOperationException` whose message names a folder
- **THEN** the status line shows the message as before, and the log records a load failure of the sessions view with `InvalidOperationException` and its frames, without the message

#### Scenario: Stack frames of a local build
- **WHEN** a Cader built in C:\Users\me\source\repos\cader fails inside `GitCli.RunOrThrowAsync`
- **THEN** the frame reads with the method name and `GitCli.cs` and its line, and "C:\Users\me" does not appear

### Requirement: The log is always on and bounded
Cader SHALL keep the most recent 500 diagnostic events in memory from start to exit, whether or not the Diagnostic log setting is on, dropping the oldest first. Each event SHALL carry its UTC time. The in-memory log SHALL never be written to disc except as the crash file or as the log file while the Diagnostic log setting is on.

#### Scenario: More events than the log holds
- **WHEN** Cader records its 501st event
- **THEN** the in-memory log holds the latest 500 and the first is gone

#### Scenario: Setting off
- **WHEN** Cader runs with the Diagnostic log setting off
- **THEN** it writes no log file and still keeps the in-memory log

### Requirement: A crash leaves a crash file
When an exception ends Cader, it SHALL write the in-memory log followed by the crash's exception summary to `%LOCALAPPDATA%\Cader\last-crash.log`, replacing any earlier crash file, before it exits. A failure to write the crash file SHALL NOT hide the original exception.

#### Scenario: Unhandled exception
- **WHEN** an exception escapes the console and ends Cader
- **THEN** `last-crash.log` holds the events recorded before it, the child program, refresh and stall counts, and the crash's summary, and the exception is still reported as today

#### Scenario: Normal exit
- **WHEN** the user quits Cader with `q`
- **THEN** no crash file is written and an existing one is left as it was

### Requirement: Refreshes are timed
Cader SHALL count each part of a refresh by part (the sessions listing, the sessions enrichment, the worktrees scan, the pull-request refresh), keeping the number of completed runs, the median and 95th percentile of the latest 1,000, and the longest. A part that takes longer than 3 seconds SHALL also be recorded as a line naming the part and its duration. A cancelled part SHALL NOT be counted; a failed part is recorded as a failure as today.

#### Scenario: A slow scan
- **WHEN** the worktrees scan takes 3.4 seconds
- **THEN** the refresh counts gain a worktrees scan run and the log has a line naming the worktrees scan and 3400 ms, with no folder, repository or worktree name

#### Scenario: The scan waits for the first listing
- **WHEN** the first worktrees scan waits 9 seconds for the sessions tab's first listing and then scans for 0.8 seconds
- **THEN** the worktrees scan is counted as 800 ms and no slow line is recorded for it

#### Scenario: A refresh is cancelled
- **WHEN** the user leaves the Worktrees tab while a scan is running and the scan is cancelled
- **THEN** the refresh counts do not change and no line is recorded

### Requirement: The UI thread is watched
Cader SHALL measure how late the UI thread runs a regular heartbeat. A beat more than 100 ms late SHALL be counted as a stall, keeping the number, the median and 95th percentile of the latest 1,000 and the longest; a beat more than 500 ms late SHALL also be recorded as a line with its lateness. A gap longer than 30 seconds SHALL be taken as the machine sleeping and neither counted nor recorded.

#### Scenario: A short stall
- **WHEN** the UI thread runs a beat 180 ms late
- **THEN** the stall counts gain one stall of 180 ms and the log gains no line

#### Scenario: A freeze the user feels
- **WHEN** the UI thread runs a beat 820 ms late
- **THEN** the stall counts gain one stall of 820 ms and the log has a line saying the UI thread stalled for 820 ms

#### Scenario: The laptop sleeps
- **WHEN** the machine sleeps for 40 minutes and the next beat runs on waking
- **THEN** no stall is counted and no line is recorded

### Requirement: Counts are written while Cader runs
While the Diagnostic log setting is on, Cader SHALL write the child program, refresh and stall counts to the log file every 10 minutes, and when it exits, each count line carrying the UTC time it was written. The counts SHALL NOT be kept as events in the in-memory log, and nothing SHALL be written while the setting is off.

#### Scenario: Ten minutes pass
- **WHEN** Cader has run for 10 minutes with the Diagnostic log setting on
- **THEN** the file gains every count line with that moment's UTC time, and the in-memory log holds no count line

#### Scenario: Setting off
- **WHEN** Cader runs for an hour with the Diagnostic log setting off
- **THEN** no file is written and the counts are still kept for the crash file

#### Scenario: Turned on part way
- **WHEN** Cader has run for 25 minutes with the setting off and the user then turns it on
- **THEN** the next count lines are written at the next 10-minute mark, and none of the earlier marks is written late

### Requirement: The Diagnostic log setting writes the same events to a file
While the Diagnostic log setting is on, Cader SHALL append each diagnostic event to `%LOCALAPPDATA%\Cader\cader.log` as one line as it is recorded, with exactly the text the in-memory log holds for that event. Turning the setting on or off SHALL take effect from the next event, without a restart.

#### Scenario: Setting on
- **WHEN** the Diagnostic log setting is on and the user opens a session in its tab and quits
- **THEN** `cader.log` gains the line naming the open-in-tab effect with its UTC time, and the counts when Cader exits, each count line with the UTC time it was written

#### Scenario: Turned on while Cader runs
- **WHEN** Cader runs with the setting off, and the user turns it on and then stops a session
- **THEN** `cader.log` gains the line naming the stop effect, and holds no line for anything recorded before the setting was turned on

#### Scenario: Turned off while Cader runs
- **WHEN** the setting is on, the user turns it off and then stops a session
- **THEN** `cader.log` gains no line for the stop, and the in-memory log still holds it

#### Scenario: A file that cannot be written
- **WHEN** the setting is on and `cader.log` is locked by another program
- **THEN** Cader carries on, the line stays in the in-memory log, and nothing on the screen changes

#### Scenario: CADER_LOG left in the environment
- **WHEN** Cader starts with `CADER_LOG` set to `%TEMP%\old.log` and the setting off
- **THEN** no file is written, neither `%TEMP%\old.log` nor `cader.log`

### Requirement: The log file is capped
When appending a line would take `cader.log` past 10 MB (10,485,760 bytes), Cader SHALL first rename it to `cader.log.1`, replacing any earlier `cader.log.1`, and write the line to a new `cader.log`, so that no more than the two files are ever kept.

#### Scenario: The file reaches its size
- **WHEN** `cader.log` holds 10,485,700 bytes and the next line is 100 bytes long
- **THEN** the old file is `cader.log.1` and `cader.log` holds only the new line

#### Scenario: An older rollover exists
- **WHEN** `cader.log.1` already exists and `cader.log` reaches its size again
- **THEN** the earlier `cader.log.1` is replaced, and only `cader.log` and one `cader.log.1` remain
