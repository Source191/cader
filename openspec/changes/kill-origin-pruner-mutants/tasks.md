## 1. Measure

- [ ] 1.1 Run Stryker over the Core files changed since `origin/main` and record the survivors and no-coverage mutants.

## 2. Kill

- [ ] 2.1 Remove the unread boolean from `OriginPruner.PruneAsync` (#368).
- [ ] 2.2 A fact per other survivor, a commit each.
- [ ] 2.3 Re-run Stryker over the same files and confirm none survive.

## 3. Finish

- [ ] 3.1 Raise the version with `Step-Version.ps1` if `src` changed.
- [ ] 3.2 `openspec validate --all`, archive the change.

Implement with: Sonnet 5.5 at medium effort.
