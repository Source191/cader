# Spec Delta

## ADDED Requirements

### Requirement: The setup names the Claude folder
The Setup section SHALL have a grey Claude folder row, directly after Settings file, naming the Claude folder Cader reads and saying whether `CLAUDE_CONFIG_DIR` chose it. A `CLAUDE_CONFIG_DIR` that is not a usable full path SHALL NOT be guessed at: the row SHALL say the variable was not used and that the default folder is read instead. The Claude bell row SHALL read the same folder's `settings.json`.

#### Scenario: The default folder
- **WHEN** Cader was started without `CLAUDE_CONFIG_DIR`
- **THEN** the Claude folder row reads C:\Users\me\.claude

#### Scenario: A folder from CLAUDE_CONFIG_DIR
- **WHEN** Cader was started with `CLAUDE_CONFIG_DIR` set to `C:\Users\me\.claude-personal`, whose `settings.json` says terminal_bell while `C:\Users\me\.claude\settings.json` has no preferredNotifChannel
- **THEN** the Claude folder row reads C:\Users\me\.claude-personal, from CLAUDE_CONFIG_DIR, and the Claude bell row reads on

#### Scenario: A value Cader cannot use
- **WHEN** Cader was started with `CLAUDE_CONFIG_DIR` set to `claude-personal`, which is not a full path
- **THEN** the Claude folder row reads C:\Users\me\.claude and says CLAUDE_CONFIG_DIR was not a full path and was not used, and nothing exits
