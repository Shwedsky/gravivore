[CmdletBinding()]
param([ValidateSet('Compile','Integrate','Validate','EditMode','PlayMode','Build')][string]$Action,[string]$TestFilter='')
$ErrorActionPreference='Stop'
$v3Root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$v3Output=Join-Path $v3Root 'docs\chapter01-v3\verification'
New-Item -ItemType Directory -Force -Path $v3Output | Out-Null
$v3Unity='C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe'
$v3Label=if($TestFilter){$Action+'Filtered'}else{$Action}
$v3Args=@('-batchmode','-projectPath',('"'+$v3Root+'"'),'-logFile',('"'+(Join-Path $v3Output ($v3Label+'.log'))+'"'))
if($Action -in @('Compile','Validate','EditMode')) {$v3Args+=@('-nographics')}
switch($Action){
 'Compile' {$v3Args+=@('-quit')}
 'Integrate' {$v3Args+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.Chapter01V3Builder.Build')}
 'Validate' {$v3Args+=@('-quit','-executeMethod','Gravivore.Editor.ProjectValidator.ValidateOrThrow')}
 'EditMode' {$v3Args+=@('-runTests','-testPlatform','EditMode','-testResults',('"'+(Join-Path $v3Output ($v3Label+'.xml'))+'"'))}
 'PlayMode' {$v3Args+=@('-runTests','-testPlatform','PlayMode','-testResults',('"'+(Join-Path $v3Output ($v3Label+'.xml'))+'"'))}
 'Build' {$v3Args+=@('-quit','-buildTarget','Android','-executeMethod','Gravivore.Editor.Build.AndroidBuild.BuildDev','-GravivoreVersion','0.1.0','-VersionCode','42')}
}
if($TestFilter){$v3Args+=@('-testFilter',$TestFilter)}
$v3Process=Start-Process -FilePath $v3Unity -ArgumentList $v3Args -WorkingDirectory $v3Root -WindowStyle Hidden -PassThru
$v3Process.WaitForExit()
Write-Output ('UNITY_'+$Action+'_EXIT='+$v3Process.ExitCode)
exit $v3Process.ExitCode
