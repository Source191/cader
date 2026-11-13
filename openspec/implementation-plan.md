# Implementation plan for the open changes

Written 2026-11-13 against `main` at 0.21.0 (`db89b8aa`). Each open change carries the same `## Implementation plan` block at the end of its `tasks.md` (grep for that heading); this file is the whole picture. It changes no code, so it needs no version bump.

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
| 3 | G: sign the executable | sign-the-executable (all) | opus | high |
| 3 | H: fetch before branching, part 1 | fetch-before-branching (1 to 5, and the words that say Cader never fetches) | opus | xhigh |
| 4 | I: fetch before branching, part 2 | fetch-before-branching (6 to 9, archive) | sonnet | high |
| 5 | J: .NET 11 retarget | upgrade-to-net11 (1.1 to 1.10) | opus | high |
| 6 | K: .NET 11 process APIs | upgrade-to-net11 (2.1 to 2.6) | opus | xhigh |
| 6 | L: C# 15 and house style | upgrade-to-net11 (3.1 to 3.9) | opus | high |
| 7 | M: .NET 11 release and docs | upgrade-to-net11 (4.1 to 4.5, archive) | sonnet | medium |

## Dependencies and what can run in parallel

- **Step 1** is four file-disjoint pull requests (openspec and README, `Desktop`, `JsonFileStore` and the Ports records, new workflow files). Run them together. Only the version and changelog lines clash.
- **E (arm64) after A:** the `installation` capability exists in `openspec\specs` only once install-with-winget is archived, so E cannot validate or archive before. E can run beside C, D and F.
- **F (diagnostic log) after A and C:** A archives the winget deltas to `config-tab` ("The Config tab shows the setup") and `tool-updates` ("Cader checks for a newer release at most once a day") that F's delta copies, so F's first task is to refresh both blocks against the archived specs. C changes `UserConfig`, which F extends. F can run beside E.
- **G (signing) after E:** both edit `pipeline.yml` and `publish.yml`, and signing must cover both zips. G's tasks are amended to say so (see its block). G can run beside H.
- **H and I (fetch) after F and C:** F and H both add a property to `Settings`, `UserConfig`, `SettingsFile` and a row to `ConfigRows`, so H takes F's shape and rebases its constructor and `Equals` edits. I follows H because the Config tab and wizard raise what H builds.
- **J to M (.NET 11) last:** its own Order section says it follows quiet-editor-start, stricter-state-files, mutation-testing-in-ci and install-with-winget; this plan adds diagnostic-log-setting and fetch-before-branching, because the C# 15 sweep touches 59 files and the settings and launch code would otherwise be swept twice. It does not wait for arm64 or signing, which are workflow-only and may be held up by owner gates: whichever of J, E and G merges second rebases the `pipeline.yml` output paths. K and L are independent (Infrastructure processes against Domain, Ports and switches) and can run together; M follows both.
- **Longest chain:** A, then C, then F, then H, then I, then J, then K or L, then M.

## Owner gates

Blocking (the pull request cannot start or finish without them):

- **E:** 1.1 (the repository can use the hosted `windows-11-arm` runner) and 1.2 (Arm installers exist for the winget dependencies). If 1.1 fails, stop and re-plan.
- **G:** 1.1 and 1.2 (signing service, identity, cost and account, the Entra federated credential, the `signing` environment limited to `main`).
- **H:** 3.1 (the `git fetch` credential spike on a developer machine), before any port change.
- **D:** 2.2 (the owner agrees the timings and break score from the measured baseline).

After merge (the change is archived without them, but they are not forgotten):

- **A:** install-with-winget 8.1 to 8.3 (clean VM install, upgrade with `U`, retire the nuget.org package) and readme-front-page's sponsor button check.
- **E:** 5.1. **G:** 5.1, 5.2. **B:** 3.2. **F:** 5.4. **I:** 9.2.

## Why these models and efforts

- **Opus, xhigh:** H (git behaviour that can hang on a credential prompt or lose local work, written test first against real git) and K (process semantics that the `ConfirmedProcess` rules depend on, where a wrong adoption is a security regression).
- **Opus, high:** G (a signing pipeline that must never leak a credential or sign from a pull request), J (judging every package and breaking change), L (a 59-file sweep with a union spike that has a decline path).
- **Opus, medium:** B (small, but the PowerShell quoting rules in `security.md` are exact).
- **Sonnet, high:** C, D, F, I, A. Each follows an established house pattern (a setting and its row, a handler, a workflow beside `claude-contract.yml`, a spec archive) where care matters more than design.
- **Sonnet, medium:** E and M. Mostly mechanical once the premises are confirmed or the measurements taken.
- **Not used:** haiku (the house rules and reflection facts punish small mistakes) and `max` effort (nothing here is an open-ended design question).

## Decisions made while planning

- **Fetch is split in two** so that the owner's spike and the git work are reviewed apart from the screens; part 1 changes the words that say Cader never fetches, so no release says something untrue.
- **.NET 11 is split in four** on its own task sections; each slice builds and passes on `net11.0` or leaves `net11.0` untouched, so `main` is always releasable. The minor `Version` bump and the `CHANGELOG.md` summary are in M.
- **Signing and arm64:** arm64 first, then signing amended to sign and verify both zips.
- **This plan is not an OpenSpec change** because it changes no behaviour, tooling or code, only schedules existing changes.
