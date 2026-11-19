# Tasks

**Implement with:** Claude Opus 5.5 (`claude-opus-5-5`) at medium effort.

## 1. The facts

- [ ] 1.1 `TableScrollShould.KeepTheNewLastRowOnScreenWhenTheSelectedLastRowGoes`; verify it passes, and fails with `EnsureValidScrollOffsets` removed from `TableScroll.Show` by hand (then revert).
- [ ] 1.2 `TableScrollShould.ShowAnEmptyTableFromItsTopWhenEveryRowGoes`; verify it passes.

## 2. Check

- [ ] 2.1 Verify `dotnet build -c Debug`, `dotnet test -c Debug --no-build` at the solution and `dotnet format --verify-no-changes` pass.

## Workflow follow-up

- Archive the change in the same pull request and verify that `openspec validate --all --strict` passes.
