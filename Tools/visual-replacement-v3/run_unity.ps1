[CmdletBinding()]
param([ValidateSet('Compile','Repair','Environment','Actors','Equipment','UI','Validate','EditMode','PlayMode','Build')][string]$Action,[string]$TestFilter='',[string]$Label='')
$ErrorActionPreference='Stop'
$replacementRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$replacementOutput=Join-Path $replacementRoot 'docs\visual-replacement-v3\verification'
New-Item -ItemType Directory -Force -Path $replacementOutput | Out-Null
$replacementUnity='C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe'
$replacementLabel=if($Label){$Label}elseif($TestFilter){$Action+'Filtered'}else{$Action}
$replacementArgs=@('-batchmode','-projectPath',('"'+$replacementRoot+'"'),'-logFile',('"'+(Join-Path $replacementOutput ($replacementLabel+'.log'))+'"'))
if($Action -in @('Compile','Validate','EditMode')){$replacementArgs+='-nographics'}
switch($Action){
 'Compile' {$replacementArgs+='-quit'}
 'Repair' {$replacementArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.VisualReplacementV3Builder.BuildRepair')}
 'Environment' {$replacementArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.VisualReplacementV3Builder.BuildEnvironment')}
 'Actors' {$replacementArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.VisualReplacementV3Builder.BuildActors')}
 'Equipment' {$replacementArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.VisualReplacementV3Builder.BuildEquipment')}
 'UI' {$replacementArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.VisualReplacementV3Builder.BuildUI')}
 'Validate' {$replacementArgs+=@('-quit','-executeMethod','Gravivore.Editor.ProjectValidator.ValidateOrThrow')}
 'EditMode' {$replacementArgs+=@('-runTests','-testPlatform','EditMode','-testResults',('"'+(Join-Path $replacementOutput ($replacementLabel+'.xml'))+'"'))}
 'PlayMode' {$replacementArgs+=@('-runTests','-testPlatform','PlayMode','-testResults',('"'+(Join-Path $replacementOutput ($replacementLabel+'.xml'))+'"'))}
 'Build' {$replacementArgs+=@('-quit','-buildTarget','Android','-executeMethod','Gravivore.Editor.Build.AndroidBuild.BuildDev','-GravivoreVersion','0.1.0','-VersionCode','44')}
}
if($TestFilter){$replacementArgs+=@('-testFilter',$TestFilter)}
$replacementProcess=Start-Process -FilePath $replacementUnity -ArgumentList $replacementArgs -WorkingDirectory $replacementRoot -WindowStyle Hidden -PassThru
$replacementProcess.WaitForExit()
Write-Output ('UNITY_'+$Action+'_EXIT='+$replacementProcess.ExitCode)
exit $replacementProcess.ExitCode
