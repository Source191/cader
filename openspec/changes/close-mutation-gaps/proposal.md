# Proposal

## Why

A full Stryker run of `Source191.Cader.Core` leaves about 229 surviving mutants out of 3,819 (2,858 tested), a score of about 86.9 % on the developer machine and 91.2 % on a runner (13 November 2026, Stryker 5.0.0, `.claude\rules\checks.md`). Each survivor is a change to the code that no test notices, so a regression of that kind could ship. Some are not real gaps (a change no test could ever tell apart, or one not worth a test), but nobody has sorted them, so nothing says how many are.

`mutation-testing-in-ci` (PR #255) reports findings after each release and weekly, but it can only say "there are survivors" while nothing is cleared. Its break score of 86 is decided for the weekly full run only, and the release (`--since`) runs use `--break-at 0`, because a small run scores in big jumps. Until the survivors are cleared, the release runs cannot be made strict, since a release that touches a test file mutates almost all of Core and would fail on survivors that were already there.

## What Changes

- **Sort the survivors** of a reference full run into three kinds:
  - **Real test gap:** a test could and should tell the mutant from the original.
  - **Equivalent:** no test can tell them apart (an arm that throws `UnreachableException`, for example). Marked in place with Stryker's comment and a reason beginning `Equivalent:`.
  - **Accepted:** a test could, but would be brittle or pointless (a diagnostic's wording, a pure speed-up). Marked the same way with a reason beginning `Accepted:`.
- **Close the real gaps in batches by area** (Domain and Ports, Diagnostics and `WorktreeInventory`, the rest of Application and `Wording`, then the views, detail, reporting, actions and launch), with new sociable tests in the house style (xunit.v3, Shouldly, hand-written fakes at the boundaries only). Each batch records its score before and after in `.claude\rules\checks.md`.
- **Ratchet the break score** after each batch to about four points under the new score (never lower than before), so a slip back is caught by the weekly run.
- **Confirm the release runs use `--break-at 0`** (a decided point of PR #255), and make it so if the workflow as merged still applies the config's break score to them.
- **Then make release runs stricter:** once the reference full run has no unmarked survivor, a release (`--since`) run fails when any mutant survived or had no covering test and was not marked equivalent or accepted, but only when at least 20 mutants were counted (small samples score in big jumps). The change is in `Write-MutationRun.ps1` and the `mutation.yml` step that reads it, with the script's first Pester tests. The failure never blocks a merge or a release (it runs after them) and still comments on the `mutation` issue.
- **Docs:** `checks.md` (the ledger of batches and scores, the marking convention and the strict rule) and `CONTRIBUTING.md`'s mutation paragraph.
- **Version and changelog:** a pull request that adds a `Stryker disable` comment under `src\` changes the packaged tool's sources, so it raises `Version` (patch) and adds a `### Internal` line to `CHANGELOG.md`, and its merge is a release. A pull request of new tests only changes neither.

## Out of scope

- **Other projects than Core:** Infrastructure, Tui and the tool project (still decided on Core's timings, `mutation-testing-in-ci`).
- **Gating pull requests on mutation testing** or making the score a merge gate.
- **Changing how Core's code works** to remove an equivalent mutant (only a comment marks it; a refactor to avoid a mutant is a separate decision).
- **Multiple Claude accounts:** a later change.

## Dependencies

- **`mutation-testing-in-ci` (PR #255) must be merged:** this change edits its workflow, config and scripts.
- **`explain-mutation-score-gap` must be merged first:** its finding says which environment's score the break score belongs to, so the baseline the batches start from is trustworthy. If it finds that tests differ by machine, those tests are fixed there, and the 128 or so extra developer-machine survivors are not triaged here.
- **`upgrade-to-net11` (J to M) should be merged first:** its C# 15 sweep touches many Core files, so survivors, line numbers and marks would move under the batches.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None: tests, comments, a workflow step, a script and documentation. Nothing users see changes, so `.openspec.yaml` declares `skip_specs: true`.

## Impact

- **Tests:** new facts in `test\Source191.Cader.Core.Tests`, and `test\Scripts\Write-MutationRun.Tests.ps1` with fixtures beside the existing ones in `.github\scripts\fixtures\mutation`.
- **Core sources:** only `// Stryker disable once <Mutator> : <reason>` comments, no behaviour change.
- **Scripts and workflow:** `.github\scripts\Write-MutationRun.ps1` (and its comment text in `Format-MutationComment.ps1`), `.github\workflows\mutation.yml`, `test\Source191.Cader.Core.Tests\stryker-config.json` (the `break` value).
- **Docs:** `.claude\rules\checks.md`, `CONTRIBUTING.md`.
- **Releases:** each pull request that adds a mark is a patch release (Version and changelog lines); the mutation workflow then runs `--since` on it.
