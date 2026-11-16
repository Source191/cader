## Context

See proposal.md, Why. Today `WorktreeInventory.ScanAsync` classifies the roots' children on the calling thread, then for each repository in name order awaits `RegisteredRowsAsync` (one `git worktree list`, then `InspectedAsync` per live worktree, each awaiting `WorktreeInspector.CountAsync`'s `git status`, `git rev-list` and `git log` in turn) and `OrphanRows`, which reads the shared `registered` set. Broken candidates and the worktree root's empty folders are judged last, against every registration.

What shapes the approach:

- **Order matters for rows, not just display:** `OrphanRows` for a repository sees the registrations of that repository and those before it, never those after. The candidates and the worktree root's empty folders see all of them.
- **Failures are per folder:** `OncePerFolder` turns a `GitException` into a fallback (no entries, or a row without counts) and logs it once per folder; it is already a `ConcurrentDictionary`. `DiagnosticLog` records under a `Lock`. Any other exception ends the scan.
- **Scans can overlap:** `RunningLoads` lets the reload after an action run beside a timed scan, so per-scan state must not be shared between scans.
- **The fake boundary:** Core tests run the real `GitCli` over `FakeProcessRunner`, which already yields once per run and guards its lists with a `Lock`; its `OnRun` hook is synchronous, so a test cannot hold a run open. `FakeFolders` only reads during a scan, which is safe from several threads.
- **The slow-refresh line** is written for a part over 3 seconds (`docs\performance-monitoring.md`), and the scan is already timed by `ConsoleLoads` through `RefreshCounts`.

## Goals / Non-Goals

**Goals:**

- Bring the owner's 7 to 12.5 second scans under the 3-second slow-refresh line, with identical rows in identical order.
- Prove the overlap, the bound and the order with deterministic sociable facts.

**Non-Goals:**

- A general concurrency helper, a bounded `IGit` decorator or a change to any port.
- Changing `WorktreeInspector`, which the detail pane and session deletion share.

## Decisions

### 1. One slot pool per scan, taken around each leaf git read

`ScanAsync` creates `using SemaphoreSlim slots = new(ScanSlots)` and passes it down. A slot is held around exactly two awaits: `git.ListWorktreesAsync` for one repository, and `inspector.CountAsync` for one worktree (its three reads stay serial, so one slot is one git process). No slot is held while awaiting other work, so a repository waiting for its worktrees' inspections never starves them: holding a slot across `RegisteredRowsAsync` would let eight listings take every slot and deadlock their own inspections. `WaitAsync` is given the scan's token (the catalogue's is not, because nothing cancels a lookup), so queued reads stop at once on cancellation.

Alternatives: a field like `PullRequestCatalogue.slots` would be shared by overlapping scans, which is better for the machine, but makes `WorktreeInventory` own a disposable (CA1001 and an `IDisposable` reaching `ConsoleComposition`, `SceneHost` and every test that builds the inventory) for a case that is rare (a reload beside a timed scan, at most two scans, so at most twice the bound). `Parallel.ForEachAsync` bounds one loop but not the nested worktree loop with the same budget, and returns no results in order. `Task.WhenAll` over a `Select` keeps input order for free, which is what the merge needs.

### 2. Eight at once

`private const int ScanSlots = 8`, with a one-line reason. The owner's log puts a scan at roughly 120 git reads of about 80 ms each, mostly process start-up; 12.5 seconds over 4 slots is still about 3.1 seconds, above the slow line before any loss to overlap, while 8 gives about 1.6 seconds and room for less than linear gains. Eight short-lived, read-only git processes is modest on a developer machine (CI's hosted runners have four processors and still pass, as the facts use fakes). `Environment.ProcessorCount` was considered and rejected: it makes the bound, and any fact about it, depend on the machine. Four, the catalogue's number, suits it because `gh` and `az` are heavier, networked programs.

### 3. Gather per repository, merge in today's order

