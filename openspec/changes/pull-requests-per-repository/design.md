## Context

See proposal.md, Why. `PullRequestCatalogue` keys answers by `<checkout key>|<branch>`, keeps a listed answer 60 s and an unavailable one three minutes, shares a running lookup through a `Lazy<Task>` per key, and runs at most four host calls at once. Each lookup reads `git remote get-url origin`, parses it as a `RemoteRepository` and asks `IPullRequests.ListAsync(repository, checkout, branch)`, which runs `gh pr list --head <branch> --limit 20` or `az repos pr list --source-branch <branch> --top 20`. `PullRequests.ForBranch` picks the row's pull request from a branch's list (an open one wins over older ones).

## Goals / Non-Goals

**Goals:** one host call per repository a minute, the same rows as today.

**Non-Goals:** a longer refresh for open pull requests; the web APIs.

## Decisions

### 1. Two kinds of answer in one catalogue

The catalogue keeps a repository answer per checkout key (the listing of its latest 100 pull requests, or unavailable, with when and whether the listing was full, that is held exactly 100) and a branch answer per checkout and branch, used only for the fallback. `Find(checkout, head)` answers from the repository listing when it holds the branch, else from a branch answer when there is one, else `Looking` while the repository's first answer is pending, else none (a listing that was not full is conclusive).

`RefreshStaleAsync(branches)` groups the branches by checkout, refreshes each stale repository answer (60 s, three minutes when unavailable) through the same lazy, shared, four-slot lookup, and once a repository answer lands, starts a branch lookup for each branch of that group the listing lacks, only when the listing was full and the branch answer is older than 15 minutes. Why 100: it is `gh`'s and `az`'s usual page, and the spec's existing "older open pull request" scenario already names the latest 100.

Alternative considered: keep per-branch lookups and lengthen their window; rejected because a newly opened pull request would then take minutes to show.

### 2. Adapters gain a repository listing, keep the branch lookup

`IPullRequests.ListRepositoryAsync(repository, checkout)` beside the existing `ListAsync(repository, checkout, branch)`. GitHub: `pr list --repo <owner>/<name> --state all --limit 100 --json <same fields>`. Azure DevOps: `repos pr list --org ... --project ... --repository ... --status all --top 100 --output json`. Each row already carries its branch (`headRefName`, `sourceRefName`), which `GitHubPullRequests` and `AzureDevOpsPullRequests` read today, so the parsing is shared. The repository listing passes no branch to az, so the batch-file rule (`BatchFileArgument.IsSafe`) still guards only the per-branch fallback, and a branch that fails it is matched from the listing.

### 3. The branch's creation time stays per branch

`ignore-reused-branch-pull-requests` (step 4 of the implementation plan, before this change) reads each branch's creation time from its reflog with each lookup and hands it to `PullRequests.ForBranch`. Here it is read for each on-screen branch of a repository with that repository's lookup (and with a branch's fallback lookup), kept per checkout and branch beside the repository answer, and passed to `ForBranch` by `Find` as before. Its catalogue facts must still pass unchanged.

### 4. Origin read per repository

`git remote get-url origin` runs once per repository lookup; the branch fallback reuses the repository answer's parsed remote, so a repository that moves host is still followed within a minute.

## Risks / Trade-offs

- [A repository listing of 100 pull requests is a larger answer than a branch's 20] → one call instead of several; the time is measured by the existing pull-request refresh count, which the PR description reports before and after.
- [A busy repository opens more than 100 pull requests a minute] → not realistic; the fallback still finds the branch.
