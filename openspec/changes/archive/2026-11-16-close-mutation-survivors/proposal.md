# Proposal

## Why

The mutation run after each release found code its tests do not pin down (issue #315, releases 0.22.0-preview.27 to .30). The run is red until each surviving mutant has a test that kills it, and the next release run will fail the same way while they stand. The last green run was a skip ("The commit is not a release"), not a pass.

## Benefit

- **Maintainers:** `mutation.yml` goes green on the next release, so a red run means a new gap again.
- **Behaviour:** six behaviours the console already has (odd-length key help, singular counts, no listing for branch rows, branch row flags, the zero-unpushed branch line, the commit id message) are held by tests, so a later refactor cannot change them silently.

## What Changes

One simplification in `src`, and tests in `Source191.Cader.Core.Tests`:

- **`WordingOfScreens`:** `KeyColumns` loses its `pair.Length == 1` case, which no key list reaches (all three have an even count) and which would have thrown on `pair[1]` if one did; `Counted` with exactly one branch ("1 branch").
- **`WordingOfDetail`:** a branch or origin row has no listing; `NoFolderChanges` for a branch with unpushed commits, with exactly zero, and for an origin row.
- **`WorktreeInventory`:** `BranchRow` reports `OwnerExists` and `FolderExists` as true.
- **`CommitId`:** the exception for a bad id names the text and says 40 or 64 hex digits.

Out of scope: any change under `src`, the 112 to 205 static mutants left to the weekly run.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None. No requirement changes; the change declares `skip_specs: true`.

## Impact

- **Tests:** `WordingOfScreensShould`, `WordingOfDetailShould`, `WorktreeInventoryBranchesShould` and `CommitIdShould`.
- **`src`:** `WordingOfScreens.KeyColumns` only.
- **Version and changelog:** `Version` rises one preview step and the changelog gets an Internal entry.
- **Users:** nothing visible.
