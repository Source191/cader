# Tasks

## 1. Trim the check job

- [x] 1.1 Change the format step in `.github\actions\build-test-pack\action.yml` to `dotnet format whitespace --folder --verify-no-changes --exclude openspec` and rewrite its comment. Verify locally: it exits 0 on the tree, and exits 2 with a trailing space added to a `src` file (then revert it).
- [x] 1.2 Add `--no-build` to the smoke check's `dotnet run` in `.github\workflows\pipeline.yml`. Verified by the draft pull request's smoke check passing.
- [x] 1.3 Add job-level `DOTNET_NOLOGO`, `DOTNET_CLI_TELEMETRY_OPTOUT`, `DOTNET_SKIP_FIRST_TIME_EXPERIENCE` and `DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE` to the `check` job. Verified by the draft pull request's step timings.
- [x] 1.4 Add `actions/cache` (pinned by commit SHA, with the version in a comment) over `~\.nuget\packages` before `build-test-pack` in the `check` job. Verified by a second draft pull request run hitting the cache, with restore and cache step timings recorded.
- [x] 1.5 Push and open a draft pull request. Compare its step timings with run 41529898838, and drop any of 1.3's last two variables or 1.4 that doesn't pay. Record the kept results in `.claude\rules\checks.md`'s Workflows section with the date.

## 2. One build per merge

- [x] 2.1 Add the "Decide the package version" step (outputs `changed`, `version`, `suffix`) to the `check` job and pass `suffix` to `build-test-pack`. Verify on the draft pull request that the package is still `-ci.N`.
- [x] 2.2 Add "Write the release notes" and the `package` artefact upload after the smoke check, when `changed`. Add the job outputs, and move `release.yml`'s `release` job into `pipeline.yml` with `needs: check`. Delete `release.yml`. Verified by the draft pull request showing the release job skipped (`actionlint` is not installed here).
- [x] 2.3 Change `publish.yml`'s verify step to `--signer-workflow` naming `pipeline.yml` on `refs/heads/main`, with its comments and error text. Verify locally: v0.20.0-preview.30 fails it and passes the same command naming `release.yml`.
- [x] 2.4 Re-point every mention of `release.yml`:
  - `.claude\rules\checks.md`, `MAINTAINING.md` and `Source191.Vantage.slnx`;
  - the comments in `.github\scripts` and `build-test-pack`;
  - `openspec\changes\install-with-winget` (text only).

  Verify with `git grep release.yml`, which should show only history.

## 3. Integration

- [x] 3.1 Run `openspec validate faster-ci` and `dotnet build -c Debug`, and check the draft pull request's `check` and `handoff` are green.

## Workflow follow-up

- **Archive:** archive the change in the same pull request once the timings are settled.
- **After merge (owner):** on the next merge that raises Version, check:
  - the `release` job ran after `check`;
  - the Release has the nupkg;
  - `gh attestation verify --signer-workflow` naming `pipeline.yml` passes.
