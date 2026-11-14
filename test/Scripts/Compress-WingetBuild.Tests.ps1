#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }

BeforeAll {
    $script:Script = Join-Path $PSScriptRoot '..\..\.github\scripts\Compress-WingetBuild.ps1'
    $script:Files = @('CHANGELOG.md', 'libonigwrap.dll', 'THIRD-PARTY-NOTICES.txt', 'cader.exe', 'logo.ico')

    # The smallest bytes that carry a PE header: the DOS header's e_lfanew at 0x3C points at "PE\0\0" and the machine field.
    function New-FakeLibrary([string] $Path, [uint16] $Machine) {
        $bytes = [byte[]]::new(0x100)
        $bytes[0x3C] = 0x80
        $bytes[0x80] = 0x50
        $bytes[0x81] = 0x45
        [BitConverter]::GetBytes($Machine).CopyTo($bytes, 0x84)
        [IO.File]::WriteAllBytes($Path, $bytes)
    }

    function New-PublishFolder([uint16] $Machine, [string[]] $Leave = @(), [string[]] $Add = @()) {
        $folder = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory $folder | Out-Null
        foreach ($name in $script:Files | Where-Object { $_ -notin $Leave }) {
            if ($name -eq 'libonigwrap.dll') { New-FakeLibrary (Join-Path $folder $name) $Machine }
            else { Set-Content -LiteralPath (Join-Path $folder $name) -Value $name }
        }
        foreach ($name in $Add) { Set-Content -LiteralPath (Join-Path $folder $name) -Value $name }
        $folder
    }

    function Invoke-Compress([string] $Folder, [string] $RuntimeIdentifier, [string] $Version = '0.22.0') {
        $out = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        $output = & pwsh -NoProfile -File $script:Script -PublishFolder $Folder -RuntimeIdentifier $RuntimeIdentifier -Version $Version -DestinationFolder $out 2>&1
        [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = ($output | Out-String); Folder = $out }
    }

    function Get-ZipEntries([string] $Zip) {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $archive = [IO.Compression.ZipFile]::OpenRead($Zip)
        try { @($archive.Entries.FullName | Sort-Object) } finally { $archive.Dispose() }
    }
}

Describe 'Compress-WingetBuild' {
    It 'names the <RuntimeIdentifier> zip for the version and runtime and holds the five files' -ForEach @(
        @{ RuntimeIdentifier = 'win-x64'; Machine = [uint16]0x8664 }
        @{ RuntimeIdentifier = 'win-arm64'; Machine = [uint16]0xAA64 }
    ) {
        $result = Invoke-Compress (New-PublishFolder $Machine) $RuntimeIdentifier

        $result.ExitCode | Should -Be 0
        $zip = Join-Path $result.Folder "cader_0.22.0_$RuntimeIdentifier.zip"
        $zip | Should -Exist
        Get-ZipEntries $zip | Should -Be ($script:Files | Sort-Object)
    }

    It 'writes the zip path as the only output so a workflow can capture it' {
        $result = Invoke-Compress (New-PublishFolder 0xAA64) 'win-arm64' '0.22.0-ci.7'

        $result.ExitCode | Should -Be 0
        $result.Output | Should -Match 'cader_0\.22\.0-ci\.7_win-arm64\.zip'
    }

    It 'fails when libonigwrap.dll is for the other architecture' -ForEach @(
        @{ RuntimeIdentifier = 'win-arm64'; Machine = [uint16]0x8664; Wrong = 'x64' }
        @{ RuntimeIdentifier = 'win-x64'; Machine = [uint16]0xAA64; Wrong = 'arm64' }
    ) {
        $result = Invoke-Compress (New-PublishFolder $Machine) $RuntimeIdentifier

        $result.ExitCode | Should -Be 1
        $result.Output | Should -Match 'libonigwrap\.dll'
        Get-ChildItem $result.Folder -Filter *.zip -ErrorAction SilentlyContinue | Should -BeNullOrEmpty
    }

    It 'fails naming a missing file' {
        $result = Invoke-Compress (New-PublishFolder 0xAA64 -Leave 'logo.ico') 'win-arm64'

        $result.ExitCode | Should -Be 1
        $result.Output | Should -Match 'missing logo\.ico'
    }

    It 'fails naming a file it must not ship' {
        $result = Invoke-Compress (New-PublishFolder 0xAA64 -Add 'cader.pdb') 'win-arm64'

        $result.ExitCode | Should -Be 1
        $result.Output | Should -Match 'cader\.pdb'
    }

    It 'refuses an unknown runtime identifier' {
        $result = Invoke-Compress (New-PublishFolder 0x8664) 'win-x86'

        $result.ExitCode | Should -Not -Be 0
    }
}
