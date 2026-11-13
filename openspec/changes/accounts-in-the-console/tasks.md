# Tasks

## 0. Rebase the deltas onto the archived specs

- [ ] 0.1 Once `honour-claude-config-folder` is archived on `main`, turn this change's ADDED requirements that now overlap its requirements into MODIFIED ones against the archived text, so the main specs never hold two requirements that disagree: config-tab's "The setup names the Claude folder" (replaced by the account rows with several accounts), session-attach's "Sessions with nothing to resume are refused" (the row's account's folder) and needs-input-notification's "Setup can turn on Claude Code's terminal bell" (each account). **Verify:** `openspec validate accounts-in-the-console --strict` passes and each MODIFIED header matches `openspec\specs` exactly.

## 1. Facts (owner spike, before any port change)

- [ ] 1.1 **Spike (owner, recorded):** with a second logged-in account in `%USERPROFILE%\.claude-personal` (made by hand as Anthropic documents, never by a test), check each fact in design decision 1, re-read the agent view docs' "Where state is stored" section for its exact words, and measure each listing's time and peak memory with one and two supervisors running. Record everything with the date and CLI version in `.claude\rules\claude-code.md`. If the listing is not scoped to the folder, update design decision 1's fallback before 3.1. **Verify:** the rule lines are present.

## 2. Domain rules

- [ ] 2.1 **Red:** `AccountNameShould` facts: `work`, `personal-2` parse; blank, `my personal`, `2nd`, a 21-character name and non-ASCII letters do not; `Validate` names the reason; comparison ignores case. **Green:** `Domain\AccountName` as an `ITextValue<AccountName>`. **Verify:** the facts, `TextValuesShould` and `SealedTypesShould` pass.
- [ ] 2.2 **Red:** `ClaudeAccountShould` facts that, with several accounts, the account at `C:\Users\me\.claude` answers `Removed(CLAUDE_CONFIG_DIR)` and one at `C:\Users\me\.claude-personal` answers `Set` with that folder, whatever the spelling's case; and that the single account of a missing list answers no change at all. **Green:** `Domain\ClaudeAccount` and `Domain\ChildVariable` (`Set`, `Removed`). **Verify:** the facts, `ClosedHierarchiesShould` and `NullFreeModelsShould` pass.
- [ ] 2.3 **Red:** `AccountsShould` facts: a missing list gives one account from a `ClaudeFolder`; a repeated name or folder and a bad name are left out with an `AccountProblem`; `HasSeveral`; `ForRepository` applies override, then root, then first, and treats an unknown name as absent with its problem. **Green:** `Domain\Accounts`. **Verify:** the facts pass.

## 3. Settings and ports

- [ ] 3.1 **Red:** `SettingsFileShould` facts that `accounts`, `rootAccounts` and `repositoryAccounts` read, that `7` or a malformed entry reads as absent without touching `repositoryRoots`, and that `ToConfig` writes all three back unchanged. **Green:** the three `UserConfig` members (nullable, with `Kept` entries in `NullFreeModelsShould`: "config.json may leave the property out"), `SettingsFile`, `CurrentSettings.Accounts`. **Verify:** `SettingsFileShould`, `JsonFileStoreShould` and `NullFreeModelsShould` pass.
- [ ] 3.2 **Red:** `ProcessRunnerIntegrationShould` facts that a `Set` reaches the child even when the test process has another value, that a `Removed` variable is absent in the child, and that no change passes nothing extra. **Green:** `ProcessRequest.ChildEnvironment` (or `Variables` extended, if `fetch-before-branching` has landed), applied after the `InheritedEnvironment` drop; `FakeProcessRunner` records it. **Verify:** the facts, `InheritedEnvironmentShould` and `FakeProcessRunnerShould` pass.
- [ ] 3.3 **Red:** `ClaudeCliShould` facts over `FakeProcessRunner` that each method runs with the account's `ChildEnvironment`, and that `ClaudeAgentsListing` stamps each row with the account it was given. **Green:** `IClaudeCli` methods take a `ClaudeAccount`; `Session.Background` and `Session.Interactive` gain `Account`; `FakeClaudeCli` answers per account and records the account of every call. **Verify:** `ClaudeCliShould`, `ClaudeAgentsListingShould`, `PortParametersShould` and the build pass.
- [ ] 3.4 **Red:** `ClaudeTranscriptsShould` and `ClaudeSettingsShould` facts (temporary folder) that each reads the account's folder handed to the method. **Green:** `ITranscripts` and `IClaudeSettings` take the account; their fakes record it. **Verify:** the facts pass.

