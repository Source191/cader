# Proposal

## Why

The Worktrees tab lists a ○ origin row for each branch of yours that exists only on origin and whose pull request was closed or left work behind. It reads origin's branches from git's own records as of the last fetch (`refs/remotes/origin/*`), and those records lose a deleted branch only when a full `git fetch --prune` or `git remote prune origin` runs. Cader never runs either: its only fetch is the one branch it brings up to date before branching (fetch-before-branching), which updates `FETCH_HEAD` but prunes nothing else. So a branch deleted on origin weeks ago stays on the tab as ○ origin ✗ closed until you run git yourself, even with `fetch.prune` set. On the owner's machine, 20 November 2026, 12 such rows were showing across 8 repositories; in MK.Sales.Agents, which Cader had fetched on 16 November, `git remote prune origin --dry-run` listed all three of its rows as stale.

The same stale records hide a worktree's or a branch row's ✗ gone flag: git reports an upstream as gone only once its remote-tracking branch has been pruned.

## Benefit

- **Users:** branches deleted on origin leave the Worktrees tab by themselves within a scan or two, and a local branch whose upstream was deleted reads ✗ gone without a manual prune, so the tab shows what still needs attention rather than history.
- **Maintainers:** one small background step beside the pull request lookups, held by facts over a scripted git, with no new key, setting or screen.

## What Changes

- **Prune after a scan:** when a worktrees scan shows a ○ origin row, Cader runs `git remote prune origin` in that row's repository in the background, never awaited, with the same no-prompt settings as the fetch before branching. The next scan, timed, heard or by F5, no longer lists the branches origin has deleted.
- **At most once an hour per repository:** each repository is pruned at most once an hour, counted from the attempt, whether it succeeded or not, so an offline machine or a repository needing sign-in costs one quick failure an hour; a failure is recorded once per repository in the diagnostic log, as the scan's own git failures are, and nothing is shown on screen.
- **The scan itself still never reaches the network:** the requirement that a scan judges branches from git's records as of the last fetch stays; only the prune after it reaches origin, and only for repositories that show an origin row.
- **README scene:** the scene's process runner answers the prune as done, as it does a fetch, so the screenshot never reaches the network and keeps its origin row.
- **Version and changelog:** a preview step and an Added entry.

Out of scope:

- A setting to turn pruning off: it only deletes git's records of branches origin no longer has, which `fetch.prune` would delete on any full fetch; an offline machine costs one failed attempt an hour. It can follow if someone asks.
- A full `git fetch --prune`: it would download new branches and commits, which no row needs.
- Pruning repositories with no ○ origin row, or remotes other than origin.
- An SSH key with a passphrase: the no-prompt settings do not cover it, so such a prune waits out the 60-second limit in the background, as the fetch before branching does (`windows.md`).
- Deleting anything on origin: Cader still never does.
- Multiple Claude accounts beyond naming a session's owner, which is a later change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `worktree-management`: "Branches are judged without reaching the network" gains a prune after the scan for repositories showing an origin row, and a new requirement says when that prune runs and that a pruned branch's row goes.

## Impact

- **Implement with:** Opus 5.5 at medium effort: a background step modelled on the pull request lookups, with a clock-based rule and scripted-git facts; the network call is the one risk, and it reuses the fetch's no-prompt settings.
- **`src`:** `Ports\IGit` and `Infrastructure\Git\GitCli` gain `PruneOriginAsync`; a new `Application\OriginPruner` holds the per-repository rule; `Application\WorktreeInventory` starts it after a scan; `Source191.Cader\Composition\ConsoleComposition` constructs it.
- **`tools`:** `ReadmeScene\SceneHost` constructs the pruner as the console does, and `SceneProcessRunner` answers the prune.
- **Tests:** `OriginPrunerShould` (new) and `WorktreeInventoryBranchesShould` (Core.Tests); `GitCliShould` and `GitCliIntegrationShould` (Infrastructure.Tests); the scene runner's facts in `ReadmeScene.Tests`.
- **Docs:** `.claude\rules\console-screen.md`'s scanning paragraph says what runs after a scan; the user guide's Worktrees section says origin rows go once origin has deleted the branch.
- **Version and changelog:** `Version` takes the next preview step when it merges; `CHANGELOG.md` gets an Added entry.
- **Users:** stale ○ origin rows disappear, and some local branches newly read ✗ gone once their upstream is pruned.
