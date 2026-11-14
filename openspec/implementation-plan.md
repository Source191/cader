# Implementation plan for the open changes

Written 2026-11-13 (N to S added 2026-11-14) against `main` at 0.21.0 (`db89b8aa`). Each open change carries the same `## Implementation plan` block at the end of its `tasks.md` (grep for that heading); this file is the whole picture. It changes no code, so it needs no version bump.

## Contents

- Rules for every implementing session
- The pull requests, in order
- Dependencies and what can run in parallel
- Owner gates
- Why these models and efforts
- Decisions made while planning

## Rules for every implementing session

- **Start:** read the change's `## Implementation plan` block, then its proposal, design and tasks. Do only that block's pull request, never the whole change when it is split.
- **Versions:** every pull request that touches `src\` raises the one `Version` line and adds a `CHANGELOG.md` line, and each merge is a release. Parallel pull requests therefore clash on those two lines: the second to merge rebases, re-raises `Version` one step above `main` and re-numbers its `CHANGELOG.md` line.
- **Archive:** a change is archived only in the pull request that completes it, so a split change is archived in its last slice. Every slice leaves `main` building, passing and releasable.
- **Deltas:** a change that modifies a requirement another change already archived refreshes its delta against `openspec\specs` first (the plan block says which), then runs `openspec validate <change> --strict`.
- **Done:** `dotnet build -c Debug`, `dotnet test -c Debug --no-build` at the solution, `dotnet format --verify-no-changes`, and `openspec validate --all`, as `CLAUDE.md` says.
- **Model and effort:** use the ones in the block (`--model` and `--effort` when starting the session). Raise the effort one step, not the model, when a red test cannot be made to fail for the right reason.

## The pull requests, in order

Step numbers are the order of merge; pull requests in the same step are independent and can run in parallel.

| Step | Pull request | Change and tasks | Model | Effort |
|---|---|---|---|---|
| 1 | A: close out winget and README | install-with-winget (4.4, 5.2, 5.3, 7.1, archive), readme-front-page (8.5, 8.6, archive) | sonnet | high |
| 1 | B: quiet editor start | quiet-editor-start (all) | opus | medium |
| 1 | C: stricter state files | stricter-state-files (all) | sonnet | high |
| 1 | D: mutation testing in CI | mutation-testing-in-ci (all) | sonnet | high |
| 2 | E: arm64 build | add-win-arm64-build (all) | sonnet | medium |
| 2 | F: diagnostic log setting | diagnostic-log-setting (all) | sonnet | high |
| 3 | H: fetch before branching, part 1 | fetch-before-branching (1 to 5, and the words that say Cader never fetches) | opus | xhigh |
| 4 | I: fetch before branching, part 2 | fetch-before-branching (6 to 9, archive) | sonnet | high |
| 5 | J: .NET 11 retarget | upgrade-to-net11 (1.1 to 1.10) | opus | high |
| 6 | K: .NET 11 process APIs | upgrade-to-net11 (2.1 to 2.6) | opus | xhigh |
| 6 | L: C# 15 and house style | upgrade-to-net11 (3.1 to 3.9) | opus | high |
| 7 | M: .NET 11 release and docs | upgrade-to-net11 (4.1 to 4.5, archive) | sonnet | medium |
| 8 | N: explain the mutation score gap | explain-mutation-score-gap (all) | sonnet | high |
| 9 | O: close mutation gaps, batches Domain and Ports | close-mutation-gaps (1 to 3) | sonnet | high |
| 10 | P: close mutation gaps, batches Diagnostics and WorktreeInventory | close-mutation-gaps (4, 5) | sonnet | high |
| 11 | Q: close mutation gaps, batch rest of Application | close-mutation-gaps (6) | sonnet | high |
| 12 | R: close mutation gaps, batches Views to Launch | close-mutation-gaps (7, 8) | sonnet | high |
| 13 | S: strict release runs | close-mutation-gaps (9, 10, archive) | opus | medium |

## Dependencies and what can run in parallel

- **Step 1** is four file-disjoint pull requests (openspec and README, `Desktop`, `JsonFileStore` and the Ports records, new workflow files). Run them together. Only the version and changelog lines clash.
- **E (arm64) after A:** the `installation` capability exists in `openspec\specs` only once install-with-winget is archived, so E cannot validate or archive before. E can run beside C, D and F.
- **F (diagnostic log) after A and C:** A archives the winget deltas to `config-tab` ("The Config tab shows the setup") and `tool-updates` ("Cader checks for a newer release at most once a day") that F's delta copies, so F's first task is to refresh both blocks against the archived specs. C changes `UserConfig`, which F extends. F can run beside E.
- **H and I (fetch) after F and C:** F and H both add a property to `Settings`, `UserConfig`, `SettingsFile` and a row to `ConfigRows`, so H takes F's shape and rebases its constructor and `Equals` edits. I follows H because the Config tab and wizard raise what H builds.
- **J to M (.NET 11) last:** its own Order section says it follows quiet-editor-start, stricter-state-files, mutation-testing-in-ci and install-with-winget; this plan adds diagnostic-log-setting and fetch-before-branching, because the C# 15 sweep touches 59 files and the settings and launch code would otherwise be swept twice. It does not wait for arm64, which is workflow-only and may be held up by owner gates: whichever of J and E merges second rebases the `pipeline.yml` output paths. K and L are independent (Infrastructure processes against Domain, Ports and switches) and can run together; M follows both.
- **N (mutation score gap) after D:** the script reuses D's `MutationReport.ps1` and the runner's full run needs D's workflow. It touches only `.github\scripts`, `test\Scripts` and `checks.md`, so it is file-disjoint from J to M and could start earlier. It is numbered after M so the .NET 11 steps (J to M, which retarget and sweep Core) do not move Core's code and tests between the paired runner and local runs; `upgrade-to-net11` records its own before and after scores in one environment, so it is self-consistent, but if N finds that the machine matters, read J's scores in that light.
- **O to S (close mutation gaps) after N, and after M:** N's finding says which environment owns the break score, so the batches' baseline is trustworthy; M finishes the 59-file C# 15 sweep of Core, which would otherwise move the survivors and marks under the batches. O, P, Q and R are sequential because each ratchets the one `break` line in `stryker-config.json` and appends to the same `checks.md` ledger; each batch that adds a Stryker mark under `src\` raises `Version` and adds a `### Internal` line, like any other pull request there. S follows R and changes `Write-MutationRun.ps1` and `mutation.yml`, and is archived with the change.
- **Longest chain:** A, then C, then F, then H, then I, then J, then K or L, then M, then N, then O, P, Q, R and S.

