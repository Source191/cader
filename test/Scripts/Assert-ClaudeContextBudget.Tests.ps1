#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }

BeforeAll {
    $script:Script = Join-Path $PSScriptRoot '..\..\.github\scripts\Assert-ClaudeContextBudget.ps1'

    # A new, empty repository root in the test drive.
    function New-Root {
        $root = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $root | Out-Null
        $root
    }

    # Writes text as UTF-8 without a byte order mark, creating the folder it goes in.
    function Write-Text([string] $Path, [string] $Text) {
        New-Item -ItemType Directory -Path (Split-Path $Path) -Force | Out-Null
        [IO.File]::WriteAllText($Path, $Text, [Text.UTF8Encoding]::new($false))
    }

    # Text of exactly the given number of UTF-8 bytes with LF endings, as the repository stores it, in lines of 100.
    function New-TextOfBytes([int] $Bytes) {
        $line = ('a' * 99) + "`n"
        $whole = [math]::Floor($Bytes / $line.Length)
        ($line * $whole) + ('b' * ($Bytes - ($whole * $line.Length)))
    }

    function Invoke-Budget([string] $Root) {
        $output = & pwsh -NoProfile -File $script:Script -Root $Root 2>&1
        [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = ($output | Out-String).Trim() }
    }
}

Describe 'Assert-ClaudeContextBudget' {
    It 'passes CLAUDE.md and a rule of exactly 12,288 bytes' {
        $root = New-Root
        Write-Text (Join-Path $root 'CLAUDE.md') (New-TextOfBytes 12288)
        Write-Text (Join-Path $root '.claude\rules\area.md') (New-TextOfBytes 12288)

        $result = Invoke-Budget $root

        $result.ExitCode | Should -Be 0
    }

    It 'fails a file one byte over, naming it' -ForEach @(
        @{ Relative = 'CLAUDE.md' }
        @{ Relative = '.claude\rules\area.md' }
    ) {
        $root = New-Root
        Write-Text (Join-Path $root $Relative) (New-TextOfBytes 12289)

        $result = Invoke-Budget $root

        $result.ExitCode | Should -Be 1
        $result.Output | Should -Match ([regex]::Escape("::error file=$($Relative.Replace('\', '/'))::"))
        $result.Output | Should -Match '12289 bytes'
    }

    It 'counts bytes, not characters' {
        $root = New-Root
        # 41 lines of 100 three-byte characters: 4,141 characters but 12,341 bytes.
        $text = (('€' * 100) + "`n") * 41
        Write-Text (Join-Path $root 'CLAUDE.md') $text

        $result = Invoke-Budget $root

        $result.ExitCode | Should -Be 1
    }

    It 'counts a CRLF checkout as the LF the repository stores' {
        $root = New-Root
        # 12,288 bytes with LF endings, 12,288 plus one per line with CRLF.
        $lf = ((('a' * 99) + "`n") * 122) + ('b' * 88)
        Write-Text (Join-Path $root 'CLAUDE.md') ($lf -replace "`n", "`r`n")

        $result = Invoke-Budget $root

        $result.ExitCode | Should -Be 0
    }

    It 'fails a line of 601 characters, naming the file and line' {
        $root = New-Root
        Write-Text (Join-Path $root '.claude\rules\area.md') ("---`n---`n" + ('x' * 601) + "`n")

        $result = Invoke-Budget $root

        $result.ExitCode | Should -Be 1
        $result.Output | Should -Match ([regex]::Escape('::error file=.claude/rules/area.md,line=3::'))
    }

    It 'passes a line of 600 characters' {
        $root = New-Root
        Write-Text (Join-Path $root 'CLAUDE.md') (('x' * 600) + "`r`n")

        $result = Invoke-Budget $root

        $result.ExitCode | Should -Be 0
    }

    It 'passes a root with no .claude\rules' {
        $root = New-Root
        Write-Text (Join-Path $root 'CLAUDE.md') "# Short`n"

        $result = Invoke-Budget $root

        $result.ExitCode | Should -Be 0
    }

    It 'never reads the generated skills and commands' {
        $root = New-Root
        Write-Text (Join-Path $root '.claude\skills\big\SKILL.md') (New-TextOfBytes 20000)
        Write-Text (Join-Path $root '.claude\commands\big.md') ('x' * 2000)

        $result = Invoke-Budget $root

        $result.ExitCode | Should -Be 0
    }
}
