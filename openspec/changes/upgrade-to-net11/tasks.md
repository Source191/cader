# Tasks

Start only once `quiet-editor-start`, `stricter-state-files`, `mutation-testing-in-ci` and `install-with-winget` have merged. Each task is one red-green-refactor cycle or smaller, with a commit per step. Where a task says "decline", the reason goes in the design's coverage table.

## 1. Baseline and the retarget gate

- [ ] 1.1 Record the before figures on `net10.0`: full test run, `dotnet format whitespace --folder --verify-no-changes --exclude openspec`, hand-off check, smoke check, a mutation run (score), the refresh timings from `docs\performance-monitoring.md`, and the self-contained size, zipped size and start-up time. Verify the numbers are written into the design's coverage tables.
- [x] 1.2 Red: a Core test that parses a sample using `closed`, `union`, `with(...)` and a labelled `break` with the Roslyn version `SourceConventionsShould` uses, and asserts no syntax error. Green: move `Microsoft.CodeAnalysis.CSharp` to a version that understands C# 15 (or record why it is held). Verify the probe test passes on `net10.0` with `LangVersion` preview if needed, and `SourceConventionsShould` and `OneTypePerFileShould` still pass.
- [x] 1.3 Check each package listed in the design (decision 3) for .NET 11 support; move the Microsoft.Extensions.* packages and the analysers, and record the version and the evidence for every package, including held ones. Verify `dotnet restore` and a Debug build succeed with no new warning on `net10.0`.
- [x] 1.4 Set `net11.0` in every project under `src`, `test` and `tools`, `global.json` to 11.0.x and `AnalysisLevel` to `11.0-recommended`; replace the `net10.0` output paths in `claude-contract.yml`, `pipeline.yml` and `tools\HeadlessTerminal\HandoffCheck.cs`. Verify `dotnet build -c Debug` finishes with no warning, fixing every new CA, IDE and Sonar diagnostic in code and suppressing none.
- [x] 1.5 Walk the breaking-change tables in the design: re-grep for `DateOnly`, `TimeOnly`, `GetCommandLineArgs`, and a type, field or alias named `closed` or `union`; confirm `Environment.ProcessPath` behaviour in `TabScripts`' executable paths. Verify the design's table says "does not apply" or "handled" for every row, with the evidence.
- [x] 1.6 Run the Core suite and the full suite on `net11.0`; fix failures at their cause. Verify both pass with the same number of tests as in 1.1 and no skipped test beyond the contract and hardware-gated ones.
- [ ] 1.7 Run the hand-off check and the smoke check on `net11.0`, then re-verify and re-date the facts in `.claude\rules\terminal-gui.md` and `.claude\rules\windows.md` (Terminal.Gui and UI Automation). Verify exit 0 from both checks and that the rule files carry the new date and versions.
- [x] 1.8 Mutation run and Claude Code contract tests on `net11.0` (`CADER_CLAUDE_CONTRACT=1` from a trusted repository, never `claude --bg --resume --help`). Verify the mutation score is no lower than in 1.1 and the contract tests pass, or that a difference is explained in the design.
- [x] 1.9 CI: check whether `windows-latest` has the .NET 11 SDK and update `pipeline.yml`, `claude-contract.yml`, `codeql.yml`, `.github\actions\build-test-pack\action.yml` (comments and wording) and `publish.yml` (recheck for a version line); add a pinned `actions/setup-dotnet` only if the image lacks the SDK, and describe it in `.claude\rules\checks.md`. Verify a pull request run of `pipeline.yml` is green, with a cold and a warm restore, and the NuGet cache key still matches.
- [x] 1.10 Regenerate `THIRD-PARTY-NOTICES.txt` for the .NET 11 assemblies the build copies; verify `Compress-WingetBuild.ps1` still finds exactly the five files and the notice lists every shipped assembly.

## 2. Process APIs

