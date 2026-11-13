# Proposal

## Why

Issue #93 left signing open: "unsigned executables trip SmartScreen and some antivirus". 0.21.0 is unsigned, so a first run of `cader.exe` downloaded from a GitHub Release (and perhaps from winget) can meet a SmartScreen warning or be blocked outright by a managed machine's application-control policy. A signature also gives a user something to check beyond the GitHub build attestation, which proves where a zip came from but not to Windows.

## What Changes

- `cader.exe` in every stable release zip carries an Authenticode signature with a timestamp, made in CI from `main` and never on a developer machine or a pull request build.
- A new `sign` job in `pipeline.yml`, between the check job and the release job, runs only for a release. It takes the smoke-checked zip, signs the executable, rebuilds the zip, runs the smoke check on the signed zip and hands that zip on; the release job then attests and attaches it as before.
- The signing identity is held by an approval-free but branch-restricted `signing` environment and reached by OIDC, so no certificate or password is stored as a secret.
- Prereleases are signed too, so what is tried is what ships.
- `publish.yml` refuses a zip whose executable is not validly signed, as well as one without provenance.
- The signing service and its cost are chosen in the design (recommendation: Azure Artifact Signing; the alternative is SignPath Foundation).
- MAINTAINING says how signing is set up, renewed and revoked.

## Capabilities

### New Capabilities

### Modified Capabilities
- `installation`: a released executable is signed, and a release whose executable is unsigned or whose signature does not verify is neither published nor submitted to winget. This capability is added by `install-with-winget`, which must be archived first.

## Impact

- **Code:** none in the tool.
- **Workflows:** a `sign` job in `pipeline.yml`, a signature check in `publish.yml`, a `Test-Signature.ps1` script (with tests) shared by both.
- **Services:** one signing account (Azure subscription resources or a SignPath project) and an Entra app registration with a federated credential for the repository's `main` branch.
- **Cost:** about US$10 a month for Azure Artifact Signing's Basic tier at the time of writing, or nothing for SignPath Foundation if the project qualifies (verify both before deciding).
- **Docs:** MAINTAINING (setup, rotation), README (a line on verifying the signature), `.claude\rules\checks.md` and `security.md`.

## Out of scope

- A `win-arm64` build, which is `add-win-arm64-build` (signing must cover its zip too, whichever lands second).
- Signing the third-party `libonigwrap.dll`; only Cader's own executable is signed.
- An MSIX or MSI installer and the Microsoft Store.
- Guaranteeing that SmartScreen never warns: reputation can lag a new publisher, and the result is measured, not promised.
- Multiple Claude accounts, which is a later change.
