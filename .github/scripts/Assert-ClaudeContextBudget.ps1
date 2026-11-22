#Requires -Version 7
<#
.SYNOPSIS
    Fails when CLAUDE.md or a rule in .claude\rules outgrows the budget Claude Code reads it under.

.DESCRIPTION
    Claude Code reads CLAUDE.md in every session and a rule whenever a file its paths match is read, so their size is
    paid on every turn. Each must be at most 12,288 UTF-8 bytes (about 3,000 tokens) and hold no line longer than 600
    characters, front matter included. The generated skills and commands under .claude are not measured. pipeline.yml
    runs it before the documentation-only skip, so a pull request that changes only these files is still checked.
    Exit code 0 means every file fits; 1 means at least one does not, each breach printed as an ::error:: line.

.EXAMPLE
    .github\scripts\Assert-ClaudeContextBudget.ps1 -Root .
#>
[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '',
    Justification = 'Workflow commands such as ::error:: go to the console, as in pipeline.yml.')]
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $Root
)

$ErrorActionPreference = 'Stop'
$maximumBytes = 12288
$maximumLine = 600

$root = (Resolve-Path -LiteralPath $Root).Path
$files = @(
    Join-Path $root 'CLAUDE.md' | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf }
    $rules = Join-Path $root '.claude\rules'
    if (Test-Path -LiteralPath $rules -PathType Container) {
        Get-ChildItem -LiteralPath $rules -Filter '*.md' -File | ForEach-Object FullName
    }
)

$breaches = 0
foreach ($file in $files) {
    $relative = [IO.Path]::GetRelativePath($root, $file).Replace('\', '/')
    $bytes = [IO.File]::ReadAllBytes($file).Length
    if ($bytes -gt $maximumBytes) {
        Write-Host "::error file=$relative::$relative is $bytes bytes; the budget is $maximumBytes. Move evidence to docs\verified-facts or split it by section."
        $breaches++
    }
    $lines = [IO.File]::ReadAllLines($file)
    for ($index = 0; $index -lt $lines.Length; $index++) {
        if ($lines[$index].Length -gt $maximumLine) {
            Write-Host "::error file=$relative,line=$($index + 1)::Line $($index + 1) is $($lines[$index].Length) characters; the budget is $maximumLine. Split it."
            $breaches++
        }
    }
}

if ($breaches -gt 0) {
    Write-Host "$breaches breach(es) of the Claude context budget."
    exit 1
}
Write-Host "All $($files.Count) files are within the Claude context budget."
