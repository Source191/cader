## 1. Set up

- [x] 1.1 Raise the version with `Step-Version.ps1` and add an Internal `CHANGELOG.md` line.
- [x] 1.2 Run Stryker scoped to every marked file on the base and keep its Ignored list as the before count.

## 2. Rewrite or test each mark, a red and a green commit per group

- [x] 2.1 Null fallbacks: `ReleaseNotes`, `ReleaseVersion`, `RemoteRepository`, `FolderPath`, `ExceptionSummary`.
- [x] 2.2 Early returns: `ReleaseNotes`' section reader, `WordingOfReleaseNotes`, `StateTransitionNotifier`.
- [x] 2.3 Guards: `TextFit`, `WhereLabel`.
- [x] 2.4 `First` after `DefaultIfEmpty`: `WorktreeInventory`, `WhereLabel`.
- [x] 2.5 Branches for states that never happen: `ListSelection`, `WizardView`, the key bar, `WordingOfOutcomes`, the help's key column, the memory breakdown.
- [x] 2.6 `cader here`'s repository check and worktree-folder answer.
- [x] 2.7 The rest, one at a time, keeping a mark only for a true equivalent.
- [x] 2.8 Check each group with a scoped Stryker run: the mutant Killed, the Ignored list smaller by exactly the mutants meant.

## 3. Prove and record

- [x] 3.1 Dispatch `mutation.yml` on the branch; confirm nothing Survived or NoCoverage that is not Ignored, and record the run. Run 43713303950 (99.97 %) left one survivor, from harden-terminal-settings-writes' remove path, now killed by a fact shown to fail with the mutant built in; no runner run was repeated for it.
- [x] 3.2 Update `.claude\rules\checks.md` (the ledger's Equivalent count, and the rule: before marking a mutant Equivalent, try a rewrite or a test) and `CONTRIBUTING.md` (the same rule beside the mark convention).
- [x] 3.3 Archive the change.
