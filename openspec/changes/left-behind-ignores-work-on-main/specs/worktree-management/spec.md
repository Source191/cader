# Spec Delta

## MODIFIED Requirements

### Requirement: Left behind is judged against the merged head
A branch SHALL be left behind when the pull request its rows show (the open one, otherwise the newest) has merged, records a head commit, and the branch has commits that head does not contain whose changes origin's default branch does not have. A merge commit, a commit on origin's default branch, or one whose changes match one there SHALL NOT count. A branch whose merged pull request's head commit is unknown to git, or not recorded, SHALL NOT be flagged left behind.

#### Scenario: A squash merge
- **WHEN** a branch's pull request was squash-merged, so none of its commits is on main, and its tip is the pull request's head
- **THEN** it is not flagged left behind

#### Scenario: A head git has not fetched
- **WHEN** a merged pull request's head commit is not in the repository
- **THEN** the branch is not flagged left behind

#### Scenario: Rebased before merging
- **WHEN** a pull request was rebased before it merged, so its merged head and main hold rebased copies, and the local branch still holds the commits from before the rebase
- **THEN** the branch is not flagged left behind

#### Scenario: Reused after merging
- **WHEN** a branch's pull request merged and the branch was later moved forward to main, so every commit on it is on main
- **THEN** the branch is not flagged left behind

#### Scenario: Work pushed after the merge
- **WHEN** a branch has one commit after its pull request merged that is not on main and whose changes main does not have
- **THEN** the branch is flagged left behind and the pane reads 1 commit after the pull request merged

#### Scenario: Origin's default branch is not recorded
- **WHEN** the repository records no default branch for origin, and a branch has two commits its merged pull request's head does not contain
- **THEN** the branch is flagged left behind with 2 commits, as when nothing is compared with main
