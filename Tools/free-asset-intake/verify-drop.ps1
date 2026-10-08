[CmdletBinding()]
param(
    [string]$PythonPath,
    [string]$IntakeRoot,
    [string]$ManifestPath
)

$ErrorActionPreference = 'Stop'
# One scan supplies both the inventory and per-source status; incomplete drops exit 0.
& (Join-Path $PSScriptRoot 'scan-intake.ps1') @PSBoundParameters -Verify
exit $LASTEXITCODE
