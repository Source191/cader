# Spec Delta

## MODIFIED Requirements

### Requirement: Sessions with nothing to resume are refused
Open-in-tab, rename and resume SHALL be refused for a session that has no process and no transcript, because Claude writes a session's transcript only on its first message, and the status line SHALL say that delete removes it. The transcript SHALL be looked for in the Claude folder Cader is using: the folder `CLAUDE_CONFIG_DIR` named when Cader started, or `%USERPROFILE%\.claude` when it was not set.

#### Scenario: Stub session
- **WHEN** the user presses Enter on a failed session whose transcript does not exist
- **THEN** nothing happens, no tab opens, and the status line says it was never sent a message and that delete removes it

#### Scenario: A session of the account CLAUDE_CONFIG_DIR names
- **WHEN** Cader was started with `CLAUDE_CONFIG_DIR` set to `C:\Users\me\.claude-personal`, and the user presses Enter on a stopped session whose transcript is under `C:\Users\me\.claude-personal\projects` and not under `C:\Users\me\.claude\projects`
- **THEN** the session is resumed in its own tab and nothing is refused
