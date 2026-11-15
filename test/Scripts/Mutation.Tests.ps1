#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }

BeforeAll {
    $script:Scripts = Join-Path $PSScriptRoot '..\..\.github\scripts'
    $script:Fixtures = Join-Path $script:Scripts 'fixtures\mutation'
    $script:Workflow = Join-Path $PSScriptRoot '..\..\.github\workflows\mutation.yml'

    function New-Run([string] $Directory, [double] $Score, [string] $ReportFixture) {
        New-Item -ItemType Directory $Directory -Force | Out-Null
        Set-Content (Join-Path $Directory 'run.json') ('{"kind":"full","score":' + $Score + '}')
        Copy-Item (Join-Path $script:Fixtures $ReportFixture) (Join-Path $Directory 'mutation-report.json')
    }

    # The smallest report the scripts read: one file with the given number of survivors.
    function New-Report([string] $Path, [int] $Survivors) {
        $mutants = 1..$Survivors | ForEach-Object {
            [ordered]@{ mutatorName = 'Boolean mutation'; replacement = 'false'; location = @{ start = @{ line = $_ } }; status = 'Survived' }
        }
        $report = [ordered]@{ projectRoot = 'C:\r\src\Core'; files = @{ 'C:\r\src\Core\Domain\Big.cs' = @{ mutants = @($mutants) } } }
        $report | ConvertTo-Json -Depth 8 | Set-Content $Path
    }

    # A report of made-up mutants in one file, so many of each status.
    function New-CountedReport([string] $Path, [int] $Killed, [int] $Survived, [int] $Ignored = 0) {
        $statuses = @(@('Killed') * $Killed) + @(@('Survived') * $Survived) + @(@('Ignored') * $Ignored)
        $line = 0
        $mutants = foreach ($status in $statuses) {
            $line++
            [ordered]@{ mutatorName = 'Boolean mutation'; replacement = 'false'; location = @{ start = @{ line = $line } }; status = $status }
        }
        $report = [ordered]@{ projectRoot = 'C:\r\src\Core'; files = @{ 'C:\r\src\Core\Domain\Small.cs' = @{ mutants = @($mutants) } } }
        $report | ConvertTo-Json -Depth 8 | Set-Content $Path
    }

    function Write-CountedRelease([string] $Name, [int] $Killed, [int] $Survived, [int] $Ignored = 0) {
        $report = Join-Path $TestDrive "$Name.json"
        New-CountedReport $report $Killed $Survived $Ignored
        & (Join-Path $script:Scripts 'Write-MutationRun.ps1') -ReportPath $report -Kind release -ReleaseTag v1 -Since v0 -HeadSha abc -OutputDirectory (Join-Path $TestDrive $Name)
    }
}

Describe 'Resolve-MutationRange' {
    BeforeAll {
        $script:Resolve = Join-Path $script:Scripts 'Resolve-MutationRange.ps1'
        $script:Repo = Join-Path $TestDrive 'repo'
        New-Item -ItemType Directory $script:Repo | Out-Null
        Push-Location $script:Repo
        git init -q
        $commits = foreach ($name in 'first', 'second', 'third', 'fourth') {
            git -c user.name=t -c user.email=t@example.test commit -q --allow-empty -m $name
            git rev-parse HEAD
        }
        $script:First, $script:Second, $script:Third, $script:Fourth = $commits
        git tag v1.0.0 $script:Second
        git tag v1.1.0 $script:Fourth
        Pop-Location
    }

    BeforeEach { Push-Location $script:Repo }
    AfterEach { Pop-Location }

    It 'runs all of Core, with no range, for a schedule' {
        $range = & $script:Resolve -EventName schedule -HeadSha $script:Fourth

        $range.Kind | Should -Be 'full'
        $range.Since | Should -BeNullOrEmpty
    }

    It 'skips a commit that carries no v tag' {
        $range = & $script:Resolve -EventName workflow_run -HeadSha $script:Third -RunsJson '[]'

        $range.Kind | Should -Be 'skip'
    }

    It 'starts from the previous v tag when no mutation run has finished' {
        $range = & $script:Resolve -EventName workflow_run -HeadSha $script:Fourth -RunsJson '[]'

        $range.Kind | Should -Be 'release'
        $range.Tag | Should -Be 'v1.1.0'
        $range.Since | Should -Be 'v1.0.0'
    }

    It 'starts from the head of the newest finished scheduled run' {
        $runs = '[{"conclusion":"success","event":"schedule","headSha":"' + $script:Third + '"}]'

        $range = & $script:Resolve -EventName workflow_run -HeadSha $script:Fourth -RunsJson $runs

        $range.Since | Should -Be $script:Third
    }

    It 'does not count a workflow_run that stopped at an untagged commit, or a cancelled run' {
        $runs = '[{"conclusion":"success","event":"workflow_run","headSha":"' + $script:Third + '"},{"conclusion":"cancelled","event":"schedule","headSha":"' + $script:First + '"}]'

        $range = & $script:Resolve -EventName workflow_run -HeadSha $script:Fourth -RunsJson $runs

        $range.Since | Should -Be 'v1.0.0'
    }

    It 'counts a failed run on a tagged commit' {
        $runs = '[{"conclusion":"failure","event":"workflow_run","headSha":"' + $script:Second + '"}]'

        $range = & $script:Resolve -EventName workflow_run -HeadSha $script:Fourth -RunsJson $runs

        $range.Since | Should -Be $script:Second
    }

    It 'mutates all of Core for the first release, which has no earlier tag or run' {
        $range = & $script:Resolve -EventName workflow_run -HeadSha $script:Second -RunsJson '[]'

        $range.Kind | Should -Be 'release'
        $range.Since | Should -BeNullOrEmpty
    }
}

