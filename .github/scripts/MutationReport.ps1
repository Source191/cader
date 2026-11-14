#Requires -Version 7
<#
.SYNOPSIS
    Reads Stryker's mutation-report.json (the mutation-testing report schema, version 2).

.DESCRIPTION
    Dot-sourced by Write-MutationRun.ps1 and Format-MutationComment.ps1. The fields it reads are
    files.<path>.mutants[] with status, mutatorName, replacement and location.start.line, checked against a real
    report on 13 November 2026 (Stryker.NET 5.0.0): each file's key is an absolute path with backslashes under
    projectRoot, so a file is named from its src\ folder down.

    The score is Stryker's own: detected (Killed and Timeout) over detected plus undetected (Survived and
    NoCoverage); Ignored, CompileError and RuntimeError mutants count for nothing. A file with none is $null.
#>

function Read-MutationReport([string] $path) {
    Get-Content $path -Raw | ConvertFrom-Json -AsHashtable
}

function Get-MutationScore([object[]] $mutants) {
    $detected = @($mutants | Where-Object { $_.status -in 'Killed', 'Timeout' }).Count
    $undetected = @($mutants | Where-Object { $_.status -in 'Survived', 'NoCoverage' }).Count
    if ($detected + $undetected -eq 0) { return $null }
    return [math]::Round(100.0 * $detected / ($detected + $undetected), 2)
}

function Get-SourceName([string] $path) {
    $marker = '\src\'
    $index = $path.IndexOf($marker, [StringComparison]::OrdinalIgnoreCase)
    if ($index -lt 0) { return $path }
    return $path.Substring($index + $marker.Length)
}

function Get-FileScores($report) {
    $scores = [ordered]@{}
    foreach ($file in $report.files.GetEnumerator()) {
        $score = Get-MutationScore $file.Value.mutants
        if ($null -ne $score) { $scores[(Get-SourceName $file.Key)] = $score }
    }
    $scores
}

function Get-UndetectedMutants($report) {
    foreach ($file in $report.files.GetEnumerator()) {
        foreach ($mutant in $file.Value.mutants) {
            if ($mutant.status -notin 'Survived', 'NoCoverage') { continue }
            [pscustomobject]@{
                File        = Get-SourceName $file.Key
                Line        = $mutant.location.start.line
                Status      = $mutant.status
                Mutator     = $mutant.mutatorName
                Replacement = $mutant.replacement
            }
        }
    }
}
