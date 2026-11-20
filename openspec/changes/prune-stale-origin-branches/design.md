# Design

## Context

`WorktreeInventory.ListedBranchesAsync` makes an origin candidate for each `GitBranch.OriginOnly` of yours (a `refs/remotes/origin/*` ref with no local branch), and `ShownAsync` shows it once its pull request flags it left behind or closed. Those refs are git's records as of the last fetch. The only fetch Cader makes is `GitCli.FetchBranchAsync`, one explicit refspec before branching, which never prunes other refs, so nothing in Cader ever removes a ref origin has deleted. A local branch's `[gone]` upstream, which `GitBranchListing` reads for ✗ gone, has the same source: git reports it only once the remote-tracking ref is pruned.

The scan already starts one piece of background work it never awaits: `pullRequests.RefreshStaleAsync(...)`, whose results the next scan shows. The Worktrees tab rescans on show, on F5, at the Worktrees refresh interval while shown and on a heard folder change (`console-screen.md`), so work landing after a scan reaches the screen within one interval. `ChangeFilter` ignores reference changes, so a prune does not itself start a rescan; that is fine, the timer picks it up.

## Goals / Non-Goals

**Goals:**

- A branch origin has deleted leaves the tab without the user running git, at the cost of at most one `git remote prune origin` per repository per hour, and only for repositories that show an origin row.
- The scan stays network-free: the prune runs after it and is never awaited by it.

**Non-Goals:**

- A setting, a key, a status message or any on-screen trace of the prune.
- Fetching new branches or commits, pruning remotes other than origin, or pruning repositories with no origin row.

## Decisions

1. **`git remote prune origin`, not `git fetch --prune`.** It asks origin for its branch list and deletes only the remote-tracking refs origin no longer has; it downloads no objects and moves no ref that still exists, so it cannot change any flag but gone, and a large repository costs the same as a small one. `fetch --prune` would also bring in every new branch and commit, which no row needs, and `fetch.prune` in the user's config is irrelevant to either.

2. **`IGit.PruneOriginAsync(FolderPath repository, CancellationToken)`** in `Ports\IGit`, implemented in `GitCli` as `RunOrThrowAsync(repository.Value, ["remote", "prune", "origin"], NoPrompts, cancellationToken)`: it throws `GitException` with git's words when git fails, as the other commands do, and its XML comment says so. `NoPrompts` (`GIT_TERMINAL_PROMPT=0`, `GCM_INTERACTIVE=false`) is the same dictionary the fetch uses, so a remote needing sign-in fails at once rather than opening Git Credential Manager's window over the console (`windows.md`, 14 November 2026). `remote` and `prune` are already in `ChildCommandWords.Git`, so the diagnostic record names the command without change.

3. **`Application\OriginPruner(IGit git, TimeProvider clock, DiagnosticLog log)`** holds the rule, so `WorktreeInventory` only says which repositories showed an origin row:
   - `Task PruneDueAsync(IEnumerable<FolderPath> repositories)` takes each distinct repository whose last attempt is an hour or more ago (or never), claims it by recording now as its attempt before git runs, and starts its prune; it answers a task for all of them, so its facts can await it, and `WorktreeInventory` discards it as it does the catalogue's.
   - The attempt time is kept in a `ConcurrentDictionary<FolderPath, DateTimeOffset>` and claimed with `TryAdd` or `TryUpdate` against the value read, so two scans overlapping (one runs at a time today, but the reload after an action is not skipped) never prune one repository twice.
   - Counting from the attempt, not the success, keeps an offline machine or a repository needing sign-in to one quick failure an hour. A failure is a `GitException` caught through an `OncePerFolder` at `FailureSite.BackgroundTask`, so a repository that keeps failing adds one event per process run and never a path; anything else reaches `CrashRecorder.RecordUnobserved` as any unobserved background fault does.
   - It runs with `CancellationToken.None`, as the pull request lookups do: the scan's token ends with the scan, and `ProcessRunner`'s 60-second limit bounds the process.
   - Alternative rejected: keeping the times in `WorktreeInventory`, which would give the inventory a clock and a second background concern beside the catalogue's; a class of its own is testable over a `FakeTimeProvider` and a scripted git without a scan.

4. **Only repositories showing an origin row.** `WorktreeInventory.ScanAsync` hands the pruner the `Repository` of each row `ShownAsync` returns whose `Health.Kind` is `WorktreeHealthKind.Origin`. A candidate still waiting on its pull request lookup is not shown and not yet a reason to reach the network; it is pruned once it shows, at most a scan later. Repositories with only stale gone upstreams are left alone: a gone flag without an origin row is a nice side effect, not a reason to touch every repository hourly.

5. **The README scene answers the prune.** `SceneProcessRunner.IsFetchFromARemote` widens to answer `git remote prune origin` as done too (renamed for what it answers, for example `ReachesARemote`), because the scene's remotes are made up and the scene never reaches the network; the scene's origin row therefore stays for the screenshot. `SceneHost` constructs `OriginPruner` as `ConsoleComposition` does, so a changed `WorktreeInventory` constructor fails the build there, as CLAUDE.md expects.

## Risks / Trade-offs

- **An SSH key with a passphrase** is not covered by `NoPrompts`; such a prune waits out the 60-second limit in the background once an hour per repository. The fetch before branching already accepts this (`windows.md`); nothing waits on it here.
- **A row lingers for up to one refresh interval** after its prune lands (15 seconds by default, never on a hidden tab until it is shown), because a reference change starts no rescan. Accepted: watching refs would make every fetch and commit rescan.
- **More rows newly read ✗ gone** once their upstream is pruned. That is the truth the tab was missing, and the BRANCH column already explains it.
- **Network use is now made without a key press.** Bounded to repositories showing an origin row, once an hour each; the proposal lists a setting to turn it off as a possible follow-up.
