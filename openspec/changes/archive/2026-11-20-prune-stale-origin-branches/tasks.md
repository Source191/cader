# Tasks

**Implement with:** Opus 5.5 at medium effort: a background step modelled on the pull request lookups, with a clock-based rule and scripted-git facts; the network call is the one risk, and it reuses the fetch's no-prompt settings.

## 1. Git adapter

- [x] 1.1 Red: `GitCliShould.PruneOriginWithoutEverPrompting`: `PruneOriginAsync` runs `["remote", "prune", "origin"]` in the repository with `GIT_TERMINAL_PROMPT=0` and `GCM_INTERACTIVE=false` in its variables (design 2), as `FetchOneBranchFromItsRemoteWithoutEverPrompting` checks the fetch. It fails to compile until `IGit.PruneOriginAsync` is declared. Commit.
- [x] 1.2 Green: `IGit.PruneOriginAsync(FolderPath, CancellationToken)`, its XML comment saying it throws `GitException` when git fails, and `GitCli`'s one-line implementation over `RunOrThrowAsync` with `NoPrompts`; 1.1 passes. Commit.
- [x] 1.3 `GitCliShould.ThrowGitsWordsWhenAPruneFails`: a non-zero exit throws `GitException` carrying git's stderr. Green with 1.2's code; verify it fails if `RunOrThrowAsync` is swapped for `RunAsync`. Commit.
- [x] 1.4 `GitCliIntegrationShould.PruneOnlyTheBranchesOriginHasDeleted`: in a temporary folder, a bare repository as origin, a clone with `origin/kept` and `origin/old-fix`, `old-fix` then deleted in the bare repository; after `PruneOriginAsync` the clone lists `origin/kept` and not `origin/old-fix`, and its local branches are unchanged. Verify with `dotnet test test\Source191.Cader.Infrastructure.Tests -c Debug --no-build`. Commit.

## 2. The pruner

- [x] 2.1 Red: `OriginPrunerShould.PruneEachRepositoryOnce`: `PruneDueAsync` over Northwind.Orders twice and Northwind.Billing once, through `GitCli` over a `FakeProcessRunner` and a `FakeTimeProvider`, runs exactly one `remote prune origin` in each. Commit.
- [x] 2.2 Green: `Application\OriginPruner(IGit, TimeProvider, DiagnosticLog)` with `PruneDueAsync` (design 3); 2.1 passes. Commit.
- [x] 2.3 Red then green: `OriginPrunerShould.WaitAnHourAfterAnAttempt`: a second call 59 minutes later runs nothing, one at 60 minutes runs the prune again; the attempt time is claimed before git runs. Commit.
- [x] 2.4 Red then green: `OriginPrunerShould.WaitAnHourAfterAFailure`: a prune that exits 128 records one `Failed` event at `FailureSite.BackgroundTask` naming `OriginPruner`, answers a completed task, and the repository is not tried again within the hour; a second failure after the hour records no second event (`OncePerFolder`). Verify `DiagnosticsPrivacyShould` still passes, so no repository path reaches the log. Commit.
- [x] 2.5 Red then green: `OriginPrunerShould.ClaimARepositoryOnceWhenTwoCallsOverlap`: two `PruneDueAsync` calls for the same repository, the first held inside git by a gated `FakeProcessRunner`, run one prune between them (design 3's `TryAdd`/`TryUpdate`). Commit.

## 3. The scan

- [x] 3.1 Red: `WorktreeInventoryBranchesShould.PruneTheRepositoryOfAnOriginRowAfterTheScan`: a scan that shows a ○ origin row in Northwind.Orders starts one `remote prune origin` there; a scan whose origin candidate is not flagged (its pull request open) starts none, and so does a scan with only worktree and branch rows. Commit.
- [x] 3.2 Green: `WorktreeInventory` takes an `OriginPruner` and, after `ShownAsync`, discards `pruner.PruneDueAsync(...)` over the repositories of the shown rows whose health is `WorktreeHealthKind.Origin` (design 4), with a one-line comment as the catalogue's has; every `new WorktreeInventory(...)` in the tests passes a pruner; 3.1 passes. Commit.
- [x] 3.3 Wiring: `ConsoleComposition` constructs one `OriginPruner` over the console's `GitCli`, `TimeProvider.System` and `DiagnosticLog` and hands it to `WorktreeInventory`. Verify `dotnet build -c Debug`. Commit.
- [x] 3.4 README scene: `SceneProcessRunner` answers `git remote prune origin` as done, as it does a fetch from a remote (design 5), and `SceneHost` constructs the pruner as `ConsoleComposition` does; a fact in `ReadmeScene.Tests` checks the prune is answered without reaching the real runner. Verify `dotnet test tools\ReadmeScene.Tests -c Debug`. Commit.
- [x] 3.5 Docs: `.claude\rules\console-screen.md`'s scanning paragraph says that a scan showing an origin row starts `OriginPruner` for that repository, at most once an hour, never awaited, and that a reference change starts no rescan, so the timer shows the result; `docs\user-guide.md`'s line 78 says Cader prunes origin's records for a repository showing an origin row at most once an hour, so a deleted branch's row goes by itself, instead of "Cader never fetches". Commit.

## 4. Finish

- [x] 4.1 Raise `Version` one preview step above `main` with `.github\scripts\Step-Version.ps1` and add an Added entry under Unreleased in `CHANGELOG.md` (origin rows for branches origin has deleted now leave the Worktrees tab by themselves, and a branch whose upstream was deleted reads ✗ gone without a manual prune). Verify `dotnet build -c Debug`, `dotnet test -c Debug --no-build` at the solution, `dotnet format --verify-no-changes` and `openspec validate prune-stale-origin-branches` pass. Commit.
- [ ] 4.2 Manual check on the owner's machine with `tools\install-local.ps1`: open the Worktrees tab with the stale origin rows of 20 November 2026 (unless already pruned by hand) and confirm they go within one refresh interval and no window or prompt appears. Record the result in the pull request.

## Workflow follow-up

- Archive the change in the same pull request (`/opsx:archive`) and verify that `openspec validate` passes.
- The proposal is opened first as a pull request from `c-not-gone` against `main`; the implementation lands on the same branch, and its description then gains What changed.
