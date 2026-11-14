# Design

## Context

See proposal.md for the motivation.

**What is on record** (`.claude\rules\checks.md`, 13 November 2026, Stryker 5.0.0, SDK 10.0.404):

- **Developer machine, full run:** 3,819 mutants, 2,858 tested; 2,498 killed, 3 timed out, 357 survived, 24 without coverage; score 86.78 %. A second run from a fresh clone scored 86.95 %.
- **`windows-latest`, full run** (run 43478448861): 2,626 killed, 3 timed out, 229 survived, 24 without coverage; score 91.22 %. One sample.
- **The arithmetic:** detected over detected plus undetected is 2,629 / 2,882 on the runner and 2,501 / 2,882 locally. The two runs differ by about 128 mutants that are Killed on the runner and Survived locally, and by nothing in timeouts or no-coverage. Task 3.1 confirms this from the two reports before anything is built on it.
- **Where the runner's survivors are** (from its `mutation-results` artefact, 253 survivors and no-coverage mutants in 78 files, by folder): `Application` 86, `Domain` 46, `Application\Diagnostics` 34, `Application\Views` 26, `Ports` 18, `Application\Detail` 14, `Application\Actions` 13, `Application\Reporting` 11, `Application\Launch` 3 and `Application\TerminalSetup` 2. The local run's survivors are the same plus about 128 more, spread over files not yet counted.

**Facts about the existing scripts** (`mutation-testing-in-ci`, `.github\scripts`):

