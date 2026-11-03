# Design

## Context

See proposal.md for the motivation and the measured step times.

**How it works today:**

- **`check.yml`:** runs on every pull request and on every push to `main`. Its `check` job is the one status branch protection requires. It packs the tool as `<Version>-ci.<run number>`, installs that package into a temporary tool path and smoke-tests it.
- **`release.yml`:** runs on the same push to `main` with no link to `check`. Its `build` job (read-only) works out whether `Version` changed, then builds, tests, packs the plain version and writes the stable release notes. Its `release` job (write, `id-token` and `attestations`, no checkout) downloads that artefact, attests it and runs `gh release create --target $GITHUB_SHA`.
- **`publish.yml`:** checks that attestation with `--signer-workflow Source191/vantage/.github/workflows/release.yml --source-ref refs/heads/main` before pushing to nuget.org.

## Goals / Non-Goals

**Goals:**

- One build and one test run per commit on `main`.
- The released nupkg is byte for byte the package the smoke check installed.
- `check` gets shorter without testing less.

**Non-Goals:**

- Any change to branch protection, the `handoff` job, CodeQL or the contract workflow.
- Any change to the release's tag, title, notes, prerelease flag or attestation format.

## Decisions

### The release is a job in `pipeline.yml`, not a `workflow_run` of it

**Alternative considered:** a `workflow_run` trigger would keep `release.yml` and `publish.yml`'s signer as they are.

**Why not:** a `workflow_run` run's `GITHUB_SHA` and OIDC claims name the tip of `main` when the release runs, which is a later commit than the one `check` built whenever merges come quickly. The provenance would then name the wrong commit. A job in the same run carries the built commit in its own claims.

**The cost:** the signer `publish.yml` checks changes.

### `check.yml` is renamed `pipeline.yml`

Once it also releases, `check.yml` undersells the file. `pipeline.yml` (workflow name `pipeline`) is the one pipeline every pull request and merge goes through. The job keeps the name `check`, because branch protection requires the job's name, not the file's. The rename is made before any release is signed, because an attestation records the signing workflow's file and `publish.yml` checks it.

**Alternatives considered:**

- **`ci.yml`:** suggests a separate release workflow.
- **`build-and-release.yml`:** wrong for pull requests, which never release.

### `publish.yml` accepts only `pipeline.yml`

`gh attestation verify --signer-workflow Source191/vantage/.github/workflows/pipeline.yml --source-ref refs/heads/main`, as before but naming the new file.

The owner will not publish any release made by the old `release.yml` (up to v0.20.0-preview.30), so those packages fail the check. Verified on 3 November 2026 with v0.20.0-preview.30: it fails this command, and passes the same command naming `release.yml`.

**Alternative considered:** a `--cert-identity-regex` accepting both files, which was this change's first design. It was dropped once the owner ruled out publishing the older releases.

### The `check` job decides the package version first

A step right after checkout, "Decide the package version", moves `release.yml`'s version-changed logic verbatim: `git diff HEAD^ HEAD` on the csproj, plus the regex read of the old `Version`. It outputs `changed`, `version` and `suffix`.

- **`changed`:** `true` only on a push to `main` whose `Version` differs from its first parent's.
- **`suffix`:** empty when `changed`, otherwise `ci.<run number>`.

The suffix is worked out in PowerShell, because an expression such as `changed && '' || 'ci.N'` always gives `'ci.N'` (an empty string is falsy).

**Why the plain version is safe here:** the runner's tool cache is empty, so installing the plain version into the temporary tool path cannot meet an older copy. That artefact is the release's own package, so `install-local.ps1 -FromArtifact` gives the same bits as the release.

### The release job is moved, not rewritten

Its steps, comments and permissions come across from `release.yml` unchanged:

- download `package`, find the nupkg;
- `actions/attest-build-provenance`;
- `gh release create` with `--target $GITHUB_SHA` and the stable notes logic.

Only `needs` changes, to `check`, and the outputs it reads (`needs.check.outputs.*`). The `check` job uploads the `package` artefact (the nupkg and `release-notes.md`) only when `changed`, and after the smoke check, so a failing smoke check leaves nothing to release. The workflow-level `permissions: contents: read` stays, and the release job raises its own as before.

### Whitespace format runs in folder mode

`dotnet format whitespace --folder --verify-no-changes --exclude openspec` reads `.editorconfig` like workspace mode, but loads no MSBuild workspace and skips SDK-generated files unless `--include-generated` is passed.

- **Why exclude `openspec`:** the archive holds a sample `split-types.cs` that would fail.
- **What it covers:** every other `.cs` file sits in a project of the solution, so the files checked are the same.
- **Alternatives considered:** running workspace mode in the background beside the tests, or `IDE0055` in the build. Neither was chosen: the first adds a process to manage, and the second turns local builds red on whitespace, while it is uncertain whether it covers final newlines.

### The smoke check runs with `--no-build`

The solution build has already built `tools\HeadlessTerminal` in Release, so `dotnet run --no-build -c Release` saves the incremental build check.

### Experiments are kept only on evidence

Each experiment is measured on the draft pull request against run 41529898838.

- **NuGet cache:** `actions/cache` pinned by commit SHA over `~\.nuget\packages`, keyed on the hash of every `*.csproj`, `Directory.Build.props`, `nuget.config` and `global.json`, with an OS prefix as the restore key. It is kept if the cache step plus the restore phase beat the 20 s restore.
- **First-run variables:** `DOTNET_NOLOGO` and `DOTNET_CLI_TELEMETRY_OPTOUT` stay regardless, since they cost nothing and send nothing. `DOTNET_SKIP_FIRST_TIME_EXPERIENCE` and `DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE` stay only if the 14 s `dotnet --version` or the 39 s before restore shrinks.

**Why the cache can be trusted:** GitHub scopes caches by ref. A pull request reads its own and `main`'s caches but writes only its own, and a run on `main` never reads a pull request's. Only workflow runs on `main`, which run reviewed code, can write the cache a release build restores from. NuGet verifies a package's signature when it is first extracted; extracted folders restored from cache are trusted as they are, which is the same trust the runner already gives the job's own earlier steps.

## Risks / Trade-offs

- **The release appears later:** about 1.5 minutes after the merge, since it now waits for `check`. → Accepted: it is also the guarantee that a failed check releases nothing.
- **The release job can't be exercised before merge:** it runs only on a push to `main` that raises `Version`. → It is moved verbatim. The owner checks the first such merge after this one:
  - the release job ran after `check`;
  - the Release has the nupkg;
  - `gh attestation verify --signer-workflow` naming `pipeline.yml` passes.
- **An older release can't be published:** → Accepted by the owner; a fix is a new version in any case.
- **Folder mode misses a file that workspace mode checked:** → Every project file lives under the repository root, and folder mode covers everything there except `openspec`. Verified locally by adding a trailing space to a `src` file and seeing exit 2.
- **A poisoned or stale cache:** → It is keyed on every project file. The experiment is dropped if it doesn't pay.

## Migration Plan

- **Rollout:** merging this pull request changes no Version, so its own push to `main` runs `check` only and no release. The next merge that raises Version is the first release made by `pipeline.yml`.
- **Rollback:** revert the merge. `check.yml` and `release.yml` return, and `publish.yml` names `release.yml` again.