## Owner gates

Blocking (the pull request cannot start or finish without them):

- **E:** 1.1 (the repository can use the hosted `windows-11-arm` runner) and 1.2 (Arm installers exist for the winget dependencies). If 1.1 fails, stop and re-plan.
- **H:** 3.1 (the `git fetch` credential spike on a developer machine), before any port change.
- **D:** 2.2 (the owner agrees the timings and break score from the measured baseline).
- **N:** 2.1 and 2.2 (the owner dispatches the runner full run, or gives its id, and makes a full run in an ordinary clone, not a linked worktree, at the same commit, then puts its `mutation-report.json` in `$env:TEMP\mutation-gap\local`; never committed, because it holds absolute paths and the user name; only the made-up fixtures are kept). Two runner samples are taken. The existing runner run's `stryker-output` expires about 13 December 2026.
- **O:** the reference run's environment (from N's finding). **S:** 9.1 (no unmarked survivor left).

After merge (the change is archived without them, but they are not forgotten):

- **A:** install-with-winget 8.1 to 8.3 (clean VM install, upgrade with `U`, retire the nuget.org package) and readme-front-page's sponsor button check.
- **E:** 5.1. **B:** 3.2. **F:** 5.4. **I:** 9.2.
- **S:** the first release run that counts 20 or more mutants shows the strict rule working, checked once with a deliberately weak test on a branch, and the resulting `mutation` issue closed.

## Why these models and efforts

- **Opus, xhigh:** H (git behaviour that can hang on a credential prompt or lose local work, written test first against real git) and K (process semantics that the `ConfirmedProcess` rules depend on, where a wrong adoption is a security regression).
- **Opus, high:** J (judging every package and breaking change), L (a 59-file sweep with a union spike that has a decline path).
- **Opus, medium:** B (small, but the PowerShell quoting rules in `security.md` are exact), and S (a small script and workflow-step change, but its thresholds and the workflow's rights are exact).
- **Sonnet, high:** C, D, F, I, A, N, O, P, Q and R. Each follows an established house pattern (a setting and its row, a handler, a workflow beside `claude-contract.yml`, a spec archive) where care matters more than design.
- **Sonnet, medium:** E and M. Mostly mechanical once the premises are confirmed or the measurements taken.
- **Not used:** haiku (the house rules and reflection facts punish small mistakes) and `max` effort (nothing here is an open-ended design question).

## Decisions made while planning

- **Fetch is split in two** so that the owner's spike and the git work are reviewed apart from the screens; part 1 changes the words that say Cader never fetches, so no release says something untrue.
- **.NET 11 is split in four** on its own task sections; each slice builds and passes on `net11.0` or leaves `net11.0` untouched, so `main` is always releasable. The minor `Version` bump and the `CHANGELOG.md` summary are in M.
- **The mutation follow-ups are two changes, in order:** `explain-mutation-score-gap` first, because a break score or a ratchet cannot be trusted while the runner scores 4.4 points above the developer machine on the same code; then `close-mutation-gaps`, in four batch pull requests and a last one that makes release runs strict only once no unmarked survivor is left.
- **This plan is not an OpenSpec change** because it changes no behaviour, tooling or code, only schedules existing changes.
