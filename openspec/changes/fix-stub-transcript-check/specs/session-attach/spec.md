## MODIFIED Requirements

### Requirement: Sessions with nothing to resume are refused
Open-in-tab, rename and resume SHALL be refused for a session that has no process and no transcript with a message in it, because Claude writes a session's messages only from its first message, and the status line SHALL say that delete removes it. A transcript holding only the session's title and name is not one with a message. The transcript SHALL be looked for in the Claude folder Cader is using: the folder `CLAUDE_CONFIG_DIR` named when Cader started, or `%USERPROFILE%\.claude` when it was not set.

#### Scenario: Stub session
- **WHEN** the user presses Enter on a failed session whose transcript does not exist
- **THEN** nothing happens, no tab opens, and the status line says it was never sent a message and that delete removes it

#### Scenario: A transcript with only a title and a name
- **WHEN** the user presses Enter on a failed session whose transcript holds only its title and its name
- **THEN** nothing happens, no tab opens, and the status line says it was never sent a message and that delete removes it

#### Scenario: A session of the account CLAUDE_CONFIG_DIR names
- **WHEN** Cader was started with `CLAUDE_CONFIG_DIR` set to `C:\Users\me\.claude-personal`, and the user presses Enter on a stopped session whose transcript is under `C:\Users\me\.claude-personal\projects` and not under `C:\Users\me\.claude\projects`
- **THEN** the session is resumed in its own tab and nothing is refused

### Requirement: Rename is refused while working or before the first message
Renaming SHALL be refused while the session is working or when its transcript holds no message, including a session just moved to the background whose transcript Claude has so far given only a title and a name.

#### Scenario: No message yet
- **WHEN** the user presses the rename key on a background session that has never been sent a message
- **THEN** nothing runs and the status line says why the session cannot be renamed

#### Scenario: Just moved to the background
- **WHEN** the user renames a background session whose transcript holds only its title and its name, because it was moved to the background with `/bg` and has not had a turn since
- **THEN** no job is removed or resumed, the session keeps running under its old name, and the status line says it has no messages yet and should be sent something first
