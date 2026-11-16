# Tasks

## 1. The rule

- [x] 1.1 Red: `TabTitleShould` in `test\Source191.Cader.Core.Tests\Domain` states the exact-name fact (design decision 3); add `Domain\TabTitle.Shows` answering false, and verify the fact fails for that reason. Commit.
- [ ] 1.2 Green: `Shows` answers true for an ordinal match. Verify the fact passes. Commit.
- [ ] 1.3 Red then green, one fact at a time: each of ◐ ◓ ◑ ◒ ✳ and a glyph outside the Basic Multilingual Plane before the name; then the refusals (another name, `fix-1234`, `notes on fix-123`, two glyphs, a letter or a digit as the glyph, no space, two spaces, a blank title) (decision 1). Verify each fails before its code and `TabTitleShould` passes after. Commit per step.
- [ ] 1.4 Refactor: `Shows` reads clearly and keeps to the house idioms (pattern matching, `Rune`, no `else`). Verify `dotnet test test\Source191.Cader.Core.Tests` passes. Commit.

## 2. The switch

- [ ] 2.1 `UiAutomationTabs.SelectIn` lists each owned window's tab items with the cached tab-item condition, keeps every element in the call's `ComObjects`, and selects the first whose `CurrentName` `TabTitle.Shows` the title (decision 2). Verify `UiAutomationTabsShould` passes, then temporarily release the cached tab-item condition and verify it fails, as in dispose-tidy, before reverting. Commit.
- [ ] 2.2 Manual: in a Cader built from this branch, with the Claude Code variables cleared (CLAUDE.md), Enter on a working session whose tab is open (title `◐ <name>` or similar) switches to that tab, and Enter on an idle session's open tab still does. Record the result and date in `windows.md`'s UI Automation line.

## 3. Facts and release

- [ ] 3.1 Record in `windows.md` the 16 November 2026 probe (Windows Terminal 1.25.3131.0 kept a tab opened with Cader's arguments titled through OSC 0, OSC 1, OSC 2 and `SetConsoleTitle`), and in `claude-code.md` the titles Claude Code 2.1.325 gives a working session's tab (`◐ <name>`, `◑ <name>`), an idle one (the name alone) and its own tabs (`✳ <topic>`), and that they reach a tab opened with `--suppressApplicationTitle` by a route not yet known (decision 4). Verify both read clearly.
- [ ] 3.2 Raise `Version` with `.github\scripts\Step-Version.ps1` and add a Fixed entry under Unreleased in `CHANGELOG.md` saying Enter switches to a working session's open tab again. Verify `dotnet build -c Debug`, `dotnet test -c Debug --no-build` at the solution and `dotnet format --verify-no-changes` pass.

## Workflow follow-up

- Archive the change in the same pull request (`/opsx:archive`) once 2.2 has passed, and verify that `openspec validate` passes.
- Open the pull request with Why, Benefit and What changed; it also carries the archive of `dispose-tidy`.
