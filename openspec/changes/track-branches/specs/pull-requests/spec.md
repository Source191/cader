# Spec Delta

## ADDED Requirements

### Requirement: A pull request records its head commit
Each pull request a lookup finds SHALL carry the commit its source branch pointed at as the host last recorded it: GitHub's head commit, or Azure DevOps's last merge source commit. A pull request whose head commit is missing or is not a commit id SHALL still be shown, with no head commit.

#### Scenario: GitHub head commit
- **WHEN** the GitHub CLI lists merged pull request #298 with the head commit d6ed49cf…
- **THEN** the pull request is shown as #298 merged and its head commit is d6ed49cf…

#### Scenario: Azure DevOps last merge source
- **WHEN** az lists completed pull request 4521 whose last merge source commit is 9c923515…
- **THEN** the pull request is shown as !4521 completed and its head commit is 9c923515…

#### Scenario: Unreadable head commit
- **WHEN** a listed pull request's head commit is blank
- **THEN** the pull request is still shown, and its branch is never flagged left behind
