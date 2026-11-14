# Design

## Context

See proposal.md for the motivation.

**Today:**

- **Running it:** mutation testing is run by hand from `test\Source191.Cader.Core.Tests`: `dotnet stryker --test-runner mtp --project Source191.Cader.Core.csproj --mutate "**/Domain/*.cs"`, scoped by glob, with `dotnet-stryker` installed globally (`CONTRIBUTING.md`).
- **On record:** there is no config file, no tool manifest and no measured run time; Core has 468 source files.
- **Releases:** `pipeline.yml`'s release job publishes a release on a push to `main` that raises `Version`, and `gh release create --target <sha>` makes the tag `v<version>` on that commit.

**GitHub facts this rests on:**

- **No cost:** standard runners on public repositories are free.
- **No `release` event:** events made with a workflow's `GITHUB_TOKEN` start no new workflow runs (`workflow_dispatch` and `repository_dispatch` aside), so a release published by the release job raises no `release` event another workflow could use.
- **`workflow_run` runs `main`'s own copy:** it always runs the workflow file on the default branch.
- **Concurrency keeps one queued run:** a group without `cancel-in-progress` keeps one run going and one pending, and a newer pending run replaces the older.

**Stryker.NET facts this rests on** (configuration.md, stryker-in-pipeline.md and reporters.md, read 3 November 2026):

- **`--since:<committish>`:** mutates and reports only code changed since that commit; a changed test file counts every mutant its tests cover as changed.
- **`--break-at <n>`:** exits non-zero below that score.
- **Markdown reporter:** writes `mutation-report.md`.

## Goals / Non-Goals

**Goals:**

- Every release's changes are mutated within an hour of the merge, without slowing a pull request, a merge or a release.
- A burst of merges costs at most two runs.
- A local run and a CI run use the same Stryker version and settings.
- Untested code in a release, and a falling whole score, reach the owner as an issue comment without anyone opening a run.

**Non-Goals:**

- Making the score a merge gate, or mutating projects other than Core (proposal.md, Out of scope).

## Decisions

### A separate workflow started by `workflow_run`, not a job in `pipeline.yml`

**Why a separate workflow:**

- A job after `release` in `pipeline.yml` would keep each merge's run open for the whole mutation run.
- `pipeline.yml`'s per-run concurrency group would let every merge of a stack start its own.
- A separate workflow has its own group, `mutation`, without `cancel-in-progress`, so a stack merged in a burst runs one job and queues one.

**The trigger:**

- `workflow_run: workflows: [pipeline], types: [completed], branches: [main]`.
- The job runs only when `github.event.workflow_run.event == 'push'` and its `conclusion == 'success'`. Without the event check, a pull request from a fork branch named `main` would also match.
- It checks out `github.event.workflow_run.head_sha` with full history and tags.
- It holds `contents: read` and `actions: read` (for the run lookup below), and no secrets.

### A release is the commit's `v*` tag

`git tag --points-at <head_sha> --list 'v*'` is non-empty exactly when the release job ran, since it tags that commit. A run with no tag (a docs or CI merge) ends at once with a notice.

**Alternative considered:** reading the run's jobs for "Publish the release". Rejected because it depends on a job's display name.

### `--since` the last commit a mutation run finished on, not the previous tag

**Why not the previous tag:** a queued run replaced by a newer one would lose its release's changes. For example, with releases A, B and C in a burst, A runs, B is replaced by C, and C `--since` B never mutates B's changes.

**What it does instead:** the job asks `gh run list --workflow mutation.yml --branch main` for the newest run that finished with `success` or `failure` (not cancelled or skipped), and uses that run's head commit. It falls back to the previous `v*` tag (`git describe --tags --abbrev=0 --match 'v*' <head_sha>^`) when there is none.

**Why failed runs count too:** a run that failed on score still covered its changes, and repeating them on every later run would only grow the run.

### Weekly full run

