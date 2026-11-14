# Design

## Context

See proposal.md for the motivation.

**What is on record** (`.claude\rules\checks.md`, 13 November 2026, Stryker 5.0.0): Core has 3,819 mutants, 2,858 tested. A full run on the developer machine scored 86.78 % and 86.95 % (357 survived, 24 without coverage); on a `windows-latest` runner 91.22 % (229 survived, 24 without coverage). `explain-mutation-score-gap` finds why the two differ.

**Where the runner's survivors are** (from its `mutation-results` artefact, run 43478448861: 253 survived or without coverage, in 78 of 466 files), by folder, which sizes the batches below. The reference run taken once the gap is explained replaces these counts, but the areas stay:

| Area | Survived or no coverage | Files | Largest files |
|---|---|---|---|
| `Domain` | 46 | 21 | `WhereLabel` 7, `EditorChoice` 4, `ReleaseNotes` 4, `NumberAwareOrder` 4, `Settings` 4 |
| `Ports` | 18 | 4 | `UserConfig` 8, `ProcessTimedOutException` 6, `EditorSetting` 3 |
| `Application\Diagnostics` | 34 | 6 | `ChildCommandWords` 24, `ChildCommand` 4, `ExceptionSummary` 3 |
| `Application` (top level) | 86 | 22 | `WorktreeInventory` 30, `CommandLine` 11, the `Wording*` files 28 between them |
| `Application\Views` | 26 | 11 | `StatusLayout` 5, `WizardState` 4, `WizardView` 4, `ListSelection` 4 |
| `Application\Detail` | 14 | 3 | `DetailLayout` 10, `DetailLoader` 3 |
| `Application\Reporting` | 11 | 2 | `ProblemReports` 10 |
| `Application\Actions` | 13 | 6 | `OpenProblemReportHandler` 4, `TabClaimer` 3 |
| `Application\Launch` and `TerminalSetup` | 5 | 3 | 2 or 3 each |

**Facts about the existing mutation workflow** (`mutation-testing-in-ci`, `.github\workflows\mutation.yml` and `.github\scripts`):

- **`Write-MutationRun.ps1`:** reads a report, writes `run.json` and a cut `mutation-report.json`, and answers `Score`, `Finding` and `Reason`. After a release, a finding is any `Survived` or `NoCoverage` mutant; after a full run, a score more than 0.5 below the previous full run's. Its Pester tests are in `test\Scripts\Mutation.Tests.ps1` (17 facts over `Resolve-MutationRange`, `Write-MutationRun`, `Format-MutationComment` and the workflow's `--break-at 0`), run by `pwsh -File` over fixtures cut from a real report.
- **The report job** posts a comment on the `mutation` issue when the job's `finding` output is true. The mutation job itself ends with a pass or fail only through Stryker's `--break-at`.
- **`stryker-config.json`** holds the break score (86), which applies to the weekly full run only: the Mutate step passes `--break-at 0` to a release run, and a `Mutation.Tests.ps1` fact holds that.
- **`--since` is nearly whole-Core when a test changes:** a changed test counts every mutant it covers as changed, and the shared fixtures and architecture tests cover almost everything, so a release that touches a test file tests 2,760 to 2,784 of the 2,858 mutants (checks.md).

