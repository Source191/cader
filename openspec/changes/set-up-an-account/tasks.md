# Tasks

## 1. Facts (owner spike, before any adapter)

- [ ] 1.1 **Spike (owner, recorded):** in a scratch account folder (never the real default one), with a logged-in second account, check each fact in design decision 1: rules and skills read through a junction; the two import lines load and whether they ask for approval; what `/memory` opens; that `Directory.Delete(junction, recursive: false)` leaves the target whole; whether Claude Code rewrites a linked skill or rule; and whether Windows Terminal 1.25 loads two JSON files from one fragment folder (design decision 8). Record the facts with dates and versions in `.claude\rules\claude-code.md` and `.claude\rules\windows.md`. If junctions are not followed, apply decision 1's fallback to the tasks below before 3.1. **Verify:** the rule lines are present.

## 2. Domain rules

- [ ] 2.1 **Red:** `ClaudeFolderPathsShould` facts: paths inside the four roots are allowed; `..` escapes, other drives, `.credentials.json`, `.claude.json`, `projects`, `sessions`, `plugins` and the history are refused; a different letter case still matches. **Green:** `Domain\ClaudeFolderPaths`. **Verify:** the facts pass.
- [ ] 2.2 **Red:** `ImportLineShould` facts: the line for each start; adding it to LF, CRLF and empty text keeps the endings; adding twice adds once; removal removes only the exact recorded line and its blank line; a line inside a code fence is not counted as present. **Green:** `Domain\ImportLine`. **Verify:** the facts pass.
- [ ] 2.3 **Red:** `ShareSelectionShould` facts that `all`, `none`, `1 3` and `1,3` parse against a list of three, and `4`, `x` and blank answer the reason. **Green:** `Domain\ShareSelection` with `Validate`. **Verify:** the facts and `TextValuesShould` pass.

## 3. Ports and adapters

- [ ] 3.1 **Red:** `JunctionsIntegrationShould` facts in a temporary folder: `Make` makes a junction that `TryReadTarget` reads back; `RemoveLink` removes it and every file in the target survives; `RemoveLink` on a real folder, or on a junction to another target, answers `NotOurs` and changes nothing. **Green:** `Ports\IJunctions` and `Adapters\Files\Junctions`; `FakeJunctions` in TestDoubles. **Verify:** the facts pass and `LayeringShould` gains `HandTheJunctionsOnlyToAccountSetup`.
- [ ] 3.2 **Red:** `ClaudeFolderFilesIntegrationShould` facts in a temporary folder: list a folder's `CLAUDE.md`, rule files and skill folders (a skill that is a link marked as such); copy a skill folder whole; move one (same volume) and see the source gone; back up files into a timestamped folder; read and write `CLAUDE.md` keeping its bytes outside the edit. **Green:** `Ports\IClaudeFolderFiles` and `Adapters\Files\ClaudeFolderFiles`; `FakeClaudeFolderFiles`. **Verify:** the facts pass.
- [ ] 3.3 **Red:** `JsonFileStoreShould` facts for the setup record (write, read, a missing or corrupt file read as no record). **Green:** the record type and the store's `accounts` folder. **Verify:** the facts pass.

## 4. Planner

