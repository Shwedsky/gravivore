[CmdletBinding()]
param([ValidateSet('Compile','Author','Lod','Inspect','Capture','Validate','EditMode','PlayMode','Android')][string]$Action,
      [string]$TestFilter='', [string]$Label='')
$ErrorActionPreference='Stop'
$architectureRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$architectureOutput=Join-Path $architectureRoot 'docs/history/implementation-passes/chapter01-architecture-production-v1/verification'
New-Item -ItemType Directory -Force -Path $architectureOutput | Out-Null
$architectureLabel=if($Label){$Label}else{$Action}
$architectureArgs=@('-batchmode','-projectPath',('"'+$architectureRoot+'"'),'-logFile',('"'+(Join-Path $architectureOutput ($architectureLabel+'.log'))+'"'))
if($Action -in @('Compile','Author','Lod','Inspect','Validate','EditMode')){$architectureArgs+='-nographics'}
switch($Action){
 'Compile' {$architectureArgs+='-quit'}
 'Author' {$architectureArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.Chapter01ArchitectureProduction.Build')}
 'Lod' {$architectureArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.Chapter01ArchitectureProduction.TuneLod')}
 'Inspect' {$architectureArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.Chapter01ArchitectureProduction.Inspect')}
 'Capture' {$architectureArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.Chapter01ArchitectureProduction.CaptureAudit')}
 'Validate' {$architectureArgs+=@('-quit','-executeMethod','Gravivore.Editor.ProjectValidator.ValidateOrThrow')}
 'EditMode' {$architectureArgs+=@('-runTests','-testPlatform','EditMode','-testResults',('"'+(Join-Path $architectureOutput ($architectureLabel+'.xml'))+'"'))}
 'PlayMode' {$architectureArgs+=@('-runTests','-testPlatform','PlayMode','-testResults',('"'+(Join-Path $architectureOutput ($architectureLabel+'.xml'))+'"'))}
 'Android' {$architectureArgs+=@('-quit','-executeMethod','Gravivore.Editor.AndroidBuild.BuildDev','-GravivoreVersion','0.1.0','-VersionCode','55')}
}
if($TestFilter){$architectureArgs+=@('-testFilter',$TestFilter)}
$architectureProcess=Start-Process -FilePath 'C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe' -ArgumentList $architectureArgs -WorkingDirectory $architectureRoot -WindowStyle Hidden -PassThru
$architectureProcess.WaitForExit()
Write-Output ('UNITY_'+$Action+'_EXIT='+$architectureProcess.ExitCode)
exit $architectureProcess.ExitCode
