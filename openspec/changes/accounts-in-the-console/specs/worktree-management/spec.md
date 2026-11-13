# Spec Delta

## ADDED Requirements

### Requirement: The Worktrees ACCOUNT column names the repository's account
With more than one account, the Worktrees table SHALL have an ACCOUNT column naming the account a new session in each worktree would use, listed in the column help, and the filter SHALL match it. With one account the column SHALL NOT be shown.

#### Scenario: A repository override
- **WHEN** Northwind.Blog is given to personal in `repositoryAccounts` and sits under work's root
- **THEN** Northwind.Blog's worktree rows read personal and Northwind.Orders' rows read work
