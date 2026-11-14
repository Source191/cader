# Tasks

Each task is one red-green-refactor cycle or smaller, with a commit per step. Run the Pester tests with `Invoke-Pester test\Scripts\Get-MutationWarning.Tests.ps1`.

## 1. The warning script

- [ ] 1.1 **Red:** `Get-MutationWarning.Tests.ps1`, "says nothing when no issue is open": an empty array and empty text both return nothing. **Green:** `.github\scripts\Get-MutationWarning.ps1` with `-IssuesJson`. **Verify:** the fact passes.
- [ ] 1.2 **Red:** "names the issue number and its URL" for one issue (a made-up `https://github.com/Northwind/cader/issues/12`). **Green:** the warning text. **Verify:** the fact passes.
- [ ] 1.3 **Red:** "names every open issue" for two, and "keeps a line break or a percent sign in a URL from becoming a second workflow command" (a made-up URL holding both). **Green:** the list and the escaping. **Verify:** the facts pass and `Invoke-Pester test\Scripts` passes as a whole.

## 2. The workflow

- [ ] 2.1 **Red:** a fact in `Get-MutationWarning.Tests.ps1`, reading `publish.yml` as `Mutation.Tests.ps1` reads `mutation.yml`: the `mutation-notice` job holds `contents: read` and `issues: read`, has no `environment` and never names a secret; the `winget` job `needs` it and its `permissions` are still `contents: read` and `attestations: read`; the top-level `permissions` is `contents: read`. **Green:** add the job (checkout pinned as in `mutation.yml`; a step that runs `gh issue list --label mutation --state open --json number,url` into the script, prints `::warning::` and the summary section only when it returns text, and prints a notice and continues when `gh` fails) and `needs: mutation-notice` on `winget`. **Verify:** the fact passes.

## 3. Documentation

- [ ] 3.1 `MAINTAINING.md`, winget, "Later versions, by hand": add the pre-publish checklist line (the open `mutation` issue under Issues, and the latest weekly full run's score in Actions, "mutation"), and say `publish.yml` shows a warning on the run's page when an issue is open and never blocks. **Verify:** by reading.
- [ ] 3.2 `.claude\rules\checks.md`, Mutation testing: one line that `publish.yml` warns of an open `mutation` issue before the approval, never blocks, and is held by `Get-MutationWarning.Tests.ps1`. **Verify:** by reading.
- [ ] 3.3 Run `openspec validate check-mutation-before-publishing --strict` and `Invoke-Pester test\Scripts`. **Verify:** each exits 0 and `pipeline`'s `check` is green.

## Workflow follow-up

- **Archive:** archive the change in the same pull request that completes it. Nothing under `src\` changes, so there is no `Version` bump or `CHANGELOG.md` line.

## Implementation plan

See `openspec\implementation-plan.md` for the whole sequence.

- **PR:** T
- **Step:** 14
- **Depends on:** nothing (it needs only the `mutation` label, which `mutation.yml` already creates on `main`)
- **Parallel with:** anything; it touches only `publish.yml`, one script and its tests, `MAINTAINING.md` and `checks.md`. It shares `checks.md` with the mutation changes, so whichever merges second rebases that file.
- **Model:** sonnet
- **Effort:** medium
- **Why:** a small tested script and one workflow job in an established pattern; the care is in the job's rights, which the workflow fact holds.
- **Owner gates:** after merge, confirm the warning appears on the next publish run (with an open `mutation` issue, the annotation and summary section are on the run's page before the approval is given; with none, nothing is shown).
