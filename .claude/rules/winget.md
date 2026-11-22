---
paths:
  - "src/Source191.Cader.Infrastructure/Packages/**"
  - "**/UpdateCheck*.cs"
  - "**/*PackageVersions*.cs"
  - "**/ProgramFolder*.cs"
  - ".github/workflows/publish.yml"
  - ".github/scripts/Compress-WingetBuild*"
  - "test/Scripts/Compress-WingetBuild*"
  - "tools/install-local.ps1"
  - "test/Scripts/InstallLocal*"
  - "src/Source191.Cader/Properties/PublishProfiles/**"
---

# winget facts (checked 2026-11-02 and 2026-11-03, winget 1.29.380, Windows 11 26100)

How winget installs and upgrades Cader, and what that means for the program. The install spikes are in `docs\verified-facts\winget.md`.

## Installing

- **Local manifests are the owner's:** `LocalManifestFiles` is an admin setting, off by default, so `winget install --manifest` of an unsubmitted manifest needs an administrator shell and is never an unattended session's.
- **Developer Mode:** unprivileged symbolic links need it (on since 3 November 2026 on the owner's machine); without it `ProgramFolderShould`'s link fact skips. It always runs on CI's runners.
- **The package folder:** a portable zip is extracted whole into `%LOCALAPPDATA%\Microsoft\WinGet\Packages\<id>_<source>`, so `libonigwrap.dll`, `CHANGELOG.md`, `logo.ico` and `THIRD-PARTY-NOTICES.txt` sit beside `cader.exe`.
- **The alias is a link:** `%LOCALAPPDATA%\Microsoft\WinGet\Links\cader.exe` is a symbolic link to the executable, and that folder is on the user PATH.
- **Started through the link,** .NET 10 answers the link's folder for both `Environment.ProcessPath` and `AppContext.BaseDirectory`, where none of the other files are, so the program reads its own files through `ProgramFolder.Of`, which follows the link with `File.ResolveLinkTarget(..., returnFinalTarget: true)`.
- **The first manifest** (13 November 2026, v0.21.0, without `ArchiveBinariesDependOnPath`) installed with its three dependencies, the hash verified, the alias added, and the console listed its sessions, so the package folder need not be on the PATH. `winget validate` warns that icons need a verified publisher and passes without them.

## Upgrading and removing

- **Upgrading while Cader runs fails** with `0x8A150052` ("remove: Access is denied" on the executable) after deleting the package's other files, so the running Cader loses `CHANGELOG.md`, `logo.ico` and the native library until an upgrade with nothing running succeeds.
- So `U` waits for Cader to exit before upgrading (`windows.md`, the update facts) and the README says to upgrade with no Cader open. `winget upgrade` keeps the same package folder and link.
- **A local-manifest install** is listed as `ARP\User\X64\<id>__DefaultSource`, so `winget uninstall --id Source191.Cader --exact` finds nothing (`0x8A150014`) and the ARP id must be given. Uninstalling removes the link and the package folder and leaves `%LOCALAPPDATA%\Cader`.
- **A dotnet tool shim** in `~\.dotnet\tools` comes before the Links folder on the PATH and shadows the winget `cader` until it is uninstalled. Not checked: winget's PATH fallback without Developer Mode.

## Arm Windows (13 November 2026)

- GitHub's hosted `windows-11-arm` runner works for this public repository (`PROCESSOR_ARCHITECTURE=ARM64`, Windows 11 10.0.26200, pwsh 7); that image has no `winget` on the PATH.
- The three dependencies each have an `arm64` installer in winget-pkgs: `Microsoft.WindowsTerminal` 1.25.3131.0, `Microsoft.PowerShell` 7.7.0.0 and `Git.Git` 2.56.0.4. Re-run `winget show` on an Arm machine when installing there.
- `Compress-WingetBuild.ps1` reads the PE machine field of `libonigwrap.dll` (0x8664 x64, 0xAA64 Arm64).

## wingetcreate

- `publish.yml` pins `wingetcreate` 1.12.13.0 with its SHA-256; `update <id> --version --urls '<url>|x64' --submit --token` updates an existing manifest, opens the pull request, and fills `ReleaseNotesUrl` and `ReleaseDate` from the GitHub release.
- Making the first manifest by hand (wingetcreate 1.28.1.0): its Tags prompt does not split on commas, its dependency item asks only for MinimumVersion, and it saves the manifests under `~\source\repos\manifests` before asking to submit.
