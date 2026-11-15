#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }

BeforeAll {
    $script:Script = Join-Path $PSScriptRoot '..\..\.github\scripts\Step-Version.ps1'

    # A copy of a csproj with the given Version, as UTF-8 without a byte order mark and with CRLF line endings.
    function New-Csproj([string] $Version) {
        $path = Join-Path $TestDrive ([guid]::NewGuid().ToString('N') + '.csproj')
        $text = "<Project>`r`n  <PropertyGroup>`r`n    <Version>$Version</Version>`r`n    <Other>1</Other>`r`n  </PropertyGroup>`r`n</Project>`r`n"
        [IO.File]::WriteAllText($path, $text, [Text.UTF8Encoding]::new($false))
        $path
    }

    function Invoke-StepVersion([string] $Version, [string[]] $Switches = @()) {
        $path = New-Csproj $Version
        $before = [IO.File]::ReadAllText($path)
        $output = & pwsh -NoProfile -File $script:Script -Csproj $path @Switches 2>&1
        $after = [IO.File]::ReadAllText($path)
        [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = ($output | Out-String).Trim(); Before = $before; After = $after; Path = $path }
    }
}

Describe 'Step-Version' {
    It 'raises <From> to <To><Label>' -ForEach @(
        @{ From = '0.21.1'; To = '0.21.2'; Switches = @(); Label = '' }
        @{ From = '0.21.2-preview.1'; To = '0.21.2-preview.2'; Switches = @(); Label = '' }
        @{ From = '0.21.1'; To = '0.21.2-preview.1'; Switches = @('-Preview'); Label = ' with -Preview' }
        @{ From = '0.21.2-preview.1'; To = '0.21.2-preview.2'; Switches = @('-Preview'); Label = ' with -Preview' }
    ) {
        $result = Invoke-StepVersion $From $Switches

        $result.ExitCode | Should -Be 0
        $result.Output | Should -Be $To
        $result.After | Should -Be ($result.Before -replace [regex]::Escape("<Version>$From</Version>"), "<Version>$To</Version>")
    }

    It 'changes only the Version line and leaves no byte order mark' {
        $result = Invoke-StepVersion '0.21.1' @('-Preview')

        $bytes = [IO.File]::ReadAllBytes($result.Path)
        $bytes[0..2] | Should -Not -Be @(0xEF, 0xBB, 0xBF)
        $result.After | Should -Be "<Project>`r`n  <PropertyGroup>`r`n    <Version>0.21.2-preview.1</Version>`r`n    <Other>1</Other>`r`n  </PropertyGroup>`r`n</Project>`r`n"
    }

    It 'refuses <Version> with or without -Preview and leaves the file as it was' -ForEach @(
        @{ Version = '0.21.1-local'; Switches = @() }
        @{ Version = '0.21.1-local'; Switches = @('-Preview') }
    ) {
        $result = Invoke-StepVersion $Version $Switches

        $result.ExitCode | Should -Not -Be 0
        $result.After | Should -Be $result.Before
    }

    It 'changes nothing under -WhatIf with -Preview but still prints the next version' {
        $result = Invoke-StepVersion '0.21.1' @('-Preview', '-WhatIf')

        $result.After | Should -Be $result.Before
        $result.Output | Should -Match '0\.21\.2-preview\.1'
    }
}