- **`MutationReport.ps1`:** `Read-MutationReport`, `Get-SourceName` (a file's key is an absolute path with backslashes, named from `\src\` down, so a runner path `D:\a\cader\cader\src\...` and a developer path compare equal) and `Get-MutationScore`.
- **The `mutation-results` artefact is cut:** `Write-MutationRun.ps1` keeps only status, mutator, replacement and start line, and no id, so it cannot be compared mutant by mutant. The full `mutation-report.json` is in the `stryker-output` artefact, kept 30 days (run 43478448861's expires about 13 December 2026).
- **Pester:** `test\Scripts\*.Tests.ps1` run in the `check` job's x64 leg. `Compress-WingetBuild.Tests.ps1` is the pattern (the script run by `pwsh -NoProfile -File`, fixtures in `$TestDrive`).
- **Stryker's report** (the mutation-testing report schema; `JsonMutant` in Stryker.NET): each mutant has `id`, `mutatorName`, `replacement`, `location` (start and end line and column), `status`, `statusReason` and, in most runs, `coveredBy` and `killedBy` test ids that name tests in the report's `testFiles`.

## Goals / Non-Goals

**Goals:**

- Know whether the 128 mutants differ because of the tests, the environment or Stryker, and say so in `checks.md`.
- Leave a reusable script for the same question after a Stryker or runner change, and for `close-mutation-gaps` to check its own before and after.

**Non-Goals:**

- Closing survivors, choosing the break score, or touching the workflow (proposal.md, Out of scope).

## Decisions

### A diff script, not a spreadsheet

**Why a script:** 3,819 mutants in two 12 MB reports cannot be compared by eye, and the comparison will be needed again (after an SDK, Stryker or runner image change, and when `close-mutation-gaps` checks that a batch moved only the mutants it meant to). It is plain PowerShell beside the other mutation scripts, with Pester tests, as the house rules require for tooling.

**What it takes and answers:**

- `-BaselinePath` and `-ComparisonPath`, two full `mutation-report.json` files, with optional `-BaselineName` and `-ComparisonName` labels (for example `runner` and `local`), and `-Top` (default 10) groups to show.
- It answers an object: the counts of each side by status, the pairs that differ by status and the mutants present on one side only. It writes the same as Markdown to `-OutputPath` when given, for pasting into `checks.md` or an issue.

### The key is source-relative file, mutator, replacement and location

- **The key:** `Get-SourceName` of the file, then `mutatorName`, `replacement` and the start and end line and column. The two machines then agree on a mutant's identity whatever their drive or username.
- **Why not the `id`:** an id is a counter of one run, and nothing in Stryker's documentation promises that two runs number the same mutants alike (a different file order or a mutant dropped as a compile error shifts every id after it). Task 3.1 states how many ids happen to agree on the real reports, as information only.
- **Duplicates:** two mutants with the same key (the same replacement at the same span from two mutators sharing a name) are paired in report order, and a count that does not match is reported as one-sided, never hidden.
- **One-sided mutants** (present in only one report) are their own group: a different count of compile-error mutants between machines would show here.

### Groups lead with the status change

The groups, in order of size:

1. **Status change** (`Survived` in the baseline and `Killed` in the comparison, `Killed` and `Timeout`, `Survived` and `NoCoverage`, and so on);
2. then **file**, then **mutator**.

The first group is expected to be the 128: the single most useful table is "which files and mutators hold the mutants that only the runner kills". When the report carries `killedBy`, each group also lists the most common killing tests (by name) on the side that killed it, because a test that kills on the runner and does not locally is the thing to look at.

### Never print or keep machine paths

The reports hold absolute paths, which on the owner's machine contain the user name. The script prints only source-relative names, the Pester tests assert that no output contains the fixture's drive or user folder, and the fixtures use made-up paths (`C:\Users\me\...` and `D:\a\agent\agent\...`). A real report is never committed.

### Candidate causes, cheapest experiment first

The 128 Killed-on-runner mutants point at tests that behave differently, so the groups are read against these candidates, each with its discriminating experiment (task 3.2 runs them in this order and stops at the first that moves the score):

| Candidate | Why it fits | Experiment |
|---|---|---|
| Environment inherited from a Claude Code session | The owner runs from a terminal that may hold `CLAUDECODE`, `CLAUDE_CODE_*`, `NO_COLOR` or `CLAUDE_PID`; a runner has none, and `CLAUDE.md` already warns these change behaviour | Repeat the local run from a clean `pwsh -NoProfile` with those removed (`Remove-Item Env:`), compare |
| Time zone and culture | The owner is in Dorset, a runner is UTC; the tool runs in invariant globalisation mode but a test may not | Read the top group's killing tests for `TimeZoneInfo`, `DateTimeOffset` offsets and culture; repeat the local run with the zone set to UTC for the process under test (`tzutil` is machine-wide, so the owner chooses) |
| Tools and folders on the machine | `git`, `claude`, `gh`, the profile, `%LOCALAPPDATA%` and the PATH exist on a developer machine and differ on a runner, and `ProgramLocator` and settings tests may read them | Read the group's tests for real folders or `PATH`; run with a stripped `PATH` |
| Drive and path shape | A runner checks out to `D:\a\...`, a developer machine to `C:\Users\...`; `FolderPath` comparison and root checks may differ | Read the group's tests for `C:` literals; run the local clone on a `D:` or `subst` drive |
| Test timeouts and parallelism | A machine with more cores runs more mutants at once; a slow-test timeout leaves a mutant `Survived` in place of `Killed` | Compare `Timeout` counts (equal, 3, so unlikely), then rerun with `--concurrency 2` |
| Coverage analysis | `perTest` coverage maps mutants to tests; a different test set per mutant would change what runs | Compare `coveredBy` for a Killed-on-runner mutant on both sides |

If none moves the score, the runner sample is the odd one: see the open question on repeating it.

### Where a fix lands

- **In a test** (the usual outcome): a normal test change, red first, with the mutants as the red (the scoped run shows the mutant surviving, then killed). No `src\` change, so no Version bump.
- **In the machine or runner:** nothing to fix. `checks.md` states which environment the break score belongs to (the runner's weekly run) and why a local run scores lower, and `CONTRIBUTING.md` says so where it describes a local run.

## Risks / Trade-offs

- **The runner's `stryker-output` artefact expires** (about 13 December 2026): → a fresh full run is made on a runner after `mutation-testing-in-ci` merges, from `workflow_dispatch`, and its head commit is the one the local run uses, so the pair is the same code.
- **A linked git worktree breaks Stryker's paths** (checks.md): → the owner's local run is made in an ordinary clone.
- **Two samples are not a distribution:** one runner run and two local runs cannot say how much a single run varies. → Two runner samples are taken (Samples and the break score).
- **The script is built before it is known to explain anything:** → it is small, and it is the only way to look at 128 mutants; if the cause is found by reading the groups it still stays as the check for later.

## Samples and the break score

- **Two runner samples:** the local score differed by 0.17 points between two runs, and the runner has one sample, so two fresh runner full runs are taken at the same commit (about 16 minutes each, free on a public repository), each paired with the local run. Two matched pairs show whether the gap is systematic (about 4 points on both) or the original sample was an outlier; a third run is added only if the two runner runs differ from each other by more than the local runs did.
- **Which environment owns the break score:** if the gap is the machine, the break score belongs to the runners weekly full run, and a local run is measured against its own recorded baseline instead. Settled by the finding, recorded in `checks.md`.
