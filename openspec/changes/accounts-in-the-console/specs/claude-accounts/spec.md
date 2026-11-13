# Spec Delta

## Purpose

Lets one Cader work with several Claude accounts, each a Claude configuration folder of its own, and decides which account every folder, row and child program belongs to, so nothing ever acts as the wrong account.

## ADDED Requirements

### Requirement: Accounts are configured in config.json
Cader SHALL read its accounts from the `accounts` list in `config.json`, each a name and a configuration folder given as a full path. When the list is missing or empty, Cader SHALL have one account, the Claude folder it would use without the list, and SHALL show and do exactly what it did before accounts existed.

#### Scenario: No accounts list
- **WHEN** `config.json` has no `accounts` and Cader was started without `CLAUDE_CONFIG_DIR`
- **THEN** Cader has one account whose folder is `C:\Users\me\.claude`, no ACCOUNT column is shown, and every session key behaves as it did before

#### Scenario: Two accounts
- **WHEN** `accounts` names work at `C:\Users\me\.claude` and personal at `C:\Users\me\.claude-personal`
- **THEN** Cader lists the sessions of both accounts and shows the ACCOUNT column

### Requirement: An account's name is plain
An account name SHALL be one to twenty ASCII letters, digits and dashes, starting with a letter, and two accounts SHALL NOT share a name ignoring case or share a folder. An entry that breaks these rules SHALL be left out, and the Config tab SHALL say which entry was left out and why.

#### Scenario: A name with a space
- **WHEN** `accounts` holds work at `C:\Users\me\.claude` and an entry named `my personal`
- **THEN** Cader has the work account only, and the Config tab says the entry my personal was left out because a name is letters, digits and dashes

#### Scenario: The same folder twice
- **WHEN** `accounts` names work and default, both at `C:\Users\me\.claude`
- **THEN** only work is an account, and the Config tab says default was left out because work already uses that folder

### Requirement: A folder's account follows its repository
The account for starting a session in a folder SHALL be the account its repository names in `repositoryAccounts`, else the account its repository root names in `rootAccounts`, else the first account in the list. A linked worktree SHALL follow its repository's main checkout, wherever the worktree's folder sits, and a name that no account has SHALL be treated as absent and flagged on the Config tab.

#### Scenario: An account per root
- **WHEN** `rootAccounts` gives `D:\personal` to personal, and the user starts a session in `D:\personal\Northwind.Blog`
- **THEN** the session is started as personal

#### Scenario: A repository overriding its root
- **WHEN** `C:\Users\me\source\repos` is work's root and `repositoryAccounts` gives `C:\Users\me\source\repos\Northwind.Blog` to personal
- **THEN** a session in Northwind.Blog is started as personal, and a session in `C:\Users\me\source\repos\Northwind.Orders` as work

#### Scenario: A worktree under the shared worktree root
- **WHEN** Northwind.Blog is personal's, and its linked worktree sits at `C:\Users\me\source\worktrees\Northwind.Blog-fix-12` under the worktree root that no account names
- **THEN** a session in that worktree is started as personal

#### Scenario: Nothing names an account
- **WHEN** a folder's repository and root name no account
- **THEN** a session there is started as the first account in the list

### Requirement: A row's account is the listing it came from
A session row SHALL belong to the account whose listing returned it, whatever its folder's account is, and every key on the row SHALL act as that account.

#### Scenario: A personal session in a work repository
- **WHEN** personal's listing returns a session running in `C:\Users\me\source\repos\Northwind.Orders`, a work repository
- **THEN** the row's ACCOUNT is personal and stopping it runs claude as personal

### Requirement: A child is given its account explicitly
With more than one account, every Claude Code program Cader starts for an account, and every tab script that runs one, SHALL be given that account explicitly rather than what Cader inherited: `CLAUDE_CONFIG_DIR` set to the account's folder, or removed for the account whose folder is `%USERPROFILE%\.claude`, so that account keeps its `.claude.json` where Claude Code expects it. With one account, children SHALL inherit `CLAUDE_CONFIG_DIR` exactly as Cader did.

#### Scenario: One account keeps what Cader inherited
- **WHEN** no `accounts` list is configured and Cader was started with `CLAUDE_CONFIG_DIR` set to `C:\Users\me\.claude`
- **THEN** every `claude` child and tab has `CLAUDE_CONFIG_DIR` set to `C:\Users\me\.claude`, as before

#### Scenario: The default folder's account
- **WHEN** Cader was started with `CLAUDE_CONFIG_DIR` set to `C:\Users\me\.claude-personal`, and the user stops a work session whose account folder is `C:\Users\me\.claude`
- **THEN** the `claude stop` child has no `CLAUDE_CONFIG_DIR` in its environment

#### Scenario: Another account
- **WHEN** the user opens a personal session in its tab
- **THEN** the tab's script sets `CLAUDE_CONFIG_DIR` to `C:\Users\me\.claude-personal` before it attaches, with the folder quoted as literal text
