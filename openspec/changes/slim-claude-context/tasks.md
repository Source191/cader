# Tasks

**Implement with:** Opus 5.5 at medium effort: no code behaviour changes, but every moved fact must survive with its date and version, and judging what is evidence and what is the fact needs care; the budget script is small and test-first.

Each task is red, green, refactor where there is code, with a commit per step. **The fact check** used below: every date (`2026-\d\d-\d\d` and a day, a month name and 2026), version string (`\d+\.\d+\.\d+`), run id and sha in the rule as it is on `main` appears in the new rule or its `docs\verified-facts` file (a throwaway pwsh loop over `git show main:<path>`, not committed).

## 1. The budget check

- [x] 1.1 `test\Scripts\Assert-ClaudeContextBudget.Tests.ps1` (red): over a temporary folder, a `CLAUDE.md` and a rule of exactly 12,288 UTF-8 bytes pass; one byte more fails naming the file; a 601-character line fails naming the file and line as `::error file=...,line=...::`; a folder with no `.claude\rules` passes; `.claude\skills` and `.claude\commands` are never read. Commit.
- [x] 1.2 `.github\scripts\Assert-ClaudeContextBudget.ps1 -Root <folder>` (green), in the style of `Assert-ChangelogUpdated.ps1` (comment-based help, `$ErrorActionPreference = 'Stop'`, exit 1 on any breach). Verify `Invoke-Pester test\Scripts\Assert-ClaudeContextBudget.Tests.ps1` passes and that running it at the repository root fails today, listing `CLAUDE.md` and the oversized rules. Commit. (It is wired into `pipeline.yml` in 6.1, once the files fit.)

## 2. Mutation testing and the checks

