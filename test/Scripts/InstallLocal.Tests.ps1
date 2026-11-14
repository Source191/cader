#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }

BeforeAll {
    $script:Tools = Join-Path $PSScriptRoot '..\..\tools'

    function New-ArtifactFolder([string[]] $RuntimeIdentifiers) {
        $folder = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory $folder | Out-Null
        foreach ($rid in $RuntimeIdentifiers) {
            $staging = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
            New-Item -ItemType Directory $staging | Out-Null
            Set-Content (Join-Path $staging 'cader.exe') $rid
            Compress-Archive -Path (Join-Path $staging 'cader.exe') -DestinationPath (Join-Path $folder "cader_0.22.0_$rid.zip")
        }
        $folder
    }
}

Describe 'Get-RuntimeIdentifier' {
    It 'maps <Architecture> to <Expected>' -ForEach @(
        @{ Architecture = 'X64'; Expected = 'win-x64' }
        @{ Architecture = 'Arm64'; Expected = 'win-arm64' }
        @{ Architecture = 'arm64'; Expected = 'win-arm64' }
    ) {
        & (Join-Path $script:Tools 'Get-RuntimeIdentifier.ps1') -Architecture $Architecture | Should -Be $Expected
    }

    It 'refuses an architecture Cader is not built for' {
        { & (Join-Path $script:Tools 'Get-RuntimeIdentifier.ps1') -Architecture 'X86' } | Should -Throw
    }

    It 'answers for this machine when no architecture is passed' {
        & (Join-Path $script:Tools 'Get-RuntimeIdentifier.ps1') | Should -BeIn @('win-x64', 'win-arm64')
    }
}

Describe 'install-local -FromArtifact' {
    It 'extracts the <Architecture> zip when both are present' -ForEach @(
        @{ Architecture = 'X64'; Expected = 'win-x64' }
        @{ Architecture = 'Arm64'; Expected = 'win-arm64' }
    ) {
        $artifact = New-ArtifactFolder @('win-x64', 'win-arm64')
        $destination = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))

        & (Join-Path $script:Tools 'install-local.ps1') -FromArtifact $artifact -Architecture $Architecture -Destination $destination *> $null

        (Get-Content (Join-Path $destination 'cader.exe')).Trim() | Should -Be $Expected
    }

    It 'fails naming the architecture when only the other zip is present' {
        $artifact = New-ArtifactFolder @('win-x64')
        $destination = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))

        { & (Join-Path $script:Tools 'install-local.ps1') -FromArtifact $artifact -Architecture Arm64 -Destination $destination } |
            Should -Throw '*cader_<version>_win-arm64.zip*'
        $destination | Should -Not -Exist
    }
}
