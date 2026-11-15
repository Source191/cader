## 1. Set up and spike

- [x] 1.1 Raise the version with `.github\scripts\Step-Version.ps1` and add an Internal `CHANGELOG.md` line; verify `Assert-VersionBumped.ps1` passes against `main`.
- [x] 1.2 Check that no fact enumerates static fields by reflection (`GetFields(`, `FieldInfo`) over a type this change converts, so none passes with nothing left to enumerate; verify by grep, and turn any such fact to properties first.
- [x] 1.3 Spike: convert `Wording.cs`'s static initialisers to computed properties and run `dotnet stryker --since:main` from `test\Source191.Cader.Core.Tests` (the release run's mode, in this ordinary clone); verify in the JSON report that `Wording.cs`'s mutants have no `static` flag, a non-empty `coveredBy` and status Killed, and stop the change if they do not.

## 2. The guard

- [ ] 2.1 Red: `SourceConventionsShould.GiveCoreNoStaticInitialiser` fails for each static field or auto-property initialiser under `src\Source191.Cader.Core` outside its kept list (`StatusMessage.None`, `DetailRequests.NoRow`); verify it fails listing the remaining files.

## 3. Convert, a green commit per group

Each group is verified by the Core and Infrastructure suites passing and by a scoped Stryker run of its files (`--mutate`) in which every mutant has a `coveredBy` and none survives; a survivor gets the fact that pins the value, red then green, before the group's commit.

- [ ] 3.1 The `Wording*` files (`Wording`, `WordingOfConsole`, `WordingOfScreens`).
- [ ] 3.2 Domain: `TabPalette`, `Key`, `DisplayState`, `BatchFileArgument`, `ClaudeCodeSupport`, `EditorChoice`, `InheritedEnvironment`, `ProgramLabel`, `ReleaseNotes`, `RowFilter`, `TerminalProfileId`, and the rest of `Domain`.
- [ ] 3.3 Ports: `TerminalProfiles`, `WorkingFolder`, `UserConfig`, `TabOwner` and the rest of `Ports`.
- [ ] 3.4 Application: the view states, `ConsoleState`, `DetailContent`, `TabScripts`, `OwnTab`, `ProblemReport`, `SetupFactsReader`, the timing constants and the rest; verify the guard fact passes.
- [ ] 3.5 Diagnostics: `ChildCommand`, `ChildCommandWords`, `DiagnosticLine`, `ExceptionSummary`, `Heartbeat` and the rest, removing the 15 static-initialiser `Accepted:` marks; verify a scoped Stryker run of the four files shows every mutant Killed with a `coveredBy`, and only the line-0 frame mark left (`git grep "Stryker disable"`).

## 4. Prove and record

- [ ] 4.1 Run `dotnet build -c Debug`, `dotnet test -c Debug --no-build` and `dotnet format --verify-no-changes`; verify all pass.
- [ ] 4.2 Run `dotnet stryker --since:main` over the whole branch; verify in the JSON report that no mutant has `static: true` and none survived or lacks coverage (the post-merge release run's mode, which the full run cannot show).
- [ ] 4.3 Dispatch `mutation.yml` on the branch; verify no static mutant, no survivor and no mutant without coverage, and record the run.
- [ ] 4.4 Update `.claude\rules\checks.md` (the static-mutant entries, the #3742 follow-up, the ledger's Accepted count back to 1) and `CLAUDE.md`'s C# idioms (Core declares no static initialiser, held by the new fact); verify `openspec validate rewrite-static-initialisers` passes.

## Workflow follow-up

- Archive the change in the pull request that completes it.
- After merge, check that the release run's comment on #262 reports no survivor, then close #262 with links to the pull requests.
