#Requires -Version 7
<#
.SYNOPSIS
    Zips the self-contained winget build for one architecture and fails unless the zip holds exactly the files the program ships.

.DESCRIPTION
    The publish folder (dotnet publish src\Source191.Cader -p:PublishProfile=<runtime identifier>) must hold cader.exe,
    Terminal.Gui's native libonigwrap.dll, and the three files the program reads or must carry beside itself: CHANGELOG.md
    (the notes of the running version), logo.ico (the Windows Terminal profiles' icon) and THIRD-PARTY-NOTICES.txt. Anything
    else, a PDB or an XML documentation file above all, fails the build rather than shipping, and so does a missing file, and
    so does a libonigwrap.dll built for the other architecture than the runtime identifier names (read from its PE header).
    The zip is flat, so winget extracts every file beside cader.exe. It is named cader_<version>_<runtime identifier>.zip, is
    written to the destination folder, and its full path is the script's only output. Exit code 0 means the zip was written and
    checked; 1 means it was not.

.EXAMPLE
    $zip = .github\scripts\Compress-WingetBuild.ps1 -PublishFolder artifacts\publish -RuntimeIdentifier win-arm64 -Version 0.22.0 -DestinationFolder artifacts\winget
#>
[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '',
    Justification = 'Workflow commands such as ::error:: go to the console, as in the other scripts here.')]
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $PublishFolder,
    [Parameter(Mandatory)] [ValidateSet('win-x64', 'win-arm64')] [string] $RuntimeIdentifier,
    [Parameter(Mandatory)] [string] $Version,
    [Parameter(Mandatory)] [string] $DestinationFolder
)

$ErrorActionPreference = 'Stop'
$expected = @('CHANGELOG.md', 'libonigwrap.dll', 'THIRD-PARTY-NOTICES.txt', 'cader.exe', 'logo.ico')
$machines = @{ 'win-x64' = 0x8664; 'win-arm64' = 0xAA64 }
$Destination = Join-Path $DestinationFolder "cader_$($Version)_$RuntimeIdentifier.zip"

# The PE header's machine field: e_lfanew at offset 0x3C points at "PE\0\0", and the two-byte machine follows it.
function Get-PeMachine([string] $Path) {
    $bytes = [IO.File]::ReadAllBytes($Path)
    if ($bytes.Length -lt 0x40) {
        return 0
    }
    $header = [BitConverter]::ToInt32($bytes, 0x3C)
    if ($header -lt 0 -or $header + 6 -gt $bytes.Length -or [BitConverter]::ToUInt32($bytes, $header) -ne 0x00004550) {
        return 0
    }
    return [int][BitConverter]::ToUInt16($bytes, $header + 4)
}

$folder = (Resolve-Path -LiteralPath $PublishFolder).Path
$published = @(Get-ChildItem -LiteralPath $folder -Recurse -File | ForEach-Object { [IO.Path]::GetRelativePath($folder, $_.FullName) })
$missing = @($expected | Where-Object { $_ -notin $published })
$extra = @($published | Where-Object { $_ -notin $expected })
if ($missing.Count -gt 0 -or $extra.Count -gt 0) {
    if ($missing.Count -gt 0) {
        Write-Host "::error::The winget build is missing $($missing -join ', ') in $folder."
    }
    if ($extra.Count -gt 0) {
        Write-Host "::error::The winget build holds files it must not ship: $($extra -join ', '). Leave them out in src\Source191.Cader\Properties\PublishProfiles\$RuntimeIdentifier.pubxml."
    }
    exit 1
}

$machine = Get-PeMachine (Join-Path $folder 'libonigwrap.dll')
if ($machine -ne $machines[$RuntimeIdentifier]) {
    Write-Host ('::error::libonigwrap.dll has machine type 0x{0:X4}, not 0x{1:X4} for {2}: the build is for another architecture.' -f $machine, $machines[$RuntimeIdentifier], $RuntimeIdentifier)
    exit 1
}

New-Item -ItemType Directory -Force $DestinationFolder | Out-Null
if (Test-Path -LiteralPath $Destination) {
    Remove-Item -LiteralPath $Destination
}
$compress = @{
    LiteralPath      = @($expected | ForEach-Object { Join-Path $folder $_ })
    DestinationPath  = $Destination
    CompressionLevel = 'Optimal'
}
Compress-Archive @compress

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::OpenRead((Resolve-Path -LiteralPath $Destination).Path)
try {
    $entries = @($archive.Entries.FullName | Sort-Object)
}
finally {
    $archive.Dispose()
}
if (($entries -join '|') -ne (($expected | Sort-Object) -join '|')) {
    Write-Host "::error::$Destination holds $($entries -join ', '); expected $($expected -join ', ')."
    exit 1
}

Write-Host "Zipped $($entries.Count) files into $Destination ($([math]::Round((Get-Item -LiteralPath $Destination).Length / 1MB, 1)) MB)."
Write-Output (Resolve-Path -LiteralPath $Destination).Path
exit 0
