# Spec Delta

## MODIFIED Requirements

### Requirement: The worktrees are scanned as the console opens
The console SHALL start one worktrees scan as it opens, whichever tab is shown, and that scan SHALL wait until the first sessions listing has returned, whether it succeeded, failed or was ended at its time limit. After it, the worktrees SHALL be rescanned each time their tab is shown, on F5 while it is shown, and at the Worktrees refresh interval while it is shown (every 15 seconds by default, and never on a timer when it is 0), never on a timer while it is hidden, and never while another scan is running. A scan's rows SHALL never replace those of a later scan.

#### Scenario: Never opened
- **WHEN** the console has been open for ten minutes on the Sessions tab
- **THEN** the worktrees have been scanned exactly once

#### Scenario: Opened during the first scan
- **WHEN** the user shows the Worktrees tab before the start-up scan has returned
- **THEN** the Worktrees tab waits for that scan, and no second scan starts

#### Scenario: A slow scan
- **WHEN** the Worktrees tab is shown and a scan of fifty repositories takes forty seconds
- **THEN** the 15-second timer starts no other scan until it returns

#### Scenario: Shown again
- **WHEN** the user leaves the Worktrees tab, runs `git worktree remove` on one of its worktrees in a shell and shows the Worktrees tab again
- **THEN** the tab shows its earlier rows at once and drops the removed worktree when the rescan returns

#### Scenario: The sessions listing failed
- **WHEN** the first sessions listing fails as the console opens
- **THEN** the start-up scan still runs and lists the worktrees with no sessions inside them

#### Scenario: The sessions listing hangs
- **WHEN** the first sessions listing has not exited after the child program time limit
- **THEN** the listing is ended and the start-up scan then runs, listing the worktrees with no sessions inside them

#### Scenario: A reload after an action overlaps the timer's scan
- **WHEN** the 15-second scan is running, the user removes a worktree, the rescan after the removal returns without it, and then the earlier scan returns with the worktree still listed
- **THEN** the removed worktree stays gone

#### Scenario: A longer interval
- **WHEN** Worktrees refresh is 60 seconds and the Worktrees tab is shown
- **THEN** the timer starts a scan 60 seconds after the last one started, not 15

#### Scenario: The timer off
- **WHEN** Worktrees refresh is 0 and the Worktrees tab has been shown for five minutes with no key pressed
- **THEN** the worktrees were scanned once, when the tab was shown

#### Scenario: F5 rescans now
- **WHEN** the user runs `git worktree add` in a shell while the Worktrees tab is shown, then presses F5
- **THEN** a scan starts at once with no spinner, and the new worktree is listed when it returns

#### Scenario: F5 during a scan
- **WHEN** a scan is running and the user presses F5
- **THEN** no second scan starts
