# Tasks

## 1. Confirm the premises

- [x] 1.1 Confirm the repository can use the hosted `windows-11-arm` runner (a draft pull request whose job prints `$env:PROCESSOR_ARCHITECTURE`); if it cannot, stop and record that in the design. Verify the job prints ARM64.
- [x] 1.2 On an Arm machine, `winget show` for `Microsoft.WindowsTerminal`, `Microsoft.PowerShell` and `Git.Git` and record that each has an `arm64` installer (dated, with the winget version) in `.claude\rules\windows.md`. Verify by the recorded facts.

## 2. The build

- [x] 2.1 Add `win-arm64.pubxml` beside `win-x64.pubxml` with its reasons in `src\Source191.Cader\README.md`. Verify `dotnet publish -p:PublishProfile=win-arm64` gives the five files and `libonigwrap.dll` is the Arm one.
- [x] 2.2 Test then change `Compress-WingetBuild.ps1` and the `build-test-pack` action to take the runtime identifier and name the zip `cader_<version>_<rid>.zip`: a script test asserting the name and the five files for each rid. Verify the action outputs the zip path for both.
- [x] 2.3 `tools\install-local.ps1`: choose the rid from the machine's architecture, for `-FromArtifact` too, tested by passing the architecture in. Verify on x64 and, by the owner, on Arm.

## 3. Workflows

- [x] 3.1 `pipeline.yml`'s check job: a matrix of `windows-latest` and `windows-11-arm`, each building, unzipping, checking `--version` and running the smoke step on its own zip; the tests and format check on the x64 leg only. Verify a draft pull request passes on both legs and uploads two artefacts.
- [x] 3.2 `pipeline.yml`'s release job: wait for both legs, find exactly one zip per architecture, attest and attach both. Verify with the next prerelease from `main` that the Release carries both zips and `gh attestation verify` passes on each. Verified 14 November 2026: v0.22.0-preview.4 carries `cader_0.22.0-preview.4_win-x64.zip` and `cader_0.22.0-preview.4_win-arm64.zip`, and `gh attestation verify` exits 0 for each.
- [x] 3.3 `publish.yml`: download and verify both zips, pass both `--urls` to `wingetcreate update`, and fail naming the architecture when either is missing. The step is on `main`; no test fork was made, so its proof moves to 5.2, the first stable publish run.

## 4. Documentation

- [x] 4.1 README (install notes both architectures), MAINTAINING, `.claude\rules\checks.md` and `windows.md`. Verify a read-through for "win-x64" statements that are now wrong.
- [x] 4.2 Raise `Version`, add the `CHANGELOG.md` entry under Added and run `Assert-VersionBumped.ps1` and `Assert-ChangelogUpdated.ps1`. Verify both pass.
- [x] 4.3 Archive the change in the pull request that completes it, after `install-with-winget` is archived. Verify `openspec validate add-win-arm64-build --strict`.

## 5. Integration checks (owner, on real machines)

- [ ] 5.1 On a Windows 11 Arm machine: `winget install Source191.Cader`, `cader` starts, `cader setup-terminal` works and a session can be started and attached. Verify the installed executable is the Arm build.
- [ ] 5.2 The first stable `publish.yml` run after this change: both zips are downloaded and verified and the winget-pkgs pull request lists an x64 and an arm64 installer. Verify on the run's page and the winget-pkgs pull request.

## Implementation plan

See `openspec\implementation-plan.md` for the whole sequence.

- **PR:** E
- **Step:** 2
- **Depends on:** A (the `installation` capability exists only once install-with-winget is archived)
- **Parallel with:** C, D, F
- **Model:** sonnet
- **Effort:** medium
- **Why:** workflow and script plumbing once the two premises in 1.1 and 1.2 are confirmed; no C# is expected.
- **Owner gates:** blocking, 1.1 (the hosted `windows-11-arm` runner is available; stop and re-plan if not) and 1.2 (Arm installers exist for the winget dependencies). After merge, 5.1 on a Windows 11 Arm machine.