- [x] 2.1 Create `docs\verified-facts\README.md` (what the folder holds, that each file pairs with a rule of the same name, that a new fact's evidence goes here and its bullet in the rule) and `docs\verified-facts\mutation-testing.md` holding the close-mutation-gaps ledger, the runner-gap investigation and the `--since` measurements from `checks.md`. Commit.
- [x] 2.2 New `.claude\rules\mutation-testing.md`, scoped to `.github/workflows/mutation.yml`, `test/Source191.Cader.Core.Tests/stryker-config.json`, `.github/scripts/*Mutation*`, `test/Scripts/*Mutation*` and `.config/dotnet-tools.json`: the current facts only (the tool and its pin, the config, scoping a hand run, local scores never compared with the break score, static mutants left to the weekly run, the strict release rule, the range, the break score and thresholds, the marks policy, Roslyn and `LangVersion` 14 with what to do when Stryker moves on), each pointing at its evidence. Verify the fact check and that the file is at most 12 KB. Commit.
- [x] 2.3 `checks.md` keeps Workflows, Hand-off check, Smoke check and Claude Code contract tests, opens with one line naming `mutation-testing.md` for its old Mutation testing section, and moves the cache sizes, timings and Dependabot incident detail to `docs\verified-facts\checks.md`; split each bullet over 600 characters. Verify the fact check, the budget script passes for this file, and `Format-MutationComment.ps1`, `mutation.yml`, `pipeline.yml` and `src\Source191.Cader.Core\README.md` still land on the file that holds what they cite. Commit.

## 3. Windows, Windows Terminal and winget

- [x] 3.1 New `.claude\rules\windows-processes.md` (the Windows process facts, the credential-free fetch and .NET 11's process APIs) and `winget.md` (the winget facts and Arm Windows), each scoped to the globs of `windows.md` that concern it (`Processes/**`, `*ProcessRegistry*`, `ProcessTree*`, `ConfirmedProcess*`, `ProgramLabel*`, `InheritedEnvironment*`, `GitCli*`, `ProcessApisShould`, `StartAndForgetShould`, `ParentStandIn/**` for the first; `Packages/**`, `UpdateCheck*`, `*PackageVersions*`, `ProgramFolder*`, `publish.yml`, `Compress-WingetBuild*`, `tools/install-local.ps1` for the second), their evidence in `docs\verified-facts\windows-processes.md` and `winget.md`. Commit.
- [x] 3.2 `windows.md` keeps the Windows Terminal and ShellExecute facts with its narrowed globs, opens with one line naming the two new files, and moves the `--suppressApplicationTitle` probes, the tab-shade measurement and the editor-console investigation to `docs\verified-facts\windows.md`. Verify the fact check for all three rules, each at most 12 KB with no line over 600, and that `TerminalProfileId`, `FileAssociations`, `ProgramFolder` and `GitCli` cite a file (or the pointer line) that holds their fact. Commit.

## 4. The other rules

- [x] 4.1 `terminal-gui.md`: move the Tabs re-measuring spike, the `TopResizable` spike (resizable-detail-pane was withdrawn), the decompilation narratives and the keys spike plan to `docs\verified-facts\terminal-gui.md`, keeping each fact and its consequence. Verify the fact check, at most 12 KB, no line over 600. Commit.
- [x] 4.2 `console-screen.md`: split each bullet over 600 characters (Layout's first, the Config tab's) by subject, and move change names and preview versions that only date a fact to `docs\verified-facts\console-screen.md`; if it is still over 12 KB, split Layout and Refreshing into two rules with the same scope (design, Risks). Verify the fact check, the budget. Commit.
- [x] 4.3 `claude-code.md`: move the glyph capture counts, the `/bg` transcript investigation and the accounts sampling to `docs\verified-facts\claude-code.md`; drop `openspec/changes/**` from its scope, keeping `openspec/specs/session-*/**`. Verify the fact check, the budget, and that `ClaudeContractShould`, `ClaudeHelpSurfaceShould`, `ClaudeCli`, `ClaudeCodeSupport`, `claude-contract.yml` and `pipeline.yml` still cite what the file holds. Commit.
- [ ] 4.4 `security.md` and `locale.md`: split long bullets in `security.md` (Terminal settings writes, Diagnostics) and drop `**/*.csproj` from `locale.md`'s scope in favour of the four projects that set `InvariantGlobalization` (`src/Source191.Cader/*.csproj` and the three test projects'). Verify the budget for both. Commit.

## 5. CLAUDE.md, config.yaml and the contributor's guide

- [ ] 5.1 New `.claude\rules\effects-and-views.md` and `values-and-null.md` with the scopes in design decision 3, holding `CLAUDE.md`'s Reducers, effects and keys, Wording, the key-to-tool routing, and Values, sessions and null with the Try-method and closed-hierarchy detail, unchanged in meaning. Verify each at most 12 KB and that every test `CLAUDE.md` named in those sections is still named. Commit.
- [ ] 5.2 `CLAUDE.md` down to about 10 KB (design decision 3): the index gains the five new rules and says to read `effects-and-views.md` before planning a key, effect, view or dialog; each enforced rule is one line naming its test; the security and Claude Code repeats go. Verify the budget passes for `CLAUDE.md`, every `*Should` name it held is in `CLAUDE.md` or a rule, and `SourceConventionsShould`'s summary comment still describes what `CLAUDE.md` states. Commit.
- [ ] 5.3 `openspec\config.yaml` `context` per design decision 5, then `openspec validate --all` passes. `CONTRIBUTING.md`: "CLAUDE.md's Code rules are the authority" becomes `CLAUDE.md` and `.claude\rules`, and its description of `.claude\` mentions `docs\verified-facts`. Commit.

## 6. Wire in and check

- [ ] 6.1 `pipeline.yml`: a "Check the Claude context budget" step in `check`, after "Check the changelog was updated" and before "Skip the build for a documentation-only change", running `.github/scripts/Assert-ClaudeContextBudget.ps1 -Root .`. Verify the script exits 0 at the root, `Invoke-Pester test\Scripts` passes, and the pull request's `check` run shows the step. Commit.
- [ ] 6.2 Measure and record in the pull request description: the bytes of `CLAUDE.md` and of each rule before and after, and what loads for a Tui file, `TabScripts.cs` and `pipeline.yml` before and after. `dotnet build -c Debug` and `dotnet format --verify-no-changes` still pass (nothing under `src\` changed: `git diff --name-only main -- src` is empty).

## Workflow follow-up

- Archive the change in the same pull request, with `openspec archive slim-claude-context`.
