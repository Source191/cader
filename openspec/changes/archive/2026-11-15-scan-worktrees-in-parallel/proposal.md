## Why

Every Worktrees scan takes 7 to 12.5 seconds on the owner's machine (126 `slow refresh worktrees scan in 9499 ms` lines in one diagnostic log, still there in 0.22.0-preview.20). The process counts show why: a scan runs one `git worktree list` per repository under the roots (32 to 34 of them) and then `git status`, `git rev-list` and `git log` for each live worktree, about 80 ms each, and `WorktreeInventory.ScanAsync` awaits every one of them in turn. With a 15-second rescan, the tab spends most of its time scanning, and a removal's reload is just as slow.

## What Changes

- A scan lists the repositories and inspects their live worktrees several at a time, at most eight git reads at once per scan, instead of one after another.
- The rows, their order, the leftover and orphan rules, the once-per-folder failure handling and cancellation are unchanged: each repository's results are merged in the same order as today, so an orphan is decided against exactly the registrations it sees now.
- `FakeProcessRunner` gains an awaitable hook so a test can count git reads as they start and hold them open, proving the overlap and the bound deterministically and without sleeps.
- No change to what a user sees but speed; the diagnostic log's `worktrees scan` counts, already timed by `ConsoleLoads` through `RefreshCounts`, show the improvement.

Out of scope:

- `git status --no-optional-locks` (the scan's status may refresh a worktree's index while a session works in it, as it does today); a follow-up if wanted.
- Running `WorktreeInspector.CountAsync`'s three reads at once: with eight slots shared by the whole scan it gains nothing while worktrees are queued, and it is shared with the detail pane and session deletion.
- The UI-thread stalls, and the detail pane reading git again on every refresh (later follow-ups).
- Session folders that no longer exist, the diagnostic log's format, and clearing the log (separate changes running in parallel).
- Multiple Claude accounts, which is a later change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None: no requirement changes. The worktree-management spec says nothing of how a scan is ordered or how long it takes, and its "A slow scan" scenario (no second scan starts while one runs) stays true. The change declares `skip_specs: true`.

## Impact

- **Code:** `src\Source191.Cader.Core\Application\WorktreeInventory.cs` (`ScanAsync`, `RegisteredRowsAsync`); nothing in Ports, the adapters or the screen.
- **Tests:** `test\Source191.Cader.TestDoubles\FakeProcessRunner.cs` (the hook), `test\Source191.Cader.Core.Tests\Application\WorktreeInventoryLeftoversShould.cs` (overlap, the bound, order and failure facts); `test\Source191.Cader.Infrastructure.Tests\WorktreeInventoryShould.cs` keeps running the real-git cases unchanged.
- **Release:** a version step and a Changed line in `CHANGELOG.md`.
