param(
 [string]$Method,
 [ValidateSet('', 'EditMode', 'PlayMode')][string]$TestPlatform = '',
 [string]$TestFilter = '',
 [string]$Name = 'Import',
 [switch]$NoGraphics
)
$ErrorActionPreference = 'Stop'
$taskRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$unityExe = 'C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe'
if (!(Test-Path -LiteralPath $unityExe)) { throw 'Unity 6.3 LTS executable is missing.' }
$output = Join-Path $taskRoot 'Builds\ActorProductionV2'
New-Item -ItemType Directory -Force -Path $output | Out-Null
$log = Join-Path $output ($Name + '.log')
$argv = @('-batchmode', '-projectPath', ('"' + $taskRoot + '"'), '-logFile', ('"' + $log + '"'))
if ($NoGraphics) { $argv += '-nographics' }
if ($Method) { $argv += @('-executeMethod', $Method, '-quit') }
elseif ($TestPlatform) {
 $argv += @('-runTests', '-testPlatform', $TestPlatform, '-testResults', ('"' + (Join-Path $output ($Name + '.xml')) + '"'))
 if ($TestFilter) { $argv += @('-testFilter', $TestFilter) }
} else { $argv += '-quit' }
$process = Start-Process -FilePath $unityExe -ArgumentList $argv -WindowStyle Hidden -PassThru
$process.WaitForExit()
Write-Output "Unity $Name exited $($process.ExitCode). Log: $log"
exit $process.ExitCode
