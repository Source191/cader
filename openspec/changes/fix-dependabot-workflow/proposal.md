# Proposal

## Why

On 13 November 2026 every Dependabot pull request (#226 to #237) failed `check`, and the five updates had to be brought in by hand in #246. `dependabot-version-bump` (archived 30 October 2026) meant a Dependabot pull request to go green with no hand edits, but it never has: its owner task to create the token was left unticked, and three faults in the workflow would stop it even with the token. Dependabot runs weekly, so the same failures come back every week until they are fixed.

## What Changes

- **The token:** the owner creates `DEPENDABOT_BUMP_TOKEN` (a fine-grained token, Contents read and write on `Source191/cader`, which was `Source191/vantage` when the first change was written) as a Dependabot secret. When it is missing, the bump step fails with an error that names the secret and MAINTAINING.md's section, instead of a notice followed by a version check that asks for a hand bump.
- **The format step:** the Dependabot branch's second checkout is removed straight after the bump step, so `dotnet format whitespace --folder` no longer reads `dependabot-branch\openspec` (which failed the four test-only pull requests, #228, #233, #236 and #237) and `hashFiles('**/*.csproj')` no longer counts its project files in the NuGet cache key.
- **A stable base:** on a stable `Version`, the bump step starts a preview of the next patch (`0.21.1` becomes `0.21.2-preview.1`) and writes its line under Unreleased, Internal, which `Assert-ChangelogUpdated.ps1` accepts. A Dependabot merge is then a GitHub prerelease, and the owner decides when the next stable release collects those lines. On a preview base it raises the preview number, as today. `Step-Version.ps1` gains a `-Preview` switch for this; run by hand without it, the script behaves as it does now.
- **Rebasing:** the bump commit's message carries `[dependabot skip]`, so Dependabot keeps rebasing the pull request (dropping the bump commit) when `main` moves, and the step raises `Version` again from the new base. Today Dependabot stops rebasing a pull request once anyone else has pushed to it.
- **One weekly pull request:** NuGet version updates come as one grouped pull request (`groups` with `patterns: ["*"]`, applying to version updates), so a week's updates need one bump and make one release. Security updates still arrive one at a time and at once. The actions' pins keep their own pull requests and the CodeQL group.
- **Docs:** MAINTAINING.md's "Dependabot and the version bump token", CONTRIBUTING.md's Versions and changelog paragraphs, `.claude\rules\checks.md`, and the comments in `dependabot.yml` and `pipeline.yml` say what the workflow now does, including `@dependabot recreate` as the way to start a stuck pull request again.
- **No behaviour change to Cader:** only `.github` and the docs change, so this change declares `skip_specs: true` and raises no `Version`.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None. No spec covers the workflows; `installation` and `tool-updates` describe what users install and are offered, which this change leaves as it is.

## Impact

- **Workflows:** `.github\workflows\pipeline.yml` (the Dependabot steps of the `check` job), `.github\dependabot.yml`.
- **Scripts:** `.github\scripts\Step-Version.ps1` (the `-Preview` switch).
- **Docs:** `MAINTAINING.md`, `CONTRIBUTING.md`, `.claude\rules\checks.md`.
- **The owner:** creating the token and recording its expiry; reviewing and merging the weekly pull request as now.
- **Releases:** Dependabot merges on a stable base become prereleases rather than stable releases; nothing reaches winget without `publish.yml`, as now.

## Out of scope

- Multiple Claude accounts, which is a later change.
- Auto-merge of Dependabot pull requests: the owner's review and merge stay.
- A GitHub App in place of the personal access token.
- Grouping the actions' pins beyond the existing CodeQL group, and grouping security updates.
- Re-running or reopening #226 to #237: #246 superseded them.
- Changing `Assert-VersionBumped.ps1`, `Assert-ChangelogUpdated.ps1` or the release rules they enforce.
