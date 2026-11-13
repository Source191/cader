# Spec Delta

## ADDED Requirements

### Requirement: Accounts are kept in config.json
`config.json` SHALL keep the accounts as `accounts` (a list of `name` and `configFolder`), the root accounts as `rootAccounts` (a root's folder to an account name) and the repository overrides as `repositoryAccounts` (a repository's main checkout to an account name). Each SHALL read as absent when missing or unreadable, without changing how any other setting reads, and a save SHALL keep all three as they were.

#### Scenario: A file from an earlier version
- **WHEN** `config.json` holds only `repositoryRoots` of `D:\work`
- **THEN** Cader has one account and scans `D:\work` as before

#### Scenario: A number where the list belongs
- **WHEN** `config.json` holds `repositoryRoots` of `D:\work` and `accounts` of `7`
- **THEN** Cader has one account, still scans `D:\work`, and the Config tab says the accounts could not be read

#### Scenario: Saving another setting keeps the accounts
- **WHEN** two accounts and a repository override are configured and the user changes the worktree root on the Config tab
- **THEN** `config.json` still holds both accounts and the override
