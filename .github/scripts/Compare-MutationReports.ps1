#Requires -Version 7
<#
.SYNOPSIS
    Compares two full Stryker mutation-report.json files mutant by mutant.

.DESCRIPTION
    Pairs the mutants of the baseline and the comparison report by source-relative file, mutator, replacement and
    exact location (never by machine path, and not by the per-run id), and groups the pairs whose status differs by
    status change, then file, then mutator, largest first. For a group where one side Killed, it names the tests
    that killed on that side (killedBy, resolved through the report's testFiles) when the report carries them.
    Mutants present in only one report, or in a different number of times, are listed as one-sided, never dropped.

    It answers an object (Baseline, Comparison, Paired, IdsAgreeing, Changes, OneSided and Markdown) and writes
    the Markdown to OutputPath when given. It prints only source-relative paths: a real report holds absolute
    paths and the user name, so never commit or paste one.

.EXAMPLE
    .github\scripts\Compare-MutationReports.ps1 -BaselinePath runner.json -ComparisonPath local.json -BaselineName runner -ComparisonName local
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $BaselinePath,
    [Parameter(Mandatory)] [string] $ComparisonPath,
    [string] $BaselineName = 'baseline',
    [string] $ComparisonName = 'comparison',
    [string] $OutputPath,
    [int] $Top = 10
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'MutationReport.ps1')

$statuses = 'Killed', 'Timeout', 'Survived', 'NoCoverage', 'Ignored', 'CompileError', 'RuntimeError'

function Get-TestNames($report) {
    $names = @{}
    if (-not $report.ContainsKey('testFiles')) { return $names }
    foreach ($file in $report.testFiles.Values) {
        foreach ($test in $file.tests) { $names[[string]$test.id] = $test.name }
    }
    $names
}

# Every mutant of a report, keyed by source-relative file, mutator, replacement and location, with a running
# number for a repeated key so duplicates pair in report order.
function Get-KeyedMutants($report) {
    $names = Get-TestNames $report
    $seen = @{}
    $keyed = [ordered]@{}
    foreach ($file in $report.files.GetEnumerator()) {
        $source = Get-SourceName $file.Key
        foreach ($mutant in $file.Value.mutants) {
            $start = $mutant.location.start
            $end = $mutant.location.end
            $key = "$source|$($mutant.mutatorName)|$($mutant.replacement)|$($start.line):$($start.column)-$($end.line):$($end.column)"
            $seen[$key] = 1 + [int]$seen[$key]
            $killers = @($mutant.killedBy | Where-Object { $null -ne $_ } | ForEach-Object { $names[[string]$_] ?? "test $_" })
            $keyed["$key#$($seen[$key])"] = [pscustomobject]@{
                File = $source; Mutator = $mutant.mutatorName; Replacement = $mutant.replacement
                Line = $start.line; Status = $mutant.status; Id = [string]$mutant.id; Killers = $killers
            }
        }
    }
    $keyed
}

function Get-StatusCounts($mutants) {
    $counts = [ordered]@{}
    foreach ($status in $statuses) { $counts[$status] = @($mutants | Where-Object Status -eq $status).Count }
    $counts
}

function Get-Group($pairs) {
    foreach ($file in $pairs | Group-Object { $_.Baseline.File }) {
        foreach ($mutator in $file.Group | Group-Object { $_.Baseline.Mutator }) {
            $killers = $mutator.Group | ForEach-Object { if ($_.Baseline.Status -eq 'Killed') { $_.Baseline.Killers } else { $_.Comparison.Killers } }
            [pscustomobject]@{
                File = $file.Name; Mutator = $mutator.Name; Count = $mutator.Count
                KillingTests = @($killers | Where-Object { $_ } | Group-Object | Sort-Object Count -Descending |
                    Select-Object -First 3 | ForEach-Object { [pscustomobject]@{ Name = $_.Name; Count = $_.Count } })
            }
        }
    }
}

$baseline = Get-KeyedMutants (Read-MutationReport $BaselinePath)
$comparison = Get-KeyedMutants (Read-MutationReport $ComparisonPath)

