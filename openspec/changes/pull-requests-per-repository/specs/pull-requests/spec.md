## RENAMED Requirements

- FROM: `### Requirement: Lookups are made per branch and cached`
- TO: `### Requirement: Lookups are made per repository and cached`

## MODIFIED Requirements

### Requirement: Lookups are made per repository and cached
Cader SHALL ask a host for a repository's latest 100 pull requests at a time, only for repositories that have a row on screen, SHALL reuse a repository's answer for 60 seconds and a failed or unavailable answer for three minutes, SHALL NOT start a second lookup for a repository while one runs, and SHALL run at most four lookups at once.

#### Scenario: Several sessions in one repository
- **WHEN** three sessions run on three branches of the same repository, which has 40 pull requests
- **THEN** one lookup is made for the repository and each row takes its pull request from that answer

#### Scenario: Repositories without a row are left alone
- **WHEN** forty repositories sit under the roots and the worktrees view shows rows for three of them
- **THEN** lookups run for those three repositories and for no other repository

#### Scenario: Within the window
- **WHEN** the tables refresh every five seconds for 50 seconds after a repository was looked up
- **THEN** no further lookup is made for that repository

#### Scenario: Window passed
- **WHEN** a refresh happens more than 60 seconds after a repository's last lookup
- **THEN** a new lookup starts and the rows keep the previous answer until it arrives

#### Scenario: Unavailable answer kept longer
- **WHEN** the host CLI reported the user is not signed in and the tables refresh for two minutes
- **THEN** no further lookup is made for that repository until three minutes have passed

#### Scenario: Older open pull request
- **WHEN** a branch's open pull request is older than the repository's latest 100 pull requests
- **THEN** its row still shows that pull request

#### Scenario: A pull request just opened
- **WHEN** a pull request is opened for fix-123 and the repository is looked up again a minute later
- **THEN** fix-123's row shows the new pull request

#### Scenario: Bounded burst
- **WHEN** twelve repositories need a lookup at the same moment
- **THEN** four lookups run at once and the rest follow as each finishes

### Requirement: A branch name that is not plain text is not given to az
A branch name that is not plain text for a batch file SHALL NOT be given to az: no lookup of that branch on its own SHALL run, and its row SHALL show a pull request only when the repository's answer holds one for it. Otherwise the row's PR column SHALL be blank, and the pull request key SHALL say the branch name was not given to the Azure CLI.

#### Scenario: Percent sign in a branch name
- **WHEN** a worktree of an Azure DevOps repository is on the branch fix/50%-off and the repository's answer holds no pull request for it
- **THEN** az is not started for that branch, its PR column is blank, and the pull request key says the branch name was not given to the Azure CLI

#### Scenario: Found in the repository's answer
- **WHEN** a worktree of an Azure DevOps repository is on the branch fix/50%-off and the repository's answer holds pull request 42 for it
- **THEN** its PR column shows pull request 42, and the branch name was never part of an az command

## ADDED Requirements

### Requirement: A branch older than the latest 100 is looked up on its own
When a repository's answer holds exactly 100 pull requests and none for a branch with a row on screen, Cader SHALL look that branch up on its own and reuse that answer for 15 minutes; when the answer holds fewer than 100, a branch missing from it SHALL have no pull request and SHALL NOT be looked up on its own.

#### Scenario: An old branch in a busy repository
- **WHEN** a repository's answer holds 100 pull requests, none for spike, whose pull request is older
- **THEN** spike is looked up on its own, its row shows that pull request, and spike is not looked up on its own again for 15 minutes

#### Scenario: A branch with no pull request in a small repository
- **WHEN** a repository's answer holds 12 pull requests, none for fix-123
- **THEN** fix-123 is not looked up on its own and its row shows no pull request
