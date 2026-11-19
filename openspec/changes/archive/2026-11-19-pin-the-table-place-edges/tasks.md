# Tasks

**Implement with:** Claude Opus 5.5 (`claude-opus-5-5`) at medium effort.

## 1. The facts

- [x] 1.1 `TableScrollShould.KeepTheNewLastRowOnScreenWhenTheSelectedLastRowGoes`; verify it passes. Removing `EnsureValidScrollOffsets` from `TableScroll.Show` by hand leaves it green, because `View`'s `Viewport` setter keeps the offset within the content as well (checked in a throwaway program: `RowOffset` set to 88 on 35 rows reads 23); record that in `terminal-gui.md`.
- [x] 1.2 `TableScrollShould.ShowAnEmptyTableFromItsTopWhenEveryRowGoes`; verify it passes.

## 2. Check

- [x] 2.1 Verify `dotnet build -c Debug`, `dotnet test -c Debug --no-build` at the solution and `dotnet format --verify-no-changes` pass.

## Workflow follow-up

- Archive the change in the same pull request and verify that `openspec validate --all --strict` passes.
