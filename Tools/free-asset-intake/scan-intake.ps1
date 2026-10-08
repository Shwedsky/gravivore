[CmdletBinding()]
param(
    [string]$PythonPath,
    [string]$IntakeRoot,
    [string]$ManifestPath,
    [switch]$Verify
)

$ErrorActionPreference = 'Stop'
try {
    $intakePythonArgs = @()
    $intakeCandidates = @()
    if ($PythonPath) {
        $intakeCandidates += $PythonPath
    } else {
        if ($env:GRAVIVORE_INTAKE_PYTHON) { $intakeCandidates += $env:GRAVIVORE_INTAKE_PYTHON }
        foreach ($intakeName in @('python', 'python3', 'py')) {
            $intakeCommand = Get-Command $intakeName -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($intakeCommand -and $intakeCommand.Source -notmatch '\\WindowsApps\\') {
                $intakeCandidates += $intakeCommand.Source
            }
        }
        $intakeCandidates += Join-Path $env:USERPROFILE '.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
    }
    $intakePython = $null
    foreach ($intakeCandidate in $intakeCandidates) {
        if (-not (Test-Path -LiteralPath $intakeCandidate -PathType Leaf)) { continue }
        $intakeProbeArgs = @()
        if ([IO.Path]::GetFileNameWithoutExtension($intakeCandidate) -eq 'py') { $intakeProbeArgs += '-3' }
        & $intakeCandidate @intakeProbeArgs '-B' '-c' 'import sys; sys.exit(0 if sys.version_info >= (3, 9) else 1)' 2>$null
        if ($LASTEXITCODE -eq 0) {
            $intakePython = $intakeCandidate
            $intakePythonArgs = $intakeProbeArgs
            break
        }
    }
    if (-not $intakePython) {
        throw 'Python 3.9+ was not found. Pass -PythonPath or set GRAVIVORE_INTAKE_PYTHON to an existing Python executable.'
    }
    $intakeArguments = @('-B', (Join-Path $PSScriptRoot 'scan_intake.py'))
    if ($IntakeRoot) { $intakeArguments += @('--intake-root', $IntakeRoot) }
    if ($ManifestPath) { $intakeArguments += @('--manifest', $ManifestPath) }
    if ($Verify) { $intakeArguments += '--verify' }
    & $intakePython @intakePythonArgs @intakeArguments
    exit $LASTEXITCODE
} catch {
    [Console]::Error.WriteLine('INTAKE_TOOLING_ERROR: ' + $_.Exception.Message)
    exit 2
}