Every Sunday at 03:00 UTC (`schedule`), and on `workflow_dispatch`, the same job runs with no `--since`.

- **Why:** it gives the whole Core score for the record, and catches code that `--since` never counted as changed.
- **Its group:** it shares the `mutation` concurrency group, so it never runs beside a per-release run.

### Settings in `stryker-config.json`, the tool in a manifest

**`test\Source191.Cader.Core.Tests\stryker-config.json`** holds:

- **What it mutates:** `project` (`Source191.Cader.Core.csproj`) and `test-runner: mtp` (xunit.v3 scores 0 % under the VSTest bridge).
- **Reporters:** `html`, `markdown`, `json` (for the report job) and `progress`.
- **Thresholds:** `high` and `low`, and `break` once a baseline is measured.

**`.config\dotnet-tools.json`** pins `dotnet-stryker`. `dotnet tool restore` installs it in CI and for contributors, which replaces the global install in `CONTRIBUTING.md`. A hand-scoped local run still adds `--mutate` globs on the command line.

### Windows runner, sharing `pipeline`'s NuGet cache read-only

**Why Windows:** Core targets plain `net10.0`, but its `FolderPath` tests use Windows paths and ordinal case-insensitive comparison, so Linux would fail them.

**Why the cache is read-only:** the job restores the same `nuget-Windows-<hash>` key with `actions/cache/restore` and never saves. Only `pipeline` writes that cache, and Stryker's own package comes from the tool manifest restore.

### Timeouts and report

