# Spec Delta

## ADDED Requirements

### Requirement: The log files can be cleared
Clearing the diagnostic log SHALL delete `cader.log` and then `cader.log.1`, and nothing else: not the in-memory log, not the crash file, and no other file. A file another program has open SHALL be kept and reported, never fail the console. A line written after the clear, by this Cader or another, SHALL start a new `cader.log`.

#### Scenario: The in-memory log survives
- **WHEN** Cader holds 120 events in memory and the user clears the log
- **THEN** the in-memory log still holds those 120 events, followed by the clear's own events

#### Scenario: The crash file is left alone
- **WHEN** `last-crash.log` exists and the user clears the log
- **THEN** `last-crash.log` is unchanged

#### Scenario: Another Cader is writing
- **WHEN** a second Cader with the setting on records an event just after the clear
- **THEN** `cader.log` exists again holding that event's line, and neither Cader fails

#### Scenario: A rollover at the same moment
- **WHEN** another Cader finds `cader.log` full and moves it to `cader.log.1` while the clear runs
- **THEN** no more than the two files exist afterwards, any file the move left behind is shown on its row and cleared by the next d, and neither Cader fails

#### Scenario: A file held open
- **WHEN** another program holds `cader.log` open without sharing deletion and the user clears the log
- **THEN** `cader.log` is kept, `cader.log.1` is still deleted, and the clear reports one file kept

### Requirement: A clear is recorded
A clear SHALL be recorded as a diagnostic event holding only the number of files deleted, the number of bytes deleted and the number of files kept, with no path or file name. While the Diagnostic log setting is on, it SHALL be written to the new `cader.log` ahead of the line for the clear's own effect.

#### Scenario: Two files cleared with the log on
- **WHEN** the setting is on, nothing else is recorded while the clear runs, and the clear deletes `cader.log` of 2,516,582 bytes and `cader.log.1` of 10,485,700 bytes
- **THEN** the new `cader.log` starts with a line saying the diagnostic log was cleared, 2 files, 13002282 bytes, 0 kept, and no part of C:\Users\me appears in it

#### Scenario: Nothing to clear
- **WHEN** the dialog's Delete finds neither file
- **THEN** the event says 0 files, 0 bytes, 0 kept
