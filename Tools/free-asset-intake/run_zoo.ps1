[CmdletBinding()]
param([ValidateSet('Compile','Inspect')][string]$Action='Inspect')
$ErrorActionPreference='Stop'
$intakeRepo=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$zooRoot=Join-Path $intakeRepo 'ExternalAssetIntake\FreeAssetIntakeV1\_work_v2\unity_zoo'
$zooLog=Join-Path $intakeRepo ('ExternalAssetIntake\FreeAssetIntakeV1\_work_v2\reports\Unity_'+$Action+'.log')
$zooUnity='C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe'
if(!(Test-Path -LiteralPath (Join-Path $zooRoot 'Assets\Editor\IntakeZoo.cs'))){throw 'Prepare the art-only scratch project first.'}
$zooArgs=@('-batchmode','-quit','-projectPath',('"'+$zooRoot+'"'),'-logFile',('"'+$zooLog+'"'))
if($Action -eq 'Compile'){$zooArgs+='-nographics'}
if($Action -eq 'Inspect'){$zooArgs+=@('-executeMethod','IntakeZoo.Build')}
$zooProcess=Start-Process -FilePath $zooUnity -ArgumentList $zooArgs -WorkingDirectory $zooRoot -WindowStyle Hidden -PassThru
$zooProcess.WaitForExit()
Write-Output ('UNITY_ZOO_'+$Action+'_EXIT='+$zooProcess.ExitCode)
exit $zooProcess.ExitCode
