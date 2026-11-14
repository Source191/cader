# Tasks

## 1. Apply and set up (owner)

- [ ] 1.1 Apply to SignPath Foundation at https://signpath.org/apply. Prepare first: MFA on the GitHub account and on the SignPath sign-in; the repository URL (public, Apache-2.0, released on GitHub Releases and winget); a one-paragraph description of what Cader does and that its executable is built only by `pipeline.yml` on GitHub-hosted runners from `main`; the team (the owner as sole author, reviewer and approver, and the "Owner approves main" ruleset that reviews every change by anyone else); the files to sign (`cader.exe` for `win-x64` and `win-arm64`, with the third-party `libonigwrap.dll` left unsigned); and the network use (the anonymous daily update check against GitHub, turned off on the Config tab, and problem reports only when the user submits one). Record in the design the date applied, the date accepted, and any condition SignPath sets (above all on a sole maintainer and on the update check). Verify by the acceptance email naming the organisation id and project.
- [ ] 1.2 Set up the project: install the SignPath GitHub App on `Source191/cader` only; link the GitHub.com trusted build system; a `release-signing` policy with the approval process on, the owner as its one approver, allowed branch `main`, and GitHub-hosted runners required; an artefact configuration with a `<zip-file>` root holding `win-x64/cader.exe` and `win-arm64/cader.exe` as PE files restricted to product name `Cader`; a CI user with only the submitter role on that policy, and its API token. Create the `signing` environment (deployment branches `main` only, no required reviewer) with `SIGNPATH_API_TOKEN` as its one environment secret and `SIGNPATH_ORGANIZATION_ID`, `SIGNPATH_PROJECT_SLUG`, `SIGNPATH_SIGNING_POLICY_SLUG` and `SIGNPATH_ARTIFACT_CONFIGURATION_SLUG` as its variables. Verify with a throwaway workflow on `main` that signs a sample pair after approval, that the signed files' subject is recorded in the design for `Test-Signature.ps1`, and that the same workflow on another branch is refused the environment; delete the workflow.

## 2. The verifier and the metadata

- [ ] 2.1 Read the product name and version of a published `cader.exe` (`(Get-Item cader.exe).VersionInfo`); if the product name is not `Cader`, test then set `Product` in `src\Source191.Cader\Source191.Cader.csproj` (a fact in `Infrastructure.Tests` reading the built executable's version resource). Verify the product name is `Cader` and the product version is `Version`.
- [ ] 2.2 Test then write `.github\scripts\Test-Signature.ps1` (status `Valid`, a timestamp, the expected signer subject as a parameter; each failure says which, naming the file): Pester tests under `test\Scripts` over a signed sample, an unsigned file and a file altered after signing. Verify the three cases give exit 0, 1 and 1.

## 3. Workflows

- [ ] 3.1 Move the check job's smoke steps (seed the repository, unzip, version check, smoke check, failure capture) into a composite action `.github\actions\smoke-check` taking the zip's path, and call it from `check`. Verify a pull request's `check` and `check (win-arm64)` still pass the smoke check.
- [ ] 3.2 `pipeline.yml`: a `sign` job (`needs: check`, only when `changed`, `environment: signing`, `windows-latest`, `contents: read` and `actions: read`, no checkout) that downloads both `release-<rid>` artefacts, extracts each `cader.exe` to `<rid>\cader.exe`, uploads the pair, submits it with `signpath/github-action-submit-signing-request` pinned to `f6d04783b4569d051e0c80105fe66e82819d0092 # v3.0` (`wait-for-completion-timeout-in-seconds: 3600`, `timeout-minutes: 70`), and uploads the signed pair as `signed-executables`. Verify with the next prerelease from `main` that one signing request appears in SignPath, waits for approval and completes.
- [ ] 3.3 `pipeline.yml`: a `signed` matrix job (`needs: [check, sign]`, the two runners, no environment) that replaces each zip's `cader.exe` with the signed one, rezips with `Compress-WingetBuild.ps1`, runs `Test-Signature.ps1` and the smoke-check action on the signed zip, and uploads `signed-<rid>` (the x64 leg with the release notes). Verify in that prerelease's run that both legs pass on signed zips.
- [ ] 3.4 `pipeline.yml`'s release job needs `signed`, reads `signed-*` and attests both; a failed or unapproved sign job stops the release. Verify by denying one prerelease's signing request that no release is published and the run fails at `sign`, then by rerunning the failed jobs and approving that it is.
- [ ] 3.5 `publish.yml`: run `Test-Signature.ps1` on each downloaded zip's executable before the provenance check passes it on, failing naming the architecture. Verify with a dry run on a test fork using an unsigned zip, which must fail.
- [ ] 3.6 A pull request check still builds and smoke-checks unsigned zips and never reaches `sign` or the `signing` environment. Verify a draft pull request's checks pass with the environment's deployment history untouched.

## 4. Documentation

- [ ] 4.1 README: a "Code signing policy" section (the line "Free code signing provided by SignPath.io, certificate by SignPath Foundation", the team and roles, a link to the Privacy line), a line saying the publisher shown is SignPath Foundation and how to check it (Properties, Digital Signatures, or `Get-AuthenticodeSignature`), and a "Code signing policy" link in the stable release notes (`Write-ReleaseNotes.ps1`, with its test). MAINTAINING: the SignPath project, the CI user and token, the `signing` environment, approving a request, what to do when one is denied or expires (rerun the failed jobs), rotating the token, and what happens if SignPath pauses the project; the Workflows table gains `sign` and `signed`, and Releases loses "follows check by about half a minute". `.claude\rules\checks.md` and `security.md`. Verify a read-through for any text that says releases are unsigned or that a release needs no approval.
- [ ] 4.2 Raise `Version`, add the `CHANGELOG.md` entry under Added and run `Assert-VersionBumped.ps1` and `Assert-ChangelogUpdated.ps1`. Verify both pass.
- [ ] 4.3 Archive the change in the pull request that completes it. Verify `openspec validate sign-the-executable --strict` and `openspec validate --all`.

## 5. Integration checks (owner, on real machines)

- [ ] 5.1 On a clean Windows 11 x64 machine and on an Arm machine, download the release zip in a browser, extract and run `cader.exe`; right-click, Properties, Digital Signatures shows SignPath Foundation and the timestamp. Record whether SmartScreen warned (dated) in `windows.md`.
- [ ] 5.2 `winget install Source191.Cader` of the signed release on a machine with App Control for Business in audit mode, and note any block. Verify by the event log.

## Implementation plan

See `openspec\implementation-plan.md` for the whole sequence.

- **PR:** G
- **Step:** 3
- **Depends on:** A and E (both merged: two zips exist and the installation spec is in `openspec\specs`)
- **Parallel with:** H
- **Model:** opus
- **Effort:** high
- **Why:** a signing pipeline must never sign from a pull request or leave its token reachable from another branch, its verifier is a security check, and the split between a token-holding `sign` job and the smoke-checking `signed` jobs is easy to get subtly wrong.
- **Owner gates:** blocking, 1.1 (the SignPath Foundation application and its acceptance, which can take days and may set conditions) before anything starts, and 1.2 (the project, policy, CI user and `signing` environment) before 3.2. Every release after merge waits for the owner's approval in SignPath. After merge, 5.1 and 5.2 on real machines.
