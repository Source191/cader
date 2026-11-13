# Spec Delta

## ADDED Requirements

### Requirement: Session keys act as the row's account
Open-in-tab, stop, delete, rename, adopt, resume, resume-all and close-empty SHALL run every Claude Code program, and every tab script, as the selected row's account, and SHALL look for its transcript in that account's folder.

#### Scenario: Attach a personal session
- **WHEN** the user presses Enter on a personal background session while Cader runs without `CLAUDE_CONFIG_DIR`
- **THEN** the tab attaches with `CLAUDE_CONFIG_DIR` set to personal's folder and the session opens

#### Scenario: Rename checks the right transcript
- **WHEN** the user renames a stopped personal session whose transcript is under `C:\Users\me\.claude-personal\projects` only
- **THEN** the rename is not refused, and `claude rm` and the resume both run as personal

#### Scenario: Resume-all with two accounts
- **WHEN** the user presses resume-all with one stopped work session and one stopped personal session shown
- **THEN** each opens in its own tab as its own account
