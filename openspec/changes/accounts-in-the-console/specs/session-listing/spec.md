# Spec Delta

## ADDED Requirements

### Requirement: Each account is listed on its own
With more than one account, each refresh SHALL run the CLI's listing once per account, one after another, each as its account, and SHALL show every account's rows in one table. A listing that fails SHALL keep that account's last rows, name the account in the banner, and leave the other accounts' rows refreshed.

#### Scenario: Two accounts listed
- **WHEN** work's listing returns two sessions and personal's returns one
- **THEN** the table shows three rows, two of them work's and one personal's

#### Scenario: One account's listing fails
- **WHEN** personal's listing exits with an error saying it could not connect, and work's succeeds
- **THEN** the banner reads that personal's listing failed with the CLI's words, personal's rows are the ones last shown, and work's rows are refreshed

#### Scenario: The same id in two accounts
- **WHEN** both listings return a session with the job id 1a2b3c4d
- **THEN** the table shows two rows, one per account, and selecting one never selects the other after a refresh

### Requirement: The ACCOUNT column names a row's account
With more than one account, the Sessions table SHALL have an ACCOUNT column after REPOSITORY naming each row's account, listed in the column help and the help dialog, and the filter SHALL match the account name. With one account the column SHALL NOT be shown.

#### Scenario: Several accounts
- **WHEN** work and personal are configured
- **THEN** each row's ACCOUNT reads work or personal, and filtering by personal keeps only personal's rows

#### Scenario: One account
- **WHEN** no `accounts` list is configured
- **THEN** the Sessions table has no ACCOUNT column
