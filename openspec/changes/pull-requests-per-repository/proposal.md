## Why

Pull-request lookups are the slowest and most network-heavy thing Cader does. In an hour of real use it ran `az repos pr list` 393 times (median 10.2 s, 95th percentile 18 s; the Azure CLI is Python and costs a second or more of CPU just to start), `gh pr list` 175 times and `git remote get-url` 568 times, and a pull-request refresh took 14.6 s at the median and 73 s at the 95th percentile. Every lookup asks the host about one branch, so a repository with eight branches on screen costs eight host calls a minute where one listing of its recent pull requests answers all eight.

**Implement with:** Opus 5.5 at medium effort: it reworks `PullRequestCatalogue`'s keying and windows over the existing lookup pattern (lazy shared lookups, four slots) and changes two adapters' arguments, with sociable catalogue tests and the adapters' existing facts; no new threading model.

## What Changes

- Cader looks up a repository's pull requests with one listing of its latest 100 (all states) a minute, for the repositories that have a row on screen, and each row takes its branch's pull requests from that listing.
- A branch that is not in a listing of exactly 100 (so possibly older than all of them) is looked up on its own, as today, but that answer is kept for 15 minutes, since an old branch's pull request seldom changes. When the listing holds fewer than 100, a branch missing from it has none.
- The origin remote is read once per repository lookup, not once per branch.
- On Azure DevOps a branch whose name is not plain text for a batch file still never reaches az, but its row now shows a pull request the repository's listing holds for it.
- Unavailable answers (not signed in, CLI missing) are still kept for three minutes, now per repository.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `pull-requests`: lookups are made per repository, with a per-branch fallback for branches older than the latest 100; a branch name az must not be given still shows a pull request found in the repository's listing.

## Delivery

- **Order:** 3 of 11, wave 1 (cheap wins). Best after `fewer-background-polls`, whose spec edits it does not touch, so either order works; independent of the scan changes.
- **Together:** ships alone, in one pull request.
- **Expected saving:** one host call per repository a minute instead of one per branch; on the measured hour that is roughly the 568 lookups divided by the branches each repository had on screen. Where every repository has only one branch on screen it saves nothing but the `git remote get-url` runs.

## Out of scope

- Calling the hosts' web APIs directly instead of `gh` and `az` (it would need token handling the CLIs do today).
- Changing what the PR column, the detail pane or the pull-request key show.
- Multiple Claude accounts beyond naming a session's owner, a later change.

## Impact

- `Application\PullRequestCatalogue`: answers keyed by repository and by branch for the fallback, the 15-minute fallback window, the origin read per repository.
- `Ports\IPullRequests` and `Adapters\PullRequests\GitHubPullRequests` and `AzureDevOpsPullRequests`: a repository listing (`gh pr list --repo <owner>/<name> --state all --limit 100 --json ...`; `az repos pr list ... --status all --top 100 --output json`) beside the branch lookup.
- `test\Source191.Cader.TestDoubles\FakePullRequests`.
- `CHANGELOG.md` and `Version` in `src\Source191.Cader\Source191.Cader.csproj`.
- **Implement with:** Opus 5.5 at medium effort (reason above).
