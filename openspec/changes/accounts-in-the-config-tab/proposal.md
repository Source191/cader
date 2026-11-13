# Proposal

## Why

After `set-up-an-account` there is a command for adding an account, but which account owns a root or a repository is still edited by hand in `config.json`, and a person who lives in the console has to leave it to add or rename an account. Issue #203 asks for an Accounts section on the Config tab that does the same steps as the command, and a way to say "this repository is my personal one" from where the repository is shown.

## What Changes

- **An Accounts section on the Config tab** between the settings and Setup, a row per account (name and folder), then an **Add account** row. It replaces the read-only account rows `accounts-in-the-console` put under Setup. With one account it shows that account and Add account only.
- **Add account** opens a dialog that runs the `setup-account` steps through the same planner: the name, the four starts each with its sentence, the items to share as a checklist, the roots to give the account, and the full preview, which F5 accepts. The login tab opens and the bell is offered as the command does.
- **Enter on an account row** opens its dialog: rename it (the name is checked as a person types), see its folder, change which roots belong to it, or remove it with the same preview and rules as `setup-account --remove`. The account in `%USERPROFILE%\.claude` cannot be removed, only renamed.
- **`a` on a Worktrees row** chooses the account for that row's repository: any account, or "follow the root", which clears the override. It is saved at once and the row's ACCOUNT follows.
- Removing a repository root in the roots dialog also clears its `rootAccounts` entry.
- The smoke script gains the Add account dialog.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `config-tab`: the Accounts section, the Add account dialog, the account dialog, and the root dialog's tidy-up.
- `worktree-management`: the `a` key sets a repository's account.

## Impact

- **Code:**
  - Core Application: `ConfigRow`/`ConfigRows` (the section), new reducers `AccountDialogView` and `AddAccountView` over `AccountSetupPlanner`, the `ChooseRepositoryAccount` effect with its `ChooseRepositoryAccountHandler` and outcome, `SaveSettingsHandler` for the root tidy-up, `Wording`/`WordingOfConfig`/`WordingOfAccountSetup`, `Wording.ToneOf`.
  - Tui: `AccountDialog`, `AddAccountDialog` and the repository account chooser in `DialogEffects`.
  - `tools\HeadlessTerminal`'s `SmokeScript`.
- **Tests:** the reducers' `*ViewShould`, the handler's `...HandlerShould`, `DialogEffectsShould`, `EffectDispatcherShould`, `KeyHelpParityShould`, `ConfigRowsShould`.
- **Docs:** `docs\user-guide.md`, `.claude\rules\console-screen.md` (the section and dialogs), README screenshot if the Config tab shows in it, `CHANGELOG.md` with a minor `Version` raise.

## Delivery

- **Pull request:** PR 4 of 4 for issue #203, one pull request.
- **Order:** last, after `set-up-an-account` has merged; it reuses that change's planner and removal runner.
- **Implementation:** Sonnet 5.5 at high effort. It follows the established reducer, effect and dialog patterns closely; the risky file work is already done and tested by `set-up-an-account`.

## Out of scope

- Choosing an account for one session in the wizard.
- Moving sessions, transcripts or settings between accounts.
- Editing what an account shares after it is set up (run the start again by removing and adding, or edit the files).
