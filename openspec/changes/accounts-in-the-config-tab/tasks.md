# Tasks

## 0. Rebase the deltas onto the archived specs

- [ ] 0.1 Once `accounts-in-the-console` and `set-up-an-account` are archived on `main`, turn this change's config-tab ADDED requirement "The Config tab has an Accounts section" into a MODIFIED of the archived "The Config tab shows the accounts" (the account rows move out of Setup), and check `account-setup`'s requirements still read true beside the dialog. **Verify:** `openspec validate accounts-in-the-config-tab --strict` passes and each MODIFIED header matches `openspec\specs` exactly.

## 1. Setup core without prompts

- [ ] 1.1 **Red:** `AccountSetupRunnerShould` facts that a run given an `AccountSetupAnswers` asks nothing and writes what the plan says. **Green:** split `AccountSetupRunner` into the answers core and the command's prompt layer. **Verify:** the new facts and every existing `AccountSetupRunnerShould` and `AccountRemovalRunnerShould` fact pass.

## 2. Settings rules

- [ ] 2.1 **Red:** `SettingsShould` (or `AccountsShould`) facts: renaming an account rewrites the list, root and repository values; dropping a root drops its `rootAccounts` entry; setting and clearing a repository override. **Green:** the `Domain\Accounts` operations. **Verify:** the facts pass.

## 3. Handlers

- [ ] 3.1 **Red:** `SetRepositoryAccountHandlerShould` facts that choosing personal writes the override, Follow the root removes it, and one account answers the only-one outcome. **Green:** `SetRepositoryAccount`, the handler, its outcome, its `Composition.Handlers` line, `WordingOfOutcomes` and `Wording.ToneOf` with `StatusTonesShould` lines. **Verify:** the facts, `EffectDispatcherShould` and `HandlersShould` pass.
- [ ] 3.2 **Red:** `RunAccountSetupHandlerShould` and `RenameAccountHandlerShould` facts over the fakes: setup runs the plan and raises the login-tab and bell steps; rename makes one settings save and one fragment write; remove runs `AccountRemovalRunner`. **Green:** the effects, handlers and outcomes. **Verify:** the facts pass.
- [ ] 3.3 **Red:** `SaveSettingsHandlerShould` fact that saving roots drops the removed root's account. **Green:** the handler. **Verify:** the fact passes.

## 4. Reducers and rows

- [ ] 4.1 **Red:** `ConfigRowsShould` facts for the Accounts heading, account rows and Add account, between the settings and Setup, with stable keys; the Setup section no longer lists accounts. **Green:** `ConfigRow`, `ConfigRows`, `WordingOfConfig`. **Verify:** the facts pass.
- [ ] 4.2 **Red:** `AddAccountViewShould` facts: the name problem as typed; each start re-plans; the checklists change the plan; F5 with a problem keeps the dialog; F5 with none raises `RunAccountSetup`; Esc raises nothing. **Green:** `AddAccountState`, `AddAccountView`. **Verify:** the facts and `ReducersShould` pass.
- [ ] 4.3 **Red:** `AccountDialogViewShould` facts: rename, roots, remove with its preview, and no remove for the default folder's account. **Green:** `AccountDialogState`, `AccountDialogView`. **Verify:** the facts pass.
- [ ] 4.4 **Red:** `WorktreesViewShould` fact that `a` raises `ChooseRepositoryAccount` for the row's repository; `KeyHelp` line in `WorktreesKeys`. **Green:** the reducer case and `Wording`. **Verify:** the fact and `KeyHelpParityShould` pass.

## 5. Dialogs

- [ ] 5.1 **Red:** `DialogEffectsShould` facts that each new dialog effect has a dialog. **Green (Tui binders, untested):** `AddAccountDialog`, `AccountDialog` and the repository account chooser, each calling `DialogTabTitle.Apply` with its `DialogName`. **Verify:** the facts pass, and each dialog at 120 x 36 in the headless terminal shows every control and the preview.
- [ ] 5.2 Add the Add account dialog to `SmokeScript` (open, type a name, Esc). **Verify:** `dotnet run -- smoke <exe> <worktree-folder>` exits 0 against a local install.

## 6. Docs and release

- [ ] 6.1 **Docs:** `docs\user-guide.md` (the Accounts section, the dialogs and `a`), `.claude\rules\console-screen.md`, and the README screenshot through `tools\New-ReadmeScreenshot.ps1` if the Config tab shows in it. **Verify:** the guide matches the screen.
- [ ] 6.2 Raise `Version` (minor) and add `CHANGELOG.md` lines under Added. **Verify:** `Assert-VersionBumped.ps1` and `Assert-ChangelogUpdated.ps1` against `main`.

## 7. Integration checks

- [ ] 7.1 Run `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes` and `openspec validate accounts-in-the-config-tab --strict`. **Verify:** each exits 0.
- [ ] 7.2 **Manual (owner):** in a throwaway Windows user, add an account from the dialog, rename the default one, give a repository to the new account with `a`, start a session there and see it start as that account, then remove the account from its dialog. **Verify:** by eye and in `config.json`.

## Workflow follow-up

- Archive the change in the pull request that completes it, so `openspec\specs` describes `main`.
- Close issue #203 with this pull request.
