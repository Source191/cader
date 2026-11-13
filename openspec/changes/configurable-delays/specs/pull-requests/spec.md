# Spec Delta

## MODIFIED Requirements

### Requirement: Lookups are made per branch and cached
Cader SHALL ask a host for the pull requests of one branch at a time, only for branches that have a row on screen, SHALL reuse a branch's answer for `pullRequestCacheSeconds` (60 seconds unless the setting says otherwise) and a failed or unavailable answer for three times that (three minutes by default), SHALL NOT start a second lookup for a branch while one is running, and SHALL run at most four lookups at once.

#### Scenario: Several sessions in one repository
- **WHEN** three sessions run on three branches of the same repository
- **THEN** one lookup is made per branch, each asking the host for that branch only, and each row takes its pull request from its own answer

#### Scenario: Repositories without a row are left alone
- **WHEN** forty repositories sit under the roots and the worktrees view shows rows for three of them
- **THEN** lookups run for those three repositories' branches and for no other repository

#### Scenario: Within the window
- **WHEN** the tables refresh every five seconds for 50 seconds after a branch was looked up
- **THEN** no further lookup is made for that branch

#### Scenario: Window passed
- **WHEN** a refresh happens more than 60 seconds after a branch's last lookup
- **THEN** a new lookup starts and the row keeps the previous answer until it arrives

#### Scenario: Unavailable answer kept longer
- **WHEN** the host CLI reported the user is not signed in and the tables refresh for two minutes
- **THEN** no further lookup is made for that branch until three minutes have passed

#### Scenario: Older open pull request
- **WHEN** a branch's open pull request is older than the repository's latest 100 pull requests
- **THEN** its row still shows that pull request

#### Scenario: Bounded burst
- **WHEN** twelve rows need a lookup at the same moment
- **THEN** four lookups run at once and the rest follow as each finishes

#### Scenario: A configured window
- **WHEN** `pullRequestCacheSeconds` is 20 and a refresh happens 25 seconds after a branch's last lookup
- **THEN** a new lookup starts, and a failed answer for that branch is kept for 60 seconds