## 4. Listing and rows

- [ ] 4.1 **Red:** `ConsoleLoadsShould` facts: two accounts are listed in turn and merged; one account's failure keeps its previous rows and answers a banner naming it; one account behaves exactly as before. **Green:** the per-account listing in `ConsoleLoads`, row identity `(AccountName, JobId)` in the Sessions view. **Verify:** the facts, `SessionsViewShould` (selection kept across a refresh with a shared job id) and `HeartbeatShould` pass.
- [ ] 4.2 **Red:** `WordingShould` facts for the per-account banner. **Green:** `Wording`. **Verify:** the facts and `SentenceLiteralsShould` pass.

## 5. Tab scripts and session handlers

- [ ] 5.1 **Red:** `TabScriptsShould` facts that every script that runs claude (attach, resume, trust, login-free plain tab, `cader here`'s none) begins with the account line, quoted (a folder holding `'` and U+2019), and `Remove-Item` for the default folder's account. **Red:** `PowerShellQuotingShould` fact over real `pwsh` that the decoded script sets the variable to the folder exactly. **Green:** `TabScripts`. **Verify:** both pass.
Each of 5.2 to 5.8 is **Red:** a `...HandlerShould` fact that a personal row's key calls `FakeClaudeCli` and `ITranscripts` as personal and opens any tab with personal's account line; **Green:** the handler passes the session's account through; **Verify:** that test class, `EffectDispatcherShould` and `HandlersShould` pass.

- [ ] 5.2 `OpenTabHandler` (Enter: attach, or resume a stopped session, with the transcript check).
- [ ] 5.3 `StopSessionHandler`.
- [ ] 5.4 `DeleteSessionHandler` (`claude rm`, and the worktree offer unchanged).
- [ ] 5.5 `RenameSessionHandler` (`claude rm` then the resume, both as the account).
- [ ] 5.6 `AdoptSessionHandler` (the resume under the supervisor as the account).
- [ ] 5.7 `ResumeSessionsHandler` (resume-all across two accounts, each tab as its own).
- [ ] 5.8 `CloseEmptyTerminalsHandler` (the transcript check in each row's account).

## 6. Launch and here

- [ ] 6.1 **Red:** `LaunchPlanShould` and `LaunchServiceShould` facts that a plan carries the account `Accounts.ForRepository` gives for the repository (override, root, first; a worktree under the worktree root follows its repository), and that the start, the wait for the listing and the tab all use it. **Green:** `LaunchRequest`, `LaunchPlan`, `LaunchPlanBuilder`, `LaunchService`, `StartSessionHandler`. **Verify:** the facts pass.
- [ ] 6.2 **Red:** `LaunchServiceShould` and `WordingOfOutcomesShould` facts that an untrusted start with several accounts names the account and the trust tab runs as it. **Green:** the launch problem carries the account; `Wording`. **Verify:** the facts pass.
- [ ] 6.3 **Red:** `HereCommandShould` facts: the shell's variable wins and is printed with several accounts; without it the repository's account is used; one account prints nothing new. **Green:** `HereCommand`, `HereCommandRunner`. **Verify:** the facts pass.
- [ ] 6.4 **Red:** `WizardViewShould` facts that the wizard state carries the repository's account and shows the Account line only with several accounts. **Green:** `WizardState`, `WizardView`, `Wording`. **Green (Tui binder, untested):** the Account label in `WizardControls`. **Verify:** the facts, `ReducersShould`, and the wizard at 120 x 36 in the headless terminal.

## 7. Columns, pane, Config tab and setup-terminal

- [ ] 7.1 **Red:** `SessionsScreenDefinitionShould` and `WorktreesScreenDefinitionShould` facts that ACCOUNT is present only with several accounts, after REPOSITORY, with its fit; `ColumnHelp` lines; the filter matches the account. **Green:** the definitions and `Wording.SessionsColumns`/`WorktreesColumns`. **Verify:** the facts, `KeyHelpParityShould` and the long-name facts pass.
- [ ] 7.2 **Red:** `DetailLoaderShould` and `WordingOfDetailShould` facts for the Account line of a session and of a worktree (repository, root, first), and `DetailLayoutShould` that it fits. **Green:** `Detail` and `WordingOfDetail`. **Verify:** the facts pass.
- [ ] 7.3 **Red:** `ConfigRowsShould` and `WordingOfConfigShould` facts for the account rows, the root rows' account, overrides, left-out entries and unknown names. **Green:** `ConfigRows`, `WordingOfConfig`. **Verify:** the facts pass.
- [ ] 7.4 **Red:** `TerminalSetupRunnerShould` facts that with two accounts the bell is offered for each in turn, an account already on is skipped with its words, and one unreadable file does not stop the other. **Green:** `TerminalSetupRunner` over `Accounts`. **Verify:** the facts pass.

## 8. Wiring, checks and docs

- [ ] 8.1 **Wiring (untested):** `SharedParts` builds `Accounts` from `CurrentSettings` and the `ClaudeFolder`; `ConsoleComposition`, `HereComposition`, `TerminalSetupComposition` and `tools\ReadmeScene\SceneHost` pass them. **Verify:** `dotnet build -c Debug` and `tools\ReadmeScene.Tests` pass.
- [ ] 8.2 **Contract tests:** a `Category=ClaudeContract` fact that `claude agents --json --all` with a second throwaway `CLAUDE_CONFIG_DIR` on CI lists none of the first folder's sessions, if the contract workflow's credentials can serve a second folder; otherwise record why in `checks.md`. **Verify:** `claude-contract.yml` passes or the note is present.
- [ ] 8.3 **Docs:** README "Several Claude accounts" (Anthropic's `CLAUDE_CONFIG_DIR` set-up, then the `accounts` entries by hand, and what is per account: login, sessions, supervisor, trust, settings, plugins, MCP servers; Console sign-ins without an API key are not separated), `docs\user-guide.md`, `.claude\rules\console-screen.md` (the columns, pane line, wizard line, Config rows), `.claude\rules\security.md` (the child environment and the account line), `openspec\config.yaml`'s proposal rule reworded to drop "including multiple Claude accounts, which is a later change", and the root `CLAUDE.md` (the Claude Code CLI facts that now hold per account, and the child environment under Security and privacy). **Verify:** `openspec validate --all` still passes.
- [ ] 8.4 Raise `Version` (minor) and add `CHANGELOG.md` lines under Added. **Verify:** `Assert-VersionBumped.ps1` and `Assert-ChangelogUpdated.ps1` against `main`.
- [ ] 8.5 Run `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes`, `openspec validate accounts-in-the-console --strict`, the hand-off check (`dotnet build src\Source191.Cader -c Release`, then `dotnet run -- check` in `tools\HeadlessTerminal`) and the smoke check against a local install. **Verify:** each exits 0.
- [ ] 8.6 **Manual (owner):** the `CONTRIBUTING.md` manual checks with two accounts: start a personal session in a work root's repository that is overridden, attach, detach, rename, stop, delete, adopt a personal terminal session, and see each act as personal; run Cader from a shell with `CLAUDE_CONFIG_DIR` set to personal and see work's keys still act as work, with `%USERPROFILE%\.claude.json` untouched. **Verify:** by eye, and each account's `claude agents --json` afterwards.

## Workflow follow-up

- Archive the change in the pull request that completes it, so `openspec\specs` describes `main`.
- `set-up-an-account` starts from the archived `claude-accounts` spec.