**Stryker facts this rests on** (Stryker.NET's `ignore-mutations.md`, read 14 November 2026):

- **The comment:** `// Stryker disable once <Mutators>: <reason>` ignores the next line's mutants for the named mutators (or `all`), with the reason recorded; the mutator names are the ones in parentheses on Stryker's mutations page (`Boolean`, `Arithmetic`, `Update` and so on).
- **In the report:** an ignored mutant has status `Ignored` and its reason in `statusReason`; the score counts only Killed and Timeout over those and Survived and NoCoverage, so an ignored mutant counts for nothing.

## Goals / Non-Goals

**Goals:**

- Every survivor of the reference full run is either killed by a test or marked with a stated reason.
- The score rises batch by batch and the break score follows it.
- After that, a release that leaves a new unmarked survivor in a sizeable run fails visibly.

**Non-Goals:**

- Changing production code to avoid a mutant, mutating other projects, or gating a merge (proposal.md, Out of scope).

## Decisions

### Three kinds of survivor, and what each costs

| Kind | Meaning | What is done | Example |
|---|---|---|---|
| Real test gap | A sensible test tells the mutant from the original | A new fact kills it | A comparison operator on a boundary nobody tests |
| Equivalent | No test can tell the two apart | A mark in the source, reason starting `Equivalent:` | The discard arm that throws `UnreachableException`; a mutation of a value that is overwritten before it is read |
| Accepted | A test could, but it would be brittle or would test the wrong thing | A mark in the source, reason starting `Accepted:` | A short-circuit that only saves time; a default that every caller overrides |

- **Order of judgement:** try to kill it first. Equivalent is claimed only after trying and reading the code; Accepted only when the reason names what a test would have to pin and why that is not worth it.
- **Accepted is kept to a minimum, and is never the default for testing or logging code:** testing and logging matter here. A surviving mutant in test support code (fakes, fixtures, helpers) or in logging or diagnostic code (diagnostic events and their words, exception summaries, child-command labels, problem reports) is closed with a test, not Accepted; the diagnostic events are already tested and a mutant in them is a gap in those tests. An Accepted mark is for a mutant that cannot be tested sensibly in any other way, and it carries a one-line reason a reviewer can challenge (what a test would have to pin, and why that is not worth pinning). A reviewer who cannot accept the reason asks for the test.
- **One convention:** `// Stryker disable once <Mutator> : Equivalent: <reason>` or `... : Accepted: <reason>`, on the line above the code, so `git grep "Stryker disable"` finds every mark and `git grep "Accepted:"` lists the ones a reviewer can challenge. The mutator name is checked, not assumed: after marking, the scoped run shows the mutant `Ignored` with the reason, and still `Survived` if the name was wrong.
- **Why both kinds are marked:** the strict release rule needs a clean reference run, so anything left over must be visible in the source. Counting Accepted marks in `checks.md` stops the second kind becoming a way to silence a run.
- **Comments in Core are allowed here:** a mark is a comment, and the house conventions (`SourceConventionsShould`) read syntax trees for other rules; the first batch runs the whole suite after its first mark to confirm none objects.

### Batches by area, each recorded

The batches, in order (sizes from the runner's run above):

1. **`Domain`** (46) and 2. **`Ports`** (18): small pure types where a fact is cheap.
3. **`Application\Diagnostics`** (34): `ChildCommandWords` is a table of words, 24 survivors, so the triage here is mostly "is each word pinned, or is the table's content the only behaviour" (the privacy rules in `security.md` make these tests worth having, so fixtures use made-up commands).
4. **`WorktreeInventory`** (30): the largest single file; the existing `WorktreeInventoryLeftoversShould` shows the fixtures.
5. **The rest of `Application`** (56): `CommandLine` (11), the `Wording*` files (28), `MemoryBreakdown`, `WorktreeInspector`, `KeyBar`.
6. **`Views`, `Detail` and `Reporting`** (51).
7. **`Actions`, `Launch` and `TerminalSetup`** (18).

**Each batch:**

- **The cycle for a file:** red is a scoped run (`dotnet stryker --mutate "**/<Folder>/<File>.cs"` from `test\Source191.Cader.Core.Tests`, about 2.5 minutes fixed cost plus the file's mutants) showing the named mutants `Survived`; green is a sociable fact that kills them (real collaborators, fakes only at the boundaries) and a repeated scoped run showing them `Killed`; a mark is a commit of its own.
- **After the batch:** a full run, the score and survivor count noted in `checks.md`'s ledger line, and the break score ratcheted.
- **Where the score is taken:** the environment `explain-mutation-score-gap` names as the break score's owner (a runner full run from `workflow_dispatch`, if the machine matters).

### The break score ratchets by four points, and never falls

- **The rule:** after a batch, `break` becomes the whole-Core score less four points, rounded down, and never lower than it was (four points under the recorded score). Four leaves room for the runner's run-to-run difference (0.2 points on the developer machine, an unmeasured variance on a runner) and for one or two new survivors in a release.
- **Only the weekly run uses it:** the release runs already use `--break-at 0` (`mutation-testing-in-ci`), so the ratchet changes one number in `stryker-config.json` and no workflow line.

### Strict release runs, switched on last

**The rule:** a `--since` run fails the job when it counted at least 20 mutants (Killed, Timeout, Survived and NoCoverage together, which is what the score is over) and any of them is Survived or NoCoverage. A marked mutant is `Ignored`, so it is neither counted nor failing.

**Why a minimum of 20:** a run of a few mutants scores 0 or 100 on one change, which would fail a release over one small survivor in a tiny change. Below 20 the run still comments as today (`Finding`), but does not fail. It is a parameter of the script (`-MinimumCounted`, default 20), so a later change can revise it with the reason recorded.

**Why last:** with 229 survivors, any release that touches a test file mutates nearly everything and fails on survivors that were there already. Strictness is enabled only once the reference full run (on the runner) shows no unmarked Survived or NoCoverage mutant, task 9.1's check, so a failure then means a new survivor.

**How:**

- **The script:** `Write-MutationRun.ps1` gains `-MinimumCounted` (default 20) and answers `Counted` and `Failing` (a release run with `Counted -ge MinimumCounted` and any undetected mutant); `run.json` records both.
- **The workflow:** `mutation.yml`'s "Record the run" step writes `fail` to the step output, and a last step, after the artefacts are uploaded, runs `exit 1` with the count when it is true (so the artefacts and the `report` job's comment are still produced: `report` already runs on `always()`). The comment says the run failed for that reason.
- **No effect on merges or releases:** the run starts after the release and holds only read rights; a failure is GitHub's usual failed-run notice, with the issue comment naming the mutants.
- **A full run is unchanged:** it still compares with the previous score and `break`.

**Tests:** new facts in `test\Scripts\Mutation.Tests.ps1`, beside the existing `Write-MutationRun` ones (which already pin today's behaviour: a release with survivors is a finding, a clean one is not, a full run compares scores), over reports the test builds with a few made-up mutants each: 20 counted with a survivor fails, 19 counted with a survivor does not, 20 counted with no survivor passes, an `Ignored` mutant counts for nothing; and a fact that the workflow's last step fails the job on `fail=true`.

### Marks make a release

A mark is a change under `src\`, which `ToolPaths.ps1` counts as the packaged tool, so `Assert-VersionBumped.ps1` and `Assert-ChangelogUpdated.ps1` fail a pull request that carries one without a `Version` patch bump and a `CHANGELOG.md` line under `### Internal`; each merge then is a release (and starts a `--since` mutation run on that very change, which should come back clean). Pull requests of new tests only touch `test\`, which needs neither. Marks ship in their batch's pull request, with its `Version` bump.

## Risks / Trade-offs

- **A mutant that survives sometimes (a slow test, a loaded machine) fails a release by chance:** → the failure blocks nothing and the comment lists the mutants; it is rerun, fixed or, if equivalent, marked. If it recurs, the cause belongs in `explain-mutation-score-gap`'s findings.
- **Accepted marks pile up and hide gaps:** → each has a one-line reason a reviewer can challenge, testing and logging code is never Accepted by default, and the count is in `checks.md` after each batch.
- **A mark with the wrong mutator name silences nothing:** → the scoped rerun must show the mutant `Ignored`.
- **A batch of marks is a release that changes nothing a user sees:** → a patch bump and an Internal line, the cost of the house rule that anything under `src\` is a release.
- **A task per file is large in number:** each batch lists its biggest files by name and treats the tail (a few mutants each, about 50 files) as one checklist, one commit per file, because the tail is known only from the reference run.
- **Strictness fails every release of a rarely changed area for old survivors the reference run did not show:** → `--since` covers the same mutants as the full run, which the switch-on check proved clean.

## Decisions recorded

- **Release strictness on `NoCoverage`:** counted as a survivor, as `mutation-testing-in-ci` already does for the finding.
- **The values:** four points of margin, a minimum of 20 counted mutants, and Accepted marks allowed but kept to a minimum (above).
