# Proposal

## Why

A parameter with a default value lets a caller leave an argument out, and the code then decides for them without saying so. A caller that forgets `FolderLeft` on a removal outcome says the folder was not left behind; a caller that forgets `Facts`' flags says the branch was not created here and not merged. The call site reads as complete when it is not.

## Benefit

- Every call site states each value it passes, so a reader sees the decision where it is made, and a forgotten argument is a compile error instead of a quiet default.
- A convention check in `SourceConventionsShould` fails for any new default parameter value under `src`, so the habit does not come back.

## What Changes

- No parameter under `src` has a default value, except the optional JSON members of the state-file records `UserConfig`, `EditorSetting`, `TabFile`, `UpdateCheckFile` and `LastRunFile`, which use an explicit `= null` so a missing member reads as absent.
- Sites found: `FolderLeft` on `RemoveWorktreeOutcome.Removed`, `.RemovedAndDeletedBranch`, `.BranchKept` and `DeleteSessionOutcome.RemovedWorktreeAndBranch`, `.BranchNotMerged`; `WorktreeRow.Facts`' two flags; `GitException`'s `inUse`; `DetailLine`'s `Tone` and `Span`; `Settings.Resolve`'s update-check, place and diagnostic-log parameters; and `SettingsText`'s three trailing members. Each call site now passes its value, naming the argument where a bare `true` or `false` would be unclear.
- `SourceConventionsShould.GiveNoParameterADefaultValue` reads the syntax trees under `src` and names each parameter with a default.
- CLAUDE.md's C# idioms: no default parameter values, the state-file records excepted.
- The package version rises one step with an Internal changelog line.

Out of scope: the test projects and `tools`, whose helpers may keep defaults; the cases of the outcome records are not redesigned; multiple Claude accounts.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None. No behaviour changes; `.openspec.yaml` declares `skip_specs: true`.

## Impact

- `src\Source191.Cader.Core` (the records and methods above and their call sites), `src\Source191.Cader.Infrastructure` (`GitCli`), tests that construct those types, `SourceConventionsShould`, CLAUDE.md, `Source191.Cader.csproj` and `CHANGELOG.md`.
