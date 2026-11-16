# Implementation plan for the open changes

Rewritten 2026-11-16 against `main` at 0.22.0-preview.30 (`45bbaceb`), when the stack of 14 November had merged and every change in it was archived. It changes no code, so it needs no version bump.

## Contents

- Rules for every implementing session
- The stack, in order
- Then a stable release
- Deferred until after that release
- Owner gates

## Rules for every implementing session

- **One stack, built in order:** each pull request branches from the one below it and targets that branch, opens with a "Stacked on #N" line, and is merged bottom first. Nothing in the stack runs in parallel.
- **Versions:** each pull request that touches `src\` runs `.github\scripts\Step-Version.ps1` once on top of the branch below it and adds its `CHANGELOG.md` line under `## Unreleased`. Nothing in the stack goes stable.
- **Archive:** a change is archived only in the pull request that completes it.
- **Done:** `dotnet build -c Debug`, `dotnet test -c Debug --no-build` at the solution, `dotnet format --verify-no-changes`, and `openspec validate --all`, as `CLAUDE.md` says. The stack stops at the first pull request that cannot pass them.
- **After each merge:** check that no archived change's folder has come back into `openspec\changes` (a merge between stacked branches brought back `mark-sessions-whose-folder-is-gone` once).

## The stack, in order

| # | Pull request | Change and tasks | Gate before merge |
|---|---|---|---|
| 1 | #318: close the output pipes | close-output-pipes (all) | Its own |
| 2 | #320: dispose tidy | dispose-tidy (all) | 2.3, Enter twice on a session with an open tab |
| 3 | Close out | archive wizard-invisible-filter-diagnostics, delete the resurrected mark-sessions-whose-folder-is-gone, fold csharp15-in-core into issue #299; this plan | None |
| 4 | Finish track-branches | track-branches (5.5, 6.2, 6.3, archive) and the README images | 5.5 and 6.3 by the owner; if `⑂` takes two cells, it becomes `⎇` with a version raise |

## Then a stable release

Once 4 has merged, the owner releases a stable 0.22.0 and submits it to winget by running `publish.yml`. Nothing below starts before that.

## Deferred until after that release

Each is planned again, with care, from its issue once the release is out.

- **Native AOT (#297):** one change and one pull request, the only change between two winget submissions so any problem traces to it alone. Before the accounts work, so the new state records are written under the trim and AOT analysers and source-generated JSON.
- **Several Claude accounts (#203, parts 2 to 4):** in order, each proposed again with `/opsx:propose` from the issue, each starting with the owner's spike with a second, logged-in Claude account.
- **Core on C# 15 (#299):** waits for a Stryker.NET release that compiles C# 15 (5.1.0, the latest on 16 November 2026, does not); the change's proposal, design and tasks are kept in the issue. Not inside the AOT release window.
- **Signing (#263):** deferred until someone is blocked by SmartScreen, application control or antivirus; after AOT if taken, as AOT changes the executable.
- **Domains (#290):** the owner's Cloudflare work, not a pull request; any time.

## Owner gates

Carried from the stack of 14 November, whose changes are archived without them; not re-checked on 16 November:

- **add-win-arm64-build:** 5.1 on a Windows 11 Arm machine, and 5.2, the first stable publish run carrying both installers.
- **check-mutation-before-publishing:** the warning appears on the next publish run while a `mutation` issue is open.
- **fix-dependabot-workflow:** 2.2 and 4.2, the next weekly Dependabot pull request going green with no hand edits, and the token's expiry in MAINTAINING.md.
- **fetch-before-branching:** 8.2 (the README GIF) and 9.2. **open-the-diagnostic-log:** 4.4's by-eye part. **restore-hidden-terminal-profiles:** 7.1 and 7.2. **honour-claude-config-folder:** 6.2.
- **announce-winget-approval:** 2.1, remove the README's winget notice once microsoft/winget-pkgs#479528 merges.
- **close-mutation-gaps:** the first strict release run, and the deliberate-failure check.
- **upgrade-to-net11:** 1.1 and 2.6 (refresh timings over the owner's own state) and 1.7 (re-observing the Terminal.Gui and UI Automation facts).
