# Tasks

Start only once `mutation-testing-in-ci` (PR #255), `explain-mutation-score-gap` and, preferably, `upgrade-to-net11` have merged. File names and counts below are from the runner's full run of 13 November 2026 (run 43478448861: 253 survived or without coverage in 78 files); task 1.1's reference run replaces the counts and may add or drop files, and the area order stays.

**The file cycle** (each batch task says "File cycle" for one file; it is one red-green-refactor cycle and one commit, with the facts named first):

- **Red:** from `test\Source191.Cader.Core.Tests`, `dotnet stryker --mutate "**/<Folder>/<File>.cs"` lists the file's mutants `Survived` or `NoCoverage` (a scoped run costs about 2.5 minutes fixed plus the file's mutants). Sort each into a real gap, Equivalent or Accepted (design.md). Accepted is kept to a minimum: a mutant in test support code or in logging or diagnostic code is closed with a test, never Accepted by default, and every Accepted mark carries a one-line reason a reviewer can challenge. For a real gap, write the sociable fact (xunit.v3, Shouldly, real collaborators, a hand-written fake only at a boundary), run it and see it pass against the real code.
- **Green:** the repeated scoped run shows the gap's mutants `Killed`. For an Equivalent or Accepted mutant, add `// Stryker disable once <Mutator> : Equivalent: <reason>` (or `Accepted:` with its one-line reason) above its line, and see the scoped run show it `Ignored` with the reason.
- **Refactor and verify:** the fact reads well and stays free of machine paths, real repositories and people (`Northwind.*`, `C:\Users\me`); `dotnet test test\Source191.Cader.Core.Tests -c Debug --no-build` passes.

The tail of a batch (a few mutants per file in many files) is one checklist task, with a file cycle and a commit for each file in it: its length is known only from the reference run.

## 1. Baseline and conventions

- [ ] 1.1 Take the reference run: a full run in the environment `explain-mutation-score-gap` names as the break score's owner (a `workflow_dispatch` of `mutation.yml` if the machine matters), download its `stryker-output` and list every `Survived` and `NoCoverage` mutant by file, line and mutator. Add the first ledger line to `.claude\rules\checks.md`'s Mutation testing (date, Stryker version, score, survivors, no-coverage). **Verify:** the counts match the run's `run.json` and summary.
- [ ] 1.2 **Red:** the first mark in Core, on one agreed Equivalent mutant (for example a discard arm that throws `UnreachableException`), written as `// Stryker disable once <Mutator> : Equivalent: <reason>`. **Green:** the scoped run shows `Ignored` with the reason, and `dotnet build -c Debug` (warnings are errors), `dotnet test -c Debug --no-build` and `dotnet format --verify-no-changes` still pass, so `SourceConventionsShould`, `SentenceLiteralsShould` and the analysers have nothing against a comment. **Verify:** if one objects, record the fix in design.md before going on.

## 2. Batch 1: Domain (46)

- [ ] 2.1 File cycle, `WhereLabel` (7), facts in `WhereLabelShould`.
- [ ] 2.2 File cycle, `EditorChoice` (4), facts in `EditorChoiceShould`.
- [ ] 2.3 File cycle, `ReleaseNotes` (4), facts in `ReleaseNotesShould`.
- [ ] 2.4 File cycle, `NumberAwareOrder` (4), facts in `NumberAwareOrderShould`.
- [ ] 2.5 File cycle, `Settings` (4), facts in `SettingsShould`.
- [ ] 2.6 The tail of `Domain` (23 mutants in 16 files, among them `ProgramLabel`, `FolderPath`, `MemorySize`, `ReleaseVersion`, `JobId` and `RemoteRepository`), a file cycle per file. **Verify:** each file's scoped run is clean of unmarked survivors.
- [ ] 2.7 Record and ratchet: a full run, the batch's ledger line in `checks.md` (killed, Equivalent, Accepted, left), and `break` in `stryker-config.json` set to the whole score less four points, rounded down and never lower than before. If the batch added a mark under `src\`, raise `Version` by a patch with `.github\scripts\Step-Version.ps1` and add a `### Internal` line to `CHANGELOG.md`. **Verify:** `Assert-VersionBumped.ps1` and `Assert-ChangelogUpdated.ps1` pass against `main` (not needed for a tests-only batch), and `dotnet build -c Debug`, `dotnet test -c Debug --no-build` and `dotnet format --verify-no-changes` pass.

