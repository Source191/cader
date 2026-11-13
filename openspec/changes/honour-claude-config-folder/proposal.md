# Proposal

## Why

Anthropic's documented way to run a second Claude account is to start `claude` with `CLAUDE_CONFIG_DIR` naming a folder of its own ([Log in with multiple accounts](https://code.claude.com/docs/en/authentication#log-in-with-multiple-accounts)). Cader already passes that variable on to every tab and session (`Domain\InheritedEnvironment`), but two adapters hard-code `%USERPROFILE%\.claude`, so a Cader started with the variable set half-works and fails in ways the user cannot see:

- `ClaudeTranscripts.ForCurrentUser` looks for transcripts in the default folder, so adopt (`b`), rename (`r`), Enter and resume refuse a session of the other account as "never sent a message".
- `ClaudeSettings.ForCurrentUser` makes `cader setup-terminal` and the Config tab's Claude bell row read and write the default account's `settings.json`, not the one the sessions use.

This is the first of the four changes for issue #203 and is a bug fix in its own right: it makes Cader follow the folder Claude Code itself would use, with nothing new to set up.

## What Changes

- Cader works out **the Claude folder** once as it starts, the way Claude Code does: `CLAUDE_CONFIG_DIR` from its own environment when it is set, otherwise `%USERPROFILE%\.claude`. A value that is not a usable full path is never guessed at: Cader uses the default folder and says so on the Config tab.
- Transcript checks (adopt, rename, Enter, resume, resume-all) look under that folder's `projects`.
- `cader setup-terminal` offers the bell in that folder's `settings.json`, and its plan names the file.
- The Config tab's Setup section gains a **Claude folder** row (the folder, and whether it came from `CLAUDE_CONFIG_DIR`), so the user can always see which account's files Cader is reading. The Claude bell row reads the same folder.
- `.claude\rules\claude-code.md` records how Claude Code reads `CLAUDE_CONFIG_DIR` (a `~` path, a relative path, a trailing separator), verified against the CLI.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `session-attach`: the transcript a refusal rests on is looked for in the Claude folder Cader is using.
- `needs-input-notification`: setup offers the bell in the Claude folder's `settings.json`, not always `~\.claude\settings.json`.
- `config-tab`: the Setup section gains a Claude folder row, and the Claude bell row reads that folder.

## Impact

- **Code:**
  - Core Domain: a `ClaudeFolder` value (the folder and where it came from) and its rule for reading the variable.
  - Core Application: `SetupFacts`/`SetupFactsReader` and the Config rows for the new row; `Wording` and `WordingOfConfig`.
  - Infrastructure: `ClaudeTranscripts` and `ClaudeSettings` lose `ForCurrentUser` and take the folder.
  - Tool project wiring: `SharedParts` reads the variable once; `ConsoleComposition` and `TerminalSetupComposition` hand the folder to both adapters.
- **Tests:** `ClaudeFolderShould`, the adapters' tests over a temporary folder, `ConfigRowsShould`, `SetupFactsReaderShould`, `WordingOfConfigShould`, `TerminalSetupRunnerShould`.
- **Docs:** `docs\user-guide.md` (the Claude folder row, and that a Cader started with `CLAUDE_CONFIG_DIR` follows it), `.claude\rules\claude-code.md`, `.claude\rules\console-screen.md`, `CHANGELOG.md` with a patch `Version` raise.
- **Overlap:** `diagnostic-log-setting` removes the Diagnostic log setup row and adds a settings row; this change adds a setup row after Settings file. Whichever lands second rebases its `ConfigRows` edit.

## Delivery

- **Pull request:** PR 1 of 4 for issue #203, on its own branch from `main`; nothing depends on the others.
- **Order:** first. `accounts-in-the-console` builds on the `ClaudeFolder` value and the folder-taking adapters.
- **Implementation:** Sonnet 5.5 at medium effort. It is a small, well-bounded fix through two adapters, one setup row and the wiring, with no screen or key work.

## Out of scope

- Several Claude accounts in one console (`accounts-in-the-console`), `cader setup-account` (`set-up-an-account`) and the Config tab's Accounts section (`accounts-in-the-config-tab`), which are the later changes for issue #203.
- Changing `InheritedEnvironment`: `CLAUDE_CONFIG_DIR` still reaches every child as Cader inherited it.
- Reading or writing anything else under the Claude folder.
