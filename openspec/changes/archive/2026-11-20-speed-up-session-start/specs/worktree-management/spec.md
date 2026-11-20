# Spec Delta

## MODIFIED Requirements

### Requirement: The worktrees are rescanned when git records a change
Whether or not the Worktrees tab is shown, Cader SHALL rescan within about two seconds of a listed repository's git folder gaining or losing a worktree's record, of a checkout changing a branch in its main or a linked worktree, and of a folder appearing in or leaving a repository root's top level. Changes in a burst SHALL start at most one scan a second, never two at once, and a change seen during a scan SHALL start one more after it. While a session started from the wizard is being launched, a change SHALL be kept rather than start a scan, and the worktrees SHALL be scanned after the launch ends.

#### Scenario: Up to date when shown
- **WHEN** the Sessions tab is shown and the user runs `git worktree add` in a shell, waits three seconds and shows the Worktrees tab
- **THEN** the new worktree is listed at once, before the rescan for the show returns

#### Scenario: Removed in a shell
- **WHEN** the Worktrees tab is shown and the user runs `git worktree remove` on one of its worktrees in a shell
- **THEN** the worktree's row goes within about two seconds

#### Scenario: A worktree made by Claude Code
- **WHEN** a session started with `claude --worktree` adds a worktree under `.claude\worktrees` in a listed repository
- **THEN** it is listed within about two seconds

#### Scenario: A branch checked out
- **WHEN** the user runs `git switch feature/refund-retry` in a listed worktree
- **THEN** within about two seconds a rescan runs, and with the worktree selected the detail pane's Branch line reads feature/refund-retry

#### Scenario: A repository cloned into a root
- **WHEN** the user clones Northwind.Billing into C:\Users\me\source\repos
- **THEN** Northwind.Billing is listed within about two seconds of the clone's folder appearing, and its worktrees are watched from then on

#### Scenario: A root changed in settings
- **WHEN** the user saves D:\work as a second repository root
- **THEN** after the next scan, folders appearing in D:\work and worktrees added to its repositories start a rescan too

#### Scenario: A watched repository deleted
- **WHEN** a listed repository's folder is deleted
- **THEN** a rescan drops it, no error is shown, and it is no longer watched

#### Scenario: A worktree the wizard creates
- **WHEN** a launch from the wizard creates the worktree fix-123 in Northwind.Orders
- **THEN** no scan starts while the launch runs, and fix-123 is listed by the scan that starts after it

