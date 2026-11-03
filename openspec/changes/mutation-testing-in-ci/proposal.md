# Proposal

## Why

Mutation testing (Stryker, `.claude\rules\checks.md`) is the only check that shows whether the tests would catch a regression, not just whether they pass. Today it runs only when someone remembers to run it by hand, scoped by hand, so most releases ship with their changes never mutated.

Running it on every pull request would add a long job to the path every merge waits on. Running it after a release, on that release's changes only, catches gaps while the change is fresh and costs nothing: the repository is public, so GitHub's standard runners are free, and the job never sits on a pull request's or a release's path.

## What Changes

- **New workflow `.github\workflows\mutation.yml`:** runs Stryker on `src\Source191.Vantage.Core` through `test\Source191.Vantage.Core.Tests` (`--test-runner mtp`) on a Windows runner, with read-only rights and no secrets.
- **After each release, on that release's changes:**
  - It starts when `pipeline` finishes on `main` (`workflow_run`). A release created with the workflow's token starts no `release` event, so that event can't be used.
  - It carries on only when the commit carries a `v*` tag, which means the release job ran.
  - It mutates only the code changed since the previous release's tag (`--since:<previous tag>`).
- **Coalesced:** one concurrency group that never cancels, so a stack merged in a burst runs at most one job and queues one; GitHub drops the older queued run.
- **Weekly full run:** a schedule (and `workflow_dispatch`) mutates all of Core, for the whole score and anything the per-release runs missed.
- **Report:**
  - Stryker's markdown report goes in the job summary, and the HTML report is an artefact.
  - Below a break score set from the first measured baseline (`--break-at`), the run fails, and GitHub's usual failed-run notice is the alert.
  - It never blocks a merge or a release.
- **Pinned tool:** `dotnet-stryker` pinned in a new tool manifest `.config\dotnet-tools.json`. The settings go in `stryker-config.json` beside the Core tests, so a local run and CI run the same way.
- **Docs:** `.claude\rules\checks.md` (Mutation testing, with the measured times), `CONTRIBUTING.md` and `MAINTAINING.md` where they list the workflows, and `CLAUDE.md`'s one line on mutation testing.
- **No Version bump or changelog line:** nothing under the tool paths in `ToolPaths.ps1` changes.

## Out of scope

- **Gating:** mutation testing on pull requests, or as a required check.
- **Other projects:** mutating `Source191.Vantage.Infrastructure`, `Tui` or the tool project. Their tests drive real git, processes and Terminal.Gui, which is slow per mutant and better judged once Core's times are known.
- **Issue automation:** opening or commenting on an issue for surviving mutants, as `claude-contract.yml` does for CLI drift. The failed-run notice is enough until the noise level is known.
- **Stryker dashboard or baseline storage** (`--with-baseline`).
- **Multiple Claude accounts:** a later change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None: a CI workflow, a tool manifest, a Stryker config and documentation. Nothing users see changes, so `.openspec.yaml` declares `skip_specs: true`.

## Impact

- **New files:** `.github\workflows\mutation.yml`, `.config\dotnet-tools.json` and `test\Source191.Vantage.Core.Tests\stryker-config.json`.
- **Docs:** `.claude\rules\checks.md`, `CONTRIBUTING.md`, `MAINTAINING.md`, `CLAUDE.md` and `Source191.Vantage.slnx` (solution items).
- **Runners:** one Windows runner per coalesced release run and one weekly; no cost on a public repository. It never occupies a pull request's or a release's path.
