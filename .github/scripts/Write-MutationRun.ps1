#Requires -Version 7
<#
.SYNOPSIS
    Writes run.json for a mutation run and answers whether it found something to report.

.DESCRIPTION
    mutation.yml runs this after Stryker, beside mutation-report.json, and uploads both as mutation-results. run.json
    holds the kind (release or full), the release tag, the --since commit, the head commit, the whole score and, for a
    full run with a previous one, that run's score and the five files whose scores fell most. The report job reads
    only these two files.

    The results folder also gets mutation-report.json cut to the fields below, so the artefact stays small.

    A finding is, after a release, any Survived or NoCoverage mutant in the changed code, and after a full run, a
    whole score below the previous full run's. PreviousDirectory is the folder holding that run's run.json and
    mutation-report.json; without it (the artefact expired, or this is the first full run) the comparison is skipped
    with a notice and this run becomes the baseline.

    Answers an object with Score, Finding and Reason.

.EXAMPLE
    .github\scripts\Write-MutationRun.ps1 -ReportPath mutation-report.json -Kind full -HeadSha abc -OutputDirectory out
#>
[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '',
    Justification = 'The notice goes to the workflow log; the answer is the only output.')]
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $ReportPath,
    [Parameter(Mandatory)] [ValidateSet('release', 'full')] [string] $Kind,
    [string] $ReleaseTag = '',
    [string] $Since = '',
    [Parameter(Mandatory)] [string] $HeadSha,
    [Parameter(Mandatory)] [string] $OutputDirectory,
    [string] $PreviousDirectory = ''
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'MutationReport.ps1')

$report = Read-MutationReport $ReportPath
$allMutants = @($report.files.Values | ForEach-Object { $_.mutants })
$score = Get-MutationScore $allMutants

$run = [ordered]@{
    kind     = $Kind
    release  = $ReleaseTag
    since    = $Since
    head     = $HeadSha
    score    = $score
}

$finding = $false
$reason = 'Nothing to report.'

if ($Kind -eq 'release') {
    $undetected = @(Get-UndetectedMutants $report)
    $finding = $undetected.Count -gt 0
    if ($finding) { $reason = "$($undetected.Count) mutants survived or had no covering test." }
}
else {
    $previousRunPath = if ($PreviousDirectory) { Join-Path $PreviousDirectory 'run.json' } else { '' }
    $previousReportPath = if ($PreviousDirectory) { Join-Path $PreviousDirectory 'mutation-report.json' } else { '' }
    if (-not $previousRunPath -or -not (Test-Path $previousRunPath) -or -not (Test-Path $previousReportPath)) {
        Write-Host '::notice::No earlier full run''s results were found, so the score is not compared; this run is the baseline.'
        $reason = 'No earlier full run to compare with.'
    }
    else {
        $previousScore = (Get-Content $previousRunPath -Raw | ConvertFrom-Json).score
        $run['previousScore'] = $previousScore
        $finding = $null -ne $score -and $null -ne $previousScore -and $score -lt $previousScore
        if ($finding) {
            $before = Get-FileScores (Read-MutationReport $previousReportPath)
            $after = Get-FileScores $report
            $run['fellMost'] = @($after.GetEnumerator() |
                Where-Object { $before.Contains($_.Key) -and $_.Value -lt $before[$_.Key] } |
                ForEach-Object { [ordered]@{ file = $_.Key; before = $before[$_.Key]; after = $_.Value } } |
                Sort-Object { $_.after - $_.before } |
                Select-Object -First 5)
            $reason = "The score fell from $previousScore to $score."
        }
    }
}

# Only what the score and the comment read, and only the mutants that count: a full report is many megabytes
# of coverage ids, and an artefact of those would outlive its use.
$counted = 'Killed', 'Timeout', 'Survived', 'NoCoverage'
$trimmed = [ordered]@{ projectRoot = $report.projectRoot; files = [ordered]@{} }
foreach ($file in $report.files.GetEnumerator()) {
    $trimmed.files[$file.Key] = [ordered]@{
        mutants = @($file.Value.mutants | Where-Object { $_.status -in $counted } | ForEach-Object {
            [ordered]@{
                mutatorName = $_.mutatorName
                replacement = $_.replacement
                location    = [ordered]@{ start = [ordered]@{ line = $_.location.start.line } }
                status      = $_.status
            }
        })
    }
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$run | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $OutputDirectory 'run.json')
$trimmed | ConvertTo-Json -Depth 8 -Compress | Set-Content (Join-Path $OutputDirectory 'mutation-report.json')

[pscustomobject]@{ Score = $score; Finding = $finding; Reason = $reason }
