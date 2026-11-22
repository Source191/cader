## Context

See proposal.md, Why. What the code does today:

- `GitHubPullRequests` runs `gh pr list --head <branch> --state all --json number,url,state,isDraft,headRefName,headRefOid,createdAt`; `AzureDevOpsPullRequests` runs `az repos pr list --source-branch <branch> --status all` and reads `creationDate` and `lastMergeSourceCommit`. Neither reads when a pull request finished.
- `PullRequestCatalogue.RefreshAsync` lists one branch per lookup off the UI thread and keeps the `Answer` (listing, host, time) for a minute; `Find` hands the branch's rows to `Domain\PullRequests.ForBranch`, which takes the open one, otherwise the newest by `CreatedAt`.
- `BranchFlags` reads the chosen pull request: `NoPullRequest` only for the host's own "none", and left behind only after a merge with a recorded head. The left-behind count already ignores commits origin's default branch has (`GitCli.CountCommitsAfterAsync`), which is why the reused `c-status` showed `#371 merged` but not `▲ n commits after #371`.
- Checked on 22 November 2026, git 2.56.1.windows.1: `git reflog show --date=unix --format=%gd%x09%gs refs/heads/c-status` printed `c-status@{1795338771}	branch: Created from origin/main` for the branch `git worktree add <path> -b c-status origin/main` made (Cader's own `AddWorktreeAsync`); `main`'s oldest entry was `clone: from https://github.com/Source191/cader.git`. `gh pr view 371 --json closedAt,mergedAt` gave the same time for both on a merged pull request.
- Azure DevOps documents [`closedDate`](https://learn.microsoft.com/dotnet/api/microsoft.teamfoundation.sourcecontrol.webapi.gitpullrequest.closeddate?view=azure-devops-dotnet) as "the date when the pull request was closed (completed, abandoned, or merged externally)", serialised only when set.

## Goals / Non-Goals

**Goals:** a finished pull request from an earlier branch of the same name stops showing; nothing that shows correctly today stops showing; the rule is one pure Domain function with sociable tests.

**Non-Goals:** origin rows (no local branch, no reflog); any change to caching, slots, the PR key's wording or `BranchFlags`.

## Decisions

### 1. The branch's birth comes from the reflog's oldest entry, and only from git's "branch: Created from"

`IGit.BranchCreatedAtAsync(FolderPath repository, BranchName branch, CancellationToken)` answers `DateTimeOffset?`: `GitCli` runs `git reflog show --date=unix --format=%gd%x09%gs refs/heads/<branch>` (a full ref name, so the branch can never read as an option, as `CountCommitsAfterAsync` already insists), takes the last line, and answers its `@{<seconds>}` as UTC when the subject after the tab starts with `branch: Created from`; null for no output, another subject (`clone: from`, or an oldest entry left after expiry), an unreadable line or a git failure. That is the IGit house shape for a query (null when there is no answer). `git branch`, `git checkout -b`, `git switch -c` and `git worktree add -b` all write that subject, and a rename (`git branch -m`) moves the reflog with its oldest entry intact. The reflog lives in the shared repository, so asking the main checkout answers for every worktree's branch.

Alternatives: the first commit unique to the branch (absent on a fresh branch, which is exactly the case to fix); the PR's head commit being an ancestor of the branch (a fresh branch from main contains a merged head, so it cannot tell reuse from pulling main after a merge); Cader's own record of worktrees it created (misses branches made outside Cader).

### 2. A finished pull request carries `Finished`, a `RecordedTime`

A new Domain record `RecordedTime` with the cases `At(DateTimeOffset)` and `NotRecorded`, shaped like `HeadCommit` (Core stays C# 14, so it is an abstract record with sealed nested cases, and the solution's models never use null for meaning). `PullRequest` gains `Finished`. `GitHubPullRequests` adds `closedAt` to its `--json` list; `AzureDevOpsPullRequests` reads `closedDate`. Each sets `At` only for a finished state (merged, closed, completed, abandoned) whose value parses as an invariant UTC time, and `NotRecorded` otherwise, so an open pull request with any value, or a finished one with a missing, blank or unreadable value, is never left out. The branch's birth is the same type: the catalogue turns the port's null into `NotRecorded` on the line that receives it.

Alternative: compare the pull request's `CreatedAt` with the branch's birth, needing no new field. Rejected: a local branch made to review a colleague's pull request is younger than the pull request, which would then vanish once it merged.

### 3. The rule lives in `PullRequests.ForBranch`

`ForBranch(IReadOnlyList<PullRequest> forBranch, RecordedTime branchCreated)` first drops each pull request whose `Finished` is `At` a time strictly before `branchCreated`'s `At`, then chooses as today. Strictly before, because the reflog is in whole seconds and a tie is no evidence of reuse. With everything dropped the answer is `Missing(NoPullRequest)`, the host's own "none" for this branch, so the Worktrees tab flags it · no PR and the pull request key's existing words apply with no new gap or wording.

### 4. The catalogue reads the birth with each lookup and keeps it in the answer

`RefreshAsync` asks `git.BranchCreatedAtAsync(checkout, branch)` beside the listing and stores it in `Answer`; `Find` passes it to `ForBranch`. It is read once per lookup (once a minute per branch on screen, inside the existing slot-free part of the lookup, since git is local and quick), so a branch deleted and recreated under the same name is seen within the window. A failed git read is `NotRecorded` and changes nothing.

## Risks / Trade-offs

- [The host's clock and the machine's differ] → only a branch recreated within the skew of a merge is misjudged, and only towards today's behaviour or towards a blank column for minutes-old work; accepted.
- [Reflog expiry (90 days by default for reachable entries) removes the "Created from" entry] → the oldest remaining subject is not "Created from", so the answer is `NotRecorded` and today's behaviour returns.
- [`core.logAllRefUpdates` turned off] → no reflog, `NotRecorded`, today's behaviour.
- [A user deliberately keeps a branch after its merge, deletes it locally and checks it out again from origin to look at it] → its old pull request disappears from the row; the branch really is new, and the PR is still reachable on the host.
- [One more git process per branch lookup] → a reflog read is a few milliseconds against the host CLI's second or more, inside a lookup that already runs at most once a minute per branch.
