<#
.SYNOPSIS
    Builds the self-contained winget build, or extracts one from a check run's artefact, into a folder of its own
    for testing before it is released, and prints the executable's full path.

.DESCRIPTION
    The build goes to %LOCALAPPDATA%\Cader\local, replacing whatever an earlier run put there. Nothing is added to the
    PATH and no winget install is touched, so the `cader` command still runs the installed release; run
    the local build by the printed path.

    A local build is published as <Version>-local.<UTC timestamp>, a prerelease, so it never checks for updates or offers
    one. A running cader.exe from the local folder locks its files, so any such process is stopped first; a Cader
    started from anywhere else is left running. Safe to re-run.

.PARAMETER Architecture
    X64 or Arm64: which build to publish or extract. Defaults to this machine's own (tools\Get-RuntimeIdentifier.ps1), so an Arm
    machine gets the native build; passing it is for tests and for trying the other architecture's zip.

.PARAMETER Destination
    The folder the build is put in. Defaults to %LOCALAPPDATA%\Cader\local; tests pass a temporary one.

.PARAMETER FromArtifact
    A folder holding cader_<version>_win-x64.zip and cader_<version>_win-arm64.zip, typically one downloaded from a GitHub Actions check run with
    `gh run download <run id>`, or from a GitHub prerelease. The newest zip for this machine's architecture in the folder (or below it) is extracted as it
    is; nothing is built.

.EXAMPLE
    .\tools\install-local.ps1

.EXAMPLE
    gh run download 123456789 --name cader-0.21.0-ci.42-win-arm64 --dir .\downloaded
    .\tools\install-local.ps1 -FromArtifact .\downloaded
#>
[CmdletBinding()]
param(
    [string]$FromArtifact,
    [System.Runtime.InteropServices.Architecture]$Architecture = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture,
    [string]$Destination = (Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) 'Cader\local')
)

$ErrorActionPreference = 'Stop'

$RepoRoot = Split-Path -Parent $PSScriptRoot
$Project = Join-Path $RepoRoot 'src\Source191.Cader\Source191.Cader.csproj'
$Published = Join-Path $RepoRoot 'artifacts\publish'
$RuntimeIdentifier = & (Join-Path $PSScriptRoot 'Get-RuntimeIdentifier.ps1') -Architecture $Architecture
$Executable = Join-Path $Destination 'cader.exe'

$running = @(Get-Process -Name cader -ErrorAction SilentlyContinue | Where-Object { $_.Path -and $_.Path.StartsWith($Destination, [StringComparison]::OrdinalIgnoreCase) })
if ($running.Count -gt 0) {
    $running | Stop-Process -Force
    Write-Host "Stopped $($running.Count) cader.exe process(es) running from $Destination."
}

if ($FromArtifact) {
    $source = (Resolve-Path -LiteralPath $FromArtifact).Path
    $zip = Get-ChildItem -LiteralPath $source -Recurse -Filter "cader_*_$RuntimeIdentifier.zip" |
        Sort-Object LastWriteTime |
        Select-Object -Last 1
    if ($null -eq $zip) {
        Write-Error "No cader_<version>_$RuntimeIdentifier.zip found in '$source'."
        exit 1
    }

    if (Test-Path -LiteralPath $Destination) {
        Remove-Item -LiteralPath $Destination -Recurse -Force
    }
    Expand-Archive -LiteralPath $zip.FullName -DestinationPath $Destination
    Write-Host "Extracted $($zip.Name) to $Destination."
}
else {
    $baseVersion = (Select-Xml -Path $Project -XPath '//Version').Node.InnerText
    $stamp = [DateTimeOffset]::UtcNow.ToString('yyyyMMddHHmmss')
    $version = "$baseVersion-local.$stamp"
    Write-Host "Publishing Cader $version for $RuntimeIdentifier."
    if (Test-Path -LiteralPath $Published) {
        Remove-Item -LiteralPath $Published -Recurse -Force
    }
    dotnet publish $Project -p:PublishProfile=$RuntimeIdentifier -p:Version=$version
    if ($LASTEXITCODE -ne 0) {
        Write-Error "dotnet publish failed with exit code $LASTEXITCODE."
        exit $LASTEXITCODE
    }

    if (Test-Path -LiteralPath $Destination) {
        Remove-Item -LiteralPath $Destination -Recurse -Force
    }
    New-Item -ItemType Directory -Force $Destination | Out-Null
    Copy-Item -Path (Join-Path $Published '*') -Destination $Destination -Recurse
}

Write-Host "Installed at $Executable" -ForegroundColor Green
Write-Host "Run it by that path; the cader command on the PATH still runs the installed release."