# Tasks

**Implement with:** Claude Opus 5.5 (`claude-opus-5-5`) at medium effort.

Each fact is red against its mutant: apply the issue's replacement by hand, see the fact fail, revert. Commit per fact.

## 1. AccountCatalogue

- [ ] 1.1 Rename with two accounts keeps the other's label (line 84, `true ? each with { Label = label } : each`).
- [ ] 1.2 A changed login with two accounts leaves the other untouched (line 137, `true ? updated : account`).
- [ ] 1.3 `InMemoryState.HoldNextAccountWrite`; a second rename made while the first one's write is held is written after it (lines 75 and 92, the rename's `WaitAsync` and `Release` removed).
- [ ] 1.4 The same after a refresh (line 101, the refresh's `WaitAsync` removed).
- [ ] 1.5 A refresh whose write fails answers normally and keeps the login in memory (line 112, `is OperationCanceledException`).

## 2. Wording and text values

- [ ] 2.1 The pane's Account line names its label for another account and an unseen owner (`WordingOfAccounts` lines 27 and 28).
- [ ] 2.2 The rename dialog's title and prompt word for word (`WordingOfAccounts` lines 34 and 37).
- [ ] 2.3 A branch row's Changes line names its label (`WordingOfDetail` line 76).
- [ ] 2.4 `AccountLabel.From` and `OrganisationId.From` name the refused text (`AccountLabel` line 17, `OrganisationId` line 15).

## 3. Check

- [ ] 3.1 Stryker scoped to the five files: every listed mutant Killed.
- [ ] 3.2 `dotnet build -c Debug`, `dotnet test -c Debug --no-build` at the solution and `dotnet format --verify-no-changes` pass.

## Workflow follow-up

- Archive the change in the same pull request (`/opsx:archive`) and verify that `openspec validate --all --strict` passes.
- Open the pull request with Why, Benefit and What changed, saying it closes #344.
