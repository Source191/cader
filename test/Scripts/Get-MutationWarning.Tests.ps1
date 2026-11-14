#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }

BeforeAll {
    $script:Script = Join-Path $PSScriptRoot '..\..\.github\scripts\Get-MutationWarning.ps1'
    $script:Workflow = Join-Path $PSScriptRoot '..\..\.github\workflows\publish.yml'

    function Get-Job([string] $Name) {
        $text = Get-Content $script:Workflow -Raw
        $match = [regex]::Match($text, "(?ms)^  $([regex]::Escape($Name)):\r?\n(.*?)(?=^  [\w-]+:\r?\n|\z)")
        $match.Success | Should -BeTrue -Because "publish.yml has a $Name job"
        $match.Groups[1].Value
    }
}

Describe 'Get-MutationWarning' {
    It 'says nothing when no issue is open' {
        & $script:Script -IssuesJson '[]' | Should -BeNullOrEmpty
        & $script:Script -IssuesJson '' | Should -BeNullOrEmpty
    }

    It 'names the issue number and its URL' {
        $json = '[{"number":12,"url":"https://github.com/Northwind/cader/issues/12"}]'

        $warning = & $script:Script -IssuesJson $json

        $warning | Should -Match '#12'
        $warning | Should -Match ([regex]::Escape('https://github.com/Northwind/cader/issues/12'))
        $warning | Should -Match 'before approving'
    }

    It 'names every open issue' {
        $json = '[{"number":12,"url":"https://github.com/Northwind/cader/issues/12"},{"number":15,"url":"https://github.com/Northwind/cader/issues/15"}]'

        $warning = & $script:Script -IssuesJson $json

        $warning | Should -Match '#12'
        $warning | Should -Match '#15'
        $warning | Should -Match ([regex]::Escape('issues/15'))
    }

    It 'keeps a line break or a percent sign in a URL from becoming a second workflow command' {
        $url = "https://github.com/Northwind/cader/issues/12%0A`r`n::error::y"
        $json = ConvertTo-Json @(@{ number = 12; url = $url }) -AsArray

        $warning = & $script:Script -IssuesJson $json

        $warning | Should -Not -Match '[\r\n]'
        $warning | Should -Not -Match '%0A'
        $warning | Should -Match '%250A'
    }
}

Describe 'publish.yml mutation-notice job' {
    It 'holds only contents and issues read, no environment and no secret' {
        $job = Get-Job 'mutation-notice'

        $job | Should -Match '(?m)^    permissions:\r?\n      contents: read\r?\n      issues: read\r?\n'
        $job | Should -Not -Match '(?m)^    environment:'
        $job | Should -Not -Match 'secrets\.'
        $job | Should -Match 'GH_TOKEN: \$\{\{ github\.token \}\}'
    }

    It 'comes before the winget job, which keeps its own rights' {
        $winget = Get-Job 'winget'

        $winget | Should -Match '(?m)^    needs: mutation-notice\r?\n'
        $winget | Should -Match '(?m)^    environment: winget\r?\n'
        $winget | Should -Match '(?m)^    permissions:\r?\n      contents: read\r?\n      attestations: read\r?\n'
    }

    It 'keeps the workflow default at contents read' {
        $text = Get-Content $script:Workflow -Raw

        $text | Should -Match '(?m)^permissions:\r?\n  contents: read\r?\n\r?\njobs:'
    }
}