Describe 'Write-MutationRun' {
    BeforeAll { $script:Write = Join-Path $script:Scripts 'Write-MutationRun.ps1' }

    It 'reports a finding, and the score, after a release with survivors' {
        $result = & $script:Write -ReportPath (Join-Path $script:Fixtures 'mutation-report.json') -Kind release -ReleaseTag v1 -HeadSha abc -OutputDirectory (Join-Path $TestDrive 'release')

        $result.Finding | Should -BeTrue
        $result.Score | Should -Be 84
    }

    It 'reports nothing after a release whose mutants were all killed' {
        $result = & $script:Write -ReportPath (Join-Path $script:Fixtures 'mutation-report-clean.json') -Kind release -ReleaseTag v1 -HeadSha abc -OutputDirectory (Join-Path $TestDrive 'clean')

        $result.Finding | Should -BeFalse
    }

    It 'writes run.json and a report cut to the counted mutants' {
        $out = Join-Path $TestDrive 'cut'
        & $script:Write -ReportPath (Join-Path $script:Fixtures 'mutation-report.json') -Kind release -ReleaseTag v1 -Since v0 -HeadSha abc -OutputDirectory $out | Out-Null

        (Get-Content (Join-Path $out 'run.json') -Raw | ConvertFrom-Json).since | Should -Be 'v0'
        (Get-Content (Join-Path $out 'mutation-report.json') -Raw) | Should -Not -Match 'Ignored|coveredBy'
    }

    It 'reports a full run whose score fell by more than the tolerance, with the files that fell most' {
        $previous = Join-Path $TestDrive 'previous-high'
        New-Run $previous 100 'mutation-report-clean.json'

        $result = & $script:Write -ReportPath (Join-Path $script:Fixtures 'mutation-report.json') -Kind full -HeadSha abc -OutputDirectory (Join-Path $TestDrive 'full-lower') -PreviousDirectory $previous

        $result.Finding | Should -BeTrue
        (Get-Content (Join-Path $TestDrive 'full-lower\run.json') -Raw | ConvertFrom-Json).fellMost[0].file | Should -BeLike '*NumberAwareOrder.cs'
    }

    It 'ignores a fall within the tolerance, or a rise' {
        New-Run (Join-Path $TestDrive 'previous-near') 84.3 'mutation-report.json'
        New-Run (Join-Path $TestDrive 'previous-low') 70 'mutation-report.json'

        (& $script:Write -ReportPath (Join-Path $script:Fixtures 'mutation-report.json') -Kind full -HeadSha abc -OutputDirectory (Join-Path $TestDrive 'full-near') -PreviousDirectory (Join-Path $TestDrive 'previous-near')).Finding | Should -BeFalse
        (& $script:Write -ReportPath (Join-Path $script:Fixtures 'mutation-report.json') -Kind full -HeadSha abc -OutputDirectory (Join-Path $TestDrive 'full-higher') -PreviousDirectory (Join-Path $TestDrive 'previous-low')).Finding | Should -BeFalse
    }

    It 'takes a full run with no previous run as the baseline' {
        $result = & $script:Write -ReportPath (Join-Path $script:Fixtures 'mutation-report.json') -Kind full -HeadSha abc -OutputDirectory (Join-Path $TestDrive 'baseline')

        $result.Finding | Should -BeFalse
    }

    It 'fails a release run that counted 20 mutants and left one surviving' {
        $result = Write-CountedRelease 'twenty-one-left' -Killed 19 -Survived 1

        $result.Failing | Should -BeTrue
        $result.Counted | Should -Be 20
        $run = Get-Content (Join-Path $TestDrive 'twenty-one-left\run.json') -Raw | ConvertFrom-Json
        $run.failing | Should -BeTrue
        $run.counted | Should -Be 20
    }

    It 'does not fail a release run that counted 19' {
        $result = Write-CountedRelease 'nineteen-one-left' -Killed 18 -Survived 1

        $result.Finding | Should -BeTrue
        $result.Failing | Should -BeFalse
    }

    It 'does not fail one that counted 20 with no survivor' {
        $result = Write-CountedRelease 'twenty-none-left' -Killed 20 -Survived 0

        $result.Failing | Should -BeFalse
    }

    It 'counts an Ignored mutant for nothing' {
        $result = Write-CountedRelease 'nineteen-and-ignored' -Killed 18 -Survived 1 -Ignored 5

        $result.Counted | Should -Be 19
        $result.Failing | Should -BeFalse
    }
}

