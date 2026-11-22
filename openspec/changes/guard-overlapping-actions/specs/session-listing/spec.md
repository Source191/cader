## ADDED Requirements

### Requirement: A late Claude version still reaches the header
When the Claude Code CLI's version has not answered within start-up's five seconds, the header SHALL show Claude unknown until it answers and SHALL then show the version, with no restart.

#### Scenario: A cold start
- **WHEN** Cader starts and the Claude Code CLI takes eight seconds to print 2.1.330
- **THEN** the first settings row reads Claude unknown at first and Claude 2.1.330 once the version has been printed

#### Scenario: No answer at all
- **WHEN** the Claude Code CLI's version command fails
- **THEN** the first settings row reads Claude unknown and the view works as usual
