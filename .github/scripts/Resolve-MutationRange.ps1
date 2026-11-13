#Requires -Version 7
<#
.SYNOPSIS
    Decides what a mutation run should mutate: a release's changes since the last finished run, or all of Core.

.DESCRIPTION
    mutation.yml calls this first. Answers an object with Kind ('release', 'full' or 'skip'), Tag (the release's
    v* tag), Since (the commit or tag Stryker's --since is given, empty for a full run) and Reason.

    - schedule and workflow_dispatch: a full run, with no range.
    - workflow_run: a release only when HeadSha carries a v* tag, which pipeline.yml's release job makes on the
      commit it publishes; otherwise 'skip' with a notice. The range starts at the head commit of the newest finished
      (success or failure) mutation run on main that is an ancestor of HeadSha, so a run replaced in the queue never
      loses its release's changes, and falls back to the previous v* tag, then to no range at all.

    RunsJson is the answer of `gh run list --workflow mutation.yml --branch main --status completed --json
    conclusion,headSha`; without it the script asks gh. Run it locally with a recorded answer to try a case.

.EXAMPLE
    .github\scripts\Resolve-MutationRange.ps1 -EventName workflow_run -HeadSha (git rev-parse HEAD) -RunsJson '[]'
#>
[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '',
    Justification = 'The notice goes to the workflow log; the answer is the only output.')]
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [ValidateSet('workflow_run', 'schedule', 'workflow_dispatch')] [string] $EventName,
    [Parameter(Mandatory)] [string] $HeadSha,
    [string] $RunsJson
)

$ErrorActionPreference = 'Stop'

function Get-ReleaseTag([string] $commit) {
    $tags = @(git tag --points-at $commit --list 'v*' --sort=-v:refname)
    if ($tags.Count -gt 0) { return $tags[0] }
    return ''
}

function Get-FinishedRunCommit([string] $commit, [string] $runsJson) {
    if (-not $runsJson) {
        $runsJson = gh run list --workflow mutation.yml --branch main --status completed --limit 20 --json conclusion,headSha
        if ($LASTEXITCODE -ne 0) { throw 'gh run list failed' }
    }
    $finished = @($runsJson | ConvertFrom-Json | Where-Object { $_.conclusion -in 'success', 'failure' })
    foreach ($run in $finished) {
        git merge-base --is-ancestor $run.headSha $commit 2>$null
        $ancestor = $LASTEXITCODE -eq 0
        $global:LASTEXITCODE = 0   # not an ancestor is an answer, not a failed step
        if ($ancestor -and $run.headSha -ne $commit) { return $run.headSha }
    }
    return ''
}

function Get-PreviousTag([string] $commit) {
    $previous = git describe --tags --abbrev=0 --match 'v*' "$commit^" 2>$null
    $found = $LASTEXITCODE -eq 0
    $global:LASTEXITCODE = 0   # a missing tag is an answer, not a failed step
    if ($found) { return "$previous" }
    return ''
}

if ($EventName -ne 'workflow_run') {
    return [pscustomobject]@{ Kind = 'full'; Tag = ''; Since = ''; Reason = "A full run of Core ($EventName)." }
}

$tag = Get-ReleaseTag $HeadSha
if (-not $tag) {
    Write-Host "::notice::No v* tag on $HeadSha, so no release was made and there is nothing to mutate."
    return [pscustomobject]@{ Kind = 'skip'; Tag = ''; Since = ''; Reason = 'The commit is not a release.' }
}

$since = Get-FinishedRunCommit $HeadSha $RunsJson
$reason = "Release $tag since the last finished mutation run."
if (-not $since) {
    $since = Get-PreviousTag $HeadSha
    $reason = "Release $tag since the previous release tag."
}
if (-not $since) {
    Write-Host "::notice::No earlier mutation run or v* tag before $tag, so the whole of Core is mutated."
    $reason = "Release $tag with no earlier range, so all of Core."
}

[pscustomobject]@{ Kind = 'release'; Tag = $tag; Since = $since; Reason = $reason }
