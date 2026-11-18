# Proposal

## Why

Someone who uses two Claude accounts (for example a work one and a personal one) in the same Claude folder cannot tell from Cader which account a session was started under. Resuming, renaming or adopting it under the wrong login fails or continues under the wrong account, and they only find out afterwards. Claude Code now records each session's owner in its transcript (CLI 2.1.325 and later), so Cader can say which account to log in as before the user acts.

## What Changes

- Cader reads each session's owner from the first `session-owner` line of its transcript: its `organizationUuid`, nothing else. A transcript without that line, from an older CLI or not yet written, has no recorded owner.
- Cader reads the current login with the documented `claude auth status --json` (`email`, `orgId`, `orgName`) once at start and again at most once a minute, off the screen thread, so no row ever waits for it.
- Cader remembers each account it sees logged in, keyed by organisation, with its email, organisation name and a short label, in a new state file `accounts.json` under `%LOCALAPPDATA%\Cader`. A new account's label is the next free number (`1`, `2`, …) in the order Cader first sees each one.
- The Sessions tab gains a narrow ACCOUNT column showing the owner's label, with `●` before it when the owner is the current login. It is blank when no owner is recorded, and shows `?` when the owner has not been seen logged in yet.
- The Sessions detail pane gains an Account fact: label, email and organisation, and either that it is the current login or which account to log in as. The pane grows from five rows to six.
- The Config tab gains an Accounts heading and one row per remembered account (label in SETTING; email, organisation and whether it is logged in now in VALUE). This is the legend for the column, and Enter on a row renames its label in a dialog (1 to 8 characters, no two accounts sharing one).
- `openspec\config.yaml`'s context is amended. Cader may also read the first `session-owner` line's `organizationUuid`, and it depends on `claude auth status --json`. The proposal rule calling multiple accounts "a later change" is reworded, since this change is that one in part.
- Nothing about accounts reaches the diagnostic log, the crash file or a problem report: no email, label or organisation.

Out of scope:

- Switching account for the user. Cader names the account; the user runs `/login` themselves.
- Telling apart two seats in one Team or Enterprise organisation. They share an organisation, so they share one label; `claude auth status` gives no account id to separate them.
- Forgetting or merging a remembered account from the console (`accounts.json` can be edited by hand).
- Accounts kept apart by `CLAUDE_CONFIG_DIR`, which Cader already follows one folder at a time, and any other multiple-account support beyond naming a session's owner, which stays a later change.
- Blocking or warning on resume, rename or adopt under another account.

## Capabilities

### New Capabilities

- `session-accounts`: whose account a session belongs to, how Cader learns the current login and remembers the accounts it has seen, their labels and renaming them, and the privacy of what it remembers.

### Modified Capabilities

- `session-listing`: the Sessions table gains the ACCOUNT column, named in the help and its header's tooltip.
- `detail-pane`: the session pane gains the Account fact, in two columns of six rows.
- `config-tab`: the Accounts heading and account rows after the setup, Enter on one renames its label, and neither is counted as a setting.

## Impact

- **Ports:** `ITranscripts` gains the owner of a session; `IClaudeCli` gains the current login; a new `IKnownAccounts` port for `accounts.json`.
- **Adapters:** `ClaudeTranscripts` reads the `session-owner` line; `ClaudeCli` runs `claude auth status --json`; `JsonFileStore` reads and writes `accounts.json`.
- **Application:** a new account catalogue (like `PullRequestCatalogue`, never making a row wait), `SessionRow` gains its owner, `SessionRowEnricher`, `Wording` (column, pane, Config rows, dialog, problems), `ConfigRows` and `ConfigView`, a rename effect and its handler.
- **Tui:** the ACCOUNT column in `SessionsScreenDefinition`, and the label dialog in `DialogEffects` through the existing `NameDialog`.
- **Composition:** wiring in `ConsoleComposition` and `tools\ReadmeScene`'s `SceneHost`, with `scene.json` given made-up accounts.
- **Checks:** a Claude contract fact for `auth status --json` and the `session-owner` line; `claude-code.md` records both facts with date and version.
- **Docs and release:** `docs\user-guide.md`, `CHANGELOG.md`, and a `Version` raise in `src\Source191.Cader\Source191.Cader.csproj`.
