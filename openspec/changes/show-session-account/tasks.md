# Tasks

## 1. Project rules and recorded facts

- [x] 1.1 Amend `openspec\config.yaml`'s context so the transcript sentence also allows the first `session-owner` line's `organizationUuid` and the CLI surface includes `claude auth status --json`; reword the proposal rule's "multiple Claude accounts, which is a later change" to "multiple Claude accounts beyond naming a session's owner"; verify with `openspec validate show-session-account`
- [x] 1.2 Record in `.claude\rules\claude-code.md`, dated 18 November 2026 with CLI 2.1.327, the `auth status --json` fields (no account id, about 1.2 s) and the `session-owner` line (fields, CLI 2.1.325 on, where it appears, one owner per file); verify by reading it beside the design's Context

## 2. Domain values

- [x] 2.1 `OrganisationIdShould`: GUID text parses through `TryParse` and `From`, other text does not, two spellings differing only in case are equal; then `Domain\OrganisationId`; verify with the Core test suite
- [x] 2.2 `AccountLabelShould`: `Validate` trims, accepts 1 to 8 characters, refuses blank (`Blank`), nine characters (`TooLong`) and a control character (`ControlCharacter`); then `Domain\AccountLabel` and `AccountLabelProblem`; verify with `TextValuesShould` and the Core tests
- [x] 2.3 `AccountLabelsShould`: `NextFree` answers 1 for none, 1 when only `work` is used, 2 for 1 and 3; then `Domain\AccountLabels`; verify with the Core tests

## 3. Ports and test doubles

- [x] 3.1 Add `Ports\SessionOwner`, `Ports\ClaudeLogin`, `Ports\KnownAccount` and `Ports\IKnownAccounts`, `ITranscripts.OwnerOf` and `IClaudeCli.LoginAsync` with their convention comments; extend `FakeTranscripts`, `FakeClaudeCli` and `InMemoryState` in `test\Source191.Cader.TestDoubles`; verify the build passes with `ClosedHierarchiesShould`, `NullFreeModelsShould` and `PortParametersShould` green

## 4. Adapters

- [x] 4.1 `ClaudeTranscriptsShould` (temporary folder as the Claude folder): an owner line's `organizationUuid` is `Recorded`; no line, an unreadable id and no transcript are `NotRecorded`; a line after many messages is still found; then `ClaudeTranscripts.OwnerOf`; verify with the Infrastructure tests
- [x] 4.2 `ClaudeTranscriptsShould`: a `Recorded` owner is not read again after the file is deleted; a `NotRecorded` transcript is read again only after it changes; then the adapter's cache; verify with the Infrastructure tests
- [x] 4.3 `ClaudeCliShould` with a stub `IProcessRunner`: logged-in JSON is `LoggedIn`; `loggedIn` false, a missing email or `orgId`, non-JSON and a non-zero exit are `LoggedOut` or `NotRead` as the design says; the arguments are `auth status --json`; then `ClaudeCli.LoginAsync`; verify with the Infrastructure tests
- [x] 4.4 `JsonFileStoreShould` (temporary folder): accounts written are read back; a missing, null-holding or repeated-key `accounts.json` reads as none; then `IKnownAccounts` on `JsonFileStore` with `accounts.json` and its file record; verify with the Infrastructure tests
- [x] 4.5 `ChildCommandWordsShould`: `claude auth status --json` is named whole; then add `auth` and `status` to `ChildCommandWords.Claude`; verify with the Core tests

## 5. Account catalogue and rows

- [x] 5.1 `AccountCatalogueShould`: `Find` answers `Unrecorded`, `Unseen`, and `Known` with `LoggedInNow` true or false from loaded accounts and a login; then `Application\AccountCatalogue` with `LoadAsync` and `Find`; verify with the Core tests
- [x] 5.2 `AccountCatalogueShould`: `RefreshStaleAsync` remembers a new login as the next free number and writes the store, updates a changed email and keeps the label, does nothing within a minute (`FakeTimeProvider`), runs one login at a time, and leaves no current login after `LoggedOut` or `NotRead`; then the refresh; verify with the Core tests
- [x] 5.3 `AccountCatalogueShould`: `RenameAsync` writes the new label and answers changed, and answers unchanged for the same label; then the rename; verify with the Core tests
- [x] 5.4 `SessionRowEnricherShould`: each row carries the `RowAccount` for its transcript's owner, and a slow login never delays `EnrichAsync`; then `SessionRow.Account` and the enricher's calls; verify with the Core tests

