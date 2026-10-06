[CmdletBinding()]
param(
    [string]$UnityPath = 'C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe',
    [ValidateSet('All', 'PlayMode')][string]$Phase = 'All'
)

$ErrorActionPreference = 'Stop'
$taskProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
$taskVerificationDir = Join-Path $taskProjectRoot 'Builds\VisualV2Verification'
New-Item -ItemType Directory -Path $taskVerificationDir -Force | Out-Null
$taskResults = @()

$taskPlatforms = if ($Phase -eq 'PlayMode') { @('PlayMode') } else { @('EditMode', 'PlayMode') }
foreach ($taskPlatform in $taskPlatforms) {
    $taskXmlPath = Join-Path $taskVerificationDir "$taskPlatform.xml"
    $taskLogName = if ($taskPlatform -eq 'PlayMode') { 'PlayMode-graphics.log' } else { "$taskPlatform.log" }
    $taskLogPath = Join-Path $taskVerificationDir $taskLogName
    $taskGraphicsArguments = if ($taskPlatform -eq 'PlayMode') { @('-force-d3d11', '-force-gfx-direct') } else { @('-nographics') }
    $taskArguments = @('-batchmode') + $taskGraphicsArguments + @('-projectPath', "`"$taskProjectRoot`"",
        '-runTests', '-testPlatform', $taskPlatform, '-testResults', "`"$taskXmlPath`"", '-logFile', "`"$taskLogPath`"")
    Write-Host "Running existing-runtime $taskPlatform tests; no blockout integration."
    $taskProcess = Start-Process -FilePath $UnityPath -ArgumentList $taskArguments -WindowStyle Hidden -PassThru
    $taskProcess.WaitForExit()
    $taskProcess.Refresh()
    $taskResult = [ordered]@{ platform = $taskPlatform; exitCode = $taskProcess.ExitCode; log = $taskLogPath; results = $taskXmlPath }
    if (Test-Path -LiteralPath $taskXmlPath) {
        [xml]$taskXml = Get-Content -LiteralPath $taskXmlPath -Raw
        $taskResult['total'] = [int]$taskXml.'test-run'.total
        $taskResult['passed'] = [int]$taskXml.'test-run'.passed
        $taskResult['failed'] = [int]$taskXml.'test-run'.failed
        $taskResult['skipped'] = [int]$taskXml.'test-run'.skipped
        $taskResult['result'] = [string]$taskXml.'test-run'.result
    }
    else { $taskResult['result'] = 'No test result file produced' }
    $taskResults += $taskResult
    Write-Output ($taskResult | ConvertTo-Json -Compress)
}

if ($Phase -eq 'All') {
$taskValidationLog = Join-Path $taskVerificationDir 'ProjectValidator.log'
$taskArguments = @('-batchmode', '-nographics', '-quit', '-projectPath', "`"$taskProjectRoot`"",
    '-executeMethod', 'Gravivore.Editor.ProjectValidator.ValidateOrThrow', '-logFile', "`"$taskValidationLog`"")
Write-Host 'Running project validator on the existing runtime.'
$taskProcess = Start-Process -FilePath $UnityPath -ArgumentList $taskArguments -WindowStyle Hidden -PassThru
$taskProcess.WaitForExit()
$taskProcess.Refresh()
$taskResults += [ordered]@{ platform = 'ProjectValidator'; exitCode = $taskProcess.ExitCode; log = $taskValidationLog }
}
$taskSummaryName = if ($Phase -eq 'PlayMode') { 'PlayModeRetry.json' } else { 'UnityChecks.json' }
$taskResults | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $taskVerificationDir $taskSummaryName) -Encoding utf8
Write-Output ($taskResults[-1] | ConvertTo-Json -Compress)
if (@($taskResults | Where-Object { $_.exitCode -ne 0 -or $_.failed -gt 0 }).Count -gt 0) { exit 1 }