- [ ] 4.1 **Red:** `AccountSetupPlannerShould` facts for Clean: a missing folder plans `MakeFolder`, `RegisterAccount` (with `default` first when there is no list), `WriteProfile`, `OfferBell`, `OpenLoginTab`; a second run plans every step as done. **Green:** `AccountSetupPlanner`, `AccountSetupPlan`, `SetupStep`. **Verify:** the facts, `ClosedHierarchiesShould` and `NullFreeModelsShould` pass.
- [ ] 4.2 **Red:** planner facts for Copy and Link to default: only the chosen items; nothing in the default account written; an item already in the new folder kept. **Green:** the planner. **Verify:** the facts pass.
- [ ] 4.3 **Red:** planner facts for Shared folder: `Backup` before the first `Move` or `AddImport` of the default account; each `Move` straight followed by its `MakeLink`; both `CLAUDE.md` choices; a skill that is a link offered to link only; every path accepted by `ClaudeFolderPaths`. **Green:** the planner. **Verify:** the facts pass.
- [ ] 4.4 **Red:** planner facts: an existing non-empty folder with no setup record plans only `RegisterAccount`, `WriteProfile` and `OfferBell`; the same folder with a record (Claude Code's files in it after a login) carries on from the record; a `--root` that is not a configured root is a refusal; the source and `default` are `C:\Users\me\.claude` even when the shell's `ClaudeFolder` came from `CLAUDE_CONFIG_DIR`. **Green:** the planner. **Verify:** the facts pass.
- [ ] 4.5 **Red:** planner facts for `--yes`: every item taken, Shared folder's `CLAUDE.md` started empty with the default's text kept, and no restore step planned by `--remove`; Link to default offers the rules as one item. **Green:** the planner. **Verify:** the facts pass.

## 5. Runners and wording

- [ ] 5.1 **Red:** `AccountSetupRunnerShould` facts over the fakes and `InMemoryState`: the mode question and its four sentences; the item list and pick; the preview (every step, and the never-shared paragraph); `n` writes nothing; `--yes` without `--mode` is Clean; a failed step stops the run, names what was and was not done, exits 1, and the record lists only what succeeded. **Green:** `AccountSetupRunner`. **Verify:** the facts pass.
- [ ] 5.2 **Red:** `AccountRemovalRunnerShould` facts: only recorded links and lines are removed; a replaced link is left and reported; folders, copies and moved files remain and are named; the restore question appears only for the last shared account and puts the default back after a backup. **Green:** `AccountRemovalRunner`. **Verify:** the facts pass.
- [ ] 5.3 **Red:** `WordingOfAccountSetupShould` facts for every question, mode sentence, preview line, step result and the printed PowerShell line; `SourceConventionsShould.SpellBritish` over them. **Green:** `WordingOfAccountSetup`. **Verify:** the facts and `SentenceLiteralsShould` pass.
- [ ] 5.4 **Red:** `TabScriptsShould` facts for the login tab and the account profile's script (quoted folder, Base64). **Red:** `TerminalFragmentShould` facts (temporary folder) that the account profiles are written to `accounts.json` alone, that writing `cader.json` leaves it untouched, that `WindowsTerminal` still opens tabs without `--profile` when only `accounts.json` exists, and that removal deletes both. **Green:** `TabScripts.Login`, `TerminalProfiles`, `TerminalFragment`, `TerminalSetupRunner`'s remove wording. **Verify:** the facts and `PowerShellQuotingShould` pass.

## 6. Command line and wiring

- [ ] 6.1 **Red:** `CommandLineShould` facts: `setup-account personal --mode link --root D:\personal --yes` reaches the action with those values; `--remove` with `--mode` is a parse error; the root help lists setup-account; `setup-account -h` lists its options. **Green:** the command in `CommandLine.Build` with descriptions, and the `CommandActions` member. **Verify:** the facts pass.
- [ ] 6.2 **Wiring (untested):** `Composition\AccountSetupComposition` and its lambda in `Program.cs`, locating Claude Code and `wt.exe` as `setup-terminal` does. **Verify:** `dotnet build -c Debug`, then `cader setup-account scratch --help` in a local install.

## 7. Docs and release

- [ ] 7.1 **Docs:** README "Several Claude accounts" (the command first, the manual way after); `docs\user-guide.md` (each start with what it touches, edits shared or not, and how to undo; never `Remove-Item -Recurse` a junction); `.claude\rules\security.md` (the allowed roots, junction removal, backups, the record); `CONTRIBUTING.md`'s manual checks gain a setup-account pass; the root `CLAUDE.md` (the command line's command list, and the Security and privacy bullet on deleting folders, which gains junction removal through `IJunctions`) and `openspec\config.yaml`'s context (Cader now writes inside the Claude folders, for `setup-account` only). **Verify:** the guide's table matches design decision 3, and `openspec validate --all` passes.
- [ ] 7.2 Raise `Version` (minor) and add `CHANGELOG.md` lines under Added. **Verify:** `Assert-VersionBumped.ps1` and `Assert-ChangelogUpdated.ps1` against `main`.

## 8. Integration checks

- [ ] 8.1 Run `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes` and `openspec validate set-up-an-account --strict`. **Verify:** each exits 0.
- [ ] 8.2 **Manual (owner):** in a Windows sandbox or a throwaway Windows user (never the owner's real `%USERPROFILE%\.claude`), run each of the four starts, log in through the tab, start a session in Cader as the new account and see shared rules and skills load where expected, then `--remove` each and compare the default folder with its backup. **Verify:** by eye and with `Compare-Object` over the folder listings.

## Workflow follow-up

- Archive the change in the pull request that completes it, so `openspec\specs` describes `main`.
- `accounts-in-the-config-tab` reuses `AccountSetupPlanner` for its dialog.
