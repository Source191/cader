# Design

## Context

`pipeline.yml`'s `check` job checks out the merge ref, then, for `github.actor == 'dependabot[bot]'`, checks out the head branch into `dependabot-branch` (inside the workspace, because `actions/checkout` refuses a path outside it) and runs "Raise the version for Dependabot" there. That step runs `Assert-VersionBumped.ps1 -Base origin/<base> -Head HEAD`; on a missing bump it runs `Step-Version.ps1` and `Add-InternalChangelogEntry.ps1`, commits as the Actions bot and pushes with `DEPENDABOT_BUMP_TOKEN`, and the pushed commit's run (whose actor is the token's owner, so the Dependabot steps are skipped) is the one branch protection reads. The Dependabot secret list was empty on 13 November 2026. `Assert-ChangelogUpdated.ps1` requires a stable `Version` to have its own section and an empty Unreleased. The "Owner approves main" ruleset does not require approval of the last push (`require_last_push_approval: false`), so the owner can approve a pull request whose last commit their token pushed. GitHub Docs (dependabot-options-reference, checked 13 November 2026 through Context7): Dependabot stops rebasing a pull request once extra commits are pushed to it unless their message contains `[dependabot skip]`; `groups` with `patterns: ["*"]` puts every matching update in one pull request, `applies-to` defaults to `version-updates`.

## Goals / Non-Goals

**Goals:**
- A Dependabot pull request, whether it touches the tool or not, goes green on its own once the token exists.
- Hand use of `Step-Version.ps1` is unchanged.

**Non-Goals:**
- Making the bump step itself testable beyond the scripted checks below; the scripts have no test framework and this change adds none.

## Decisions

- **Remove the second checkout in its own step.** A step straight after the bump step, with the same Dependabot condition plus `always()`, runs `Remove-Item -Recurse -Force dependabot-branch` (from the workspace root, since the bump step's working directory is that folder). Every later step then sees only the merge ref.
  - *Rejected: `--exclude dependabot-branch` on the format step.* It fixes the symptom in one place while the cache key's `hashFiles` and any later folder scan still read the copy.
- **`-Preview` on `Step-Version.ps1`.** With the switch, a stable `X.Y.Z` becomes `X.Y.(Z+1)-preview.1`; a prerelease raises its trailing number exactly as without the switch. The bump step passes it; nothing else does. The changelog line then goes under Unreleased, Internal, as `Add-InternalChangelogEntry.ps1` already writes it.
  - *Rejected: a stable patch with its own section* (the owner chose previews, 13 November 2026): each Dependabot merge would become the Latest release with notes holding only Internal lines.
- **`[dependabot skip]` in the bump commit's message**, so Dependabot force-pushes over it when it rebases; the step then raises `Version` again from the rebased csproj, which makes the original design's assumption true.
- **A missing token is an error.** `::error::` naming `DEPENDABOT_BUMP_TOKEN` and MAINTAINING.md's section, then `exit 1`, so the run stops at the cause. The old comment that an empty secret means a person pushed is dropped: a person's push has that person as the actor, so the step never runs for it.
- **One `nuget` group** with `applies-to: version-updates` and `patterns: ["*"]`; the pull request's title ("Bump the nuget group with N updates") becomes its changelog line, and the pull request lists the packages.
  - *Rejected: build the line from `dependabot/fetch-metadata`.* Another action holding the run's token, for a line the What's new dialog never shows.

## Risks / Trade-offs

- [A grouped pull request with a breaking update fails the build] → the owner comments `@dependabot ignore <package> major version` or fixes it on a branch of their own, then `@dependabot recreate`.
- [The token expires] → the push fails with the error naming the secret; its expiry is recorded in tasks.md and MAINTAINING.md says how to renew it.
- [No Dependabot update is due when this merges] → the live check (task 4.2) waits for the next weekly run or a security update, and the change is archived only after it is recorded.

## Migration Plan

Merge, create the token, then run Insights, Dependency graph, Dependabot, "Check for updates" for NuGet. Rolling back is reverting the pull request; the secret can stay.
