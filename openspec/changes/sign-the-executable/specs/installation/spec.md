# Spec Delta

## ADDED Requirements

### Requirement: A released executable is signed
The `cader.exe` in every zip attached to a GitHub Release SHALL carry a valid Authenticode signature with a trusted timestamp, made by the release workflow from main. A pull request build or a local build SHALL NOT be signed with the release identity.

#### Scenario: A stable release
- **WHEN** a push to main raises `Version` to 0.22.0
- **THEN** the `cader.exe` in the release's zip has a valid signature whose publisher is the project's signing identity, and a timestamp

#### Scenario: A pull request build
- **WHEN** a pull request's check builds the zip
- **THEN** its `cader.exe` is unsigned, and the check reads no signing credential

#### Scenario: The signed zip is the one smoke-checked
- **WHEN** the release workflow has signed the executable
- **THEN** the smoke check runs on the signed zip before the release is published

### Requirement: An unsigned release is never published or submitted
A release whose executable is unsigned, or whose signature does not verify, SHALL NOT be published, and the publish workflow SHALL NOT submit a zip to the winget community repository unless its executable's signature verifies.

#### Scenario: Signing fails
- **WHEN** the signing service is unavailable during a release
- **THEN** the release job does not run, no release is published, and the run fails naming the signing step

#### Scenario: A zip with a broken signature
- **WHEN** the owner runs the publish workflow for a tag whose zip holds an unsigned or altered executable
- **THEN** the run fails and no pull request is made to the winget community repository

### Requirement: The signing credential is not a stored secret
The release workflow SHALL reach the signing service with a short-lived token issued to the repository's workflow on main, and the repository SHALL hold no certificate, private key or signing password.

#### Scenario: A workflow on another branch
- **WHEN** a workflow run on a branch other than main asks for the signing token
- **THEN** the token is refused and nothing is signed
