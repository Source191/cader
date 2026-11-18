# Proposal

## Why

The new-session wizard leaves out a repository root whose folder does not exist, as it should, since no session can start there. But nothing says so: a mistyped or removed root simply vanishes from the wizard, and the Config tab still lists it as if it were fine.

## What Changes

- On the Config tab, a repository root whose folder is not found reads `<path> (not found)` in VALUE, and its tooltip says to create the folder or remove the root.
- The check is made when the Config tab reads its setup, off the UI thread; while that read is under way the roots read as before.

## Out of scope

- Colouring the row or counting a missing root in the Config tab's "to set up" count.
- Warning about a missing worktree root.
- Multiple Claude accounts, which is a later change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `config-tab`: a repository root that is not found is marked as such.

## Impact

- `Application\Views\ConfigRow` (`RepositoryRoot.Found`), `Application\ConsoleLoads` (checks each root through `IFolders` when the setup is read), `Application\WordingOfConfig` (the value and the tooltip).
- `src\Source191.Cader\Composition\ConsoleComposition.cs` and `tools\ReadmeScene\SceneHost.cs` pass the folders to `ConsoleLoads`.
- `CHANGELOG.md` and the version.
