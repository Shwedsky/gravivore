[CmdletBinding()]
param([ValidateSet('Compile','Validate','EditMode','PlayMode','Build')][string]$Action,[string]$TestFilter='')
$ErrorActionPreference='Stop'
$uxRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$uxOutput=Join-Path $uxRoot 'docs\chapter01-gameplay-ux\verification'
New-Item -ItemType Directory -Force -Path $uxOutput | Out-Null
$uxUnity='C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe'
$uxLabel=if($TestFilter){$Action+'Filtered'}else{$Action}
$uxArgs=@('-batchmode','-projectPath',('"'+$uxRoot+'"'),'-logFile',('"'+(Join-Path $uxOutput ($uxLabel+'.log'))+'"'))
if($Action -in @('Compile','Validate','EditMode')) { $uxArgs+=@('-nographics') }
switch($Action){
 'Compile' {$uxArgs+=@('-quit')}
 'Validate' {$uxArgs+=@('-quit','-executeMethod','Gravivore.Editor.ProjectValidator.ValidateOrThrow')}
 'EditMode' {$uxArgs+=@('-runTests','-testPlatform','EditMode','-testResults',('"'+(Join-Path $uxOutput ($uxLabel+'.xml'))+'"'))}
 'PlayMode' {$uxArgs+=@('-runTests','-testPlatform','PlayMode','-testResults',('"'+(Join-Path $uxOutput ($uxLabel+'.xml'))+'"'))}
 'Build' {$uxArgs+=@('-quit','-buildTarget','Android','-executeMethod','Gravivore.Editor.Build.AndroidBuild.BuildDev','-GravivoreVersion','0.1.0','-VersionCode','40')}
}
if($TestFilter){$uxArgs+=@('-testFilter',$TestFilter)}
$uxProcess=Start-Process -FilePath $uxUnity -ArgumentList $uxArgs -WorkingDirectory $uxRoot -WindowStyle Hidden -PassThru
$uxProcess.WaitForExit()
Write-Output ('UNITY_'+$Action+'_EXIT='+$uxProcess.ExitCode)
exit $uxProcess.ExitCode
