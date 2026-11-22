## MODIFIED Requirements

### Requirement: The worktrees are scanned as the console opens
The console SHALL start one worktrees scan as it opens, whichever tab is shown. That scan SHALL start reading git at once and SHALL take the sessions inside each worktree from the first sessions listing, waiting for it to return, whether it succeeded, failed or was ended at its time limit, before showing its rows.

#### Scenario: Opened during the first scan
- **WHEN** the user shows the Worktrees tab before the start-up scan has returned
- **THEN** the Worktrees tab waits for that scan, and no second scan starts

#### Scenario: Git read while the listing runs
- **WHEN** the console opens and the first sessions listing takes three seconds
- **THEN** git starts reading the repositories before the listing returns, and the scan's rows, shown after the listing returns, mark the worktrees that hold a listed session

#### Scenario: The sessions listing failed
- **WHEN** the first sessions listing fails as the console opens
- **THEN** the start-up scan still runs and lists the worktrees with no sessions inside them

#### Scenario: The sessions listing hangs
- **WHEN** the first sessions listing has not exited after the child program time limit
- **THEN** the listing is ended and the start-up scan then shows its rows, listing the worktrees with no sessions inside them

## ADDED Requirements

### Requirement: A scan's reads wait only for a free slot
Within a worktrees scan no git read SHALL wait for another read to finish except to take one of the scan's eight slots, and the rows SHALL be the same and in the same order as when the reads run one at a time.

#### Scenario: A repository with many worktrees
- **WHEN** Northwind.Orders has twelve worktrees and four unused branches
- **THEN** their reads run up to eight at once, and the rows are listed in the same order as before

#### Scenario: Reads that finish out of order
- **WHEN** the later repositories' reads return before the earlier ones'
- **THEN** the rows and the orphan rows are the same as when they return in order
