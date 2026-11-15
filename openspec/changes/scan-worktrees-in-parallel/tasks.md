## 1. The fake can hold a git read open

- [x] 1.1 Add `WhileRunning` (`Func<ProcessRequest, Task>`, default `Task.CompletedTask`) to `test\Source191.Cader.TestDoubles\FakeProcessRunner.cs`, called first in `RunAsync`, before its yield, and awaited there, with a summary line saying a hook's synchronous part runs on the caller's path so a test can count starts and hold runs open (design decision 5). Verify `dotnet build -c Debug` and `dotnet test test\Source191.Cader.Core.Tests -c Debug --no-build` pass unchanged.

## 2. Pin today's rows before anything moves

- [x] 2.1 Add `WorktreeInventoryLeftoversShould.StillListTheOtherRepositoryWhenOneIsUnreadable` (Northwind.Orders unreadable with a `GitException`, Northwind.Billing with one live worktree): the Billing rows are all there with their counts and one `Failed` event is logged. It passes on the serial scan; commit it as the pin for the failure handling.

## 3. Repositories are listed together, in today's order

- [x] 3.1 Red: `WorktreeInventoryLeftoversShould.ListTwoRepositoriesAtOnceAndKeepTheirOrder`: Northwind.Billing and Northwind.Orders (one live worktree each), every `worktree list` held; two listings have started when `ScanAsync` returns its task. Then Orders' hold opens, the test awaits its worktree's `status` start (proof Orders' listing answered), and Billing's hold opens; the rows equal the serial scan's (Billing's before Orders'). Holds open in a `finally`. Verify it fails on the start count (one, not two).
- [x] 3.2 Green: `RegisteredRowsAsync` answers a private record of its registered paths and rows instead of writing the shared set; `ScanAsync` starts every repository with `Task.WhenAll` and merges the results in repository order (union the paths, add the rows, then `OrphanRows`), as design decision 3 says. Verify 3.1, 2.1 and the rest of `WorktreeInventoryLeftoversShould` pass.
- [ ] 3.3 Refactor: keep `ScanAsync` readable (a named merge step if it helps), with no change to the leftover, candidate and empty-folder rules. Verify the Core suite and `dotnet format --verify-no-changes`.

## 4. A repository's worktrees are inspected together

- [ ] 4.1 Red: `WorktreeInventoryLeftoversShould.InspectThreeWorktreesOfOneRepositoryAtOnce`: three live worktrees of Northwind.Orders, every `status` held; the test awaits the third `status` start with `WaitAsync(TimeSpan.FromSeconds(10))`, then opens the hold; each row has its own counts in git's listing order. Verify it fails with a `TimeoutException`, the serial scan having started only one.
- [ ] 4.2 Green: `RegisteredRowsAsync` builds its rows with `Task.WhenAll` over the entries (which keeps their order), each live one through `InspectedAsync` as now. Verify 4.1 and `AskGitThreeThingsPerRegisteredWorktreeAndNothingAboutMergeState` (still serial within a worktree) pass.

## 5. At most eight git reads at once per scan

- [ ] 5.1 Red: `WorktreeInventoryLeftoversShould.StartAtMostEightGitReadsAtOnce`: ten repositories, every `worktree list` held; exactly eight listings have started when `ScanAsync` returns its task, and all ten rows' repositories are listed once the hold opens. Verify it fails with ten started.
- [ ] 5.2 Green: `ScanAsync` creates `using SemaphoreSlim slots = new(ScanSlots)` (`private const int ScanSlots = 8`, with its one-line reason) and takes a slot only around `git.ListWorktreesAsync` and `inspector.CountAsync`, never across a repository's inspections (design decisions 1 and 2). Call `slots.WaitAsync()` with no token yet. Verify 5.1, 4.1 and 3.1 pass.
- [ ] 5.3 Red: `WorktreeInventoryLeftoversShould.StartNoGitReadOnceTheScanIsCancelled`: ten repositories, the eight started listings held, the token cancelled, then the hold opened; the scan throws `OperationCanceledException` and no request names the ninth or tenth repository. Verify it fails because they are asked.
- [ ] 5.4 Green: pass the scan's token to `slots.WaitAsync`. Verify 5.3 and the Core suite pass.

## 6. The whole solution and the documentation

- [ ] 6.1 Run `dotnet test -c Debug --no-build` at the solution, including `Infrastructure.Tests\WorktreeInventoryShould` over real git and `DiagnosticsPrivacyShould`; loosen any fact that asserted the order of git requests across folders to the order within one folder, and record each in the commit. Verify the solution suite passes three runs in a row.
- [ ] 6.2 Run Stryker on `WorktreeInventory` (`.claude\rules\checks.md`); kill any new surviving mutant with a fact (5.1 already kills `ScanSlots` raised or lowered), and mark only a true equivalent with a Stryker comment and its reason. Verify the report shows no unexplained survivor in the changed lines.
- [ ] 6.3 Add one sentence to `.claude\rules\console-screen.md`, Refreshing: a scan lists repositories and inspects worktrees at most eight git reads at once and merges them in repository order. Verify `git diff` shows only that sentence there.

## 7. Release housekeeping

- [ ] 7.1 Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` with `.github\scripts\Step-Version.ps1` (0.22.0-preview.20 to preview.21, or the next free number), and add a `## Unreleased`, `### Changed` line to `CHANGELOG.md` saying the Worktrees tab now scans several repositories and worktrees at once, so a refresh that took about ten seconds takes a fraction of that. Verify `Assert-VersionBumped.ps1 -Base main -Head HEAD` and the changelog check pass, and `dotnet format --verify-no-changes` is clean.

## Workflow follow-up

- The owner's check, not an agent's (it runs the real console over the owner's roots and settings): install the build with `tools\install-local.ps1`, run it with the diagnostic log on for ten minutes with the Worktrees tab shown, and confirm `%LOCALAPPDATA%\Cader\cader.log` gains no new `slow refresh worktrees scan` line and the `worktrees scan` p95 is under 3 seconds; record the median, p95 and longest in the pull request.
- Write the pull request description as Why (scans of 7 to 12.5 seconds), Benefit (a Worktrees tab that keeps up with its 15-second refresh) and What changed.
- Archive the change in the same pull request, once its tasks are done, so `openspec\specs` matches `main` (no spec delta: `skip_specs: true`).
