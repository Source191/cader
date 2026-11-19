# Proposal

## Why

#351 keeps each table's place across a refresh, and `TableScrollShould` holds it, but two edges the owner asked about are not pinned at the table: the selected row was the last one and a refresh removes it, so no row has its number any more, and a refresh that removes every row. The reducer's choice (the selectable row nearest the old position, or none) is already held by the `ListSelection` facts; what the table then does with its kept offset is not.

## Benefit

- **Maintainers:** a change to `TableScroll` that left the view past the end of a shorter table, or broke on an empty one, fails a fact instead of reaching a user.

## What Changes

- **`TableScrollShould`** gains two facts: the last of 100 rows selected and scrolled to, then a refresh of 35 rows selecting the new last row (as `ListSelection.Load` does) shows the last 12 rows with it on screen; and a refresh of no rows leaves the table at its top without throwing.

Out of scope:

- Any change to `TableScroll` or `ListSelection`.
- Multiple Claude accounts beyond naming a session's owner, which is a later change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None. No requirement changes; the change declares `skip_specs: true`.

## Impact

- **Implement with:** Claude Opus 5.5 (`claude-opus-5-5`) at medium effort: two facts over a class whose behaviour is known.
- **Tests:** `test\Source191.Cader.Core.Tests\Tui\TableScrollShould.cs`.
- **Version and changelog:** none; nothing under `src` changes.
