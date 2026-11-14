# Design

## Context

See proposal.md for the motivation.

**Facts about what exists:**

- **`publish.yml`:** run by hand with a stable tag; one job (`winget`) with `environment: winget` (the owner is its required reviewer, deployment branches are limited to `main`, `WINGET_PKGS_TOKEN` is an environment secret) and `permissions: contents: read` and `attestations: read`. It checks nothing out.
- **The mutation issue:** `mutation.yml`'s `report` job comments on the one open issue labelled `mutation`, or opens one; it stays open until the owner closes it. It finds it with `gh issue list --label mutation --state open`.
- **The weekly score:** the Sunday full run writes it to its job summary and to `run.json` in the `mutation-results` artefact.
- **Pester:** `test\Scripts\*.Tests.ps1` run in `pipeline`'s `check` job; `Mutation.Tests.ps1` is the pattern, including its way of reading a workflow file as text to hold a fact about it.

## Goals / Non-Goals

**Goals:**

- The owner is told about open mutation findings at the moment of publishing a stable release, on the run's own page.
- The checklist says where to look, so the habit does not depend on the warning.

**Non-Goals:**

- Blocking, gating or scoring anything (proposal.md, Out of scope).

## Decisions

- **A job of its own, before the `winget` job.** The `winget` job waits for the owner's approval before any step, so a step inside it would show its warning only after the owner had approved. A separate job, `mutation-notice`, has no `environment`, so it runs at once and its annotation and summary are on the run's page while the approval is pending; `winget` declares `needs: mutation-notice` so the order is fixed. The job always succeeds (the script never throws on an empty or odd list, and a failed `gh` call becomes a notice, not a failure), so `winget` is never held back by it.
- **Least rights.** The new job holds `contents: read` (an `actions/checkout`, pinned as in `mutation.yml`, to get the script) and `issues: read`, and nothing else. `issues: read` is added to this job only: the workflow's top-level `permissions` stays `contents: read` and the `winget` job's block is unchanged. `GH_TOKEN` is the job's `github.token`; no secret is added. A run started from another branch would check out that branch's copy of the script; with no secret and no write right in the job, the worst a changed script could do is print different text, and the `winget` job, which has the secret, still runs only from `main`.
- **The script takes the issue list, not GitHub.** `Get-MutationWarning.ps1 -IssuesJson <text>` reads the array that `gh issue list --label mutation --state open --json number,url` prints and returns the warning text: for one issue, a sentence naming `#<number>` and its URL and saying to look before approving; for several, the same naming each; for an empty array or empty text, nothing. The URL and number come from GitHub but go into a workflow command, so the script strips line breaks and escapes `%` as workflow commands require; the issue title is not used, being text from outside. The workflow step prints `::warning::<text>` and appends a `## Mutation findings` section with the text to `$env:GITHUB_STEP_SUMMARY` only when the script returned something.
- **Where a person looks** (MAINTAINING.md): the open issue is the Issues tab filtered by the `mutation` label; the weekly score is the newest "mutation" run started by `schedule` in Actions, whose job summary has the score (compare it with the break score in `stryker-config.json`).
- **Never blocks.** Survivors are normal (the weekly break score is the only gate, in `mutation.yml`), so the warning informs and the owner decides whether to approve or cancel.

## Risks / Trade-offs

- **The warning says there is an open issue, not whether it matters.** Acceptable: it points at the issue, which has the detail.
- **`gh issue list` can fail** (an outage, a missing right). The step prints a notice and continues; the checklist line is the fallback.
