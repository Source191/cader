# Tasks

## 1. Find which process writes the title

- [ ] 1.1 Spike (design decision 1), outside the codebase: with the Claude Code variables cleared, start a throwaway background session in a scratch folder under `%TEMP%`, attach to it in a tab opened with Cader's `wt` arguments and `$env:CLAUDE_CODE_DISABLE_TERMINAL_TITLE = '1'`, give it a prompt that takes a few seconds, and read the tab's name through UI Automation while it works; if the glyph remains, repeat with the variable on the session's start instead. Remove the session with `claude rm`. Record the result, date and CLI version in `claude-code.md`.

## 2. Set the variable where it works

- [ ] 2.1 If the attach is enough: red then green, `TabScriptsShould` states that the attach script sets `CLAUDE_CODE_DISABLE_TERMINAL_TITLE` to `1` before `claude attach`, and `TabScripts.Attach` adds the line (decision 2). Commit per step.
- [ ] 2.2 If the session writes the title: red then green, `ClaudeCliShould` states that the start and resume requests carry `CLAUDE_CODE_DISABLE_TERMINAL_TITLE=1` in `Variables`, and `ClaudeCli` adds it (decision 2). Commit per step. Mark whichever of 2.1 and 2.2 the spike ruled out as not needed, with the reason.
- [ ] 2.3 Hand-off check (CLAUDE.md, the change touches attach): `dotnet build src\Source191.Cader -c Release`, then `dotnet run -- check` from `tools\HeadlessTerminal`. Verify exit 0.
- [ ] 2.4 Manual: in a Cader built from this branch, with the Claude Code variables cleared, Enter on a session, prompt it, and verify the tab keeps the session's name while it works and the bell still rings when it waits. Record the result in `windows.md`'s UI Automation line.

## 3. Release

- [ ] 3.1 Raise `Version` with `.github\scripts\Step-Version.ps1` and add a Changed entry under Unreleased in `CHANGELOG.md`. Verify `dotnet build -c Debug`, `dotnet test -c Debug --no-build` at the solution and `dotnet format --verify-no-changes` pass.

## Workflow follow-up

- Archive the change in the same pull request (`/opsx:archive`) and verify that `openspec validate` passes.
- Open the pull request with Why, Benefit and What changed, linking Claude Code's environment variables page.
