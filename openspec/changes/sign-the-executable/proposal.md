# Proposal

## Why

Issue #93 left signing open: "unsigned executables trip SmartScreen and some antivirus". 0.21.0 is unsigned, so a first run of `cader.exe` downloaded from a GitHub Release (and perhaps from winget) can meet a SmartScreen warning or be blocked outright by a managed machine's application-control policy. A signature also gives a user something to check beyond the GitHub build attestation, which proves where a zip came from but not to Windows.

## What Changes

- `cader.exe` in both release zips (`win-x64` and `win-arm64`) carries an Authenticode signature with a timestamp, made through SignPath Foundation's free open-source signing from a workflow run on `main`, never on a developer machine or a pull request build. The certificate is SignPath Foundation's, so the signature names SignPath Foundation as the publisher.
- A new `sign` job in `pipeline.yml`, after the check job, runs only for a release: it sends both executables to SignPath in one signing request, which the owner approves in SignPath, and hands the signed pair on. A new `signed` job per architecture rezips each signed executable, verifies its signature and runs the smoke check on the signed zip; the release job then attests and attaches both signed zips as before.
- The SignPath API token is held only by a `signing` environment that accepts only `main` and has no GitHub reviewer, belongs to a submit-only CI user, and cannot sign without the owner's approval; no certificate or private key is held anywhere in the repository.
- Prereleases are signed too, so what is tried is what ships.
- `publish.yml` refuses a zip whose executable is not validly signed, as well as one without provenance, naming the architecture.
- The README gains the code signing policy SignPath requires and a line on checking the signature; MAINTAINING says how signing is set up, approved, rotated and revoked.

## Capabilities

### New Capabilities

### Modified Capabilities
- `installation`: a released executable is signed, and a release whose executable is unsigned or whose signature does not verify is neither published nor submitted to winget.

## Impact

- **Code:** possibly one line in the tool: `Product` set to `Cader` in `src\Source191.Cader\Source191.Cader.csproj` if the executable's product name is not already `Cader`, which SignPath's terms require (with the version bump every such change needs).
- **Workflows:** `sign` and `signed` jobs in `pipeline.yml`, the smoke steps moved into a shared composite action, a signature check in `publish.yml`, a `Test-Signature.ps1` script (with Pester tests) shared by both.
- **Services:** a SignPath Foundation project with the GitHub.com trusted build system, the SignPath GitHub App on the repository, a CI user and its API token, and the `signing` environment.
- **Cost:** nothing in money. Each release, prereleases and Dependabot bumps included, waits for the owner to approve its signing request in SignPath.
- **Docs:** README (the code signing policy and checking the signature), MAINTAINING (setup, approval, rotation), the stable release notes (a code signing policy link), `.claude\rules\checks.md` and `security.md`.

## Out of scope

- Signing the third-party `libonigwrap.dll`; only Cader's own executables are signed.
- An MSIX or MSI installer and the Microsoft Store.
- Guaranteeing that SmartScreen never warns: reputation can lag, and the result is measured, not promised.
- A certificate in the owner's or the project's own name, which would need a paid service.
- Multiple Claude accounts, which is a later change.
