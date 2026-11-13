# Spec Delta

## ADDED Requirements

### Requirement: A release carries a build for each architecture
Every release published by the release workflow SHALL attach a zip of the self-contained single-file build for `win-x64` and another for `win-arm64`, each named `cader_<version>_<architecture>.zip`, each with build provenance attested for the repository's release workflow and each holding the same files. A release SHALL NOT be published unless both zips were built and passed the smoke check on a runner of their own architecture.

#### Scenario: A stable release
- **WHEN** a push to main raises `Version` to 0.22.0 and both builds pass
- **THEN** the GitHub Release v0.22.0 carries `cader_0.22.0_win-x64.zip` and `cader_0.22.0_win-arm64.zip`, each with an attestation

#### Scenario: One architecture fails
- **WHEN** the `win-arm64` smoke check fails
- **THEN** no release is published and no zip is attached

### Requirement: winget installs the build for the machine
The winget manifest submitted for a stable release SHALL list an `x64` and an `arm64` installer, each naming its zip and that zip's SHA-256, so that `winget install Source191.Cader` installs the build for the machine's architecture.

#### Scenario: An Arm machine
- **WHEN** `winget install Source191.Cader` runs on Windows 11 on Arm and the user then runs cader --version
- **THEN** the installed executable is the `win-arm64` build and the output is the release's version

#### Scenario: The owner submits a release
- **WHEN** the owner runs the publish workflow for v0.22.0 and approves it
- **THEN** the pull request to the winget community repository names both zips and both SHA-256 values

#### Scenario: A zip's provenance does not verify
- **WHEN** either zip was not attested by the release workflow from main
- **THEN** the publish run fails and no pull request is made

### Requirement: A local install matches the machine
`tools\install-local.ps1` SHALL publish, and `-FromArtifact` SHALL extract, the build for the architecture of the machine it runs on.

#### Scenario: An Arm machine
- **WHEN** a developer runs `tools\install-local.ps1` on Windows 11 on Arm
- **THEN** the printed executable is a `win-arm64` build
