## 1. Set up

- [x] 1.1 Raise the version with `.github\scripts\Step-Version.ps1 -Preview` and add an Internal `CHANGELOG.md` line; verify `Assert-VersionBumped.ps1 -Base main -Head HEAD` passes.

## 2. Release runs set static mutants aside, a red and a green commit each

- [x] 2.1 Red then green: `Write-MutationRun` reports no finding and does not fail a release run whose only survivors are static, and records `staticSetAside` in `run.json`; a full run still counts them. Verify with `Invoke-Pester test\Scripts`.
- [x] 2.2 Red then green: `Format-MutationComment` lists only the non-static survivors of a release run and says how many static ones were left to the weekly full run. Verify with `Invoke-Pester test\Scripts`.
- [x] 2.3 Red then green: the cut report keeps a mutant's `static` flag, so the comment reads the same set as the run. Verify with `Invoke-Pester test\Scripts`.

## 3. The gaps and the survivor #302 found, each verified by the Core suite

- [x] 3.1 `TerminalProfilesShould.RingWithASoundAndFlashTheWindowAndTheTaskbar` pins `TerminalProfiles.BellStyle`.
- [x] 3.2 `WordingShould.GiveEveryConfigKeyATipOfItsOwn` pins the Config tab's key tips; verify it fails with one tip blanked by hand.
- [x] 3.3 `ConfigViewShould`'s `At` checks the selection landed on the key; verify each Enter fact fails with its row's key blanked by hand.
- [x] 3.4 `CharacterKindsShould.ReadALoneSurrogateAtTheStartAsTheReplacementCharacter` pins what `EnumerateRunes` does today, then refactor `CharacterKinds.First` onto `Rune.DecodeFromUtf16`; verify the Core suite passes before and after.
- [x] 3.5 Refactor: drop `DisplayState.Known`'s unreachable guard, and split `ReleaseVersion` at the first hyphen; verify the Core suite passes.

## 4. Prove and record

- [ ] 4.1 Run `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes` and `Invoke-Pester test\Scripts`; verify all pass.
- [ ] 4.2 Run `Write-MutationRun.ps1` and `Format-MutationComment.ps1` on the downloaded results of runs 43720610875 and 43723005967 as release runs; verify the first leaves no finding and the second reports only `CharacterKinds` (now fixed).
- [ ] 4.3 Update `.claude\rules\checks.md` (the release rule, the two runs, when to remove the rule: #3742 and #3695) and `MAINTAINING.md`'s mutation issue line; verify `openspec validate skip-static-mutants-in-release-runs` passes.

## Workflow follow-up

- Close #302 with a comment linking this pull request.
- Archive the change in the pull request that completes it.
- After merge, check the release run's comment on #262 and close #262 when it reports no survivor.