## 6. Sessions column and pane

- [x] 6.1 `WordingOfSessionsShould` (or the existing column wording facts): `AccountCell` reads `● work`, `personal`, `?` and blank; `SessionsColumns` holds ACCOUNT between PR and AGE with help naming ●, ? and the Config tab; then the `Wording` members; verify with the Core tests and `KeyHelpParityShould`
- [x] 6.2 Add the ACCOUNT column (`ColumnFit.Natural`) to `SessionsScreenDefinition`; verify by building and by the help and tooltip scenarios in the session-listing delta through the existing screen facts
- [x] 6.3 `WordingOfDetailShould`: the Account line for the current login, another account (naming the email to log in as), an unseen owner and no recorded owner, and six rows with Account at the foot of the left column; then `SessionFactRows = 6` and the fact; verify with the Core tests

## 7. Config tab legend and rename

- [x] 7.1 `ConfigRowsShould`: no accounts means no Accounts heading; two accounts give the heading then rows in number-aware label order after the setup; counts stay 8 settings and the title counts 15 rows; then `ConfigRow.Heading`'s section, `ConfigRow.Account` and `ConfigRows.For`; verify with the Core tests
- [x] 7.2 `ConfigViewShould`: Down from the last setup row steps over the Accounts heading; a filter by `northwind` finds account rows by label, email or organisation and hides the heading; c and f on an account row say there is no path; verify with the Core tests
- [x] 7.3 `WordingOfConfigShould`: an account row's SETTING, VALUE (with logged in now) and tooltip description; then the `Wording` members; verify with the Core tests
- [x] 7.4 `RenameAccountHandlerShould`: `RenameAccount` answers `Renamed(old, new)` and writes `InMemoryState`, or `Unchanged`; then `RenameAccountHandler`, `RenameAccountOutcome`, its line in `Composition.Handlers`, its words in `WordingOfOutcomes` and its tone in `Wording.ToneOf` with a line in `StatusTonesShould`; verify with `EffectDispatcherShould` and `HandlersShould`
- [x] 7.5 `ConfigViewShould`: Enter on an account row raises `AskForAccountLabel`; `AccountLabelConfirmed` with a good label raises `RenameAccount`; blank, too long, control character and taken (ignoring case) give a status message naming the problem; `AccountLabelCancelled` says nothing changed; then the reducer cases and their `StatusMessage` words; verify with the Core tests
- [x] 7.6 Show `AskForAccountLabel` with `NameDialog` under `DialogName.AccountLabel` in `DialogEffects`, reloading the Config rows after the outcome as a settings save does; verify with `DialogEffectsShould` and `ScreenLoopShould`

## 8. Privacy, wiring and contract

- [x] 8.1 `DiagnosticsPrivacyShould`: a rename and a failed login run with the log on leave no email, label, organisation name or id in the log or a problem report; verify with the Core tests
- [x] 8.2 Wire `AccountCatalogue`, `IKnownAccounts` and its `LoadAsync` in `ConsoleComposition`, and the same parts in `tools\ReadmeScene`'s `SceneHost` with made-up Northwind accounts in `scene.json`; verify with `dotnet build -c Debug` and `tools\ReadmeScene.Tests`
- [ ] 8.3 `ClaudeContractShould` fact (`Category=ClaudeContract`): `auth status --json` has a boolean `loggedIn`, and when `email` and `orgId` are present the contract session's transcript has a `session-owner` line whose `organizationUuid` matches `orgId`; verify locally with `CADER_CLAUDE_CONTRACT=1` as `checks.md` says, and record which branch CI took

## 9. Documentation and release

- [ ] 9.1 Describe the ACCOUNT column, the pane's Account line and the Config tab's Accounts rows and rename in `docs\user-guide.md`, with the shared-organisation limit; verify by reading it beside the specs
- [ ] 9.2 Add the `CHANGELOG.md` entry and raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` as CONTRIBUTING's Versions says; verify with `Assert-VersionBumped.ps1`
- [ ] 9.3 Remake the README images with `tools\New-ReadmeScreenshot.ps1` if the new column changes `sessions.png`; verify the image shows the made-up accounts only
- [ ] 9.4 Run `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes` and `openspec validate show-session-account`; verify all pass

## Workflow follow-up

- Archive the change in the pull request that completes it.
- After the first contract run, record in `claude-code.md` whether `auth status` gives `email` and `orgId` under `CLAUDE_CODE_OAUTH_TOKEN`.
