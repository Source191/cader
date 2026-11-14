# Spec Delta

## MODIFIED Requirements

### Requirement: Setup can turn on Claude Code's terminal bell
The setup command SHALL offer to set `preferredNotifChannel` to `terminal_bell` in the `settings.json` of the Claude folder Cader is using (the folder `CLAUDE_CONFIG_DIR` names, or `~\.claude` when it is not set), naming that file in its plan, creating it when it is missing, keeping every other setting and its order, and writing bare LF line endings. When the setting already has that value the command SHALL say so and leave the file untouched; when the file is not valid JSON it SHALL leave it untouched and say why.

#### Scenario: Other settings kept
- **WHEN** settings.json holds a model and a permissions block and the user accepts
- **THEN** settings.json holds the same model and permissions block in the same order, plus `"preferredNotifChannel": "terminal_bell"`, with LF line endings only

#### Scenario: Already set
- **WHEN** settings.json already sets `preferredNotifChannel` to `terminal_bell`
- **THEN** the file is not rewritten and the output says the terminal bell is already on

#### Scenario: Unreadable settings
- **WHEN** settings.json is not valid JSON
- **THEN** the file is unchanged, the output says it could not be read, and the profiles are still written

#### Scenario: The account CLAUDE_CONFIG_DIR names
- **WHEN** the user runs `cader setup-terminal` in a shell where `CLAUDE_CONFIG_DIR` is `C:\Users\me\.claude-personal`, and accepts the bell
- **THEN** the plan names `C:\Users\me\.claude-personal\settings.json`, that file holds `"preferredNotifChannel": "terminal_bell"`, and `C:\Users\me\.claude\settings.json` is unchanged
