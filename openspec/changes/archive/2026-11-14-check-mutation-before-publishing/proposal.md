# Proposal

## Why

Mutation testing runs after every merge to `main` that raises `Version` (previews included) and raises its findings on the one rolling issue labelled `mutation`, and a full run every Sunday scores all of Core. But a stable release reaches winget only when the owner runs `publish.yml` by hand, and nothing on that path mentions the mutation findings: the owner could submit a version without having looked at the open ones. A version merged into winget-pkgs reaches every user's update check and can only be superseded, never recalled, so the moment of publishing is the moment the findings matter most.

## What Changes

- **A pre-publish checklist line in `MAINTAINING.md`:** before running `publish.yml`, look at the open issue labelled `mutation` (the repository's Issues tab, filtered by the label) and at the latest weekly full run's score (Actions, "mutation", the newest run started by its schedule; the score is in its job summary). The line says where each is.
- **A visible warning in `publish.yml`:** a small job, `mutation-notice`, that runs before the `winget` job, calls `gh issue list --label mutation --state open` and, when an open issue exists, prints a `::warning::` annotation and writes a line to the job summary naming the issue number and its URL. It says nothing when there is none. It never fails the run and never blocks the submission, because survivors are normal. Because it sits before the `winget` job, the warning is on the run's page while the run waits for the owner's approval, not after the approval is given.
- **A small tested script, `.github\scripts\Get-MutationWarning.ps1`:** takes the issue list (the JSON `gh` prints) as input and returns the warning text, or nothing when the list is empty, so it is tested without GitHub. Pester tests in `test\Scripts\Get-MutationWarning.Tests.ps1`, following `Mutation.Tests.ps1`.
- **A line in `.claude\rules\checks.md`** (Mutation testing) saying `publish.yml` warns of an open `mutation` issue.

## Out of scope

- **Blocking the submission:** the warning never fails or holds the run; the owner decides.
- **Running mutation on pull requests:** the owner declined it; mutation stays off the path of a pull request, a merge and a release.
- **A manual branch dispatch of the mutation workflow:** declined too.
- **Multiple Claude accounts:** a later change.
- **Any change to the mutation workflow, its issue, its label or its break score.**

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None: a workflow job, a script and its tests under `.github` and `test\Scripts`, and documentation. The installation capability's requirement that stable releases reach winget only by the owner's approved run is unchanged, so `.openspec.yaml` declares `skip_specs: true`.

## Impact

- **Files:** `.github\workflows\publish.yml`, `.github\scripts\Get-MutationWarning.ps1`, `test\Scripts\Get-MutationWarning.Tests.ps1`, `MAINTAINING.md`, `.claude\rules\checks.md`, `openspec\implementation-plan.md`.
- **Rights:** the new job holds `contents: read` (to check out the script) and `issues: read`, no secret and no environment. The `winget` job's permissions, the `winget` environment and `WINGET_PKGS_TOKEN` are unchanged, so no new secret or write right is added.
- **Version:** nothing under `src\` changes, so no `Version` bump or `CHANGELOG.md` line.
