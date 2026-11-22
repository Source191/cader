# Tasks

**Implement with:** Opus 5.5 at medium effort: it reworks `PullRequestCatalogue`'s keying and windows over the existing lookup pattern (lazy shared lookups, four slots) and changes two adapters' arguments, with sociable catalogue tests and the adapters' existing facts; no new threading model.

Each task is red, green, refactor, with a commit per step. Catalogue facts use `FakePullRequests` and `FakeTimeProvider`; adapter facts use `FakeProcessRunner` and never reach a host.

## 1. Adapters

- [ ] 1.1 `GitHubPullRequestsShould.ListARepositorysLatestHundred` (the arguments: `--repo`, `--state all`, `--limit 100`, the same `--json` fields, no `--head`) and `ReadEachRowsBranch`; the shared row parsing stays as today. Commit.
- [ ] 1.2 `AzureDevOpsPullRequestsShould.ListARepositorysLatestHundred` (`--status all --top 100`, no `--source-branch`) and `GiveNoBranchToAzForARepositoryListing`. Commit.
- [ ] 1.3 `IPullRequests.ListRepositoryAsync` routed by host in `PullRequestHosts`; `FakePullRequests` answers it. Verify the adapters' existing branch facts still pass. Commit.

## 2. Catalogue

- [ ] 2.1 `PullRequestCatalogueShould.LookUpARepositoryOnceForSeveralBranches` and `FindEachBranchInItsRepositorysAnswer`. Commit.
- [ ] 2.2 `PullRequestCatalogueShould.AnswerNoneForABranchMissingFromAShortListing` and `AnswerLookingBeforeTheRepositorysFirstAnswer`. Commit.
- [ ] 2.3 `PullRequestCatalogueShould.LookUpABranchMissingFromAFullListing` and `KeepThatBranchAnswerFifteenMinutes`. Commit.
- [ ] 2.4 `PullRequestCatalogueShould.KeepAnUnavailableRepositoryAnswerThreeMinutes`, `ReadTheOriginOncePerRepositoryLookup` and `RunAtMostFourLookupsAtOnce` (twelve repositories). Commit.
- [ ] 2.5 `PullRequestCatalogueShould.MatchAnUnsafeAzureBranchFromTheListingOnly`: a branch failing `BatchFileArgument.IsSafe` is never looked up on its own and takes a pull request from the repository's answer. Commit.
- [ ] 2.6 `.claude\rules\security.md`, Programs and arguments: the repository listing gives az no branch. Commit.

## 3. Release

- [ ] 3.1 Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` one preview step above what `main` holds when this merges and add a CHANGELOG `Unreleased` entry under Changed (pull requests looked up per repository; far fewer gh and az runs). Record the pull-request refresh and `az repos pr list` counts from a diagnostic log before and after in the pull request description. Verify `.github\scripts\Assert-VersionBumped.ps1`, `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes` and `openspec validate pull-requests-per-repository`. Commit.

## Workflow follow-up

- Archive the change in the pull request that completes it.
