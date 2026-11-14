# Proposal

## Why

The same Core code scored 91.22 % in the one full mutation run on a `windows-latest` runner and 86.78 % and 86.95 % in two full runs on the owner's machine (`.claude\rules\checks.md`, Mutation testing, 13 November 2026, Stryker 5.0.0): a gap of about 4.4 points with no known cause, from a single runner sample. `mutation-testing-in-ci` (PR #255) proposes a break score of 86 from the runner's figure, and the follow-up `close-mutation-gaps` will ratchet that score up as survivors are closed. Until the gap is explained:

- **The break score is untrustworthy:** a score chosen from the runner can fail on a developer machine, and one chosen from the developer machine is five points too lenient for the runner. The two runs even disagree on how many tests are weak.
- **The survivor list is not settled:** the runner reports 229 survivors and 24 mutants with no covering test; the developer machine 357 and 24. Closing "real test gaps" is wasted effort if about 128 of them are survivors of the machine, not of the tests.

The run counts reconcile (2,629 detected of 2,882 scored on the runner, 2,501 of 2,882 locally), and the timeouts (3) and no-coverage mutants (24) are the same on both. So the difference is about 128 mutants that the runner kills and the developer machine does not, which points at tests that behave differently in the two environments, not at coverage or timeouts. That is a lead to check against the reports, not a finding.

## What Changes

- **A tested script, `.github\scripts\Compare-MutationReports.ps1`:** reads two full Stryker `mutation-report.json` files, pairs their mutants by source-relative file, mutator, replacement and exact location (never by machine paths, and not by the per-run mutant id alone), and groups the mutants whose status differs by status change (for example Survived to Killed), then file, then mutator. It lists the tests that killed a group on the side that killed it, when the report carries them. It builds on `MutationReport.ps1` from `mutation-testing-in-ci` and prints only source-relative paths.
- **Pester tests** in `test\Scripts\Compare-MutationReports.Tests.ps1` over two small made-up report fixtures, following `Compress-WingetBuild.Tests.ps1`.
- **One measured comparison:** a full run on a runner and a full run on the owner's machine at the same commit, compared with the script. The owner supplies the local report (design.md says how).
- **An investigation of the biggest groups**, with a cheap discriminating experiment per candidate cause (time zone, environment variables inherited from a Claude Code session, tools on `PATH`, paths and drives, test ordering and parallelism, coverage analysis).
- **The finding recorded** in `.claude\rules\checks.md` with the date and Stryker version.
- **The cause fixed if it is in the tests**, as a normal test change. If it is the machine, not the tests, the break score is documented as runner-only (the weekly run's), and a local score is stated to be lower for a known reason.
- **A decision for the owner:** whether to repeat the runner sample a few times to measure its own variance (design.md, Open Questions).

## Out of scope

- **Closing the survivors:** that is `close-mutation-gaps`, which follows this change.
- **Changing the workflow or the break score:** this change only records what the break score may be compared with; setting it is `close-mutation-gaps` and the owner.
- **Other projects than Core.**
- **Multiple Claude accounts:** a later change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None: a script under `.github`, its tests and fixtures, and documentation (and a test-only fix if the cause is found there). Nothing users see changes, so `.openspec.yaml` declares `skip_specs: true`.

## Impact

- **New files:** `.github\scripts\Compare-MutationReports.ps1`, `test\Scripts\Compare-MutationReports.Tests.ps1` and its fixtures under `.github\scripts\fixtures\mutation-compare`.
- **Docs:** `.claude\rules\checks.md` (Mutation testing), and `CONTRIBUTING.md` only if the cause changes how a developer should run Stryker.
- **No Version bump or changelog line:** nothing under the tool paths in `ToolPaths.ps1` changes, unless the fix turns out to need a change under `src\` (not expected: the tests are under `test\`).
- **Depends on** `mutation-testing-in-ci` (PR #255) being merged: the script reuses its `MutationReport.ps1`, and the runner's full report comes from its workflow.
