## ADDED Requirements

### Requirement: A started session is never reported as a failed launch
Once the supervisor has started a session, the launch SHALL be reported as started: a listing that fails afterwards SHALL be reported as started and not listed yet, and a recent repository that cannot be recorded SHALL NOT stop the launch.

#### Scenario: The listing fails after the start
- **WHEN** the user starts fix-123 from the wizard and the supervisor starts it, but the listing that follows exits with an error
- **THEN** the status line says fix-123 was started and to press Enter on it once it appears, no tab is opened, and no "Failed:" line is shown

#### Scenario: The recent repository cannot be recorded
- **WHEN** the user starts fix-123 in Northwind.Orders and Cader's state folder refuses the recent-repository record
- **THEN** fix-123 is started and opened in its own tab as usual, and the refusal is recorded only in the diagnostic log
