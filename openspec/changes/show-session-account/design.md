# Design

## Context

See proposal.md for why. What the code has today:

- `ClaudeTranscripts` (`ITranscripts.HasMessages`) finds `<Claude folder>\projects\*\<sessionId>.jsonl` and reads each line's top-level `type` until it finds a user or assistant line. `openspec\config.yaml` forbids reading anything more.
- `ClaudeCli` runs `claude` through `IProcessRunner` for the documented commands. `ChildCommandWords.Claude` lists the arguments diagnostics may name.
- `SessionRowEnricher` builds `SessionRow`s every five seconds. `PullRequestCatalogue` is the model for a slow fact that never makes a row wait: `Find` answers from what has landed, `RefreshStaleAsync` is started and never awaited.
- `JsonFileStore` owns every state file under `%LOCALAPPDATA%\Cader` (`config.json`, `update-check.json`, `last-run.json`, `tabs\`, …), and reads a file with a null, a missing required value or a repeated key as corrupt.
- The Config tab is `ConfigRows.For(settings, facts)`: the settings, `ConfigRow.Heading` (Setup), then the setup rows. `ConfigView` steps over the heading and hides it under a filter. Enter on a row raises `EditSetting`, a toggle or advice.
- Session rename goes `AskForNewName` (an `IDialogEffect`) → `NameDialog.Show` → `RenameConfirmed`/`RenameCancelled` → the reducer validates with `SessionName.Validate` → `RenameSession` → its handler.
- The session pane is `Wording.Detail(SessionDetail, …)` in two columns of `Wording.SessionFactRows` (5) rows.

Verified on 18 November 2026 with CLI 2.1.327, without printing any value:

- `claude auth status --json` (about 1.2 s) answers `loggedIn`, `authMethod`, `apiProvider`, `email`, `orgId`, `orgName`, `subscriptionType`, `planTier`, `configDirectory` and others. It has no account id.
- From CLI 2.1.325 on, transcripts carry `{"type":"session-owner","accountUuid":…,"organizationUuid":…,"sessionId":…}`, usually on line 4 to 7, repeated through the file. In 200 recent transcripts, 75 had it; no file had two owners; two accounts appeared. The latest first owner line seen was at line 1,506.

## Goals / Non-Goals

**Goals:**

- One documented CLI call and one extra transcript line are the only new reads of Claude Code.
- The Sessions refresh does no extra waiting: the login is read in the background, and an owner once found is never read again.
- The rename reuses the session rename's dialog and flow, so no new dialog code is written.

**Non-Goals:**

- No change to `HasMessages`, or to what adopt, rename, Enter and resume check.
- No account in the problem report's text, the diagnostic events or `DiagnosticEvent`'s cases.

## Decisions

### Match on the organisation, from `claude auth status`

The current login comes from `claude auth status --json`, matched to the transcript's `organizationUuid` through its `orgId`. The alternative was reading `oauthAccount.accountUuid` from `<Claude folder>\.claude.json` and matching `accountUuid` exactly. That separates two seats in one organisation, but it relies on an undocumented file whose keys can differ only in case (it holds both `C:/…` and `c:/…` project keys), which System.Text.Json reads as distinct but `ConvertFrom-Json` refuses. The documented command is the safer dependency. The same-organisation limit is accepted and stated in the proposal, to be revisited if users report it.

Only `organizationUuid` is read from the transcript. `accountUuid` would add nothing that `auth status` can be matched against.

### Values and results

- `Domain\OrganisationId`: an `ITextValue` of a GUID's text, compared ordinally ignoring case. The id of an account, matched between the two sources.
- `Domain\AccountLabel`: an `ITextValue` with `Validate` returning `AccountLabelProblem` (`Blank`, `TooLong`, `ControlCharacter`), trimmed, at most 8 characters. Uniqueness depends on the other accounts, so it is checked in the reducer, which adds `Taken`.
- `Domain\AccountLabels.NextFree(IEnumerable<AccountLabel>)`: the lowest whole number from 1 not in use.
- `Ports\SessionOwner`: `Recorded(OrganisationId)` or `NotRecorded`, from `ITranscripts.OwnerOf(SessionId)`.
- `Ports\ClaudeLogin`: `LoggedIn(OrganisationId, string Email, string OrganisationName)`, `LoggedOut` or `NotRead`, from `IClaudeCli.LoginAsync`, which never throws (like `VersionAsync`). An answer with `loggedIn` true but no email or no `orgId` that parses (an API key or a long-lived token may give one) is `LoggedOut`.
- `Ports\KnownAccount(OrganisationId, string Email, string OrganisationName, AccountLabel Label)` and `IKnownAccounts` (`ReadAccountsAsync`, `WriteAccountsAsync`). Email and organisation name are display text Cader never compares or passes to another program, so they stay strings.
- `Application\RowAccount` on `SessionRow` (an `init` property like `PullRequest`, defaulting to `Unrecorded`): `Unrecorded`, `Unseen` or `Known(KnownAccount Account, bool LoggedInNow)`.

### Reading the owner line

`ClaudeTranscripts.OwnerOf` reads lines until the first whose top-level `type` is `session-owner`, and takes its `organizationUuid` through `OrganisationId.TryParse`. No other field and no other line type is read. The adapter keeps a `Recorded` answer per session for the life of the process. A `NotRecorded` answer is kept with the transcript's length and last write time and read again only when the file changes, so a long old transcript is not rescanned every five seconds. `HasMessages` stays as it is; the two reads share the file lookup but not the line loop, so each keeps its own simple rule.

### The account catalogue

`Application\AccountCatalogue(IClaudeCli, IKnownAccounts, TimeProvider)` holds the remembered accounts and the current login in a `SharedSnapshot`. `LoadAsync` reads `accounts.json` once at start. `Find(SessionOwner)` answers a `RowAccount` from the snapshot. `RefreshStaleAsync` runs `LoginAsync` when the last run started a minute or more ago and none is running; when the login is a new organisation, or a remembered one whose email or name changed, it updates the snapshot and writes `accounts.json`. `SessionRowEnricher` calls `Find` for each row and starts `RefreshStaleAsync` without awaiting it, as it does for pull requests. `RenameAsync(OrganisationId, AccountLabel)` writes the new label and answers whether it changed.

An unreadable `accounts.json` reads as no accounts (the store's existing corrupt-file rule) and is overwritten by the next sighting. The labels in it are lost, which is the same trade `config.json` makes.

### The Config tab

`ConfigRow.Heading` gains which section it heads (`Setup` or `Accounts`) so the existing step-over, filter and counting rules cover both without new code. `ConfigRow.Account(KnownAccount, bool LoggedInNow)` follows. `ConfigRows.For` takes the catalogue's accounts in label order (`NumberAwareOrder`, so `2` sorts before `10`) and adds the heading and rows only when there is at least one. `ConfigRowsLoaded` carries them, so the rename reducer can check `Taken`.

Enter on an account row raises `AskForAccountLabel(KnownAccount)`, an `IDialogEffect`. `DialogEffects` shows it with `NameDialog` (new `DialogName.AccountLabel`), answering `AccountLabelConfirmed` or `AccountLabelCancelled`. `ConfigView` validates the text, and the rows, and raises `RenameAccount(OrganisationId, AccountLabel)` or a `StatusMessage` naming the problem. `RenameAccountHandler` calls `AccountCatalogue.RenameAsync` and answers `RenameAccountOutcome` (`Renamed(old, new)` or `Unchanged`), after which the Config rows are reloaded as after a settings save. c and f already answer `NoPathHere` for a row with no path.

### Words and layout

All text goes in `Wording`: `AccountColumn` and its `ColumnHelp` in `SessionsColumns` between PR and AGE, `AccountCell(RowAccount)` (`● work`, `personal`, `?`, blank), the pane's Account fact, the Config rows' SETTING, VALUE and description, the dialog's title and prompt, the label problems, the outcome, and their tones. `SessionFactRows` becomes 6, with Account at the foot of the left column and nothing beside it. The column fits `Natural`, so its width is its widest cell or header.

### Diagnostics

`ChildCommandWords.Claude` gains `auth` and `status` (`--json` is there already), so the run is counted as `claude auth status --json`. Nothing else is added. `LoginAsync` discards the output once parsed, and no `DiagnosticEvent` case carries an account. `DiagnosticsPrivacyShould` gains a fact that a rename and a login run leave no email, label or organisation in the log.

## Risks / Trade-offs

- [`session-owner` is not documented and may change] → `claude-code.md` records it with date and version, and a missing line only blanks the column. No contract fact reads it: a session started with no prompt writes no transcript, and a prompt would make a model call, so a change to the line shows as blank ACCOUNT cells.
- [CI authenticates with `CLAUDE_CODE_OAUTH_TOKEN`, so `auth status` there may give no email or `orgId`] → The contract fact asserts `loggedIn` is a boolean and that `LoginAsync` reads a login exactly when `auth status` names an email and an organisation id, so it holds either way. See open questions.
- [Two seats in one organisation share a label] → Stated in the proposal; a later change could read `accountUuid` if anyone needs it.
- [`accounts.json` holds emails on disc] → It is in the user's own `%LOCALAPPDATA%`, beside `.claude.json`, which already holds them, and it is never attached to a problem report or logged.
- [A transcript with no owner line is reread whenever it changes] → Only while it is being written; a new CLI writes the line within its first lines, so the scan stops early.

## Migration Plan

None: `accounts.json` starts empty and fills as logins are seen. Rolling back leaves an unread file behind.

## Open Questions

- Does `claude auth status --json` give `email` and `orgId` under `CLAUDE_CODE_OAUTH_TOKEN` in the contract workflow? The first contract run answers it, and the answer only changes which branch the contract fact asserts.
