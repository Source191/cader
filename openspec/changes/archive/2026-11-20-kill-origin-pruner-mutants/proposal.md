# Proposal

## Why

Release v0.22.0-preview.50's mutation run (issue #368) found two survivors at `OriginPruner.cs` line 35, both Boolean mutations. `PruneAsync` hands `OncePerFolder.RunAsync` a `bool` that nothing reads (`return true` on success, `_ => false` on a git failure), so no test can tell a changed value apart. The stack's other changed Core code was not mutation-tested before it was merged either.

## Benefit

The pruner carries no unread value for a mutation run to flag, and the changes in the stack of pull requests are shown to have no surviving mutant.

## What Changes

- **Remove the unread boolean:** `PruneAsync` answers the repository it pruned (no literal to mutate) instead of `true` or `false`.
- **Kill the stack's survivors:** a Stryker run over the Core files the stack changed, with a fact per survivor, a commit per fact.

## Out of scope

- Projects other than Core, which Stryker does not mutate.

## Capabilities

None: no behaviour changes (`skip_specs: true`).
