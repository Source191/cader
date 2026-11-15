# Proposal

## Why

The release runs of `mutation.yml` after v0.22.0-preview.19 and v0.22.0-preview.20 (runs 43720610875 and 43723005967) failed with survivors that the full runs on the same code killed: 223 and 10 of them, every one a static mutant (no `coveredBy`), almost all in the `Wording` tables. A static field or auto-property initialiser runs once per test process, and Stryker's MTP runner switches mutants without restarting it (stryker-mutator/stryker-net#3742), so whether a release run kills one depends on which process happens to build the value. Every release that touches a `Wording*` file fails with findings that are not real, and the noise on #262 hides the real ones: preview.20's run also found a real survivor in `CharacterKinds`, from #298.

Rewriting Core's static initialisers as computed properties (#302) removed the noise, but it trades a common C# idiom for a Cader-only rule and adds short-lived allocations on the paths that start child programs. The owner chose instead to leave static mutants to the weekly full run until Stryker tests them in a fresh process (stryker-mutator/stryker-net#3695 is open), and to keep the real gaps #302 found.

## What Changes

- **A release run leaves static mutants to the weekly full run:** a static mutant that survived or had no coverage in a release run is neither a finding nor a reason to fail it. `run.json` records how many were set aside, and the issue comment says so beside the real survivors. The weekly full run is unchanged and still counts them.
- **The gaps #302 found, closed here:**
  - A Core fact pins `TerminalProfiles.BellStyle`, which only an Infrastructure fact read.
  - `WordingShould.GiveEveryConfigKeyATipOfItsOwn` pins the Config tab's key tips.
  - `ConfigViewShould`'s `At` checks that the selection landed on the row its key names.
- **The survivor from #298:** `CharacterKinds.First` reads its first character with `Rune.DecodeFromUtf16`, which leaves no `First()` to `FirstOrDefault()` mutant and reads a lone surrogate as U+FFFD; a fact holds the lone surrogate.
- **Two simplifications:** `DisplayState.Known`'s guard that no caller can reach goes, and `ReleaseVersion` splits at the first hyphen, so its `dash < 0` test, whose `<= 0` mutant no input could tell apart, goes.
- **Docs:** `.claude\rules\checks.md` (the release rule, the two runs, the #3742 and #3695 follow-up), `MAINTAINING.md`'s mutation issue line, and an Internal changelog line.

## Out of scope

- **Rewriting static initialisers** (#302, closed in favour of this change).
- **The weekly full run, its break score and the nine Accepted marks on the diagnostic tables**, which stay as they are.
- **Mutants counted as covered by every test** (code a test class's static field or `TheoryData` runs): the runner kills every one, so they raise no false finding; `checks.md` records how to judge one by hand.
- **Contributing to Stryker.NET:** testing #3695 against Cader and reporting on #3742 is a separate step the owner takes.
- **Multiple Claude accounts**, which are later changes.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None: nothing a user can see changes, so `.openspec.yaml` declares `skip_specs: true`.

## Impact

- **Scripts:** `.github\scripts\MutationReport.ps1` (`Get-UndetectedMutants` says whether a mutant is static), `Write-MutationRun.ps1` and `Format-MutationComment.ps1`, with their Pester tests in `test\Scripts\Mutation.Tests.ps1`.
- **Code:** `CharacterKinds`, `DisplayState` and `ReleaseVersion` in Core's Domain, and their facts; the new facts in `TerminalProfilesShould`, `WordingShould` and `ConfigViewShould`.
- **Version:** a preview raise with an Internal changelog line, because files under `src\` change.
