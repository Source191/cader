## ADDED Requirements

### Requirement: Settings changes never undo one another
Settings changes SHALL be applied one at a time, each to the settings the change before it saved, so that two changes made in quick succession both take effect.

#### Scenario: Two toggles at once
- **WHEN** the user presses Enter on Check for updates and at once on Diagnostic log, both on
- **THEN** both are off, in the settings file and on the Config tab
