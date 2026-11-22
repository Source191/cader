---
paths:
  - ".github/workflows/mutation.yml"
  - ".github/scripts/*Mutation*"
  - ".github/scripts/fixtures/mutation/**"
  - "test/Scripts/*Mutation*"
  - "test/Source191.Cader.Core.Tests/stryker-config.json"
  - ".config/dotnet-tools.json"
---

# Mutation testing

Stryker on Core: the tool, the workflow and the rules for reading a score. The measurements, run ids and the close-mutation-gaps ledger are in `docs\verified-facts\mutation-testing.md`.

## The tool

- Run `dotnet stryker` from `test\Source191.Cader.Core.Tests`; the tool is pinned in `.config\dotnet-tools.json` (`dotnet tool restore`) and set in `stryker-config.json`: `--test-runner mtp`, the html, markdown, json and progress reporters, and the thresholds.
- `--test-runner mtp` is required: xunit.v3 scores 0 % under the VSTest bridge.
- `dotnet stryker --version` is a dashboard option, not the tool's version: read the version with `dotnet tool list`.
- Scope a hand run with `--mutate "**/Domain/*.cs"` style globs, never solution-relative paths. The config takes no `skip-version-check`: pass `--skip-version-check` on the command line. Reports land in `StrykerOutput\<timestamp>\reports`.
- A run's fixed part is about 2.5 minutes (build, then per-test coverage), and `--since` is checked only after it, so a mistyped committish costs that too (13 November 2026, Stryker 5.0.0).
- `--since` takes a sha or a ref name, not `<sha>~1`. In a linked git worktree it resolves the main checkout's paths and tests nothing, so measure in an ordinary clone (CI is one). A changed non-C# file in the test project, `stryker-config.json` included, makes it test every mutant.
- A changed test counts every mutant it covers as changed, and the shared fixtures and architecture tests cover nearly everything, so a release that touches tests costs 6 to 8 minutes and one that changes no Core code about 3 (13 November 2026).

## Roslyn and C# 15

- Stryker compiles the mutated Core with its bundled Roslyn: 5.0.0 and 5.1.0 carry Roslyn 5.9.0 (checked 14 November 2026), which reads `latest` as C# 14, so C# 15 syntax in Core stops the run (CS8652) and scores 0 %. Core is therefore held at `LangVersion` 14 (its README).
- When a Stryker release bundles a Roslyn that knows C# 15: raise the pin, prove it with a run scoped to a Core file using C# 15 syntax, raise Core to 15, then do the sweep deferred from upgrade-to-net11.
- That sweep: mark Domain's and Ports' hierarchies `closed` (`System.Runtime.CompilerServices.IsClosedTypeAttribute` for `ClosedHierarchiesShould`), widen `SourceConventionsShould.DeclareEveryHierarchyOutsideCoreClosed`, drop the throwing discard arms over them, and check whether `ChildCommandWords`, `ChildCommand` and `PullRequestCatalogue` can take `with(...)`.
- The tool runs on .NET 10 and drives the `net11.0` test project under `mtp` (14 November 2026).

## Reading a score

