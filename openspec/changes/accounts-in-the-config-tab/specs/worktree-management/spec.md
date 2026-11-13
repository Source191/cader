# Spec Delta

## ADDED Requirements

### Requirement: The account key sets a repository's account
With more than one account, `a` on a Worktrees row SHALL offer every account and Follow the root for the row's repository, marking the current choice; choosing one SHALL save it to `repositoryAccounts` at once (Follow the root removes the entry), and every row of that repository SHALL show the new account. With one account the key SHALL say there is only one account.

#### Scenario: A personal repository under a work root
- **WHEN** Northwind.Blog sits under work's root and the user presses `a` on one of its rows and chooses personal
- **THEN** `repositoryAccounts` gives Northwind.Blog to personal, all its rows read personal, and the status line says new sessions there start as personal

#### Scenario: Back to the root's account
- **WHEN** Northwind.Blog is given to personal and the user presses `a` and chooses Follow the root
- **THEN** its `repositoryAccounts` entry is gone and its rows read work

#### Scenario: One account
- **WHEN** no `accounts` list is configured and the user presses `a`
- **THEN** nothing is saved and the status line says there is only one Claude account and how to add another
