## Why

Cader finds a row's pull request by branch name alone, and when none is open it shows the most recent. A branch name reused for new work therefore inherits the old branch's pull request: a `c-status` worktree created from `origin/main` at 09:12 on 22 November 2026 showed `#371 merged` on its brand-new session, because an earlier `c-status` branch's #371 had merged at 14:28 the day before. The row says the work is merged when it has not started, and the Worktrees tab's BRANCH column cannot say · no PR for it.

## What Changes

- A merged, closed, completed or abandoned pull request that finished before the row's branch was created is not that branch's and is left out of its answer, as if the host had not listed it; what remains is chosen as today (an open one, otherwise the most recent), so the row shows a later pull request, or none.
- The branch's creation time is the oldest entry of git's reflog for it, when that entry is git's own "branch: Created from" record. With no such entry (the reflog expired, the branch came from a clone, or git could not answer) nothing is left out, as today.
- Each listed pull request carries when it finished: `closedAt` from the GitHub CLI (set for merged ones too), `closedDate` from the Azure CLI. An open pull request is never left out, so checking out a colleague's open pull request locally still shows it.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `pull-requests`: a reused branch name no longer shows an earlier branch's finished pull request, and each pull request records when it finished.

## Out of scope

- Telling reused names apart by anything but the local reflog (commit ancestry, the host's own branch history), and branches with no local branch (origin rows), which have no reflog and keep today's behaviour.
- Warning about a reused branch name in the new-session wizard.
- Multiple Claude accounts beyond naming a session's owner, a later change.

## Impact

- **Implement with:** Opus 5.5 at medium effort: one pure Domain rule, two adapter fields and one git query, all behind existing ports and caught by the sociable and real-git tests.
- `Domain`: `PullRequest` gains when it finished (a `RecordedTime`), and `PullRequests.ForBranch` takes the branch's creation time.
- `Ports\IGit` and `Adapters\Git\GitCli`: a new `BranchCreatedAtAsync` query reading the reflog; `TestDoubles`' fake git answers it.
- `Adapters\PullRequests`: `GitHubPullRequests` asks for `closedAt`, `AzureDevOpsPullRequests` reads `closedDate`.
- `Application\PullRequestCatalogue`: each lookup also reads the branch's creation time and keeps it with the answer.
- `docs\user-guide.md`, `CHANGELOG.md`, `.claude\rules\windows.md` (the reflog fact) and the version in `src\Source191.Cader\Source191.Cader.csproj`.
