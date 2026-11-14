# Spec Delta

## ADDED Requirements

### Requirement: The Config tab shows the accounts
With more than one account, the Setup section SHALL list each account with its folder in place of the single Claude folder row, each repository override with its account, and each entry Cader left out with the reason, and each repository root row SHALL name the account its root belongs to.

#### Scenario: Two accounts and an override
- **WHEN** work at `C:\Users\me\.claude` owns `C:\Users\me\source\repos`, personal at `C:\Users\me\.claude-personal` owns `D:\personal`, and Northwind.Blog is given to personal
- **THEN** the Setup section lists work and personal with their folders and Northwind.Blog personal, and the root rows read C:\Users\me\source\repos work and D:\personal personal

#### Scenario: An unknown account name
- **WHEN** `rootAccounts` gives `D:\personal` to persnal, which no account has
- **THEN** the D:\personal row says persnal is not an account and that the first account is used
