# Spec Delta

## ADDED Requirements

### Requirement: The delays are setting rows
The Config tab SHALL list the four delays as setting rows after the other settings, labelled Sessions refresh, Worktrees refresh, Git cache and Pull request cache, each with a VALUE in seconds such as `5 s`. Each row's tooltip SHALL name its `config.json` property and its default, and say in one sentence what a lower value costs. Enter on these rows SHALL do nothing: they are edited in `config.json`.

#### Scenario: Default delays
- **WHEN** `config.json` holds none of the four properties and the user shows the Config tab
- **THEN** the rows read Sessions refresh 5 s, Worktrees refresh 15 s, Git cache 30 s and Pull request cache 60 s

#### Scenario: A delay set by hand
- **WHEN** `config.json` holds `sessionsRefreshSeconds` of 2 and Cader starts
- **THEN** the Sessions refresh row reads 2 s

#### Scenario: Reading a row's tooltip
- **WHEN** the user hovers the Sessions refresh row
- **THEN** the tooltip names `sessionsRefreshSeconds`, its default of 5 and that a lower value brings the bell forward but runs claude more often

#### Scenario: Enter on a delay row
- **WHEN** the user presses Enter on the Git cache row
- **THEN** nothing opens and nothing changes
