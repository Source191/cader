# Spec Delta

## ADDED Requirements

### Requirement: The pane names the account
With more than one account, the detail pane SHALL have an Account line naming the selected session's account and its folder, or the account a new session in the selected worktree would use and why (its repository, its root, or the first account).

#### Scenario: A session
- **WHEN** a personal session is selected
- **THEN** the pane reads Account personal, C:\Users\me\.claude-personal

#### Scenario: A worktree that falls back
- **WHEN** a worktree whose repository and root name no account is selected, and work is first in the list
- **THEN** the pane reads Account work, the first account
