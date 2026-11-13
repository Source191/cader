# Spec Delta

## ADDED Requirements

### Requirement: A new session uses its folder's account
A session started from the wizard SHALL be started, listed and opened as its folder's account (claude-accounts spec), and with more than one account the wizard SHALL show that account on a line of its own before anything starts.

#### Scenario: The wizard names the account
- **WHEN** work and personal are configured and the user opens the wizard on Northwind.Blog, which is personal's
- **THEN** the wizard reads Account personal, and the session starts and opens as personal

#### Scenario: One account
- **WHEN** no `accounts` list is configured
- **THEN** the wizard has no Account line

### Requirement: Trust is asked for as the session's account
When a start fails because the workspace is not trusted and more than one account is configured, the wizard SHALL say which account has not trusted the folder, and the plain Claude Code tab it offers SHALL run as that account.

#### Scenario: Personal has not trusted a folder work trusts
- **WHEN** a personal session's start fails with the workspace not trusted
- **THEN** the wizard says personal has not trusted the folder, and the offered tab runs claude with `CLAUDE_CONFIG_DIR` set to personal's folder

### Requirement: cader here uses the shell's account
`cader here` SHALL start its session as the account a `claude` typed in the same shell would use: the folder the shell's `CLAUDE_CONFIG_DIR` names when it is set, otherwise the current folder's account. With more than one account it SHALL print the account's name before it attaches.

#### Scenario: A shell opened for personal
- **WHEN** the shell's `CLAUDE_CONFIG_DIR` is `C:\Users\me\.claude-personal` and the user runs `cader here fix-tests` in a work repository
- **THEN** the session starts as personal and the output names personal

#### Scenario: A plain shell
- **WHEN** the shell has no `CLAUDE_CONFIG_DIR` and the user runs `cader here fix-tests` in Northwind.Blog, which is personal's
- **THEN** the session starts as personal
