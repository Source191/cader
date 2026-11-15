# Design

## Context

Stryker's JSON report marks a mutant that runs inside a static initialiser with `static: true`. It tests such a mutant against all tests, but in one reused test process, so the value it mutates may already have been built with the mutant switched off (see proposal.md, Why). The runner's full runs kill all of them; release runs, which test a few hundred mutants, killed none in 43720610875 and most in 43714788028. `Write-MutationRun.ps1` decides findings and failing from `Get-UndetectedMutants` in `MutationReport.ps1`, and `Format-MutationComment.ps1` lists the same mutants from the cut report that `Write-MutationRun.ps1` writes.

## Goals / Non-Goals

**Goals:**

- A release run reports and fails only on mutants a release run can judge.
- What is set aside is visible: counted in `run.json` and named as a number in the comment.

**Non-Goals:**

- Changing what the weekly full run counts, or its score.

## Decisions

- **Skip by Stryker's own `static` flag, not by file or line:** the flag is what the runner's behaviour depends on, it moves with the code, and it needs no list to keep. The alternative, marking the `Wording` tables `Accepted`, hides about 220 mutants from every run, the weekly one included.
- **Only in release runs:** the weekly full run kills static mutants reliably on the runner (57 of 57 and every static one in 43713303950), so it keeps counting them and its break score still covers the tables.
- **`Get-UndetectedMutants` answers a `Static` property, and each caller chooses:** `Write-MutationRun.ps1` drops static ones from the findings of a release run; the cut report keeps the `static` flag so `Format-MutationComment.ps1` lists the same set and counts the rest. `counted` and the score stay as Stryker reports them, so `MinimumCounted` keeps its meaning (the size of the run).
- **`CharacterKinds.First` uses `Rune.DecodeFromUtf16`, not `Rune.GetRuneAt`:** `GetRuneAt` throws `ArgumentException` for a lone surrogate, which a pasted filter can hold, where `EnumerateRunes` read it as U+FFFD; `DecodeFromUtf16` answers `ReplacementChar` for invalid or empty input.

## Risks / Trade-offs

- [A real gap in a static value goes unreported by release runs] → the weekly full run still counts static mutants, and a fall in its score raises the issue; the full runs have killed every static mutant so far.
- [Stryker renames or drops the `static` field] → the fixture holds a static mutant, so the Pester facts fail if the field stops being read; a release run would then report static survivors again, as today, rather than hide anything.
- [Kept until Stryker is fixed] → `checks.md` names the condition for removing the rule: a Stryker release that tests static mutants in a fresh process (#3695 or its successor).

## Migration Plan

None: the next release run after merge applies the rule. Rolling back is reverting the pull request.