Describe 'Format-MutationComment' {
    BeforeAll { $script:Format = Join-Path $script:Scripts 'Format-MutationComment.ps1' }

    It 'lists at most 50 mutants and counts the rest' {
        $report = Join-Path $TestDrive 'big-report.json'
        New-Report $report 60
        $results = Join-Path $TestDrive 'big'
        & (Join-Path $script:Scripts 'Write-MutationRun.ps1') -ReportPath $report -Kind release -ReleaseTag v1 -Since v0 -HeadSha abc -OutputDirectory $results | Out-Null

        & $script:Format -ResultsDirectory $results -RunUrl https://example.test/run -OutputPath (Join-Path $results 'comment.md')

        $body = Get-Content (Join-Path $results 'comment.md') -Raw
        ([regex]::Matches($body, '\| Core\\Domain\\Big.cs \|')).Count | Should -Be 50
        $body | Should -Match 'and 10 more in the report'
        $body | Should -Match 'Release v1, range v0\.\.abc, run https://example.test/run'
    }

    It 'cuts the body to the character limit' {
        $results = Join-Path $TestDrive 'big'

        & $script:Format -ResultsDirectory $results -RunUrl https://example.test/run -OutputPath (Join-Path $results 'short.md') -MaxCharacters 200

        (Get-Content (Join-Path $results 'short.md') -Raw).Trim().Length | Should -BeLessOrEqual 200
    }

    It 'shows a code span safe for a table cell' {
        $results = Join-Path $TestDrive 'safe'
        & (Join-Path $script:Scripts 'Write-MutationRun.ps1') -ReportPath (Join-Path $script:Fixtures 'mutation-report.json') -Kind release -ReleaseTag v1 -HeadSha abc -OutputDirectory $results | Out-Null

        & $script:Format -ResultsDirectory $results -RunUrl https://example.test/run -OutputPath (Join-Path $results 'comment.md')

        (Get-Content (Join-Path $results 'comment.md') -Raw) | Should -Match '\\\|\\\|'
    }

    It 'says the run failed, and names the count, when run.json says failing' {
        Write-CountedRelease 'failing-comment' -Killed 19 -Survived 1 | Out-Null
        $results = Join-Path $TestDrive 'failing-comment'

        & $script:Format -ResultsDirectory $results -RunUrl https://example.test/run -OutputPath (Join-Path $results 'comment.md')

        $body = Get-Content (Join-Path $results 'comment.md') -Raw
        $body | Should -Match 'This run failed: it counted 20 mutants and 1 survived or had no covering test\.'
        $body | Should -Match 'Release v1, range v0\.\.abc, run https://example.test/run'
    }

    It 'says nothing of failing when run.json does not' {
        Write-CountedRelease 'small-comment' -Killed 18 -Survived 1 | Out-Null
        $results = Join-Path $TestDrive 'small-comment'

        & $script:Format -ResultsDirectory $results -RunUrl https://example.test/run -OutputPath (Join-Path $results 'comment.md')

        (Get-Content (Join-Path $results 'comment.md') -Raw) | Should -Not -Match 'failed'
    }
}

Describe 'mutation.yml' {
    It 'passes --break-at 0 to a release run, so only the weekly full run can fail on score' {
        $workflow = Get-Content $script:Workflow -Raw

        $workflow | Should -Match ([regex]::Escape('if ($env:KIND -eq ''release'') { $arguments += @(''--break-at'', ''0'') }'))
    }

    It 'records whether the run is failing, and fails the job last, after both uploads, when it is' {
        $workflow = Get-Content $script:Workflow -Raw
        $mutationJob = $workflow.Substring(0, $workflow.IndexOf("`n  report:"))
        $lastStep = $mutationJob.Substring($mutationJob.LastIndexOf('      - '))

        $workflow | Should -Match ([regex]::Escape('"fail=$($result.Failing.ToString().ToLowerInvariant())" >> $env:GITHUB_OUTPUT'))
        $mutationJob.IndexOf('name: stryker-output') | Should -BeGreaterThan $mutationJob.IndexOf('name: mutation-results')
        $mutationJob.IndexOf('name: stryker-output') | Should -BeLessThan $mutationJob.LastIndexOf('      - ')
        $lastStep | Should -Match ([regex]::Escape("if: always() && steps.run.outputs.fail == 'true'"))
        $lastStep | Should -Match 'exit 1'
    }
}