## 3. Batch 2: Ports (18)

- [ ] 3.1 File cycle, `UserConfig` (8), facts in `UserConfigShould` or the existing state-file facts, through `InMemoryState`, never the real `config.json`.
- [ ] 3.2 File cycle, `ProcessTimedOutException` (6), facts in `ProcessTimedOutExceptionShould`; an exception message that reports a failure is diagnostic text, so it is tested, not Accepted.
- [ ] 3.3 File cycle, `EditorSetting` (3), facts in `EditorSettingShould`; then the one other `Ports` mutant.
- [ ] 3.4 Record and ratchet, as 2.7.

## 4. Batch 3: Application\Diagnostics (34)

- [ ] 4.1 File cycle, `ChildCommandWords` (24), facts in `ChildCommandWordsShould` (a table of words: each word pinned by a fact with a made-up command; diagnostic code is not Accepted by default). `DiagnosticsPrivacyShould` and `DiagnosticEventsShould` still pass.
- [ ] 4.2 File cycle, `ChildCommand` (4), facts in `ChildCommandShould`.
- [ ] 4.3 File cycle, `ExceptionSummary` (3), facts in `ExceptionSummaryShould`; then the tail (3 mutants in 3 files), a file cycle each.
- [ ] 4.4 Record and ratchet, as 2.7.

## 5. Batch 4: WorktreeInventory (30)

- [ ] 5.1 File cycle, `WorktreeInventory` (30, of which 4 have no covering test: start with those), facts in `WorktreeInventoryShould` and `WorktreeInventoryLeftoversShould`, with `Northwind.*` names, `C:\Users\me` paths and the existing fake git.
- [ ] 5.2 Record and ratchet, as 2.7.

## 6. Batch 5: the rest of Application (56)

- [ ] 6.1 File cycle, `CommandLine` (11), facts in `CommandLineShould` (invoke the root with recording actions and `StringWriter`s).
- [ ] 6.2 File cycle, one per file for the `Wording*` files (28 between them: `WordingOfDetail` 9, `WordingOfReleaseNotes` 5, `WordingOfOutcomes` 4, `WordingOfScreens` 3, `WordingOfConfig` 3, `WordingOfConsole` 2, `Wording` 2), facts in the matching `Wording*Should` classes; a literal no fact reads is a gap.
- [ ] 6.3 File cycle, `MemoryBreakdown` (3), facts in `MemoryBreakdownShould`; then the tail of `Application` (14 mutants in about 12 files, among them `WorktreeInspector` and `KeyBar`), a file cycle each.
- [ ] 6.4 Record and ratchet, as 2.7.

## 7. Batch 6: Views, Detail and Reporting (51)

- [ ] 7.1 File cycle, `DetailLayout` (10), facts in `DetailLayoutShould`.
- [ ] 7.2 File cycle, `ProblemReports` (10), facts in `ProblemReportsShould`; nothing personal in any fixture.
- [ ] 7.3 File cycle, `StatusLayout` (5), facts in `StatusLayoutShould`.
- [ ] 7.4 File cycle, `WizardState` (4) in a new `WizardStateShould` and `WizardView` (4) in `WizardViewShould`: two files, a commit each.
- [ ] 7.5 File cycle, `ListSelection` (4), facts in `ListSelectionShould`, and `DetailLoader` (3), facts in `DetailLoaderShould`: two files, a commit each; then the tail (11 mutants in about 9 files, among them `Durations` and `ConfigViewState`), a file cycle each.
- [ ] 7.6 Record and ratchet, as 2.7.

## 8. Batch 7: Actions, Launch and TerminalSetup (18)

- [ ] 8.1 File cycle, `OpenProblemReportHandler` (4), facts in `OpenProblemReportHandlerShould` (assert the outcome record), and `TabClaimer` (3), facts in a new `TabClaimerShould`: two files, a commit each.
- [ ] 8.2 The tail (11 mutants in about 8 files, among them `DeleteSessionHandler`, `RenameSessionHandler`, `HereCommandRunner` and `TerminalSetupRunner`), a file cycle each.
- [ ] 8.3 Record and ratchet, as 2.7.

## 9. Strict release runs

