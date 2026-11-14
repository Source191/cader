## ADDED Requirements

### Requirement: The help says what a report holds
The help, and the user guide's section on reporting a problem, SHALL say that a report holds the latest diagnostic events kept in memory (whether or not the Diagnostic log setting is on), the last crash and the child program counts, that Cader never attaches `cader.log` to it, and that `cader.log`, a separate and longer history rolled at 10 MiB, can be opened with f on the Config tab's Diagnostic log file row and given to Claude or attached to the issue by hand.

#### Scenario: Reading the help
- **WHEN** the user opens the help with ? and reads the entry for !
- **THEN** it says the report holds the recent events, the last crash and the counts, that the log file is not attached, and where to open it

#### Scenario: The log is off
- **WHEN** the Diagnostic log setting is off and the user opens a report
- **THEN** the preview still holds the in-memory events, and the help's wording is true as written

#### Scenario: The user guide
- **WHEN** a user reads Report a problem in `docs\user-guide.md`
- **THEN** it says the same three things in the same words as the help
