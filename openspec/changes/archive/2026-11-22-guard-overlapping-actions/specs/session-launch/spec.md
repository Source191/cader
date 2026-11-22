## ADDED Requirements

### Requirement: A launch carries on while a dialog is open
A session being started SHALL carry on through every step whatever dialog the user opens meanwhile, and its tab SHALL open without waiting for that dialog to close; the status line SHALL show the launch's latest step once the dialog closes.

#### Scenario: Memory breakdown during a launch
- **WHEN** the user starts fix-123 from the wizard and, while it is creating its worktree, presses m on fix-456 and leaves the memory breakdown open
- **THEN** fix-123 is started and its tab opens while the breakdown is still open

#### Scenario: A branch question during a launch
- **WHEN** the user starts fix-123 and, while it is starting, removes a worktree whose branch Cader asks about
- **THEN** fix-123 is started and its tab opens while the question is still open
