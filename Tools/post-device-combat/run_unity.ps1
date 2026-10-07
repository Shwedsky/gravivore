[CmdletBinding()]
param([ValidateSet('Integrate','Validate','EditMode','PlayMode')][string]$Action,[string]$TestFilter='')
$ErrorActionPreference='Stop'
$taskRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$taskOutput=Join-Path $taskRoot 'docs\post-device-combat-readability\verification'
New-Item -ItemType Directory -Force -Path $taskOutput | Out-Null
$taskUnity='C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe'
$taskLabel=if($TestFilter){$Action+'Filtered'}else{$Action}
$taskArgs=@('-batchmode','-projectPath',('"'+$taskRoot+'"'),'-logFile',('"'+(Join-Path $taskOutput ($taskLabel+'.log'))+'"'))
switch($Action){
 'Integrate' {$taskArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.PostDeviceCombatBuilder.Apply')}
 'Validate' {$taskArgs+=@('-quit','-executeMethod','Gravivore.Editor.ProjectValidator.ValidateOrThrow')}
 'EditMode' {$taskArgs+=@('-runTests','-testPlatform','EditMode','-testResults',('"'+(Join-Path $taskOutput ($taskLabel+'.xml'))+'"'))}
 'PlayMode' {$taskArgs+=@('-runTests','-testPlatform','PlayMode','-testResults',('"'+(Join-Path $taskOutput ($taskLabel+'.xml'))+'"'))}
}
if($TestFilter){$taskArgs+=@('-testFilter',$TestFilter)}
$taskProcess=Start-Process -FilePath $taskUnity -ArgumentList $taskArgs -WorkingDirectory $taskRoot -WindowStyle Hidden -PassThru
$taskProcess.WaitForExit()
Write-Output ('UNITY_'+$Action+'_EXIT='+$taskProcess.ExitCode)
exit $taskProcess.ExitCode
