# Proposal

## Why

People with a work and a personal Claude account (issue #203) can already run both side by side the way Anthropic documents, with `CLAUDE_CONFIG_DIR`, but each account then has its own background supervisor, session list, transcripts, trust and settings. After `honour-claude-config-folder`, one Cader follows one account; a second account means a second Cader, sharing one `config.json` and one Windows Terminal fragment. The user wants one console that shows every account's sessions and drives each one as its own account, with no surprises: it must always be visible which account a row, a new session or a key acts as.

This change makes Cader understand accounts that already exist. It never creates, moves or edits anything in a Claude folder; `set-up-an-account` adds the command that does.

## What Changes

- **Accounts in `config.json`:** an `accounts` list of `{ "name", "configFolder" }`. A file without it reads as one account, the Claude folder of `honour-claude-config-folder`, and Cader behaves exactly as it does today. Account names are letters, digits and dashes.
- **Which account a new session uses:** a repository's own entry in `repositoryAccounts` first, then its root's entry in `rootAccounts`, then the first account in the list. A worktree follows its repository, wherever its folder sits. Both maps are hand-edited in this change (the Config tab edits them in `accounts-in-the-config-tab`).
- **Which account a row is:** the account whose listing it came from, never guessed from its folder.
- **One listing per account:** `claude agents --json --all` is run once per account, in turn, with that account's folder, and the rows are merged. A listing that fails names its account in the banner and keeps that account's last rows; the others still refresh.
- **Every key acts as the row's account:** attach, stop, `rm`, rename, adopt, resume, resume-all, close-empty, open-in-tab, the trust tab and new session (`n`, Enter on a worktree) give the child and the tab script that account explicitly. For the account whose folder is `%USERPROFILE%\.claude`, Cader removes `CLAUDE_CONFIG_DIR` rather than setting it, because setting it to the default folder would also move `.claude.json` (trust and MCP servers) into it.
- **An ACCOUNT column** on Sessions and Worktrees, an Account line in the detail pane and the wizard, and the account in the filter, all shown only when more than one account is configured.
- **Untrusted workspaces name the account:** trust is per account, so the wizard's "not trusted" offer says which account must trust the folder and opens the trust tab as that account.
- **`cader here`** uses the account a `claude` typed in the same shell would use: the shell's `CLAUDE_CONFIG_DIR` when set, otherwise the repository's account; with several accounts it prints which.
- **`cader setup-terminal`** offers the Claude bell in each account's `settings.json`, one question each.
- **Config tab:** with several accounts the Setup section lists each account and its folder, the root rows name their account, and any repository override is shown; an account name the file uses but does not define is flagged.
- **Facts first:** the open questions in issue #203 (is the listing scoped to the folder, do `attach`, `stop`, `rm` and `--bg --resume` need the variable, where interactive sessions and transcripts register, whether listing an account starts its supervisor, and the cost of two listings) are verified and recorded before the ports change.

## Capabilities

### New Capabilities

- `claude-accounts`: what an account is, where accounts are configured, which account a folder and a row belong to, and how a child program is given its account.

### Modified Capabilities

- `session-listing`: one listing per account, merged; a failed listing names its account; the ACCOUNT column and filter.
- `session-attach`: every session key acts as the row's account.
- `session-launch`: a new session's account, shown in the wizard; untrusted workspaces per account; `cader here`'s account.
- `worktree-management`: the ACCOUNT column on Worktrees.
- `detail-pane`: the Account line.
- `needs-input-notification`: setup offers the bell in every account.
- `config-tab`: the accounts and their roots shown in the Config tab.
- `settings`: `accounts`, `rootAccounts` and `repositoryAccounts` in `config.json`.

## Impact

- **Code:**
  - Core Domain: `AccountName` (text value), `ClaudeAccount`, `Accounts` (the configured list and the resolution rule), `AccountFor` the folder rule; `Session.Background` and `Session.Interactive` carry their account.
  - Core Ports: `IClaudeCli` and `ITranscripts` and `IClaudeSettings` take a `ClaudeAccount`; `ProcessRequest` gains a child environment (set or removed variables); `UserConfig` gains the three properties.
  - Core Application: `CurrentSettings`, `SettingsFile`, the listing refresh (`ConsoleLoads`), `TabScripts` (the account line at the head of every script), `LaunchService`, `LaunchPlanBuilder`, `HereCommand`, every session handler, the wizard state, the `Sessions` and `Worktrees` definitions, `Detail`, `ConfigRows`, `TerminalSetupRunner`, `Wording` and its partials.
  - Infrastructure: `ClaudeCli`, `ClaudeTranscripts`, `ClaudeSettings`, `ProcessRunner`.
  - Tui: the wizard's Account line; the columns are data.
  - Tool project wiring: `SharedParts`, `ConsoleComposition`, `HereComposition`, `TerminalSetupComposition`, and `tools\ReadmeScene\SceneHost`.
- **Tests:** `FakeClaudeCli` answers per account; `FakeProcessRunner` records the child environment. Contract tests for the per-account listing when CI can serve a second folder. The hand-off check and the `CONTRIBUTING.md` manual checks, because attach and tabs change.
- **Docs:** README "Several Claude accounts" section (the manual `CLAUDE_CONFIG_DIR` set-up and the `accounts` entries), `docs\user-guide.md`, `.claude\rules\claude-code.md`, `.claude\rules\console-screen.md`, `.claude\rules\security.md` (the child environment), `openspec\config.yaml` (the proposal rule that lists multiple accounts as out of scope), `CHANGELOG.md` with a minor `Version` raise.
- **Overlap:** `fetch-before-branching` adds `ProcessRequest.Variables` for set-only variables; whichever lands second extends or reuses it (design decision 5). `stricter-state-files` changes how `config.json` is read; the new properties are optional and nullable either way. `diagnostic-log-setting` and `honour-claude-config-folder` touch `ConfigRows`.

## Delivery

- **Pull request:** PR 2 of 4 for issue #203, one pull request. A part-way version would show another account's rows with keys that act as the wrong account, which is exactly the surprise to avoid. If its size is a problem for review, the plumbing of task group 3 (the account threaded through the ports with one account, no behaviour change) can go first as its own `skip_specs` change and pull request.
- **Order:** after `honour-claude-config-folder` has merged; before `set-up-an-account`, which writes the `accounts` entries this change reads.
- **Owner step first:** the spike in task group 1 needs a second, logged-in Claude account on the owner's machine.
- **Implementation:** Opus 5.5 at high effort. It changes every session key, the tab scripts, the listing and the hand-off, across all four layers, where a missed account parameter is a silent wrong-account action.

## Out of scope

- Creating, moving, copying or linking anything in a Claude folder, and adding an account by command (`set-up-an-account`).
- Editing accounts, root accounts and repository overrides from the Config tab (`accounts-in-the-config-tab`).
- Choosing a different account for one session in the wizard: a session uses its repository's account.
- Moving a session from one account to another.
- Claude Console sign-ins without an API key, which Claude Code stores outside the config folder and so cannot be kept apart by it.
