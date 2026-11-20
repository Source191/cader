## MODIFIED Requirements

### Requirement: Branches are judged without reaching the network
A worktrees scan SHALL judge origin's branches and each upstream's state from git's own records as of the last fetch, and SHALL NOT fetch or prune. Only the prune that may follow a scan SHALL reach the network.

#### Scenario: Deleted on origin since the last fetch
- **WHEN** a branch's upstream was deleted on GitHub, no fetch has run since, and the repository shows no origin row
- **THEN** its BRANCH cell does not read ✗ gone, and no git process that reaches the network is started

## ADDED Requirements

### Requirement: Branches origin has deleted are pruned after a scan
After a worktrees scan that shows a ○ origin row, Cader SHALL prune that row's repository's records of origin's branches, removing those origin no longer has, without waiting for it and without any prompt. It SHALL attempt each repository at most once an hour, whether the attempt succeeded or not, SHALL show nothing on screen when one fails, and SHALL NOT delete anything on origin or any local branch.

#### Scenario: A deleted branch leaves the tab
- **WHEN** origin/old-fix shows as a ○ origin row in Northwind.Orders and origin has deleted it
- **THEN** Northwind.Orders is pruned after that scan, and the next scan shows no row for old-fix

#### Scenario: An upstream deleted on origin reads gone
- **WHEN** Northwind.Orders shows a ○ origin row and is pruned, and its branch row spike-refunds tracks origin/spike-refunds, which origin has deleted
- **THEN** the next scan's spike-refunds row reads ✗ gone in BRANCH

#### Scenario: Once an hour per repository
- **WHEN** Northwind.Orders was pruned 20 minutes ago and a scan shows a ○ origin row in it again
- **THEN** it is not pruned again until an hour after that attempt

#### Scenario: Only repositories with an origin row
- **WHEN** a scan shows a ○ origin row in Northwind.Orders and none in Northwind.Billing
- **THEN** only Northwind.Orders is pruned

#### Scenario: A prune that fails
- **WHEN** the machine is offline and a scan shows a ○ origin row in Northwind.Orders
- **THEN** the prune fails without a prompt, the row stays, nothing is shown on screen, the failure is recorded in the diagnostic log, and Northwind.Orders is not tried again for an hour

#### Scenario: A branch origin still has
- **WHEN** a ○ origin row's branch still exists on origin
- **THEN** the prune keeps it and the row stays
