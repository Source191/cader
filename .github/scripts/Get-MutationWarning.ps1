<#
.SYNOPSIS
    Returns the warning publish.yml shows when issues labelled "mutation" are open, or nothing when none is.
.DESCRIPTION
    Reads the array that `gh issue list --label mutation --state open --json number,url` prints. The text goes into a
    workflow command, so line breaks are removed and a percent sign is escaped as workflow commands require. The issue
    title is never used, being text from outside.
.PARAMETER IssuesJson
    The JSON array of open issues; empty text or an empty array means none.
#>
[CmdletBinding()]
param(
    [AllowEmptyString()]
    [string] $IssuesJson = ''
)

$ErrorActionPreference = 'Stop'

function Format-ForWorkflowCommand([string] $Text) {
    ($Text -replace '[\r\n]', '') -replace '%', '%25'
}

if ([string]::IsNullOrWhiteSpace($IssuesJson)) {
    return
}

$issues = @(ConvertFrom-Json -InputObject $IssuesJson)
if ($issues.Count -eq 0) {
    return
}

$named = foreach ($issue in $issues) {
    '#{0} ({1})' -f [int] $issue.number, (Format-ForWorkflowCommand ([string] $issue.url))
}

"Open mutation findings: $($named -join ', '). Look at them before approving this run."
