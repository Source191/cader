# Design

## Context

- `ClaudeTranscripts` and `ClaudeSettings` already take a folder in their constructors; only their `ForCurrentUser` factories hard-code `%USERPROFILE%\.claude`. `ConsoleComposition` (transcripts and the setup facts) and `TerminalSetupComposition` (the bell) call those factories.
- `InheritedEnvironment` keeps `CLAUDE_CONFIG_DIR` for every child and Windows Terminal hands Cader's environment to every new tab, so every `claude` Cader starts already uses the folder the variable names. Only Cader's own reads are wrong.
- The Claude Code docs say `CLAUDE_CONFIG_DIR` overrides `~/.claude` and that a settings file must give it as an absolute path; they do not say how the CLI reads a `~` path, a relative path or a trailing separator from the environment.

## Goals / Non-Goals

**Goals:**
- One place decides the Claude folder, read once as Cader starts, the same for the console and every command.
- The user can always see which folder Cader reads.

**Non-Goals:**
- Choosing a folder per session or per repository (`accounts-in-the-console`).
- Watching the variable after start: a process's environment does not change under it.

## Decisions

### 1. `Domain\ClaudeFolder` decides, the tool project reads the variable

- `ClaudeFolder.From(string? variable, FolderPath home)` answers the folder and its source: `Default` (`<home>\.claude`, variable unset or blank), `FromVariable` (the variable as a full path) or `NotUsable` (the default folder, with the text Cader refused, for the Config row).
- It is a Domain rule because it is pure and tested in memory; `SharedParts` reads `Environment.GetEnvironmentVariable("CLAUDE_CONFIG_DIR")` once and hands the result to the composition.
- **Alternatives:** reading the variable inside each adapter repeats the rule twice and hides it from the Config tab; an `IEnvironment` port is more machinery than one value read at start needs.

### 2. Follow the CLI, verified first

Task 1.1 records in `.claude\rules\claude-code.md` how Claude Code 2.1.3xx reads `CLAUDE_CONFIG_DIR` set to a `~\` path, a relative path, a path with a trailing separator, and a folder that does not exist yet (does it create it?). `ClaudeFolder` then accepts what the CLI accepts and reads it the same way. If the CLI expands `~`, so does Cader; a relative path, which the CLI would read against each session's own folder, is `NotUsable` because no one folder holds every session's files.

### 3. A setup row, not a setting

The folder is a fact about how Cader was started, so it is a grey Setup row (like Settings file and Diagnostic log), not something the Config tab edits. It sits directly after Settings file so the two "which files" rows are together.

### 4. Adapters keep their shape

`ClaudeTranscripts(string claudeHome)` and `ClaudeSettings(string)` stay as they are; only the factories go. `accounts-in-the-console` will change the ports to take an account, so nothing here pre-empts that.

## Risks / Trade-offs

- [A Cader started with the variable set, then a tab opened from the "Cader session" profile in a fresh Windows Terminal window] → that tab still inherits from Cader, not from the window, because `wt` is started by Cader with its own environment; the hand-off and manual checks cover it.
- [The CLI treats some form of the variable differently from Cader] → decision 2's verification comes before the rule is coded, and the contract tests can pin it later.
- [Someone relies on Cader reading `~\.claude` while their sessions use another folder] → no sensible set-up does; the changelog names the change under Fixed.