`RegisteredRowsAsync` no longer takes the shared set. It answers a small private record of the registered paths (in git's order) and the rows, and inspects its live worktrees with `Task.WhenAll` over the entries, which keeps entry order. `ScanAsync` starts every repository at once (`Task.WhenAll(repositories.Select(...))`), then walks the results in repository order exactly as the loop does today: union that repository's paths into `registered`, add its rows, add `OrphanRows(repository, registered)`. The candidates and empty folders are then judged against the full set, as now. Every row, and its position, is therefore what the serial scan gives.

Alternative: building the full `registered` set before any `OrphanRows` would be simpler to state, but would drop an orphan row in the rare case where a later repository registers a folder under an earlier one's `.claude\worktrees`; that is a behaviour change, so it is left as an open question rather than slipped in.

### 4. Failures and cancellation as today

Each git read stays inside its `OncePerFolder.RunAsync`, so an unreadable repository or worktree still gives its fallback while its neighbours give real rows. `Task.WhenAll` waits for every repository before rethrowing, so a non-git exception or a cancellation ends the scan as it does now, after the reads already running finish; a read not yet started never starts once the token is cancelled. `RefreshCounts` still counts neither a failed nor a cancelled scan.

### 5. An awaitable hold in the fake

`FakeProcessRunner` gains `public Func<ProcessRequest, Task> WhileRunning { get; set; } = _ => Task.CompletedTask;`, called first in `RunAsync`, before its yield, and awaited there. The path from `ScanAsync` to the runner is synchronous while a slot is free (`WaitAsync` completes at once, then `OncePerFolder.RunAsync`, `GitCli` and the runner run up to their first real await), so a hook that counts each start before awaiting its hold has an exact count the moment `ScanAsync` returns its task: one on the serial scan, ten with no bound, and exactly `ScanSlots` with it. The listing and bound facts therefore fail on an assertion at red, not a timeout, and kill `ScanSlots` raised or lowered. Each test opens its holds in a `finally`, so a failed assertion never leaves a scan hanging. The inspection fact cannot count synchronously (the listing yields first), so it awaits its third start with `WaitAsync(TimeSpan.FromSeconds(10))` and fails with a `TimeoutException` at red. No sleeps or `Task.Delay`.

Facts, in `WorktreeInventoryLeftoversShould` (Northwind.* repositories under `C:\Users\me\source\repos`):

- **Order:** both listings start at once; Orders' is answered first (proved by its worktree's `status` starting), then Billing's, and the rows come back in the serial scan's order.
- **Overlap:** three live worktrees of one repository are inspected together.
- **Bound:** with ten repositories, exactly eight listings have started when `ScanAsync` returns.
- **Failure:** of two repositories read together, the unreadable one is logged once and the other's rows are all there.
- **Cancellation:** with eight listings held and the token then cancelled, the scan throws `OperationCanceledException` and the ninth and tenth repositories are never asked.

## Risks / Trade-offs

- **[Index refresh while a session works]** `git status --porcelain` may take a worktree's `index.lock` to write refreshed stat data, so a session's own `git add` in that worktree can meet "index.lock exists". This is true today; the change runs no more `status` per worktree and never two in the same worktree, since each worktree has its own index and gets one `status` per scan. → Recorded; `--no-optional-locks` is a follow-up (proposal, Out of scope).
- **[Machine load]** Eight git processes at once on a slow disc or under an antivirus scan may gain less than eight times. → The diagnostic log's `worktrees scan` median and p95 show the real figure; the constant is one line to change.
- **[Overlapping scans]** A reload beside a timed scan runs up to sixteen reads. → Rare and short; decision 1 records why a shared pool was not worth an `IDisposable`.
- **[Facts that assumed one global order]** A fact asserting the order of git requests across folders would now be racy. → The task list runs the suites and loosens any such fact to per-folder order; `AskGitThreeThingsPerRegisteredWorktreeAndNothingAboutMergeState` filters by worktree and stays as it is.
- **[A held fact on the old code]** The inspection fact fails by timeout, not assertion, at red. → Named in decision 5 and in its task, so the red step is recognised for the right reason; the others fail on a start count.
- **[The synchronous start path]** The exact start count rests on nothing between `ScanAsync` and the runner yielding first. → If a later change adds such an await, the bound fact fails loudly rather than passing wrongly, and it moves to the inspection fact's awaited-start pattern.

## Open Questions

- Should a folder under one repository's `.claude\worktrees` that a later repository registers stop showing as an orphan as well as a registered row? Decision 3 keeps today's rows; changing it would be its own small behaviour change with a spec scenario.
