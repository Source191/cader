# Proposal

## Why

The release mutation runs for 0.22.0-preview.43 (run 44376278112) and preview.44 (run 44481935001) failed under the strict release rule, both with the same 13 mutants that survived or had no covering test (issue #344): six in `AccountCatalogue`, four in `WordingOfAccounts`, one in `WordingOfDetail` and one each in `AccountLabel` and `OrganisationId`. Every release run that reaches this code fails until they are killed, so a red run says nothing new.

## Benefit

- **Maintainers:** the release mutation run goes green again, so a red one means a real gap in a new change.
- **Behaviour:** rules the accounts code already has are held by facts: a rename or a login change touches only its own account, renames and refreshes write the store in the order they took the write lock, a refresh whose write fails keeps the login in memory, and the pane's and rename dialog's words are pinned.

## What Changes

- **`AccountCatalogueShould`:** a rename with two accounts keeps the other's label (line 84); a changed login with two accounts leaves the other untouched (line 137); a second rename made while the first one's write is held is written after it (lines 75 and 92), and the same after a refresh (line 101, since a refresh that took no lock would hand back one it never held); a refresh whose store write fails answers normally and keeps the new login in memory (line 112).
- **`InMemoryState`:** `HoldNextAccountWrite(Task)` makes the next account write wait for the task, and fail if it faults, as a slow or refusing state file would; the facts above use it.
- **Wording facts:** the pane's Account line names its label for another account and for an owner not yet seen (`WordingOfAccounts` lines 27 and 28), a branch row's Changes line names its label (`WordingOfDetail` line 76), and the rename dialog's title and prompt are asserted word for word (lines 34 and 37).
- **Text values:** the messages `AccountLabel.From` and `OrganisationId.From` throw name the refused text (`AccountLabel` line 17, `OrganisationId` line 15).

Each new fact passes at once, since the code is already right; each is shown to fail with its mutant applied by hand, then reverted.

Out of scope:

- Any change under `src`: no mutant here is equivalent.
- The static mutants left to the weekly full run.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None. No requirement changes; the change declares `skip_specs: true`.

## Impact

- **Implement with:** Claude Opus 5.5 (`claude-opus-5-5`) at medium effort: facts for mutants already located, and one hook on a fake.
- **Tests:** `AccountCatalogueShould`, `WordingOfAccountsShould`, `WordingOfDetailShould`, `AccountLabelShould`, `OrganisationIdShould` (Core.Tests) and `InMemoryState` (TestDoubles).
- **Version and changelog:** none; nothing under `src` changes.
