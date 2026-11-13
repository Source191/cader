# Design

## Context

- After `set-up-an-account`: `AccountSetupPlanner` builds an `AccountSetupPlan` from a request and the disc, `AccountSetupRunner` and `AccountRemovalRunner` carry plans out over `IClaudeFolderFiles`, `IJunctions` and the record store, and `WordingOfAccountSetup` words every step.
- Config tab dialogs are reducers bound to their controls (`SettingsState`/`SettingsView`, the roots dialog), raised by `IDialogEffect`s routed to `Tui\DialogEffects`; a save goes through `SaveSettingsHandler` and `CurrentSettings`, and applies without a restart.
- Worktrees keys follow the four-piece rule: effect record, reducer case, handler and its line in `Composition.Handlers`, with `Wording` for the key help, outcome words and tone.

## Goals / Non-Goals

**Goals:**
- The dialog and the command cannot disagree: one planner, one preview text, one runner.
- Every new key and dialog follows the house patterns, so the reflection facts (`EffectRoutingShould`, `KeyHelpParityShould`, `HandlersShould`) catch a missing piece.

**Non-Goals:**
- A new way to share or unshare items after an account exists.

## Decisions

### 1. The Add account dialog is a reducer over the planner

`AddAccountState` holds the typed name (checked with `AccountName.Validate` on each change), the start, the item checklist (from `IClaudeFolderFiles` as the dialog opens), the root checklist and the current `AccountSetupPlan`, re-planned on each change. The preview pane is `WordingOfAccountSetup` over the plan, so it is the same text the command prints. F5 raises a `RunAccountSetup` effect whose handler calls `AccountSetupRunner` with the answers already given (no console questions), then the login-tab and bell steps through the existing effects.

- **Alternative:** a separate dialog flow that writes as it goes. It would drift from the command and lose the preview.

### 2. The command's console questions become answers on the request

`AccountSetupRunner` already takes its answers through `TextReader` prompts. This change splits it into the prompt layer (command only) and a core that takes an `AccountSetupAnswers` record, which the dialog fills. The command's tests keep passing unchanged.

### 3. Rename touches every reference in one save

Renaming rewrites the `accounts` entry, every `rootAccounts` and `repositoryAccounts` value naming it, the record file's name and the Windows Terminal profile's name in one `SaveSettingsHandler` save and one fragment write. The account's folder is never renamed: it holds the login and history, and Claude Code finds it only by that path.

### 4. `a` is a Worktrees key with a chooser dialog

`ChooseRepositoryAccount(FolderPath mainCheckout)` is an `IDialogEffect`; the chooser lists the accounts and Follow the root and raises `SetRepositoryAccount`, handled by `SetRepositoryAccountHandler` (its own outcome record and words). `a` on a leftover or an elsewhere row names the repository the same way the ACCOUNT column does, and on a row with no repository says there is none.

### 5. The roots dialog tidies its accounts

When `SaveSettingsHandler` saves roots, it drops `rootAccounts` keys that are no longer roots. A root that is added has no account until the person gives it one.

## Risks / Trade-offs

- [The dialog grows tall with long item lists] → the checklists scroll in a fixed-height frame; checked at 120 x 36 in the headless terminal.
- [A rename while sessions of that account run] → nothing about a running session depends on the name; the rows read the new name on the next refresh.
- [Removing an account from the console while one of its tabs is open] → the removal does not stop sessions; the preview says the account's sessions keep running and will no longer be listed.
