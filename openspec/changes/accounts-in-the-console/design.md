# Design

## Context

- After `honour-claude-config-folder`, `Domain\ClaudeFolder` is the one Claude folder Cader reads, decided once at start; `ClaudeTranscripts` and `ClaudeSettings` take it. `IClaudeCli` runs `claude` through `IProcessRunner` with whatever environment Cader inherited, minus `InheritedEnvironment`'s drops; tab scripts inherit Cader's environment through `wt.exe`.
- The Claude Code docs (fetched 13 November 2026) say: each `CLAUDE_CONFIG_DIR` folder has its own settings, session history and login; with it set "the supervisor uses that directory instead of `~/.claude` and runs as a separate instance with its own sessions" (agent view, as summarised; the exact words are re-read in task 1.1); `.claude.json` moves inside the folder when the variable is set; Console sign-ins without an API key live in `%APPDATA%\Anthropic` and are shared.
- `Session.Background` and `Session.Interactive` are what every session effect carries, so an account on them reaches every handler without new effect members.
- `CurrentSettings` reads `config.json` per scan, wizard and dialog; `SettingsFile` resolves missing properties to defaults.

## Goals / Non-Goals

**Goals:**
- With no `accounts` list, nothing visible or behavioural changes.
- Every child program and tab script is given its account explicitly, so an inherited `CLAUDE_CONFIG_DIR` can never make a key act as the wrong account.
- The account is visible wherever it decides something (row, worktree, wizard, pane, trust offer, `cader here`).

**Non-Goals:**
- Writing to any Claude folder beyond the bell setting that `setup-terminal` already writes.
- Running the listings in parallel (decision 6).

## Decisions

### 1. Verify the CLI's per-folder behaviour before the ports change

Task group 1 is an owner spike with a second logged-in account in `%USERPROFILE%\.claude-personal`, recorded with date and version in `.claude\rules\claude-code.md`:
- `claude agents --json --all` with each folder lists only that account's sessions, background and interactive.
- `attach`, `stop`, `rm` and `--bg --resume` of a personal job fail or act on nothing without the variable, and work with it.
- Interactive sessions register under `<folder>\sessions\<pid>.json` and transcripts under `<folder>\projects`.
- Whether listing an account whose supervisor is not running starts it (and its memory), and the time and peak memory of each listing.
- Whether `CLAUDE_CONFIG_DIR=%USERPROFILE%\.claude` reads `%USERPROFILE%\.claude\.claude.json` rather than `%USERPROFILE%\.claude.json` (decision 4 rests on the docs saying so).

If the listing is not scoped to the folder, decision 2's merge becomes a de-duplication by session id and the row's account is taken from the folder that registered it; the specs do not change.

### 2. `Domain` owns accounts and the resolution rule

- `AccountName` is an `ITextValue<AccountName>` (`From`, `TryParse`, `Validate` for the dialog in `accounts-in-the-config-tab`): one to twenty ASCII letters, digits and dashes, starting with a letter, compared ignoring case.
- `ClaudeAccount(AccountName Name, FolderPath Folder)` with `IsDefaultFolder(FolderPath home)`.
- `Accounts` is the first-class list: built from `UserConfig` (left-out entries kept as `AccountProblem` values for the Config tab), `Single(ClaudeFolder)` when the list is missing, `HasSeveral`, `Named(AccountName)`, and `ForRepository(FolderPath mainCheckout, FolderPath root)` that applies override, then root, then first.
- With one account its name is never shown, so the single account is named `default` internally and never written to the file by this change.

### 3. `config.json` keeps roots as they are and adds two maps

`repositoryRoots` stays a list of strings, so older Cader versions (which skip unknown properties) still read the file, and a downgrade loses nothing. `rootAccounts` and `repositoryAccounts` are objects from folder to name, read through `FolderPath.TryParse` and `AccountName.TryParse` (`Parsed<T>`), keys compared as `FolderPath`s. A root removed in the roots dialog leaves its `rootAccounts` entry, which then matches nothing and is harmless; the Config dialog change tidies it.

- **Alternative:** roots as objects `{ "folder", "account" }` reads more naturally but is a breaking change to the oldest property in the file.

