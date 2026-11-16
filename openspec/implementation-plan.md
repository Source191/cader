# Implementation plan for the open changes

Rewritten 2026-11-14 against `main` at 0.22.0-preview.4 (`a49f437e`), when A to H, T, E and the earlier steps had merged or were archived. Each open change still carries its own `## Implementation plan` block at the end of its `tasks.md`; where it disagrees with this file, this file wins. It changes no code, so it needs no version bump.

## Contents

- Rules for every implementing session
- The stack, in order
- Not in this stack
- Owner gates
- Why these models and efforts

## Rules for every implementing session

- **One stack, built in order:** each pull request branches from the one below it and targets that branch, opens with a "Stacked on #N" line, and is merged bottom first. Nothing in the stack runs in parallel.
- **Versions:** the 0.22.0 preview line is the unreleased minor, so each pull request that touches `src\` runs `.github\scripts\Step-Version.ps1` once on top of the branch below it (preview.5, preview.6 and so on) and adds its `CHANGELOG.md` line under `## Unreleased`. A task that says "minor raise" is met by that line; nothing in the stack goes stable.
- **Archive:** a change is archived only in the pull request that completes it.
- **Done:** `dotnet build -c Debug`, `dotnet test -c Debug --no-build` at the solution, `dotnet format --verify-no-changes`, and `openspec validate --all`, as `CLAUDE.md` says. The stack stops at the first pull request that cannot pass them.
- **By-eye checks:** a task that asks to look at a local install by eye is left unticked and listed under the owner's checks after merge; the HeadlessTerminal smoke check stands in where it can.

## The stack, in order

| # | Pull request | Change and tasks | Model | Effort |
|---|---|---|---|---|
| 1 | Close out | archive check-mutation-before-publishing and add-win-arm64-build; this plan | opus | low |
| 2 | Dependabot workflow | fix-dependabot-workflow (all; 4.1 done by the owner on 14 November 2026) | sonnet | high |
| 3 | I: fetch before branching, part 2 | fetch-before-branching (6 to 9, archive) | sonnet | high |
| 4 | Open the diagnostic log | open-the-diagnostic-log (all) | sonnet | high |
| 5 | Restore hidden terminal profiles | restore-hidden-terminal-profiles (all) | opus | high |
| 6 | Honour the Claude folder | honour-claude-config-folder (all; its spike uses a scratch folder and no login) | sonnet | medium |
| 7 | J: .NET 11 retarget | upgrade-to-net11 (1.1 to 1.10) | opus | high |
| 8 | K: .NET 11 process APIs | upgrade-to-net11 (2.1 to 2.6) | opus | xhigh |
| 9 | L: C# 15 and house style | upgrade-to-net11 (3.1 to 3.9) | opus | high |
| 10 | M: .NET 11 release and docs | upgrade-to-net11 (4.1 to 4.5, archive) | sonnet | medium |
| 11 | N: explain the mutation score gap | explain-mutation-score-gap (all) | sonnet | high |
| 12 to 15 | O, P, Q, R: close mutation gaps | close-mutation-gaps (1 to 8, a batch each) | sonnet | high |
| 16 | S: strict release runs | close-mutation-gaps (9, 10, archive) | opus | medium |

- **Why this order:** 3 to 6 each touch the Config tab's rows and wording, so they go one at a time and before .NET 11, whose C# 15 sweep is meant to come after them. N needs the .NET 11 code settled, and O to S need N's finding.
- **Mutation runs:** N and O to S dispatch `mutation.yml` on the stack's own branch (`gh workflow run mutation.yml --ref <branch>`), since `main` will not yet carry the stack. The local full run is made in an ordinary clone under `$env:TEMP\mutation-gap`, never committed or attached.

## Done: 16, strict release runs

Built on 15 November 2026, so the stack is complete (#271 to #285 and 16's pull request). The owner chose to mark the 31 static-initialiser survivors of run 43682241837 Accepted; the 15 marks ignore all 61 static mutants of those four files, and runner run 43689035514 scored 100 % with nothing left, so the strict release rule is switched on and close-mutation-gaps is archived. The marks come out when a Stryker release tests static mutants in a fresh process (.claude\rules\checks.md, Follow-up).

## Not in this stack

- **csharp15-in-core:** Core's C# 15 sweep, deferred by PR L of upgrade-to-net11 until a Stryker release compiles C# 15 (evidence in the archived upgrade-to-net11 design and in `.claude\rules\checks.md`); proposal, design and tasks only, no specs.
- **Several Claude accounts:** the three proposed changes were removed and their plan folded into issue #203; each part starts with a spike that needs a second, logged-in Claude account, and is proposed again from the issue when the owner takes it.

## Owner gates

After merge (the change is archived without them, but they are not forgotten):

- **add-win-arm64-build:** 5.1 on a Windows 11 Arm machine, and 5.2, the first stable publish run carrying both installers.
- **check-mutation-before-publishing:** the warning appears on the next publish run while a `mutation` issue is open.
- **fix-dependabot-workflow:** 2.2 and 4.2, the next weekly Dependabot pull request going green with no hand edits, and the token's expiry in MAINTAINING.md.
- **fetch-before-branching:** 9.2. **open-the-diagnostic-log:** 4.4's by-eye part. **restore-hidden-terminal-profiles:** 7.1 and 7.2. **honour-claude-config-folder:** 6.2.
- **announce-winget-approval:** 2.1, remove the README's winget notice once microsoft/winget-pkgs#479528 merges.
- **close-mutation-gaps:** the first strict release run, and the deliberate-failure check.
- **upgrade-to-net11:** 1.1 and 2.6 (refresh timings over the owner's own state) and 1.7 (re-observing the Terminal.Gui and UI Automation facts).- **fetch-before-branching:** 8.2, remaking the README GIF with `tools\New-ReadmeScreenshot.ps1` (it needs the desktop and ffmpeg).

## Why these models and efforts

- **Opus:** where a mistake is a silent regression in exact rules: the byte-exact edit of Terminal's settings file (5), process semantics behind `ConfirmedProcess` (8, xhigh), judging every package and breaking change (7), the 59-file sweep with a union spike (9), and the workflow that decides whether a release run fails (16).
- **Sonnet, high:** each follows an established house pattern (a row and its dialog, a handler, a workflow step, a mutation batch) where care matters more than design.
- **Sonnet, medium:** 6 and 10, small or measurement-led.
- **Not used:** haiku (the house rules and reflection facts punish small mistakes) and `max` (nothing here is an open design question).
