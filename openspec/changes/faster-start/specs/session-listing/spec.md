## ADDED Requirements

### Requirement: The console opens without waiting for Claude Code's version
The console SHALL draw its first screen without waiting for `claude --version`. When the Claude Code executable has the same size and last write time as when its version was last read, the header SHALL show that version at once; otherwise it SHALL show the version as being read until `claude --version` answers, however long that takes within the child program time limit, and SHALL then show the version, or unknown when the read failed.

#### Scenario: Claude Code unchanged since the last start
- **WHEN** Cader starts and Claude Code's executable is unchanged since Cader last read version 2.1.330
- **THEN** the first screen's header reads Claude 2.1.330 and no `claude --version` runs

#### Scenario: Claude Code updated
- **WHEN** Cader starts after Claude Code updated itself
- **THEN** the first screen is drawn at once with the Claude version shown as being read, and the header shows the new version when `claude --version` answers

#### Scenario: A slow cold start
- **WHEN** the executable changed and `claude --version` takes eight seconds to print 2.1.330
- **THEN** the header shows the version as being read at first and Claude 2.1.330 once it has been printed, with no restart

#### Scenario: The version cannot be read
- **WHEN** the executable changed and `claude --version` fails
- **THEN** the header reads Claude unknown and the view works as usual

### Requirement: The first listing starts with the console
The first sessions listing SHALL start as soon as Claude Code and git have been found, before the console's screen is built, and the Sessions tab's first rows SHALL come from it.

#### Scenario: One listing at start
- **WHEN** Cader starts
- **THEN** one `claude agents` process starts before the first screen is drawn, and no second one starts for the Sessions tab's first rows
