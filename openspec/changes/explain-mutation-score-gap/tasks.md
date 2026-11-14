# Tasks

Start only once `mutation-testing-in-ci` (PR #255) has merged: the script reuses its `MutationReport.ps1`, and the runner's full run comes from its workflow. Each task is one red-green-refactor cycle or smaller, with a commit per step. Run the Pester tests with `Invoke-Pester test\Scripts\Compare-MutationReports.Tests.ps1`.

## 1. The comparison script

- [ ] 1.1 **Red:** `Compare-MutationReports.Tests.ps1`, "pairs the same mutant on two machines": two made-up reports under `.github\scripts\fixtures\mutation-compare` (`runner.json` with files under `D:\a\agent\agent\src\...`, `local.json` under `C:\Users\me\work\cader\src\...`, a handful of mutants whose ids differ between the two), expecting every mutant paired by source-relative file, mutator, replacement and location, and the counts of each status per side. **Green:** `Compare-MutationReports.ps1` reading both with `Read-MutationReport` and keying with `Get-SourceName`. **Verify:** the fact passes.
- [ ] 1.2 **Red:** "groups the mutants whose status differs by status change, then file, then mutator, largest first", with the fixtures holding a Survived-to-Killed group in two files, a Killed-to-Timeout mutant and a Survived-to-NoCoverage mutant. **Green:** the grouping and `-Top`. **Verify:** the fact passes and the order is by group size.
- [ ] 1.3 **Red:** "lists a mutant present in only one report as its own group, and pairs duplicate keys in report order" (one fixture mutant missing on the local side, two mutants sharing a key). **Green:** one-sided and duplicate handling. **Verify:** the fact passes and a count mismatch is reported, never dropped.
- [ ] 1.4 **Red:** "names the killing tests of a group on the side that killed it" (fixture `killedBy` ids resolved through `testFiles`), and "prints no absolute path": the output holds neither `D:\a` nor `C:\Users\me`. **Green:** the killing-test listing and the source-relative names only; `-OutputPath` writes the same as Markdown. **Refactor:** share any helper with `MutationReport.ps1` rather than copy it. **Verify:** both facts pass, and `Invoke-Pester test\Scripts` passes as a whole.

## 2. Two full runs of the same commit (owner gate)

- [ ] 2.1 **Owner:** on `main`, after `mutation-testing-in-ci` and 1.1 to 1.4 have merged, run `gh workflow run mutation.yml --ref main`, then `gh run list --workflow mutation.yml --limit 1 --json databaseId,headSha`; note the `headSha` and run id. Then `gh run download <run id> --name stryker-output --dir $env:TEMP\mutation-gap\runner` (the full report is `...\reports\mutation-report.json`; the `mutation-results` artefact is cut and will not do). Dispatch twice, for two runner samples (design.md, Samples and the break score), and pair each with the local run. **Verify:** the run succeeded and the report is on disk.
- [ ] 2.2 **Owner:** in an **ordinary clone, not a linked worktree** (Stryker resolves the main checkout's paths in a worktree and tests nothing): `git clone https://github.com/Source191/cader.git $env:TEMP\mutation-gap\clone; git -C $env:TEMP\mutation-gap\clone checkout <headSha>`, then from `test\Source191.Cader.Core.Tests` there run `dotnet tool restore` and `dotnet stryker --skip-version-check`, from a plain `pwsh -NoProfile` (not a Claude Code terminal). Copy `StrykerOutput\<timestamp>\reports\mutation-report.json` to `$env:TEMP\mutation-gap\local\mutation-report.json` and tell the session the two folders. The reports hold absolute paths and the user name, so they are never committed, pasted or attached to the pull request; `StrykerOutput\` is git-ignored in the clone. **Verify:** the local report is for the same sha as the runner's.

## 3. The finding

- [ ] 3.1 Run `.github\scripts\Compare-MutationReports.ps1` on the pair. **Verify:** the output confirms or corrects the arithmetic in design.md (the Survived-to-Killed group is about 128 mutants, with timeouts and no-coverage equal); record the counts per status, the top groups, how many mutant ids happen to agree, and any one-sided mutants. If the matched pair already scores within 0.5 points, stop investigating the machine: the original sample was an outlier, and the second runner sample shows how much one runner run varies.
- [ ] 3.2 Read the top groups' killing tests and run the experiments in design.md's table in order, one at a time, repeating the local run and the comparison for each (a clean environment first, then time zone, tools and folders, drive, concurrency, coverage), stopping at the first that moves the local score towards the runner's. **Verify:** each experiment's result (the score and the size of the Survived-to-Killed group before and after) is noted.
- [ ] 3.3 Write the finding into `.claude\rules\checks.md`, Mutation testing, with the date and Stryker version: the matched pair's scores, the cause (or "not found" with what was ruled out), and which environment the break score belongs to. **Verify:** read it against 3.1's counts.

## 4. The fix, if the tests hold the cause

- [ ] 4.1 If 3.2 found the cause in a test, **Red:** a fact (or the existing fact made to fail) showing the machine dependence, for example a test that passes only in the owner's time zone; **Green:** make it independent of the machine (a fixed `TimeProvider` and zone, a temporary folder, no real `PATH`); **Verify:** the fact passes under both settings, and a scoped `dotnet stryker --mutate "**/<File>.cs"` locally now kills the group's mutants. If the cause is the machine and not the tests, change nothing in code: state in `CONTRIBUTING.md`, beside the local mutation command, that a local score is lower than the runner's for that reason. **Verify:** by reading.
- [ ] 4.2 Run `openspec validate explain-mutation-score-gap --strict` and `Invoke-Pester test\Scripts`; if 4.1 changed a test, `dotnet build -c Debug`, `dotnet test -c Debug --no-build` and `dotnet format --verify-no-changes`. **Verify:** each exits 0 and `pipeline`'s `check` is green.

## Workflow follow-up

- **Archive:** archive the change in the same pull request that completes it. Nothing under `src\` changes, so there is no `Version` bump or `CHANGELOG.md` line.
- **Tell `close-mutation-gaps`:** its baseline (the reference full run and its break score) is the one this finding names.

## Implementation plan

See `openspec\implementation-plan.md` for the whole sequence.

- **PR:** N
- **Step:** 8
- **Depends on:** D (PR #255, `mutation-testing-in-ci`, merged: the script reuses its `MutationReport.ps1` and the runner's full run needs its workflow)
- **Parallel with:** nothing planned; it is numbered last only so the .NET 11 steps (J to M) do not move Core's code and tests between the paired runs. It touches only `.github\scripts`, `test\Scripts` and `checks.md`, so it can start any time after D if the owner accepts one more full run after J to M.
- **Model:** sonnet
- **Effort:** high
- **Why:** a small tested PowerShell script beside the existing mutation scripts, then reading two reports and running experiments where care matters more than design; a test fix, if any, follows the house test rules.
- **Owner gates:** blocking, 2.1 and 2.2 (the owner dispatches the runner run, or gives the run id, and makes the local full run in an ordinary clone at the same sha, putting its `mutation-report.json` in `$env:TEMP\mutation-gap\local`; never committed). Two runner samples are taken. Time limit: run 43478448861's `stryker-output` expires about 13 December 2026, so a fresh dispatch run is used.
