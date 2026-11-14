<#
.SYNOPSIS
    Answers the runtime identifier (win-x64 or win-arm64) for an architecture, by default this machine's own.

.PARAMETER Architecture
    A System.Runtime.InteropServices.Architecture name (X64 or Arm64, any case). Defaults to the operating system's, which
    is the real one even when this PowerShell is an x64 build running under emulation on Arm Windows.
#>
[CmdletBinding()]
param(
    [System.Runtime.InteropServices.Architecture]$Architecture = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture
)

switch ($Architecture) {
    X64 { 'win-x64' }
    Arm64 { 'win-arm64' }
    default { throw "Cader is built for X64 and Arm64 Windows, not $Architecture." }
}