- [ ] 2.1 Spike tests (kept as facts) in `Infrastructure.Tests` that show how `Process.RunAsync` or `RunAndCaptureTextAsync`, `StartAndForget`, `StartDetached`, the standard-handle properties, `File.OpenNullHandle`, `KillOnParentExit` and `TryGetProcessById` behave on this machine: cancellation, timeout, tree kill, encoding, environment, grandchildren. Verify each fact passes and the design's process table is updated from what they show (adopt or decline).
- [ ] 2.2 Red: `ProcessRegistryShould` facts that a process id that has gone is left alone and a protected one refusing the action is left alone. Green: use `TryGetProcessById` or `SafeProcessHandle.TryOpen` in `ProcessRegistry.WithProcess` and drop `catch (ArgumentException)`. Refactor: remove the registry from `SourceConventionsShould.CatchArgumentExceptionOnlyWhereNamed` and from the `CLAUDE.md` sentence. Verify the `ConfirmedProcess` tests are unchanged and pass.
- [ ] 2.3 If 2.1 adopted it: `ProcessRunnerShould` stays unchanged and passing while `ProcessRunner` moves to the new run API, keeping the timeout, tree kill, UTF-8 decoding, `InheritedEnvironment` drop and no-window rule. Verify all `ProcessRunnerShould` and `ProgramLocatorShould` facts pass without edits.
- [ ] 2.4 If 2.1 adopted it: red `ProcessRunnerShould` fact that a captured child ends when the console is killed; green `KillOnParentExit` on captured children only, never on `claude --bg`, `wt.exe`, editors or tabs; a fact that those starts do not set it. Verify the facts pass.
- [ ] 2.5 Revisit `Desktop.StartAsync`, `OpenFileScript` and the hop in `Desktop.OpenAsync` against the `quiet-editor-start` facts (output never reaches Cader's screen, one argument for a folder with spaces, `&` and `'`, refusal sentence from the error code alone). Simplify only if all `DesktopShould` facts pass unchanged; record the answer on ShellExecute and the environment either way.
- [ ] 2.6 Measure refresh timings after the process work against 1.1 and record them in the design. Update `windows.md` and `security.md` for any changed process fact, with date and version.

## 3. C# 15

- [ ] 3.1 Red: change `ClosedHierarchiesShould` to expect the `closed` modifier on each Domain and Ports hierarchy (reflection on the compiler's marker), failing for them now. Green: mark `SessionNameValidation`, `LaunchPlanResult`, `NewestRelease`, `ProgramLocation` and any other closed hierarchy `closed`, one type per commit. Verify `SealedTypesShould`, CA1852, `OneTypePerFileShould` and `ClosedHierarchiesShould` pass; if `closed` cannot be combined with the nested-case record layout, decline and record why.
- [ ] 3.2 Refactor: remove each discard arm that throws `UnreachableException` in a switch over a closed type, and keep it where the type is open. Verify the build has no CS8509 and no new warning, and the tests for each switch pass.
- [ ] 3.3 Update the switch rule and the closed-hierarchy sentence in `CLAUDE.md` and add a source fact (in `SourceConventionsShould`) that no switch over a closed type keeps a throwing discard arm. Verify the fact fails on a probe sample and passes on `src`.
- [ ] 3.4 Spike unions on `Session` and one outcome shape; adopt only if the JSON converters, `NullFreeModelsShould`, one-type-per-file and the reducers' tests pass unchanged. Verify and record adopt or decline per type.
- [ ] 3.5 Collection expression arguments: refactor the `HashSet`/`Dictionary` initialisations in `ChildCommandWords`, `ChildCommand` and `PullRequestCatalogue` to `with(StringComparer...)`. Verify their existing tests pass and ordinal rules in `locale.md` still hold.
- [ ] 3.6 Extension indexers and labelled `break`/`continue`: list the sites IDE0410 and a search find; adopt where a flag or nested loop exists, otherwise record "declined, no site". Verify the build is warning-free.
- [ ] 3.7 Runtime async: confirm it is on for `net11.0`; verify the crash-report and `ExceptionSummary` tests pass and a crash report from a forced exception reads well; record the finding.
- [ ] 3.8 JSON and comparers: for each use of `JsonSerializer` and each hand-written comparer in `src`, adopt a new .NET 11 option only if the bytes written are identical (a golden-file fact in `JsonFileStoreShould`) and otherwise decline. Verify the state-file tests pass and the design table is complete.
- [ ] 3.9 House style: add `warning` rules to `.editorconfig` for each adopted feature and bring existing code into line. Verify `dotnet build` warning-free and `CLAUDE.md`'s C# idioms name the .NET 11 style.

## 4. Release, measure and document

- [ ] 4.1 Re-measure self-contained size, zipped size and start-up time on `net11.0` against 1.1; run `smoke` on the zip's `cader.exe`. Verify the figures are in `src\Source191.Cader\README.md` and the design with dates.
- [ ] 4.2 Raise `Version` by a minor and add the `CHANGELOG.md` line (runtime change, measured size or speed, and the x86-64-v2 requirement if it applies). Verify `Assert-VersionBumped.ps1` and `Assert-ChangelogUpdated.ps1` pass.
- [ ] 4.3 Update README requirements (SDK prerequisite, one sentence on the hardware baseline), CONTRIBUTING (SDK 11.0.x), `docs\performance-monitoring.md`, `openspec\config.yaml` (C# on .NET 11) and `CLAUDE.md`. Verify `Select-String` for `net10`, `.NET 10` and `10.0.` outside the archive finds only intended history.
- [ ] 4.4 Record .NET 11's end-of-support date from the .NET support policy page in the design with a note for planning .NET 12. Verify the date is quoted with its source and retrieval date.
- [ ] 4.5 Final run: `dotnet build -c Debug`, `dotnet test -c Debug --no-build` at the solution, the format check, hand-off, smoke, contract and mutation checks, `openspec validate upgrade-to-net11`. Verify all pass with no suppressed warning and the coverage tables in the design have no unresolved row.

## Workflow follow-up

- Archive this change in the pull request that completes it, so `openspec\specs` still describes `main`.
- The owner submits the stable release to winget-pkgs by running `publish.yml`.

## Implementation plan

See `openspec\implementation-plan.md` for the whole sequence.

- **Whole change:** four pull requests, J, K, L and M, each leaving `main` building and passing, with the change archived in M. It follows A, B, C, D, F, H and I (see Order); it does not wait for E or G, and whichever of J, E and G merges second rebases the `pipeline.yml` output paths.
- **PR J:** step 5, tasks 1.1 to 1.10 (retarget and package gate). **Depends on:** the above. **Parallel with:** nothing. **Model:** opus. **Effort:** high. **Why:** every package and breaking change is a judgement recorded with evidence. **Owner gates:** none.
- **PR K:** step 6, tasks 2.1 to 2.6 (process APIs). **Depends on:** J. **Parallel with:** L. **Model:** opus. **Effort:** xhigh. **Why:** adoption is allowed only where `ProgramLocator`, `InheritedEnvironment`, the timeout with tree kill and the `ConfirmedProcess` rules stay exactly as they are, and each spike decides; a wrong adoption is a security regression. **Owner gates:** none.
- **PR L:** step 6, tasks 3.1 to 3.9 (C# 15 and house style). **Depends on:** J. **Parallel with:** K. **Model:** opus. **Effort:** high. **Why:** a 59-file sweep with reflection facts, plus a union spike that may be declined. **Owner gates:** none.
- **PR M:** step 7, tasks 4.1 to 4.5 (release, measure, document, archive). **Depends on:** K and L. **Parallel with:** nothing. **Model:** sonnet. **Effort:** medium. **Why:** measurement and documentation against the figures J recorded. **Owner gates:** none.