- [ ] 9.1 Check the switch-on condition: a full run on a runner (`workflow_dispatch`) after section 8 shows no `Survived` or `NoCoverage` mutant that is not `Ignored`. **Verify:** the run's `run.json` and `mutation-report.json` list none; the count of Accepted marks is in `checks.md`. Anything left is fixed or marked first, and this section does not start until none is.
- [ ] 9.2 Run `Invoke-Pester test\Scripts\Mutation.Tests.ps1`. **Verify:** the existing `Write-MutationRun` facts pass before any change (they pin today's behaviour: a release with survivors is a finding, a clean one is not, a full run compares scores) and the `--break-at 0` fact passes, so the strictness change has a safety net.
- [ ] 9.3 **Red:** a fact in `Mutation.Tests.ps1`, "fails a release run that counted 20 mutants and left one surviving" (`Failing` true, `Counted` 20). **Green:** `-MinimumCounted` (default 20), `Counted` (Killed, Timeout, Survived and NoCoverage) and `Failing` in the answer and in `run.json`. **Verify:** the fact passes.
- [ ] 9.4 **Red:** "does not fail a release run that counted 19", "does not fail one that counted 20 with no survivor" and "counts an Ignored mutant for nothing". **Green:** the rule holds (the first two should pass already). **Verify:** all pass.
- [ ] 9.5 **Red:** a fact that `Format-MutationComment.ps1` says the run failed and names the count when `run.json` says `failing`. **Green:** the added line. **Verify:** the fact passes and the rest of the comment is unchanged.
- [ ] 9.6 `mutation.yml`: the "Record the run" step writes `fail=true` or `false` from the answer, and a last step after both artefact uploads runs `exit 1` with the count when it is true. **Verify:** `actionlint` if installed, and by reading, with 9.3's fact covering the rule itself; a `workflow_dispatch` run is a full run and never fails this way, so the workflow step is proved by the owner's check after merge (Workflow follow-up).

## 10. Docs and release

- [ ] 10.1 Update `.claude\rules\checks.md` (Mutation testing: the ledger of batches, the marking convention and the Accepted count, the ratchet rule, and the strict release rule with its 20-mutant minimum), `CONTRIBUTING.md`'s mutation paragraph (the mark convention and that a release run can fail on an unmarked survivor), and `CLAUDE.md`'s mutation line if its wording is now wrong. **Verify:** `git grep -n "Stryker disable"` finds only marks in the convention's form, and the docs say the same numbers.
- [ ] 10.2 Run `openspec validate close-mutation-gaps --strict`, `Invoke-Pester test\Scripts`, `dotnet build -c Debug`, `dotnet test -c Debug --no-build` and `dotnet format --verify-no-changes`. **Verify:** each exits 0 and `pipeline`'s `check` is green.

## Workflow follow-up

- **Pull requests:** O is sections 1 to 3, P is sections 4 and 5, Q is section 6, R is sections 7 and 8, and S is sections 9 and 10; the batch pull requests are sequential because each ratchets the one `break` line. Each pull request that adds a mark carries its own `Version` patch bump and `CHANGELOG.md` Internal line, and a rebase re-raises `Version` one step above `main`.
- **Archive:** archive the change in S, the pull request that completes it.
- **After merge (owner):** the first release run that counts 20 or more mutants shows the strict rule working; a deliberate weak test on a branch checks the failure and the issue comment, and the issue is closed afterwards.

## Implementation plan

See `openspec\implementation-plan.md` for the whole sequence.

- **PR:** O, P, Q, R (the batches), then S (the strict release rule)
- **Step:** 9 (O), 10 (P), 11 (Q), 12 (R), 13 (S)
- **Depends on:** D (PR #255), N (`explain-mutation-score-gap`) and, preferably, M (the .NET 11 steps, so the C# 15 sweep does not move the survivors); O to R run one after another, S follows R
- **Parallel with:** nothing: each batch ratchets the same `break` line and the same `checks.md` ledger
- **Model:** sonnet for O, P, Q and R; opus for S
- **Effort:** high for O, P, Q and R; medium for S
- **Why:** the batches follow one established loop (scoped run, fact, rerun) where care about equivalent mutants matters more than design; S changes a script and a workflow step that decide whether a release run fails, with exact thresholds and a workflow that must keep its minimal rights.
- **Owner gates:** blocking before O, the reference run's environment (from N's finding); blocking before S, 9.1 (no unmarked survivor left). After merge, the first strict release run and the deliberate-failure check.
