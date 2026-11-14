# Spec Delta

## ADDED Requirements

### Requirement: A released executable is signed
The `cader.exe` in every zip attached to a GitHub Release, `win-x64` and `win-arm64`, SHALL carry a valid Authenticode signature with a trusted timestamp, made through SignPath Foundation from a release workflow run on main and approved by the owner, whose publisher is SignPath Foundation. A pull request build or a local build SHALL NOT be signed with the release certificate.

#### Scenario: A stable release
- **WHEN** a push to main raises `Version` to 0.22.0 and the owner approves the signing request
- **THEN** the `cader.exe` in both `cader_0.22.0_win-x64.zip` and `cader_0.22.0_win-arm64.zip` has a valid signature whose publisher is SignPath Foundation, and a timestamp

#### Scenario: A pull request build
- **WHEN** a pull request's check builds the zips
- **THEN** each `cader.exe` is unsigned, and the check reads no signing credential

#### Scenario: The signed zip is the one smoke-checked
- **WHEN** the release workflow has signed both executables
- **THEN** each architecture's smoke check runs on its signed zip before the release is published

### Requirement: The signing account is the project's own
The SignPath project, its CI user and the `signing` environment SHALL be the project's own, set up and held by the owner, whose personal project Cader is.

#### Scenario: Checking who holds signing
- **WHEN** a person reads MAINTAINING's Ownership and signing sections
- **THEN** they name the owner as the holder of the SignPath project and its approver, and SignPath Foundation as the certificate's publisher

### Requirement: An unsigned release is never published or submitted
A release whose executable, of either architecture, is unsigned, or whose signature does not verify, SHALL NOT be published, and the publish workflow SHALL NOT submit to the winget community repository unless both zips' executables' signatures verify.

#### Scenario: Signing fails
- **WHEN** the signing service is unavailable during a release
- **THEN** the release job does not run, no release is published, and the run fails naming the signing step

#### Scenario: The signing request is not approved
- **WHEN** the owner denies the signing request, or does not approve it before the workflow stops waiting
- **THEN** no release is published, and the run fails naming the signing step

#### Scenario: A zip with a broken signature
- **WHEN** the owner runs the publish workflow for a tag where either zip holds an unsigned or altered executable
- **THEN** the run fails naming that architecture, and no pull request is made to the winget community repository

### Requirement: The signing credential is held only for main
The repository SHALL hold no certificate, private key or signing password. The one signing credential SHALL be an API token for a SignPath user that can submit signing requests but not approve them, held only as a secret of a `signing` environment that accepts runs from main alone, and SignPath SHALL accept a signing request only from a GitHub-hosted workflow run on main of this repository.

#### Scenario: A workflow on another branch
- **WHEN** a workflow run on a branch other than main names the `signing` environment
- **THEN** the run is refused the environment, the token is never given to it, and nothing is signed

#### Scenario: The token alone
- **WHEN** someone holding the token submits a signing request
- **THEN** nothing is signed until the owner approves the request in SignPath
