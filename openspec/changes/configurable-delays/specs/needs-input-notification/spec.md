# Spec Delta

## ADDED Requirements

### Requirement: The bell follows the Sessions refresh
Because the bell is raised by comparing two Sessions listings, Cader SHALL ring it no later than the first Sessions refresh after the listing that shows the transition could have been started, so the wait is at most `sessionsRefreshSeconds` plus the time the listing takes. A shorter `sessionsRefreshSeconds` SHALL bring the bell forward by the same amount. Cader SHALL NOT ring any sooner than a listing shows the transition, and the rules for when it rings are unchanged.

#### Scenario: Default interval
- **WHEN** a background session starts waiting for input and `sessionsRefreshSeconds` is 5
- **THEN** the bell rings at the first refresh that lists the session as needing input, at most about 5 seconds plus the listing's time later

#### Scenario: A shorter interval
- **WHEN** `sessionsRefreshSeconds` is 2 and a background session starts waiting for input
- **THEN** the bell rings at the first refresh that lists it as needing input, at most about 2 seconds plus the listing's time later

#### Scenario: A longer interval
- **WHEN** `sessionsRefreshSeconds` is 30 and a background session starts waiting for input
- **THEN** the bell may take up to about 30 seconds plus the listing's time, and rings once
