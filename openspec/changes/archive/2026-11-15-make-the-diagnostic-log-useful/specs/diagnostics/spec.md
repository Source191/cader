## ADDED Requirements

### Requirement: Each line names its process
Every diagnostic line, in the log file, the crash file and a problem report, SHALL carry the id of the Cader process that recorded it, after the UTC time and before the event, so the lines of several Cader processes writing to the same file can be told apart. The line recorded when Cader starts SHALL keep its version, so a process id leads to its version.

#### Scenario: Two consoles share the log
- **WHEN** a console with process id 4120 and another with process id 9388 both write to `cader.log` with the Diagnostic log setting on
- **THEN** every line from the first carries 4120 and every line from the second 9388, each process's `started` line names its own version, and no line carries both

#### Scenario: Count lines
- **WHEN** the console with process id 4120 writes its counts at a 10-minute mark
- **THEN** every count line carries 4120 after its UTC time

### Requirement: A failed start is recorded once per command and folder
A run that fails to start SHALL be recorded as a line, with Windows' error number when there is one, the first time its command fails to start in its folder, and again only after that command has started in that folder since. The folder SHALL be held only in memory to tell failures apart and SHALL never be recorded.

#### Scenario: The same start keeps failing
- **WHEN** `git rev-parse` cannot be started in the missing folder C:\Users\me\source\worktrees\Northwind.Orders-fix-123 on every 30-second scan for an hour
- **THEN** the log has one line saying git `rev-parse` was not started, with error number 267, and no more for that command in that folder

#### Scenario: Another folder or another command
- **WHEN** `git rev-parse` has already failed to start in one missing folder, and then fails to start in a second missing folder, and `git status` fails to start in the first
- **THEN** the log gains a line for each of the two new failures

#### Scenario: The folder comes back and goes again
- **WHEN** `git rev-parse` failed to start in a folder, then started there after the folder was restored, and then failed to start there again after it was deleted again
- **THEN** the log has a line for the first failure and another for the last

### Requirement: A stall line names the work that ran
A stall recorded as a line SHALL also name the longest piece of marked UI-thread work since the beat before (a refresh's or a detail's result being shown, a key, an action's outcome) by its code name and duration, or say that no marked work ran, which points at drawing or Terminal.Gui itself.

#### Scenario: A freeze after rows were shown
- **WHEN** the UI thread runs a beat 820 ms late after showing a worktrees scan's rows, which took 610 ms, and a key, which took 5 ms
- **THEN** the stall line says the UI thread stalled for 820 ms and names the worktree rows being shown and 610 ms

#### Scenario: A freeze with no marked work
- **WHEN** the UI thread runs a beat 700 ms late and no marked work ran since the beat before
- **THEN** the stall line says the UI thread stalled for 700 ms and that no marked work ran

#### Scenario: Work before the previous beat
- **WHEN** the rows were shown before one beat that ran on time, and the next beat runs 600 ms late with no marked work between them
- **THEN** the stall line says no marked work ran and does not name the rows

#### Scenario: A key that opens a dialog
- **WHEN** a key opens a dialog that stays open for 30 seconds, during which the heartbeat runs on time, and the beat after the dialog closes runs 600 ms late
- **THEN** the stall line names the key with at most the time it ran since the beat before, never the 30 seconds the dialog was open

## MODIFIED Requirements

### Requirement: Nothing personal is ever recorded
Every diagnostic record, in memory, in the log file and in the crash file, SHALL hold only facts defined in Cader's own code (kinds, code names, fixed program and subcommand names, exit codes, Windows error numbers, durations, counts, versions, exception summaries) and its own process id. No record SHALL hold a path, a repository, branch, worktree or session name, any other id, a prompt, pull-request detail, a file name, any program's output, a user or machine name, or anything typed.

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
- **WHEN** Terminal.Gui logs a warning that quotes an unrecognised input sequence "Northwind.Orders"
- **THEN** the log records a Terminal.Gui warning naming the Terminal.Gui method that raised it, and no text from Terminal.Gui's message, "Northwind.Orders" included

#### Scenario: A program cannot be started in a missing folder
- **WHEN** git cannot be started in C:\Users\me\source\worktrees\Northwind.Orders-fix-123 because the folder has gone
- **THEN** the log records git's subcommand, Windows' error number 267 and the summary, and neither "C:\Users\me" nor "Northwind.Orders" appears in it

### Requirement: Child programs are counted
Cader SHALL count every child program it runs by program (git, claude, gh, az, pwsh, wt, vswhere, dotnet, or another program), its subcommand when the leading arguments are words on Cader's fixed list for that program, and its exit code, keeping the number of runs, the median and 95th percentile duration of the latest 1,000 runs, and the longest. A run that fails to start (once per command and folder), takes longer than 5 seconds or is ended at its time limit SHALL also be recorded as a line.

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
When an effect, a view's load, a dialog flow, the update check or a background task fails, Cader SHALL record the failure with an exception summary: the type of the exception and of each inner exception, Windows' error number for a Windows error, and the stack frames as method names with the source file's name and line where known, without the exception's message, any folder in a file path or any value. The screen SHALL show the failure as it does today.

#### Scenario: A load fails
- **WHEN** the sessions view's load throws an `InvalidOperationException` whose message names a folder
- **THEN** the status line shows the message as before, and the log records a load failure of the sessions view with `InvalidOperationException` and its frames, without the message

#### Scenario: Stack frames of a local build
- **WHEN** a Cader built in C:\Users\me\source\repos\cader fails inside `GitCli.RunOrThrowAsync`
- **THEN** the frame reads with the method name and `GitCli.cs` and its line, and "C:\Users\me" does not appear

#### Scenario: A Windows error
- **WHEN** a failure is a `Win32Exception` with error number 2, inside an `InvalidOperationException`
- **THEN** the summary names both types and the number 2 beside `Win32Exception`, and none of Windows' message for the error

### Requirement: Counts are written while Cader runs
While the Diagnostic log setting is on, Cader SHALL write the child program, refresh and stall counts to the log file every 10 minutes, and when it exits, each count line carrying the UTC time it was written and saying that its figures are since the process started. The counts SHALL NOT be kept as events in the in-memory log, and nothing SHALL be written while the setting is off.

#### Scenario: Ten minutes pass
- **WHEN** Cader has run for 10 minutes with the Diagnostic log setting on
- **THEN** the file gains every count line with that moment's UTC time, and the in-memory log holds no count line

#### Scenario: Counts accumulate
- **WHEN** Cader has run `git status` 40 times by the first 10-minute mark and 30 more times by the second
- **THEN** the second mark's line for git `status` says 70 runs since start

#### Scenario: Setting off
- **WHEN** Cader runs for an hour with the Diagnostic log setting off
- **THEN** no file is written and the counts are still kept for the crash file

#### Scenario: Turned on part way
- **WHEN** Cader has run for 25 minutes with the setting off and the user then turns it on
- **THEN** the next count lines are written at the next 10-minute mark, and none of the earlier marks is written late
