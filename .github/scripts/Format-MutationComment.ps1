#Requires -Version 7
<#
.SYNOPSIS
    Builds the Markdown body of a mutation issue comment from a run's results.

.DESCRIPTION
    mutation.yml's report job runs this on the mutation-results artefact (run.json and mutation-report.json).

    - After a release: the release tag, the range and the run's link, then at most MaxMutants Survived or NoCoverage
      mutants (file from src\, line, mutator, replacement) and a count of the rest.
    - After a full run: the previous and the current whole score, and the five files whose scores fell most.

    The body is cut to 60,000 characters, under GitHub's limit for a comment. A replacement is code from the
    repository, so it is shown in a code span, with pipes and line breaks made safe for a table cell.

.EXAMPLE
    .github\scripts\Format-MutationComment.ps1 -ResultsDirectory results -RunUrl https://example.test/run -OutputPath body.md
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $ResultsDirectory,
    [Parameter(Mandatory)] [string] $RunUrl,
    [Parameter(Mandatory)] [string] $OutputPath,
    [int] $MaxMutants = 50,
    [int] $MaxCharacters = 60000
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'MutationReport.ps1')

function ConvertTo-Cell([string] $text) {
    $flat = ($text -replace '\s+', ' ').Replace('`', "'").Replace('|', '\|')
    if ($flat.Length -gt 120) { $flat = $flat.Substring(0, 117) + '...' }
    return $flat
}

function Format-ReleaseBody($run, $report) {
    $mutants = @(Get-UndetectedMutants $report | Sort-Object File, Line)
    $range = if ($run.since) { "$($run.since)..$($run.head)" } else { "all of Core up to $($run.head)" }
    $lines = [Collections.Generic.List[string]]::new()
    $lines.Add("Release $($run.release), range $range, run $RunUrl.")
    $lines.Add('')
    $lines.Add("$($mutants.Count) mutants survived or had no covering test in the changed code:")
    $lines.Add('')
    $lines.Add('| File | Line | Status | Mutator | Replacement |')
    $lines.Add('| --- | --- | --- | --- | --- |')
    foreach ($mutant in $mutants | Select-Object -First $MaxMutants) {
        $lines.Add("| $(ConvertTo-Cell $mutant.File) | $($mutant.Line) | $($mutant.Status) | $(ConvertTo-Cell $mutant.Mutator) | ``$(ConvertTo-Cell $mutant.Replacement)`` |")
    }
    if ($mutants.Count -gt $MaxMutants) {
        $lines.Add('')
        $lines.Add("and $($mutants.Count - $MaxMutants) more in the report.")
    }
    $lines -join "`n"
}

function Format-FullBody($run) {
    $lines = [Collections.Generic.List[string]]::new()
    $lines.Add("The weekly full run at $($run.head) scored $($run.score) %, below the previous full run's $($run.previousScore) %. Run $RunUrl.")
    $lines.Add('')
    $fell = @($run.fellMost)
    if ($fell.Count -gt 0) {
        $lines.Add("The files whose scores fell most (at most five):")
        $lines.Add('')
        $lines.Add('| File | Before | After |')
        $lines.Add('| --- | --- | --- |')
        foreach ($file in $fell) {
            $lines.Add("| $(ConvertTo-Cell $file.file) | $($file.before) % | $($file.after) % |")
        }
    }
    $lines -join "`n"
}

$run = Get-Content (Join-Path $ResultsDirectory 'run.json') -Raw | ConvertFrom-Json
$body = if ($run.kind -eq 'release') {
    Format-ReleaseBody $run (Read-MutationReport (Join-Path $ResultsDirectory 'mutation-report.json'))
}
else {
    Format-FullBody $run
}

$body.Substring(0, [Math]::Min($body.Length, $MaxCharacters)) | Set-Content $OutputPath
