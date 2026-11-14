# Tasks

## 1. Facts and the domain rule

- [x] 1.1 **Spike (recorded, before any code):** with Claude Code 2.1.3xx, start `claude --version` and `claude agents --json` with `CLAUDE_CONFIG_DIR` set to a scratch folder as a full path, a `~\` path, a relative path, a full path with a trailing `\`, and a folder that does not exist. Note which folder each one reads and whether a missing folder is created. Use a scratch folder under `%TEMP%`, never a real account's folder, and do not log in. Record the facts with the date and version in `.claude\rules\claude-code.md`. **Verify:** the rule lines are present.
- [x] 1.2 **Red:** `ClaudeFolderShould` facts over `C:\Users\me` as home.
  - Unset and blank give `Default` at `C:\Users\me\.claude`.
  - `C:\Users\me\.claude-personal` gives `FromVariable` at that folder; a trailing `\` reads the same folder.
  - A relative value gives `NotUsable` with the default folder and the text refused.
  - The `~` case follows what 1.1 found.

  **Green:** `Domain\ClaudeFolder` and its source cases. **Verify:** the facts pass, with `NullFreeModelsShould`, `ClosedHierarchiesShould` and `SealedTypesShould`.

## 2. Adapters and setup facts

- [x] 2.1 **Red:** `ClaudeTranscriptsShould` fact (temporary folder) that a transcript under `<folder>\projects\<project>\<id>.jsonl` is found for the folder handed in and not for another. **Green:** remove `ClaudeTranscripts.ForCurrentUser`. **Verify:** the fact passes and the solution builds.
- [x] 2.2 **Red:** `ClaudeSettingsShould` fact that `Location` is `<folder>\settings.json` for the folder handed in. **Green:** remove `ClaudeSettings.ForCurrentUser`. **Verify:** the fact passes.
- [x] 2.3 **Red:** `SetupFactsReaderShould` facts that the facts carry the `ClaudeFolder` handed in, and that the bell is read from that folder's settings (`TestSettings`). **Green:** `SetupFacts` gains the folder. **Verify:** the facts pass.

## 3. Config tab

- [x] 3.1 **Red:** `ConfigRowsShould` fact that the Claude folder setup row comes directly after Settings file with a stable key. **Red:** `WordingOfConfigShould` facts for the three sources (the folder alone; the folder, from CLAUDE_CONFIG_DIR; the default folder and that the variable was not a full path and was not used), and the row's tooltip within the description limit. **Green:** the row, `ConfigRows.For`, `WordingOfConfig`. **Verify:** the facts, `SourceConventionsShould.SpellBritish` and `KeyHelpParityShould` pass.

## 4. Setup command

- [x] 4.1 **Red:** `TerminalSetupRunnerShould` fact that with a `ClaudeSettings` over `C:\Users\me\.claude-personal` the bell plan names `C:\Users\me\.claude-personal\settings.json`. **Green:** nothing should be needed in the runner; the fact pins it. **Verify:** the fact passes.

## 5. Wiring and docs

- [x] 5.1 **Wiring (untested):** `SharedParts` reads `CLAUDE_CONFIG_DIR` once into a `ClaudeFolder`; `ConsoleComposition` and `TerminalSetupComposition` hand its folder to `ClaudeTranscripts`, `ClaudeSettings` and `SetupFactsReader`. Update `tools\ReadmeScene\SceneHost` to match. **Verify:** `dotnet build -c Debug` and `tools\ReadmeScene.Tests` pass.
- [x] 5.2 **Docs:** `docs\user-guide.md` (the Claude folder row; a Cader started with `CLAUDE_CONFIG_DIR` reads and writes that account's files), `.claude\rules\console-screen.md` (the new setup row). **Verify:** the guide's Setup rows list matches the screen.
- [x] 5.3 Raise `Version` (patch) and add a `CHANGELOG.md` line under Fixed: Cader now follows `CLAUDE_CONFIG_DIR` for transcripts and the Claude bell. **Verify:** `Assert-VersionBumped.ps1` and `Assert-ChangelogUpdated.ps1` against `main`.

## 6. Integration checks

- [x] 6.1 Run `dotnet build -c Debug`, `dotnet test -c Debug --no-build` at the solution, `dotnet format --verify-no-changes` and `openspec validate honour-claude-config-folder --strict`. **Verify:** each exits 0.
- [ ] 6.2 **Manual (owner):** in a local install started from a shell with `CLAUDE_CONFIG_DIR` set to a second, logged-in account's folder, see the Claude folder row name it, adopt or resume one of that account's sessions, and run `cader setup-terminal` and see the plan name that folder's `settings.json`. **Verify:** by eye.

## Workflow follow-up

- Archive the change in the pull request that completes it, so `openspec\specs` describes `main`.
