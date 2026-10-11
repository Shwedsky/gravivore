[CmdletBinding()]
param([ValidateSet('Compile','Author','Validate','EditMode','PlayMode')][string]$Action,
      [string]$TestFilter='', [string]$Label='')
$ErrorActionPreference='Stop'
$integrationRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$integrationOutput=Join-Path $integrationRoot 'docs/history/implementation-passes/chapter01-r2-actors-v2-integration/verification'
New-Item -ItemType Directory -Force -Path $integrationOutput | Out-Null
$integrationLabel=if($Label){$Label}else{$Action}
$integrationArgs=@('-batchmode','-projectPath',('"'+$integrationRoot+'"'),'-logFile',('"'+(Join-Path $integrationOutput ($integrationLabel+'.log'))+'"'))
if($Action -in @('Compile','Author','Validate','EditMode')){$integrationArgs+='-nographics'}
switch($Action){
 'Compile' {$integrationArgs+='-quit'}
 'Author' {$integrationArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.Chapter01ActorIntegrationBuilder.Build')}
 'Validate' {$integrationArgs+=@('-quit','-executeMethod','Gravivore.Editor.ProjectValidator.ValidateOrThrow')}
 'EditMode' {$integrationArgs+=@('-runTests','-testPlatform','EditMode','-testResults',('"'+(Join-Path $integrationOutput ($integrationLabel+'.xml'))+'"'))}
 'PlayMode' {$integrationArgs+=@('-runTests','-testPlatform','PlayMode','-testResults',('"'+(Join-Path $integrationOutput ($integrationLabel+'.xml'))+'"'))}
}
if($TestFilter){$integrationArgs+=@('-testFilter',$TestFilter)}
$integrationProcess=Start-Process -FilePath 'C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe' -ArgumentList $integrationArgs -WorkingDirectory $integrationRoot -WindowStyle Hidden -PassThru
$integrationProcess.WaitForExit()
Write-Output ('UNITY_'+$Action+'_EXIT='+$integrationProcess.ExitCode)
exit $integrationProcess.ExitCode
