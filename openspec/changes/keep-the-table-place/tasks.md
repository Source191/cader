# Tasks

**Implement with:** Claude Opus 5.5 (`claude-opus-5-5`) at medium effort.

## 1. The table keeps its place

- [ ] 1.1 Red: `TableScrollShould` (Core.Tests, Tui, a real `TableView` 12 rows high) states that showing the same selection again keeps the row offset, also after a wheel scroll; that a new selection scrolls just far enough to stay on screen, down and up; that a changed filter scrolls to the selection; and that fewer rows scroll back only as far as the last row needs.
- [ ] 1.2 Green: add `Tui\TableScroll`, which keeps the row offset across a source swap and calls `EnsureCursorIsVisible` only when the selection or filter changed.
- [ ] 1.3 `TableBody.Redraw` shows its lines through `TableScroll`, then puts the column offset back as before. Untested wiring under the Tui rule; verify with `dotnet build -c Debug` and the full suite.

## 2. Rules, release

- [ ] 2.1 `.claude\rules\terminal-gui.md` records, dated 19 November 2026 with 2.5.0, that setting `TableView.Table` selects the first cell and scrolls to it, that `SetSelection` never scrolls, and that the row offset survives a swap only when put back.
- [ ] 2.2 Raise `Version` with `.github\scripts\Step-Version.ps1` and add a Fixed entry under Unreleased in `CHANGELOG.md`. Verify `dotnet build -c Debug`, `dotnet test -c Debug --no-build` at the solution and `dotnet format --verify-no-changes` pass.

## Workflow follow-up

- Archive the change in the same pull request and verify that `openspec validate` passes.
