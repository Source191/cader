# Proposal

## Why

`openspec validate --all --strict` fails: thirteen requirements in six specs run past 500 characters, so strict validation cannot be used as a check. Those requirements also hide separate rules, such as a table's heading rule and its branch rule, or a command's happy path and its refusals, inside one long sentence that a reader has to unpick.

## Benefit

- **Maintainers:** `openspec validate --all --strict` passes, so it can run as a check and a new long requirement is caught as it is written.
- **Readers of the specs:** each requirement states one behaviour, with its examples and edge cases in scenarios.

## What Changes

- **Six specs** (`detail-pane`, `folder-actions`, `needs-input-notification`, `session-attach`, `settings`, `worktree-management`) have their long requirements split into requirements that each state one behaviour, the existing scenarios moved under the one they belong to, and a scenario added where a split-off rule had none. The first part keeps the original heading.
- **No rule changes:** every SHALL, number, name and condition stays stated in the spec.

Out of scope:

- Any change to behaviour, to the code or to the tests.
- Requirements that are already under 500 characters.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None. No requirement changes; the change declares `skip_specs: true`.

## Impact

- **Implement with:** Claude Sonnet 5.5 (`claude-sonnet-5-5`) at medium effort: editorial edits to six spec files.
- **Specs:** `openspec\specs\detail-pane`, `folder-actions`, `needs-input-notification`, `session-attach`, `settings` and `worktree-management`.
- **Version and changelog:** none; nothing under `src` changes.
