## ADDED Requirements

### Requirement: The counts say what Cader holds now
Whenever Cader writes its counts, to the log file, the crash file or a problem report, it SHALL also write one line giving its own handle count and private memory at that moment, carrying the UTC time and saying that the figures are now rather than since start. The line SHALL hold only those two numbers, and a figure Cader cannot read SHALL be written as unknown rather than leaving the line out.

#### Scenario: A mark shows what Cader holds
- **WHEN** Cader writes its counts at a 10-minute mark while it holds 1,240 handles and 96 MB of private memory
- **THEN** the file gains a line with that moment's UTC time saying Cader now holds 1,240 handles and 96 MB private memory

#### Scenario: Growth shows between marks
- **WHEN** Cader holds 1,240 handles at one mark and 3,040 at the next
- **THEN** the two lines read 1,240 and 3,040, so the growth is visible without other tools

#### Scenario: Written with the other counts only
- **WHEN** the Diagnostic log setting is off for an hour
- **THEN** no line is written to the log file, and the crash file, if Cader crashes, still carries the reading with the other counts

#### Scenario: A problem report carries it
- **WHEN** the user opens a problem report from a console holding 116,475 handles
- **THEN** the report's diagnostics show that reading beside the other counts

#### Scenario: A figure cannot be read
- **WHEN** Windows refuses the handle count at a mark
- **THEN** the line says the handle count is unknown and still gives the private memory
