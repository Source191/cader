#Requires -Version 7
<#
.SYNOPSIS
    Compares two full Stryker mutation-report.json files mutant by mutant.

.DESCRIPTION
    Pairs the mutants of the baseline and the comparison report by source-relative file, mutator, replacement and
    exact location (never by machine path, and not by the per-run id). A real report holds absolute paths and the
    user name, so never commit or paste one.

.EXAMPLE
    .github\scripts\Compare-MutationReports.ps1 -BaselinePath local.json -ComparisonPath runner.json -BaselineName local -ComparisonName runner
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $BaselinePath,
    [Parameter(Mandatory)] [string] $ComparisonPath,
    [string] $BaselineName = 'baseline',
    [string] $ComparisonName = 'comparison',
    [int] $Top = 10
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'MutationReport.ps1')

$statuses = 'Killed', 'Timeout', 'Survived', 'NoCoverage', 'Ignored', 'CompileError', 'RuntimeError'

function Get-KeyedMutants($report) {
    $seen = @{}
    $keyed = [ordered]@{}
    foreach ($file in $report.files.GetEnumerator()) {
        $source = Get-SourceName $file.Key
        foreach ($mutant in $file.Value.mutants) {
            $start = $mutant.location.start
            $end = $mutant.location.end
            $key = "$source|$($mutant.mutatorName)|$($mutant.replacement)|$($start.line):$($start.column)-$($end.line):$($end.column)"
            $seen[$key] = 1 + [int]$seen[$key]
            $keyed["$key#$($seen[$key])"] = [pscustomobject]@{
                File = $source; Mutator = $mutant.mutatorName; Replacement = $mutant.replacement
                Line = $start.line; Status = $mutant.status; Id = [string]$mutant.id
            }
        }
    }
    $keyed
}

function Get-Group($pairs) {
    foreach ($file in $pairs | Group-Object { $_.Baseline.File }) {
        foreach ($mutator in $file.Group | Group-Object { $_.Baseline.Mutator }) {
            [pscustomobject]@{ File = $file.Name; Mutator = $mutator.Name; Count = $mutator.Count }
        }
    }
}

function Get-StatusCounts($mutants) {
    $counts = [ordered]@{}
    foreach ($status in $statuses) { $counts[$status] = @($mutants | Where-Object Status -eq $status).Count }
    $counts
}

$baseline = Get-KeyedMutants (Read-MutationReport $BaselinePath)
$comparison = Get-KeyedMutants (Read-MutationReport $ComparisonPath)

$paired = @($baseline.Keys | Where-Object { $comparison.Contains($_) } |
    ForEach-Object { [pscustomobject]@{ Baseline = $baseline[$_]; Comparison = $comparison[$_] } })

$changes = @($paired | Where-Object { $_.Baseline.Status -ne $_.Comparison.Status } |
    Group-Object { "$($_.Baseline.Status) -> $($_.Comparison.Status)" } | Sort-Object @{ e = 'Count'; Descending = $true }, Name |
    ForEach-Object {
        [pscustomobject]@{
            Change = $_.Name; Count = $_.Count
            Groups = @(Get-Group $_.Group | Sort-Object @{ e = 'Count'; Descending = $true }, File, Mutator | Select-Object -First $Top)
        }
    })

[pscustomobject]@{
    Baseline    = [pscustomobject]@{ Name = $BaselineName; Total = $baseline.Count; Statuses = Get-StatusCounts $baseline.Values; Score = Get-MutationScore $baseline.Values }
    Comparison  = [pscustomobject]@{ Name = $ComparisonName; Total = $comparison.Count; Statuses = Get-StatusCounts $comparison.Values; Score = Get-MutationScore $comparison.Values }
    Paired      = $paired.Count
    IdsAgreeing = @($paired | Where-Object { $_.Baseline.Id -eq $_.Comparison.Id }).Count
    Changes     = $changes
    OneSided    = @()
}
