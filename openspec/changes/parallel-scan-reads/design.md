## Context

See proposal.md, Why. `ConsoleLoads.ScanWorktreesAsync` awaits `LatestSessions.SessionsAsync` (which waits for the first listing) before calling `WorktreeInventory.ScanAsync(sessions)`. The scan lists repositories through an eight-slot `SemaphoreSlim`, merges each repository's results in name order, then `ShownAsync` sets each row's session and awaits `FlaggedAsync` row by row (`CountCommitsAfterAsync`, two `rev-list` runs, outside any slot). `ListedBranchesAsync` awaits each unused branch's `CountCommitsOnNoRemoteAsync` in turn, each in a slot. `InspectedAsync` holds one slot for all of `WorktreeInspector.CountAsync`. `SessionRowEnricher.EnrichAsync` awaits `LocateAsync` per session in turn.

## Goals / Non-Goals

**Goals:** the same rows in the same order, with no read waiting on another without need.

**Non-Goals:** fewer reads (wave 1 and `rescan-only-what-changed`); a bigger pool.

## Decisions

### 1. Sessions as a task, awaited at the merge

`ScanAsync` takes a `Task<IReadOnlyList<Session>>` and awaits it only in `ShownAsync`, after every repository's reads have returned. `ConsoleLoads` passes `sessions.SessionsAsync(token)` unawaited. The scan's refresh time stays the git work alone: the timing wraps the inventory's reads and stops before the sessions are awaited, so `diagnostics`' "the scan waits for the first listing" scenario still holds (the wait is not counted).

Why this is safe now: the comment that the start-up scan "never competes" with the listing dates from before the pool and wave 1; `claude agents` is one process against git's many, and the measured cost of waiting (the whole listing) is larger than any contention it saves. The PR description reports the start-up scan's time before and after.

### 2. One slot per read

`WorktreeInspector.CountAsync` gains an overload taking the pool, running its reads as separate `InSlotAsync` calls started together and awaited together; the detail pane keeps the plain form. Unused branches start all their counts together, each in a slot, and `rows.AddRange` keeps listing order by awaiting in order. `FlaggedAsync` runs for every row at once, each count in a slot, and `ShownAsync` keeps the rows' order by awaiting the tasks in order. The slot is still held only around one git run, so no read waits on another for a slot (the rule `InSlotAsync` documents).

### 3. Enrichment through its own pool of eight

`EnrichAsync` starts `LocateAsync` for every distinct folder through a pool of eight and builds the rows in listing order once all have answered; the 30-second location cache and the folder-gone check are unchanged. Distinct folders, so two sessions in one folder share one read.

## Risks / Trade-offs

- [An order-dependent bug: a parallel merge changes which orphan rows appear] → the merge step itself is unchanged (it runs after every repository's reads, in name order); `WorktreeInventoryLeftoversShould` keeps its orphan facts and gains one with reads that finish in reverse order.
- [Cancellation leaves reads running] → `InSlotAsync` already checks the token once the slot is held; a fact cancels mid-scan and asserts no read starts afterwards.
- [Eight more git processes at once during start-up while `claude agents` runs] → measured in the PR; the pool is unchanged.