- **The runner's score is the one that counts:** a local full run scores 1.6 to 3.5 points below the `windows-latest` runner and moves about 2 points between runs, so it is never compared with the break score (explained 14 November 2026, Stryker 5.0.0).
- **The gap is static mutants:** every covered mutant behaves alike on both machines; the gap is entirely mutants with no per-test coverage (empty `coveredBy`), which Stryker's MTP runner runs unreliably on the developer machine. Not the tests, the Claude Code variables or parallelism.
- **So, locally:** trust only a scoped run's covered mutants, and believe a static survivor only when the runner also reports it.
- **Static initialisers:** a static value is built once per test process and the MTP runner switches mutants without restarting it (stryker-mutator/stryker-net#3742), so no fact can be credited with killing a mutant in a static initialiser.
- **Covered by every test:** a mutant that also runs outside a test (a test class's static field, `TheoryData`) is counted as covered by every test, and neither machine's result for it proves anything; check one by hand (blank the value, run the Core suite) before believing or dismissing it.
- **A field initialiser fact:** a fact that reads a test class's static field never kills a mutant in code that ran in the field's initialiser, so call the code inside the fact.

## Marks

- **Before a mark:** try a rewrite or a test first. A mark usually points at code that can be simpler, or at a value a fact can read (a guard's `ParamName`); a mark is for a true equivalent only, and its reason says why no rewrite or fact reaches it.
- **The form:** `// Stryker disable once <Mutator> : Equivalent: <reason>` above a statement ignores that mutator's mutants anywhere on the statement's lines; the analysers, `SourceConventionsShould`, `SentenceLiteralsShould` and `dotnet format` accept it.
- **One mark per statement:** Stryker reads only the `disable once` comment nearest a statement, so two mutators share one mark, comma-separated (15 November 2026). A mark above the wrong line ignores far more than its statement, so check the `Ignored` list of a scoped run, not only the survivors.
- **Counting marks:** count Ignored mutants in a scoped run over every marked file, because the runner's cut report leaves them out. Find them with `git grep "Stryker disable"`.
- **Accepted:** the 61 static-initialiser mutants of `ChildCommand`, `ChildCommandWords`, `DiagnosticLine.CrashMarker` and `ExceptionSummary.AwaitMachinery`, the owner's choice of 15 November 2026, plus `ExceptionSummary`'s line-0 frame. Each reason names the fact that pins the value.
- **When #3742 is fixed** (watch it and stryker-mutator/stryker-net#3695): remove the static-initialiser marks, take out skip-static-mutants-in-release-runs' release rule, run the four files scoped then a runner full run, and drop Accepted back to 1.

## The workflow

- **When it runs:** `mutation.yml` runs Core's mutation tests on a Windows runner, never on a pull request or the path of a merge or release: after `pipeline` finishes on `main` for a push whose commit carries a `v*` tag (`workflow_run`, so a release made with the workflow's token is still seen; otherwise a notice and stop), every Sunday at 03:00 UTC, and by `workflow_dispatch`.
- **The job:** one `mutation` concurrency group, no cancel-in-progress. It holds `contents: read` and `actions: read` and no secret, restores `pipeline`'s NuGet cache read-only, appends `mutation-report.md` to the job summary and uploads `stryker-output` and `mutation-results` (`run.json` and the cut report) for 30 days.
- **The range** (`Resolve-MutationRange.ps1`): after a release, `--since` the head of the newest finished mutation run on `main` that is an ancestor of the release (a `workflow_run` counts only if its commit carries a `v*` tag; a schedule or dispatch run always counts), else the previous `v*` tag, else all of Core. A schedule or dispatch run has no range.
- **A finding** (`Write-MutationRun.ps1`): after a release, any `Survived` or `NoCoverage` mutant in the changed code that Stryker did not mark `static` (left to the weekly run, counted as `staticSetAside`); after a full run, a score more than 0.5 points below the previous full run's, read from its `mutation-results` (skipped with a notice when expired).
- **The report job:** `report` holds only `issues: write`, runs nothing from the repository, and posts the comment (`Format-MutationComment.ps1`, at most 50 mutants, cut to 60,000 characters) on the open issue labelled `mutation`, or opens one assigned to `vars.MUTATION_WATCHER`. The owner closes it.
- **The strict release rule:** a release run that counted at least 20 mutants (`-MinimumCounted`) and left any non-static Survived or NoCoverage is `Failing`: `run.json` records `counted` and `failing`, the comment opens with "This run failed", and the last step, after both uploads, exits 1. A smaller run only comments; a full run never fails this way. `Mutation.Tests.ps1` holds the edges (20 fails, 19 does not).
- **Before publishing:** `publish.yml`'s `mutation-notice` job (no environment, `contents: read`, `issues: read`) lists open `mutation` issues as a `::warning::` and a summary section (`Get-MutationWarning.ps1`); it never blocks.

## The break score

- `stryker-config.json` sets `high`, `low` and `break` to 95, the owner's choice of 15 November 2026 after the runner's 100 % run; it began at 86 (14 November 2026). Stryker refuses `low` below `break` and accepts `low` equal to `high`.
- It applies to the weekly full run only: a release run passes `--break-at 0` (its score covers only the mutants tested) and fails only by the strict release rule. A failed full run is the alert, through GitHub's failed-run notice; nothing blocks a merge or a release.
- A score below `break` ends the run with exit 2, and `--break-at 0` on the command line overrides the config (checked 14 November 2026).
- The scripts' fixtures are in `.github\scripts\fixtures\mutation`; `test\Scripts\Mutation.Tests.ps1` (Pester, run by `pipeline`) covers the scripts on them and reads `mutation.yml` for `--break-at 0` and the failing step.