$paired = @($baseline.Keys | Where-Object { $comparison.Contains($_) } |
    ForEach-Object { [pscustomobject]@{ Baseline = $baseline[$_]; Comparison = $comparison[$_] } })
$oneSided = @(
    $baseline.Keys | Where-Object { -not $comparison.Contains($_) } | ForEach-Object { $baseline[$_] | Select-Object @{ n = 'Side'; e = { $BaselineName } }, File, Line, Mutator, Replacement, Status }
    $comparison.Keys | Where-Object { -not $baseline.Contains($_) } | ForEach-Object { $comparison[$_] | Select-Object @{ n = 'Side'; e = { $ComparisonName } }, File, Line, Mutator, Replacement, Status }
)

$changes = @($paired | Where-Object { $_.Baseline.Status -ne $_.Comparison.Status } |
    Group-Object { "$($_.Baseline.Status) -> $($_.Comparison.Status)" } | Sort-Object @{ e = 'Count'; Descending = $true }, Name |
    ForEach-Object {
        [pscustomobject]@{
            Change = $_.Name; Count = $_.Count
            Groups = @(Get-Group $_.Group | Sort-Object @{ e = 'Count'; Descending = $true }, File, Mutator | Select-Object -First $Top)
        }
    })

$result = [pscustomobject]@{
    Baseline     = [pscustomobject]@{ Name = $BaselineName; Total = $baseline.Count; Statuses = Get-StatusCounts $baseline.Values; Score = Get-MutationScore $baseline.Values }
    Comparison   = [pscustomobject]@{ Name = $ComparisonName; Total = $comparison.Count; Statuses = Get-StatusCounts $comparison.Values; Score = Get-MutationScore $comparison.Values }
    Paired       = $paired.Count
    IdsAgreeing  = @($paired | Where-Object { $_.Baseline.Id -eq $_.Comparison.Id }).Count
    Changes      = $changes
    OneSided     = $oneSided
    Markdown     = ''
}

function Format-Markdown($result) {
    $lines = [Collections.Generic.List[string]]::new()
    $lines.Add('# Mutation report comparison')
    $lines.Add('')
    $lines.Add('| | Mutants | ' + ($statuses -join ' | ') + ' | Score |')
    $lines.Add('|---|---|' + ('---|' * $statuses.Count) + '---|')
    foreach ($side in $result.Baseline, $result.Comparison) {
        $lines.Add("| $($side.Name) | $($side.Total) | " + (($statuses | ForEach-Object { $side.Statuses[$_] }) -join ' | ') + " | $($side.Score) |")
    }
    $lines.Add('')
    $lines.Add("Paired: $($result.Paired); ids agreeing: $($result.IdsAgreeing); one-sided: $($result.OneSided.Count). A change reads $($result.Baseline.Name) -> $($result.Comparison.Name).")
    foreach ($change in $result.Changes) {
        $lines.Add('')
        $lines.Add("## $($change.Change): $($change.Count)")
        $lines.Add('')
        $lines.Add('| Count | File | Mutator | Killing tests |')
        $lines.Add('|---|---|---|---|')
        foreach ($group in $change.Groups) {
            $tests = ($group.KillingTests | ForEach-Object { "$($_.Name) ($($_.Count))" }) -join '; '
            $lines.Add("| $($group.Count) | $($group.File) | $($group.Mutator) | $tests |")
        }
    }
    if ($result.OneSided.Count -gt 0) {
        $lines.Add('')
        $lines.Add("## Present in one report only: $($result.OneSided.Count)")
        $lines.Add('')
        $lines.Add('| Side | File | Line | Mutator | Status |')
        $lines.Add('|---|---|---|---|---|')
        foreach ($mutant in $result.OneSided | Select-Object -First $Top) {
            $lines.Add("| $($mutant.Side) | $($mutant.File) | $($mutant.Line) | $($mutant.Mutator) | $($mutant.Status) |")
        }
    }
    $lines -join "`n"
}

$result.Markdown = Format-Markdown $result
if ($OutputPath) { Set-Content -LiteralPath $OutputPath -Value $result.Markdown }
$result
