# Proposal

## Why

A pull request's `check` job takes about four minutes (run 41529898838 on 2 November 2026):

| Step | Time | Detail |
|---|---|---|
| SDK start-up | 14 s | |
| Build | 111 s | 39 s before restore starts, about 20 s restore, about 50 s compile |
| Tests | 40 s | |
| Whitespace format | 16 s | |
| Smoke check | 26 s | |

Every merge to `main` then builds and tests the same commit twice, once in `check.yml` and again in `release.yml`. The release also never waits for the check, so it can ship a commit whose smoke check failed. Merging a stack of pull requests pays for all of this on every merge.

## What Changes

- **One build per merge:**
  - `release.yml`'s two jobs move into `check.yml`. On a push to `main` that raises `Version`, the `check` job packs the plain version (no `-ci.N` suffix), installs and smoke-tests that exact package, and writes the release notes.
  - A new `release` job (`needs: check`, the same job-level write permissions, no checkout) attests and publishes it as before. `release.yml` is deleted.
  - A release now waits for the check, about a minute and a half later than today, and never happens for a commit whose check failed.
- **Provenance:** `publish.yml` accepts packages attested by `check.yml` or by `release.yml` (older tags), still only from `refs/heads/main` of this repository. It checks the certificate identity with a regex in place of `--signer-workflow`.
- **Format in folder mode:** `dotnet format whitespace --folder --verify-no-changes --exclude openspec`, which needs no MSBuild workspace (2.4 s locally, against 7 s).
- **Smoke check:** `dotnet run --no-build`, since the solution build already built HeadlessTerminal in Release.
- **Measured experiments, kept only if a draft pull request's runs show they pay:**
  - **NuGet cache:** `actions/cache` over `~\.nuget\packages`.
  - **.NET first-run variables:** `DOTNET_NOLOGO` and `DOTNET_CLI_TELEMETRY_OPTOUT` stay regardless, as they are harmless; `DOTNET_SKIP_FIRST_TIME_EXPERIENCE` and `DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE` stay only if the cold start shrinks.
- **Docs and references:** `.claude\rules\checks.md`, `MAINTAINING.md`, the comments in `.github\scripts` and `build-test-pack` that name `release.yml`, `Source191.Vantage.slnx`, and the open `install-with-winget` change's text all point at the release job in `check.yml`.
- **No Version bump or changelog line:** nothing under the tool paths in `ToolPaths.ps1` changes.

## Out of scope

- **"Require branches to be up to date":** turning it off in branch protection is a repository setting the owner decides separately.
- **Speed-ups that change what is tested:** splitting jobs, trimming tests or changing the runner image. The Infrastructure tests' 38 s is left as it is.
- **CodeQL and the nightly contract workflow:** both unchanged.
- **Multiple Claude accounts:** a later change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None. Users get the same package, release notes and GitHub Releases. The `release-notes` requirement on a stable Release's notes is met by the same steps, moved, so `.openspec.yaml` declares `skip_specs: true`.

## Impact

- **Workflows and scripts:** `.github\workflows\check.yml`, `release.yml` (deleted), `publish.yml`, `.github\actions\build-test-pack\action.yml`, and comments in `.github\scripts`.
- **Docs:** `.claude\rules\checks.md`, `MAINTAINING.md`, `Source191.Vantage.slnx`, and `openspec\changes\install-with-winget` (text only).
- **Branch protection:** unchanged; it still requires `check`.
