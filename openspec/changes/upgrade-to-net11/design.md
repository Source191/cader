# Design

## Context

See proposal.md for why. Today (13 November 2026): every project targets `net10.0`; `global.json` pins SDK 10.0.301 (`latestMinor`); `AnalysisLevel` is `10.0-recommended`; `LangVersion` is `latest`, so C# 15 applies as soon as the target moves. The machine has SDK 11.0.100 installed, so every check can be run before CI has it. The winget build is self-contained, so the runtime moves for every user.

Facts below are marked **verified** (read on Microsoft Learn on 13 November 2026), **observed** (found in this repository) or **unverified** (to be confirmed by the first task that needs it; do not rely on the issue's wording).

## Goals / Non-Goals

- **Goal:** a green build, test run and every check on `net11.0` first (phase 1 is a gate), then the adoptions one by one, each its own commit, so a regression is easy to place.
- **Goal:** every item the issue lists ends as adopted, declined with a reason, or shown not to apply, in the coverage tables below, which the apply phase fills in as it goes.
- **Non-goal:** any change a user can see, apart from the runtime itself. State files keep their on-disk format.

## Decisions

### 1. One change, two gates

Phase 1 retargets and fixes what breaks, with no adoption. It must pass: `dotnet build -c Debug`, the Core and full test suites, `dotnet format whitespace --folder --verify-no-changes --exclude openspec`, the hand-off check, the smoke check, and a mutation run, all also run **before** (on `net10.0`) so a difference in behaviour or score is visible. Later phases adopt features, each ending with the suite green. Alternative: split into a retarget change and follow-ups (the issue's suggestion); the owner chose one change, so the phases and their commits do that job instead.

### 2. Breaking changes: coverage

| Item | Status | Finding |
| --- | --- | --- |
| `Environment.GetCommandLineArgs()[0]` keeps the apphost invocation name ([Learn](https://learn.microsoft.com/dotnet/core/compatibility/core-libraries/11/environment-getcommandlineargs-apphost-invocation-name)) | Does not apply (PR J) | No use of `GetCommandLineArgs` in `src`, `test` or `tools` (re-grepped 14 November 2026). The page names `Environment.ProcessPath` as the unaffected way to the executable, which is what `SharedParts` (and so `TabScripts`) reads, with `ProgramFolder.Of` following a winget link either way. A winget install started through its link is the owner's check after merge; the tab scripts were not run unattended because they open tabs. |
| `DateOnly` and `TimeOnly` `TryParse` throw for invalid input | Does not apply (PR J) | Re-grepped 14 November 2026: neither type is used in `src`, `test` or `tools`. |
| Restore does not search the global packages folder for a higher version ([Learn](https://learn.microsoft.com/dotnet/core/compatibility/sdk/11/restore-global-packages-folder-version-search)) | Handled (PR J) | Every reference names an exact version that exists on nuget.org, so restore never needed a higher one. The cache key changed with the retarget: PR J's first run (43638076129) missed it, restored main's cache through the restore key (257 MB x64, 406 MB Arm) and saved a new entry (490 MB x64, since main's .NET 10 packages stay beside the .NET 11 ones); WARM-RUN. Local restore and build on a cold `obj` passed with no NU warning. |
| Minimum hardware: x86-64-v2 baseline for x86/x64, R2R target x86-64-v3 on Windows (**verified**) | Applies | .NET fails to start on x86/x64 CPUs without `CX16`, `POPCNT`, `SSE3`, `SSSE3`, `SSE4.1`, `SSE4.2`; Microsoft says Windows 11 requires this already and all CPUs supported on Windows 10 have it. Cader supports Windows 11 only, so no user is newly excluded. Record it in the README requirements as one sentence anyway. The Windows Arm64 minimum is unchanged. |
| Roslyn: `closed` and `union` become contextual keywords, `with(...)` in a collection expression binds as arguments ([Learn](https://learn.microsoft.com/dotnet/csharp/whats-new/breaking-changes/compiler%20breaking%20changes%20-%20dotnet%2011)) | Does not apply (PR J) | Re-grepped 14 November 2026: no type, alias or field named `closed` or `union`, and no method named `with`; the `net11.0` build has no CS9380, CS1519 or CS9370. `unsafe` and `safe` changes are C# 16 only. |
| Other .NET 11 core-library items ([Learn list](https://learn.microsoft.com/dotnet/core/compatibility/11)) | Does not apply (PR J) | Re-grepped 14 November 2026: no Zip, Tar, Cbor, Deflate or GZip stream, Complex, BigInteger, Tensor, named mutex or pipe, `GetCallingAssembly` or `PackagePart` in `src`. `Math.Round` changed only its `digits` overloads; `LogoIcon` and `WordingOfScreens` call the one-argument `Math.Round(double)`, which the page names unaffected. `Nullable.GetUnderlyingType` throws only for custom `Type` subclasses; the architecture tests pass runtime types. The saturating float-to-small-integer casts: every `(byte)`, `(short)` and `(ushort)` cast in the solution is of an integer or a character. `Environment.TickCount`: not used. No `SYSLIB` obsoletion fires in the warning-free `net11.0` build. |
| SDK items (`dnx` scripts bypass `global.json`, VSTest drops Newtonsoft.Json, restore) | Does not apply (PR J) | `dnx dotnet-counters` in `docs\performance-monitoring.md` runs a tool, not this solution, so the SDK it picks does not matter (the doc's ".NET 10 SDK" wording is task 4.3's). Every test project runs on Microsoft.Testing.Platform (`global.json`), not VSTest. |
| Nine Microsoft.Extensions libraries join the shared framework ([Learn](https://learn.microsoft.com/dotnet/core/compatibility/extensions/11/extensions-in-shared-framework)) | Handled (PR J) | Logging.Abstractions was referenced by Core and Infrastructure; the references are removed (NU1510 failed the build) and the assembly now comes with the runtime, so it leaves the build output and the notices (task 1.10). |
| Analyzer and compiler diagnostics (warnings are errors) | Handled (PR J) | The `net11.0` build under `AnalysisLevel` `11.0-recommended` raised, all fixed in code: NU1510 for the Logging.Abstractions references (removed); CA2016 and S8949 on 13 `WriteLineAsync` calls in `TerminalSetupRunner` and `HereCommandRunner`, which now forward their token to the new `CancellationToken` overload; IDE0028 on `CommandLine.Build` (a nested `Subcommands` initialiser, not `with(...)`, because Stryker's Roslyn 5.9.0 reads `LangVersion` latest as C# 14 and refused it with CS8652), `FakeFolders` and `DriverOptions` (collection expression arguments: neither is compiled by Stryker); CA2027 in `CapturedRun` (`WaitAsync` with `SuppressThrowing`); CS9164 in `ProcessRegistryNative.ExecutableName` (the inline array is converted to a `ReadOnlySpan` before `MemoryMarshal.Cast`). |

### 3. Packages: coverage

For each, the apply phase records the version chosen and how .NET 11 support was established (a `net11.0` target or tested-on statement, else a clean build and full run with no new warning). Nothing below is confirmed yet.

- Terminal.Gui 2.5.0: check releases and issues first; run the hand-off and smoke checks; re-verify and re-date `.claude\rules\terminal-gui.md`.
- System.CommandLine 2.0.14, Microsoft.Extensions.Logging.Abstractions, FileSystemGlobbing and TimeProvider.Testing (10.11.0 today): move to 11.0.x if released. `System.Drawing.Common` is already 11.0.0 in `tools\ReadmeScene`.
- Interop.UIAutomationClient 10.19041.0: check it loads; re-verify the UI Automation tab facts in `.claude\rules\windows.md`.
- Microsoft.CodeAnalysis.CSharp 5.9.0 (test project): `SourceConventionsShould` and `OneTypePerFileShould` parse `src`; it must parse C# 15 (`closed`, `union`, `with(...)`, labelled jumps). Add a probe test that parses a sample using each keyword before any adoption.
- BannedApiAnalyzers 5.6.0, SonarAnalyzer.CSharp 10.35.0.4138, xunit.v3 4.0.2, Shouldly 4.3.0, NetArchTest.Rules 1.3.2: confirm each under Microsoft.Testing.Platform on `net11.0`.
- Stryker.NET: confirm `--test-runner mtp` works on `net11.0` (workflow from `mutation-testing-in-ci`); keep the before and after scores.
- The `nuget-package-updates` skill may do the sweep. A held or replaced package is explained here.

Recorded in PR J (14 November 2026). "lib" is the target folders in the package as restored; `dotnet list package --outdated` on that day named only xunit.v3 4.0.3 and Microsoft.CodeAnalysis.CSharp 5.12.0 as newer.

| Package | Version | Decision and evidence |
| --- | --- | --- |
| Microsoft.Extensions.Logging.Abstractions | 11.0.0, then removed | Already 11.0.0 (Dependabot, `78c06fd1`). .NET 11 puts it in the shared framework and a `PackageReference` to it raises NU1510 ([Learn](https://learn.microsoft.com/dotnet/core/compatibility/extensions/11/extensions-in-shared-framework)), so the references in Core and Infrastructure go in the retarget commit; the assembly then comes with the runtime. |
| Microsoft.Extensions.FileSystemGlobbing | 11.0.0 | Already moved; ships a `net11.0` lib; not one of the nine shared-framework libraries, so kept. |
| Microsoft.Extensions.TimeProvider.Testing | 11.0.0 | Already moved; ships a `net11.0` lib. |
| System.Drawing.Common (ReadmeScene) | 11.0.0 | Already moved; ships a `net11.0` lib. |
| System.CommandLine | 3.0.0 | Newest; lib `net10.0` and `netstandard2.0`, which `net11.0` consumes; held on a clean build and full run. |
| Terminal.Gui | 2.5.0 | Newest; lib `net10.0` only. Held on a clean build and full run on `net11.0`, with CI's smoke check on both architectures as the evidence the screen still runs (the hand-off and terminal facts are task 1.7). |
| Interop.UIAutomationClient | 10.19041.0 | Newest; lib `netcoreapp3.0` and `netstandard2.0`; a COM interop assembly with no runtime dependency; held on a clean build and the `TerminalTab` tests. |
| Microsoft.CodeAnalysis.CSharp (test) | 5.9.0 to 5.12.0 | 5.12.0 is the compiler SDK 11.0.100 ships (`Roslyn\bincore`, 5.12.0-1.26529.118), so the source facts parse code as the build compiles it. Task 1.2 had no red: 5.9.0 under `LanguageVersion.Preview` already parsed `closed`, `union`, `with(...)` and a labelled `break` without an error and read the union and closed record as type declarations. The probe (`SourceFilesShould`) sits in Infrastructure.Tests, beside the Roslyn reference, not in Core. |
| Microsoft.CodeAnalysis.BannedApiAnalyzers | 5.6.0 | Newest; analyser only (no lib); held on a clean `net11.0` build with RS0030 still failing a banned call. |
| SonarAnalyzer.CSharp | 10.36.0.15337 | Newest; analyser only; held on a clean `net11.0` build. |
| xunit.v3 | 4.0.2 | Held: lib `net472` and `net8.0`, which `net11.0` consumes under Microsoft.Testing.Platform; 4.0.3 is a patch Dependabot's weekly `nuget` group brings, and nothing in .NET 11 needs it. |
| Shouldly | 4.3.0 | Newest; lib `net8.0`, `net9.0`, `netstandard2.0`; held on the full run. |
| NetArchTest.Rules | 1.3.2 | Newest; lib `netstandard2.0`; held on the full run (`LayeringShould`). |
| Stryker.NET (`dotnet-stryker`) | 5.0.0 | Held; checked by the `net11.0` mutation run in figure 7. |

### 4. Process APIs: coverage

**Verified** to exist in .NET 11 (Learn, what's new in libraries): `Process.Run`/`RunAsync`, `RunAndCaptureText`/`RunAndCaptureTextAsync`, `ReadAllLines`/`ReadAllLinesAsync` with `ProcessOutputLine`, `Process.StartAndForget`, `ProcessStartInfo.StartDetached`, `ProcessStartInfo.KillOnParentExit` (Windows only), `Process.TryGetProcessById`, `SafeProcessHandle.Open`/`TryOpen`. **Unverified:** signatures, how timeouts and cancellation behave, whether `KillOnParentExit` reaches grandchildren, whether the standard-handle properties and `File.OpenNullHandle` ship as the issue says, and the overlapped-I/O change. The first task of each adoption reads the API reference and writes a spike test against the real behaviour before any production change.

Constraints every adoption carries (otherwise declined): `ProgramLocator` full paths; `InheritedEnvironment` drop; UTF-8 output decoding; the timeout that ends the child and its tree (`ProcessRunner`); no console window for a captured child; `ConfirmedProcess` untouched.

| Candidate | Where | Decision rule |
| --- | --- | --- |
| `RunAsync` / `RunAndCaptureTextAsync` | `ProcessRunner.RunAsync` | Adopt only if timeout plus tree kill, cancellation and the UTF-8 decoding stay as tested in `ProcessRunnerShould`; the existing tests are the spec and must pass unchanged. |
| `ReadAllLinesAsync` | git and Claude CLI adapters | Adopt only where an adapter parses lines today; the Claude listing is one JSON document, so likely declined. |
| `StartAndForget`, `StartDetached`, standard-handle properties, `File.OpenNullHandle` | `Desktop.StartAsync`, `OpenFileScript`, the PowerShell hop in `Desktop.OpenAsync` | `quiet-editor-start` lands first and fixes the editor key with a cleaned PowerShell child. Revisit afterwards: a direct start with output discarded may replace that child. ShellExecute still cannot take an environment, so the hop in `OpenAsync` likely stays; record the answer either way. The `DesktopShould` facts of `quiet-editor-start` must pass unchanged. |
| `TryGetProcessById`, `SafeProcessHandle.TryOpen` | `ProcessRegistry.WithProcess`, `ProcessRegistryNative` | Adopt to drop the `catch (ArgumentException)` there, then remove that place from `SourceConventionsShould.CatchArgumentExceptionOnlyWhereNamed` and the matching sentence in `CLAUDE.md`. |
| `KillOnParentExit` | captured short-lived children only (`claude agents`, git, `gh`, `az`) | Never on `claude --bg`, `wt.exe`, editors, solutions or any tab (they must outlive the console). Adopt only if a spike shows it ends the child tree when the console is killed, and costs nothing measurable. |
| Overlapped I/O for redirected output | none (free) | Measure the refresh timings from `docs\performance-monitoring.md` before and after; record the result. |

### 5. C# 15 and language: coverage

- **Closed hierarchies (verified):** `closed` makes a class abstract and fixes its direct descendants to the declaring assembly; a `switch` over every direct descendant needs no default arm. Apply to the two-case Domain and Ports hierarchies named in `CLAUDE.md` (`SessionNameValidation`, `LaunchPlanResult`, `NewestRelease`, `ProgramLocation`, plus any other hierarchy `ClosedHierarchiesShould` already treats as closed). Remove their `UnreachableException` discard arms. Open hierarchies (`StatusMessage`, `Outcome`, `LaunchProblem`, `WizardProblem`, `WizardInput`, `DiagnosticEvent`) keep their fallback arms, because the reflection facts rely on them. Derivation is not transitive, so a nested sealed case is fine. `ClosedHierarchiesShould` changes from "abstract record with nested sealed cases" to "carries `closed`"; update the switch rule in `CLAUDE.md`. Confirm first that `closed` works with `abstract record` (the Learn example uses `closed record class`) and with the nested-case layout, and that `SealedTypesShould`, CA1852 and Sonar accept it.
- **Union types (verified syntax):** `public union Pet(Cat, Dog, Bird);` with exhaustive switches. Candidates: `Session.Background`/`Session.Interactive` and outcome shapes. Decline wherever a union would break the one-top-level-type-per-file rule, the nested-case convention, the JSON converters, or `NullFreeModelsShould`; decide per type in a spike. Union support in System.Text.Json is **unverified**; no state file may change format.
- **Collection expression arguments (verified):** `[with(StringComparer.Ordinal), ...]` for the `HashSet` and `Dictionary` initialisations in `ChildCommandWords`, `ChildCommand` and `PullRequestCatalogue`; keep the ordinal rule in `locale.md`. Adopt where it replaces a constructor argument, nowhere else.
- **Extension indexers, labelled `break`/`continue` (verified):** adopt only where a nested loop or a flag variable exists today (IDE0410 will name them); expect few or none, and record "declined, no site" if so.
- **Runtime async (unverified):** on by default for `net11.0` per the issue. Check crash reports and `ExceptionSummary` still read well, and that the diagnostic tests that read stack text pass.
- **System.Text.Json (JSON Lines verified to exist):** adopt a new option only where `JsonFileStore`, `ClaudeAgentsListing`, `TerminalFragment` and `WingetPackageVersions` benefit and the bytes written stay identical. Overlaps `stricter-state-files`, which lands first. Source generation is declined unless it measurably cuts start-up time.
- **`EqualityComparer<T>.Create`:** adopt only if a hand-written comparer exists only to compare one key; the current code compares through `FolderPath`, `StringComparer` and `ITextValue`, so likely none.
- **House style:** `.editorconfig` gains `warning` rules for each adopted feature that has an IDE rule (for example IDE0410); `CLAUDE.md`'s C# idioms section names them; reflection or source facts added where checkable (for example "no `UnreachableException` arm in a switch over a closed type"). Existing code is brought into line in the same change.

### 6. Build, CI, release, support

- `global.json` to the 11.0.x SDK (`rollForward` unchanged). Check whether `windows-latest` has the 11.0 SDK; if not, add `actions/setup-dotnet` pinned by commit like the other actions in `pipeline.yml`, `claude-contract.yml` and `codeql.yml` (CodeQL runs on `ubuntu-latest` with build mode none), and note it in `.claude\rules\checks.md`.
- `publish.yml`: **observed** to hold no `dotnet-version` line (only `windows-latest`), so the issue's item does not apply today; recheck the file at apply time.
- `.github\actions\build-test-pack\action.yml` comments name `global.json (10.0.301...)`; update the wording and the `net10.0` output paths (`claude-contract.yml`, `pipeline.yml`, `HandoffCheck.cs`, `src\Source191.Cader\README.md`).
- NuGet cache key already includes `global.json` and every `*.csproj`; keep.
- winget build: re-measure the self-contained single-file size, zipped size and start-up time against the 1 November 2026 figures (about 40 MB, 34 MB zipped on 10.0.403) in `install-with-winget`, and run the smoke check on the zip's `cader.exe`. Record before and after in `src\Source191.Cader\README.md`.
- Version: minor bump of `Version` in `src\Source191.Cader\Source191.Cader.csproj` (0.21.0-preview.1 today, so the next minor) and a `CHANGELOG.md` line; `Assert-VersionBumped.ps1` and `Assert-ChangelogUpdated.ps1` must pass.
- Support date: .NET 11 is a Standard Term Support release; its end-of-support date is **unverified** here and is recorded from the .NET support policy page in task 6.3 before the change is archived, with a note for planning the move to .NET 12.

### 7. Before and after figures

Measured on the owner's Windows 11 developer machine (x64) unless a runner is named. Before: `net10.0` on SDK 10.0.404 at `76df5d69` (PR #276), 14 November 2026. After: `net11.0` on SDK 11.0.100 (runtime 11.0.0) at PR J's head, the same day. Start-up is the median of 15 runs of the published `cader.exe --version` after one warm-up run (it reaches only the command line, so it reads no state). Size is the published single-file `cader.exe` and the zip `Compress-WingetBuild.ps1` writes.

| Figure | Before (`net10.0`) | After (`net11.0`) |
| --- | --- | --- |
| Full test run | 3,182 tests (PR #276 plus the two C# 15 parser facts of task 1.2): 3,173 passed, 9 skipped (the 7 Claude contract facts and the 2 help-snapshot cases, gated on `CADER_CLAUDE_CONTRACT`), 0 failed | 3,182 tests: 3,173 passed, the same 9 skipped, 0 failed, three runs in a row (after `ProcessRunnerIntegrationShould` was given a collection of its own; before that its machine-wide ping check failed three of three full runs on pings other classes started) |
| `dotnet format whitespace --folder --verify-no-changes --exclude openspec` | exit 0 | exit 0 (and `dotnet format --verify-no-changes` exit 0) |
| Hand-off check | passed on the runner (`handoff`, run 43634112566) | passed on the runner (`handoff`, run 43638076129, SDK 11.0.100 from `setup-dotnet`) and in `claude-contract.yml` dispatched on the branch (run 43638116833, which builds the Debug `net11.0` path); not run locally, because `cader here` reads the real `config.json` |
| Smoke check | passed on both runners (`check` and `check (win-arm64)`, run 43634112566) | passed on both runners (`check` and `check (win-arm64)`, run 43638076129); not run locally, because Cader's state folder cannot be redirected |
| Mutation run (Stryker 5.0.0, full, local, fresh clone) | 87.81 %: 2,690 killed, 3 timed out, 347 survived, 27 no coverage (3,067 tested; 464 compile errors, 543 ignored); 7 min 16 s | 87.71 %: 2,687 killed, 3 timed out, 350 survived, 27 no coverage (3,067 tested; the same 464 and 543); 6 min 54 s. 0.10 points lower, inside the 0.2-point run-to-run spread `checks.md` records: the undetected counts moved by one to three in 19 files the retarget did not touch, both ways. Stryker 5.0.0 (tool on .NET 10) runs `--test-runner mtp` against `net11.0` test projects, but compiles Core with its own Roslyn 5.9.0, which refused C# 15's collection expression argument (CS8652) and stopped the first run; 5.1.0 bundles the same Roslyn. |
| Refresh timings (`docs\performance-monitoring.md`) | not measured: they need a live console over the owner's own state, so the owner records them | not measured, for the same reason (task 2.6 compares them) |
| Self-contained `cader.exe` (win-x64) | 38.5 MiB | 41.2 MiB (runtime 11.0.0), although six Microsoft.Extensions assemblies left the build: the shared framework they joined is bundled whole |
| Zip (win-x64) | 33.6 MiB | 36.0 MiB; `Compress-WingetBuild.ps1` found exactly its five files |
| Start-up (`cader --version`, median) | 557 ms (544 ms measured again beside the `net11.0` build) | 548 ms: no measurable change |

## Risks / Trade-offs

- [Terminal.Gui does not support .NET 11 yet] → Hold the retarget until it does or the hand-off and smoke checks pass on `net11.0`; record the evidence. Do not retarget on hope.
- [A dependency (Stryker, Sonar, Roslyn) lags C# 15] → The probe test in decision 3 finds it early; keep a held package at its current version with the reason, never suppress the analyser.
- [`closed` or unions fight the repository's conventions] → Spike per type; declining is an allowed outcome and is recorded.
- [Collision with in-progress changes] → The order in the proposal; rebase on `main` after each merges.
- [One change grows too large to review] → Each phase is its own commit series and ends green, so review can go phase by phase.
- [A different runtime changes timing] → Before and after measurements of refresh timings, size and start-up.

## Migration Plan

Work on a branch from `main` after the changes in the proposal's Order have merged. Phase 1 lands green first; later phases follow in the same pull request. Rollback is reverting the pull request: nothing on disk (state files, settings, fragments) changes format, so a user can go back to the previous winget version.

## Open Questions

- Whether `actions/setup-dotnet` is needed depends on the runner image on the day. Answered in PR J (14 November 2026): yes. `windows-latest` ran `windows-2025-vs2026` 20261103.298.1 and `windows-11-arm` ran `windows-11-vs2026-arm64` 20261101.203.1, whose published SDK lists stop at 10.0.403, so a pinned `actions/setup-dotnet` v6.0.0 (the newest release at least two weeks old; v6.1.0 was eleven days old) reads `global.json` in the build-test-pack action, the `handoff` job, `claude-contract.yml` and `mutation.yml`, and installed 11.0.100 on both architectures. `codeql.yml` is unchanged: its `ubuntu-latest` image (20261111.370.1) lists 11.0.100 and build mode none builds nothing. `publish.yml` runs no `dotnet` and has no version line.
