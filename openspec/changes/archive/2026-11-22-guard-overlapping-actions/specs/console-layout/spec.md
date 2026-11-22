## ADDED Requirements

### Requirement: An action already running is not started again
While an action is running for a row, the same action on the same row SHALL NOT be started again from any tab; the status line SHALL say it is still running. The same key on another row, or another action on the same row, SHALL start as usual.

#### Scenario: x pressed twice
- **WHEN** the user presses x on fix-123 and presses x again before the stop has reported
- **THEN** fix-123 is stopped once, the status line says the stop is still running, and no refusal from Claude Code is shown

#### Scenario: Another row
- **WHEN** the user presses x on fix-123 and then x on fix-456 before the first stop has reported
- **THEN** both sessions are stopped

### Requirement: Quit asks while an action is running
Pressing q while any tab has an action running SHALL ask whether to quit, with Quit and Keep open, and Esc SHALL keep the console open; with nothing running q SHALL quit at once as before.

#### Scenario: Quit during a removal
- **WHEN** the user presses q while a worktree removal is still running
- **THEN** Cader asks whether to quit while an action is running, and the removal carries on

#### Scenario: Keep open
- **WHEN** the question is shown and the user presses Esc
- **THEN** the console stays open and the removal reports as usual

#### Scenario: Nothing running
- **WHEN** no action is running and the user presses q
- **THEN** the console quits at once