- **Timeouts:** a per-release run is given 60 minutes, and the weekly run 300 (a hosted job's limit is 360).
- **Report:** `mutation-report.md` is appended to `$GITHUB_STEP_SUMMARY`, and `StrykerOutput` is uploaded as an artefact kept 30 days.
- **Break score:** `--break-at` comes from the config for a full run, so a score below it fails the run and GitHub sends its usual failed-run notice. A release run overrides it with `--break-at 0`: its score covers only the mutants tested, and its survivors go to the issue.

### Findings go to one rolling `mutation` issue, from a job of its own

This follows `claude-contract.yml`'s `report` job.

**The mutation job's side:**

- It stays read-only. It uploads a `mutation-results` artefact: Stryker's `mutation-report.json` and a small `run.json` holding the kind (`release` or `full`), the release tag, the `--since` commit and the whole score.
- Its outputs say whether there is a finding.

**The `report` job:**

- It runs with `needs: mutation` and `if: always()` and a finding, holding only `issues: write` (and `actions: read` for the weekly comparison). It checks nothing out and runs nothing from the repository.
- It makes the label with `gh label create mutation --force`.
- It comments on the open `mutation` issue, or opens one titled "Mutation testing found untested code", assigned to `vars.MUTATION_WATCHER` when that is set.
- The body is cut to 60,000 characters, under GitHub's comment limit, as the contract job does.

**What counts as a finding:**

- **After a release:** any mutant in the changed code with status `Survived` or `NoCoverage`. `NoCoverage` means no test runs that line at all, which is the starker gap.
- **The comment lists:**
  - the release tag;
  - the range (`<since>..<head>`) and the run's link;
  - for each mutant, its file (relative to `src\`), line, mutator and replacement, at most 50, then "and N more in the report".
- **After a weekly full run:** a whole-Core score lower than the previous full run's, read from that run's `mutation-results` artefact (`gh run download`). The comment gives both scores and the five files whose scores fell most.
- **Why the weekly run doesn't list survivors:** a full run of Core always has survivors, and listing them every week would bury the per-release findings.

**Why one rolling issue:**

- The owner is notified of each comment, and closing the issue says "dealt with".
- Rejected: one issue per release, which piles up during a stack; and closing the issue automatically when a later run is clean, which would close it on a release that never touched the code in it.

**The data:** the JSON report follows Stryker's mutation-testing report schema: `files.<path>.mutants[]` with `status`, `mutatorName`, `replacement` and `location.start.line`. Task 2.1 checks these names against a real report before the job is written.

## Risks / Trade-offs

- **The per-release run is too slow to be useful:** → The first tasks measure a full run and a `--since` run locally and on a runner. If a `--since` run of a typical release passes 30 minutes, the per-release trigger is dropped in favour of the weekly run alone, and this design is updated before the workflow lands.
- **A break score set too high fails often and gets ignored:** → It is set from the measured baseline (the open question below), and changed only by a later change with the reason recorded.
- **`workflow_run` runs `main`'s copy of the workflow, even when a pull request started it:** → It runs only for `push` events on `main`, holds no write rights or secrets, and only reads and builds code already merged to `main`.
- **The issue is noisy:** every release that leaves a survivor comments, and equivalent mutants (a change no test could tell apart) will keep turning up. → Each comment is one release's changed code only. Equivalent mutants can be silenced in place with Stryker's `// Stryker disable` comment and a reason, which the comment's list makes easy to find. If it is still too noisy after a few weeks, a later change raises the bar (for example `NoCoverage` only).
- **The previous full run's artefact has expired or is missing:** → The weekly comparison is skipped with a notice, and that run's score becomes the next baseline.
- **The per-release runs miss intermediate commits:** the commit found by `gh run list` is the last that finished, so a merge whose run was replaced is still in the next run's range. → The weekly full run covers anything `--since` misses.

## Open Questions

- **The break score's value:** decided on 14 November 2026: 86 (see Found while building).

## Found while building

- **The owner decided task 2.2 on 14 November 2026 (option A):** keep the per-release trigger and the weekly full run; the break score is 86, from the 91.22 % full run on a runner on 13 November 2026 (less five points, rounded down; 86.78 % on the developer machine); it applies to the weekly full run only, and a release (`--since`) run passes `--break-at 0`, since its score covers only the mutants tested; the weekly comparison keeps its 0.5 point tolerance.
- **The timings favour the per-release trigger:** locally a `--since` run took 3 to 8 minutes and a full run 9 to 11 (`checks.md`), and a full run on a runner 15.5 minutes (91.22 %), so a `--since` run is under 30 minutes in every case measured.
- **The weekly comparison has a tolerance of 0.5 points:** two full runs of the same code scored 86.78 and 86.95 %, so an exact "lower than before" would raise the issue by chance.
- **A `--since` run barely narrows when tests change:** a changed test counts every mutant it covers, and the shared fixtures cover nearly everything, so the saving shows only for a release that changes no tests; a changed non-C# file in the test project, `stryker-config.json` included, tests everything.
- **The report job's rights are one fewer:** it holds only `issues: write`. The mutation job (with `actions: read`) does the previous-run lookup and writes the whole comment into `mutation-results`, so nothing from the repository runs with write rights.
- **The range ignores a `workflow_run` that stopped at the notice:** an untagged commit's run ends as a success without mutating, so only a `workflow_run` on a `v*`-tagged commit, or any schedule or dispatch run, can start a later range.
- **`mutation-results` holds a cut report:** the full JSON is 12 MB of coverage ids, so `Write-MutationRun.ps1` keeps only the status, mutator, replacement and line of the four counted statuses (about 450 KB), and the scripts' fixtures have that shape.
- **A new workflow cannot be dispatched before it is on `main`:** 3.1's and 3.5's dispatch runs, and the deliberate weak-test run, are for the owner after the merge.
- **What ran on a runner:** a temporary `push` trigger on the branch (removed again) ran the full-run path on `windows-latest`: checkout, the range step, the read-only cache restore, `dotnet tool restore`, Stryker, the job summary, the previous-run lookup (none), `run.json` and both artefacts all passed (run 43478448861, 16 min 12 s). The `workflow_run` path, the `--since` run and the `report` job have not run on a runner, and the first `workflow_dispatch` is the owner's after the merge.
