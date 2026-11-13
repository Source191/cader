# Design

## Context

0.21.0's release job "checks nothing out and runs no build" and holds only the rights to attest and publish (`pipeline.yml`); the zip is built and smoke-checked by the check job and handed over as an artefact. Signing needs a build step with a credential, so it must not be put in either job as it stands. winget records the SHA-256 of the zip, so the executable must be signed before the zip that is hashed and attested is made.

## Decisions

- **Signing service:** recommend **Azure Artifact Signing** (formerly Trusted Signing). It issues short-lived certificates, needs no certificate to store or renew, signs from GitHub Actions by OIDC with the `azure/artifact-signing-action` (verify the current action name and version), and fits the owner's Azure experience. Eligibility must be checked first: it has been limited to organisations in some countries and to individuals in others, and identity validation takes days. Alternative: **SignPath Foundation**, free for open-source projects that meet its conditions (a public repository, an OSI licence, a published code-signing policy), signing from its own service with a GitHub Action; slower to approve and a third party in the release path. Rejected: a purchased OV or EV certificate (annual cost, a private key to protect, and an HSM requirement for new certificates).
- **A `sign` job between check and release:** it runs only on a push to main that changes `Version`, downloads the smoke-checked zip, extracts it, signs `cader.exe`, rezips it with `Compress-WingetBuild.ps1` (same files, same name), runs the smoke check on the signed zip, and uploads it as the artefact the release job picks up. The check job still builds an unsigned zip for every run, so a pull request needs no credential. The release job is unchanged except for which artefact it reads, so it still attests and attaches exactly what was signed and checked.
- **OIDC, one environment:** an Entra app registration with a federated credential for `repo:Source191/cader:ref:refs/heads/main` (or the `signing` environment), the role limited to signing with one certificate profile. The `signing` environment accepts only `main` and needs no approver, because the merge to main is already the owner's decision; adding a reviewer would stop every release until someone clicks.
- **One verifier:** `Test-Signature.ps1` wraps `Get-AuthenticodeSignature` (status `Valid`, a timestamp, the expected publisher name) and is run by the sign job after signing and by `publish.yml` on the downloaded zip, so the two checks cannot drift.
- **Sign only Cader's executable:** `libonigwrap.dll` is third party; SmartScreen reputation attaches to the signed executable that is run.
- **Prereleases are signed:** the same job, so the thing tried is the thing shipped and the signing path is exercised every release rather than only on stable ones.
- **Alternative: sign in the check job on main:** simpler, but it puts the credential in the job that runs the test suite and third-party build steps; a separate job with only signing steps keeps the credential's reach small.

## Risks and open questions

- **[Eligibility and cost]** decided by the owner before any task starts: which service, under which identity (a company or the owner), at what monthly cost. Task 1.1 records the answer; the tasks assume Azure Artifact Signing.
- **[SmartScreen reputation]** a new publisher's reputation may lag even with a valid signature; the integration check records what a clean machine shows rather than promising no warning.
- **[winget hash]** nothing already in winget-pkgs changes: signing only applies to new versions, whose hash is of the signed zip.
- **[Signature breaks on rezip]** a zip does not alter the executable's bytes, but the smoke check on the signed zip proves it.
- **[Service outage]** a signing outage blocks a release; accepted, since an unsigned release is what this change exists to prevent, and a rerun after the outage is cheap.
