# Tasks

## 1. Wording

- [x] 1.1 `WordingOfScreens.KeyColumns`: remove the dead `pair.Length == 1` case (design decision 4), so an odd key list lays out instead of failing; the three help tests keep holding the even layout. Verify with `Skip(1)` or the padding mutated by hand that they fail. Commit.
- [x] 1.2 `WordingOfScreensShould`: exactly one branch reads "1 branch" in the title count (kills `Counted`, lines 111). Commit.
- [x] 1.3 `WordingOfDetailShould`: a branch row and an origin row have no listing (line 67). Commit.
- [x] 1.4 `WordingOfDetailShould`: `NoFolderChanges` for an origin row, a branch with unpushed commits (amber), and a branch with none (plain, the `_` arm), including exactly one unpushed commit for the `> 0` boundary (lines 74 and 75). If the boundary mutant is equivalent, record why instead. Commit.

## 2. Domain and scan

- [x] 2.1 `WorktreeInventoryBranchesShould`: a branch row reports `OwnerExists` and `FolderExists` true (line 247). Commit.
- [x] 2.2 `CommitIdShould`: the exception for a bad id names the text and says 40 or 64 hex digits (line 15). Commit.

## 3. Check

- [x] 3.1 Raise `Version` with `.githubscriptsStep-Version.ps1` and add an Internal entry under Unreleased in `CHANGELOG.md`. Verify `dotnet build -c Debug`, `dotnet test -c Debug --no-build` at the solution and `dotnet format --verify-no-changes` pass.

## Workflow follow-up

- Archive the change in the same pull request (`/opsx:archive`) and verify that `openspec validate` passes.
- Open the pull request with Why, Benefit and What changed, saying it closes #315.