### 4. The child's environment: set, or remove for the default folder

- `ProcessRequest` gains a `ChildEnvironment` (an immutable list of `Set(name, value)` and `Removed(name)`), applied in `ProcessRunner.StartInfo` after the `InheritedEnvironment` drop. If `fetch-before-branching` has landed `Variables`, this becomes that member's richer form, and its `GIT_*` entries become `Set`s.
- `ClaudeAccount.Environment(home)` answers `Removed("CLAUDE_CONFIG_DIR")` for the account whose folder is `%USERPROFILE%\.claude` and `Set("CLAUDE_CONFIG_DIR", folder)` otherwise. Setting the variable to the default folder would move `.claude.json` (user MCP servers, trust, onboarding) to `%USERPROFILE%\.claude\.claude.json`, so the default account would lose its trust and MCP servers: the worst surprise available.
- `TabScripts` starts every script that runs claude with one account line: `$env:CLAUDE_CONFIG_DIR = <PowerShellScript.Quote(folder)>` or `Remove-Item Env:\CLAUDE_CONFIG_DIR -ErrorAction Ignore`. The value goes through `Quote` and the whole script through Base64 as today (security.md).
- **Alternative:** relying on `wt.exe` inheriting Cader's environment works for one account only.

### 5. Ports take the account, adapters stay stateless

`IClaudeCli.ListSessionsAsync(ClaudeAccount)`, `StopAsync(ClaudeAccount, JobId)`, `RemoveAsync(ClaudeAccount, JobId)`, the start and resume, and `ITranscripts.HasMessages(ClaudeAccount, SessionId)`, `IClaudeSettings` methods taking the account. `ClaudeCli` builds each `ProcessRequest` with the account's `ChildEnvironment`; `ClaudeTranscripts` and `ClaudeSettings` lose their folder field. `VersionAsync` stays account-free (the binary is the same).

- **Alternative:** one `IClaudeCli` per account, built in the composition. It hides the account from the handlers, which is where a wrong one would slip in unseen.

### 6. Listings run in turn, each failure its own

`ConsoleLoads` lists the accounts one after another on the refresh's pool thread, each under the existing child time limit, and merges the rows; a failure becomes a per-account banner fact and that account's previous rows are kept. Sequential keeps the peak at one listing child (about 189 MB) at the cost of a longer refresh; task 1.1 measures it, and a refresh slower than the interval is already handled by the heartbeat. Row identity becomes `(AccountName, JobId)` so selection survives a refresh.

### 7. The row's account comes from the listing; a new session's from its folder

`ClaudeAgentsListing` is given the account it parses for and puts it on each `Session`. The wizard, Enter on a worktree and `n` ask `Accounts.ForRepository` through `WorktreeInspector`'s main checkout and root. `cader here` reads its own `ClaudeFolder` (the shell's variable): when that matches a configured account it is used, when it names an unconfigured folder `cader here` uses it as an unnamed account, and with no variable it asks `Accounts.ForRepository`. That is what a bare `claude` in the same shell would do.

### 8. Visible only when it matters

The ACCOUNT columns, the pane line, the wizard line and the Config rows are data in `Wording` gated on `Accounts.HasSeveral`, so a one-account user sees today's screens exactly. The Sessions column sits after REPOSITORY with its own fit, so long names never hide columns (session-listing spec).

## Risks / Trade-offs

- [A second supervisor and listing double the background memory and the refresh time] → sequential listings, measured in task 1.1, and the README says each account runs its own supervisor.
- [A handler misses the account and acts as the inherited one] → every port method takes it, so a missing one fails to compile; `FakeClaudeCli` and `FakeProcessRunner` record the account and environment and every handler test asserts it.
- [Listing an account starts its supervisor] → recorded in task 1.1; if so, the README says so, and an account with no sessions still costs a supervisor.
- [Trust must be given again per account] → the trust offer names the account (session-launch spec) and the README says each account trusts each repository once.
- [`cader here` and the console pick different accounts for the same folder when the shell has the variable set] → that is deliberate and printed; it matches `claude` in that shell.

## Migration Plan

Nothing to migrate: a `config.json` without the new properties reads as one account. Rolling back to an older Cader ignores the properties.
