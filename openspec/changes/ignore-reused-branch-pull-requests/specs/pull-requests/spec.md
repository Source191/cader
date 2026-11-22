## ADDED Requirements

### Requirement: A reused branch name does not show an earlier branch's pull request
A merged, closed, completed or abandoned pull request that finished before the row's local branch was created SHALL be left out of the branch's answer, as if the host had not listed it. The branch's creation time SHALL be git's own record of creating it, the oldest entry of its reflog; with no such record nothing SHALL be left out. An open, active or draft pull request SHALL never be left out.

#### Scenario: Branch name reused after a merge
- **WHEN** branch c-status was created from origin/main at 09:12 on 22 November 2026 and its only pull request, #371, merged at 14:28 on 21 November
- **THEN** its row's PR column is blank, the Worktrees tab flags it · no PR, and the pull request key says no pull request was found for the branch

#### Scenario: Merged after the branch was created
- **WHEN** branch fix-123 was created at 09:00 and its pull request #123 merged at 16:00 the same day
- **THEN** its row shows #123 merged

#### Scenario: A newer pull request after an old one
- **WHEN** branch c-status was created after #371 merged and has since had pull request #380 merged
- **THEN** its row shows #380 merged

#### Scenario: Open pull request older than the local branch
- **WHEN** the user creates a local branch fix-123 to review a colleague's pull request #123, opened the day before and still open
- **THEN** its row shows #123 open

#### Scenario: Abandoned on Azure DevOps before the branch was created
- **WHEN** branch fix-4400 was created after its only pull request, !4400, was abandoned
- **THEN** its row's PR column is blank

#### Scenario: No record of the branch's creation
- **WHEN** a branch's reflog has expired, or the branch came with a clone, and its only pull request merged a week ago
- **THEN** its row shows that pull request merged, as before

### Requirement: A pull request records when it finished
Each finished pull request a lookup finds SHALL carry when it finished as its host recorded it: GitHub's closed time (set for a merged pull request too) or Azure DevOps's closed date. A finished pull request whose time is missing or unreadable SHALL still be shown, and SHALL never be left out as an earlier branch's.

#### Scenario: GitHub merged pull request
- **WHEN** the GitHub CLI lists merged pull request #371 closed at 2026-11-21T14:28:23Z
- **THEN** the pull request is shown as #371 merged and finished at 14:28:23 UTC on 21 November 2026

#### Scenario: Azure DevOps completed pull request
- **WHEN** az lists completed pull request 4521 with the closed date 2026-11-20T10:05:00Z
- **THEN** the pull request is shown as !4521 completed and finished at 10:05 UTC on 20 November 2026

#### Scenario: Unreadable finish time
- **WHEN** a listed merged pull request's closed time is missing
- **THEN** the pull request is still shown, whenever its branch was created
