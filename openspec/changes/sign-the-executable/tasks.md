# Tasks

## 1. Decide and set up (owner)

- [ ] 1.1 Decide the signing service, the identity and the monthly cost; confirm eligibility (Azure Artifact Signing's supported countries and identity types, or SignPath Foundation's conditions) and record the answer in the design, changing the later tasks if it is SignPath. Verify the design names the service and the account.
- [ ] 1.2 Create the account and certificate profile, an Entra app registration with a federated credential for `main`, and the `signing` environment limited to `main`; store the account and profile names as environment variables of the `signing` environment, not secrets. Verify a test workflow on `main` signs a sample file and one on another branch is refused.

## 2. The verifier

- [ ] 2.1 Test then write `.github\scripts\Test-Signature.ps1` (status `Valid`, a timestamp, the expected publisher name; each failure says which): script tests over a signed sample, an unsigned file and a file altered after signing. Verify the three cases give exit 0, 1 and 1.

## 3. Workflows

- [ ] 3.1 `pipeline.yml`: a `sign` job that runs only for a release, restricted to the `signing` environment, that extracts the check job's zip, signs `cader.exe`, rezips with `Compress-WingetBuild.ps1`, runs `Test-Signature.ps1` and the smoke step on the signed zip, and uploads it. Verify with the next prerelease from `main` that the Release's zip holds a signed executable.
- [ ] 3.2 `pipeline.yml`'s release job reads the signed artefact and attests it; a failed sign job stops the release. Verify by a run with signing deliberately failing on a test branch that no release is published.
- [ ] 3.3 `publish.yml`: run `Test-Signature.ps1` on the downloaded zip before the provenance check passes it on. Verify with a dry run on a test fork using an unsigned zip, which must fail.
- [ ] 3.4 A pull request check still builds and smoke-checks an unsigned zip and reads no signing credential. Verify a draft pull request's check passes with the `signing` environment untouched.

## 4. Documentation

- [ ] 4.1 MAINTAINING (the account, the federated credential, what to do when signing fails or the account lapses), README (a line on checking the signature), `.claude\rules\checks.md` and `security.md`. Verify a read-through for any text that says releases are unsigned.
- [ ] 4.2 Raise `Version`, add the `CHANGELOG.md` entry under Added and run `Assert-VersionBumped.ps1` and `Assert-ChangelogUpdated.ps1`. Verify both pass.
- [ ] 4.3 Archive the change in the pull request that completes it, after `install-with-winget` is archived. Verify `openspec validate sign-the-executable --strict`.

## 5. Integration checks (owner, on real machines)

- [ ] 5.1 On a clean Windows 11 machine, download the release zip in a browser, extract and run `cader.exe`; right-click, Properties, Digital Signatures shows the publisher and timestamp. Record whether SmartScreen warned (dated) in `windows.md`.
- [ ] 5.2 `winget install Source191.Cader` of the signed release on a machine with App Control for Business in audit mode, and note any block. Verify by the event log.
