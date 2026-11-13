# Proposal

## Why

Issue #93 asked for `win-x64` and `win-arm64` builds. 0.21.0 shipped `win-x64` only, which runs under emulation on Arm Windows: slower to start, heavier on memory, and a poor fit for a console that sits open all day beside Claude Code. Arm Windows 11 laptops are now common among developers, and winget picks an installer by architecture, so the native build can be offered without changing how anyone installs.

## What Changes

- A second publish profile, `win-arm64`, builds the same self-contained single-file program for Arm; the zip is `cader_<version>_win-arm64.zip` beside `cader_<version>_win-x64.zip`.
- `pipeline.yml` builds, unzips and smoke-checks each architecture on a runner of that architecture, so the Arm zip is run before it is released. The release job attests and attaches both zips.
- `publish.yml` verifies both attestations and submits one manifest with an `x64` and an `arm64` installer, so `winget install Source191.Cader` picks the native one.
- A release is published only when both zips pass; one architecture never ships alone.
- `tools\install-local.ps1` publishes for the machine's own architecture, and `-FromArtifact` extracts the zip that matches it.
- The README, MAINTAINING and `.claude\rules\checks.md` and `windows.md` say that two zips are built and checked.

## Capabilities

### New Capabilities

### Modified Capabilities
- `installation`: a release carries a build for each of `win-x64` and `win-arm64`, and winget installs the one that matches the machine. This capability is added by `install-with-winget`, which must be archived first.

## Impact

- **Code:** a new `win-arm64.pubxml` in `src\Source191.Cader\Properties\PublishProfiles` and its reasons in `src\Source191.Cader\README.md`; no C# changes are expected, because the native library (`libonigwrap.dll`) ships a `win-arm64` runtime.
- **Workflows:** `build-test-pack` and `Compress-WingetBuild.ps1` take an architecture; `pipeline.yml`'s check and release jobs gain a second leg and `publish.yml` a second URL.
- **Docs:** README, MAINTAINING, `checks.md`, `windows.md`.
- **Cost:** a second Windows runner (Arm) per check run, and a zip of about 34 MB more per release.

## Out of scope

- Code signing, which is `sign-the-executable`.
- `win-x86`, macOS and Linux.
- Multiple Claude accounts, which is a later change.
- Hand-testing Terminal.Gui and Windows Terminal on real Arm hardware beyond the owner's one check in the tasks.
