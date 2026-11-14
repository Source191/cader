#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }

BeforeAll {
    $script:Script = Join-Path $PSScriptRoot '..\..\.github\scripts\Compare-MutationReports.ps1'
    $script:Fixtures = Join-Path $PSScriptRoot '..\..\.github\scripts\fixtures\mutation-compare'

    # The local report is the baseline, so a change reads "Survived -> Killed" for a mutant only the runner kills.
    function Invoke-Compare([string] $Baseline = (Join-Path $script:Fixtures 'local.json'), [string] $Comparison = (Join-Path $script:Fixtures 'runner.json'), [hashtable] $More = @{}) {
        & $script:Script -BaselinePath $Baseline -ComparisonPath $Comparison -BaselineName local -ComparisonName runner @More
    }

    # The smallest report the script reads: one file holding the given mutants (a mutator, a replacement, a line and a status each).
    function New-Report([string] $Name, [hashtable[]] $Mutants) {
        $list = @($Mutants | ForEach-Object {
            [ordered]@{
                id = [guid]::NewGuid().ToString('N'); mutatorName = $_.Mutator; replacement = $_.Replacement; status = $_.Status
                location = @{ start = @{ line = $_.Line; column = 1 }; end = @{ line = $_.Line; column = 9 } }
            }
        })
        $report = [ordered]@{ files = @{ 'C:\r\src\Core\Domain\Big.cs' = @{ mutants = $list } } }
        $path = Join-Path $TestDrive "$Name.json"
        $report | ConvertTo-Json -Depth 8 | Set-Content $path
        $path
    }
}

Describe 'Compare-MutationReports' {
    It 'pairs the same mutant on two machines by file, mutator, replacement and location, not by id' {
        $result = Invoke-Compare

        $result.Paired | Should -Be 7
        $result.OneSided | Should -BeNullOrEmpty
        $result.IdsAgreeing | Should -Be 1
        $result.Baseline.Statuses.Killed | Should -Be 2
        $result.Baseline.Statuses.Survived | Should -Be 5
        $result.Comparison.Statuses.Killed | Should -Be 5
        $result.Comparison.Statuses.Timeout | Should -Be 1
        $result.Comparison.Statuses.NoCoverage | Should -Be 1
    }

    It 'groups the mutants whose status differs by status change, then file, then mutator, largest first' {
        $result = Invoke-Compare

        $result.Changes.Change | Should -Be @('Survived -> Killed', 'Killed -> Timeout', 'Survived -> NoCoverage')
        $result.Changes | ForEach-Object { $_.Count } | Should -Be @(4, 1, 1)
        $result.Changes[0].Groups | ForEach-Object { "$($_.File) $($_.Mutator) $($_.Count)" } | Should -Be @(
            'Source191.Cader.Core\Domain\Alpha.cs Equality mutation 2',
            'Source191.Cader.Core\Application\Beta.cs String mutation 1',
            'Source191.Cader.Core\Domain\Alpha.cs Boolean mutation 1')
    }

    It 'shows only the Top largest groups of a change' {
        $result = Invoke-Compare -More @{ Top = 2 }

        $result.Changes[0].Groups.Count | Should -Be 2
        $result.Changes[0].Count | Should -Be 4
    }

    It 'lists a mutant present in only one report as its own group' {
        $both = @{ Mutator = 'Boolean mutation'; Replacement = 'false'; Line = 1; Status = 'Killed' }
        $only = @{ Mutator = 'String mutation'; Replacement = '""'; Line = 2; Status = 'Survived' }

        $result = Invoke-Compare (New-Report 'a' @($both, $only)) (New-Report 'b' @($both))

        $result.Paired | Should -Be 1
        $result.OneSided.Count | Should -Be 1
        $result.OneSided[0].Side | Should -Be 'local'
        $result.OneSided[0].Mutator | Should -Be 'String mutation'
    }

    It 'pairs duplicate keys in report order and reports a count that does not match as one-sided' {
        $twin = { param($status) @{ Mutator = 'Boolean mutation'; Replacement = 'false'; Line = 1; Status = $status } }

        $same = Invoke-Compare (New-Report 'a' @((& $twin 'Survived'), (& $twin 'Killed'))) (New-Report 'b' @((& $twin 'Killed'), (& $twin 'Killed')))
        $fewer = Invoke-Compare (New-Report 'c' @((& $twin 'Killed'), (& $twin 'Killed'))) (New-Report 'd' @((& $twin 'Killed')))

        $same.Paired | Should -Be 2
        $same.Changes.Change | Should -Be 'Survived -> Killed'
        $same.Changes[0].Count | Should -Be 1
        $fewer.Paired | Should -Be 1
        $fewer.OneSided.Count | Should -Be 1
    }

    It 'names the killing tests of a group on the side that killed it' {
        $result = Invoke-Compare

        $equality = $result.Changes[0].Groups[0]
        $equality.KillingTests | ForEach-Object { "$($_.Name) $($_.Count)" } | Should -Be @('AlphaShould.Decide 1', 'AlphaShould.Reject 1')
        $timeout = $result.Changes | Where-Object Change -eq 'Killed -> Timeout'
        $timeout.Groups[0].KillingTests.Name | Should -Be 'GammaShould.Block'
    }

    It 'prints no absolute path and writes the same as Markdown to OutputPath' {
        $output = Join-Path $TestDrive 'comparison.md'

        $result = Invoke-Compare -More @{ OutputPath = $output }

        $markdown = Get-Content $output -Raw
        $markdown.TrimEnd() | Should -Be $result.Markdown.TrimEnd()
        $markdown | Should -Match 'Source191\.Cader\.Core\\Domain\\Alpha\.cs'
        $markdown | Should -Match 'Survived -> Killed: 4'
        $markdown | Should -Match 'AlphaShould\.Decide'
        $everything = $markdown + ($result | ConvertTo-Json -Depth 8)
        $everything | Should -Not -Match ([regex]::Escape('D:\a'))
        $everything | Should -Not -Match ([regex]::Escape('C:\Users\me'))
        $everything | Should -Not -Match 'D:\\\\a'
        $everything | Should -Not -Match 'C:\\\\Users\\\\me'
    }
}
