#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }

BeforeAll {
    $script:Script = Join-Path $PSScriptRoot '..\..\.github\scripts\Compare-MutationReports.ps1'
    $script:Fixtures = Join-Path $PSScriptRoot '..\..\.github\scripts\fixtures\mutation-compare'

    # The local report is the baseline, so a change reads "Survived -> Killed" for a mutant only the runner kills.
    function Invoke-Compare([string] $Baseline = (Join-Path $script:Fixtures 'local.json'), [string] $Comparison = (Join-Path $script:Fixtures 'runner.json'), [hashtable] $More = @{}) {
        & $script:Script -BaselinePath $Baseline -ComparisonPath $Comparison -BaselineName local -ComparisonName runner @More
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
        $result.Changes.Count | Should -Be @(4, 1, 1)
        $result.Changes[0].Groups | ForEach-Object { "$($_.File) $($_.Mutator) $($_.Count)" } | Should -Be @(
            'Domain\Alpha.cs Equality mutation 2', 'Application\Beta.cs String mutation 1', 'Domain\Alpha.cs Boolean mutation 1')
    }

    It 'shows only the Top largest groups of a change' {
        $result = Invoke-Compare -More @{ Top = 2 }

        $result.Changes[0].Groups.Count | Should -Be 2
        $result.Changes[0].Count | Should -Be 4
    }
}
