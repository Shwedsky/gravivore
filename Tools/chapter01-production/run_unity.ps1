[CmdletBinding()]
param([ValidateSet('Compile','Integrate','Validate','EditMode','PlayMode','Build')][string]$Action,[string]$TestFilter='')
$ErrorActionPreference='Stop'
$chapterRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$chapterOutput=Join-Path $chapterRoot 'docs\chapter01-production\verification'
New-Item -ItemType Directory -Force -Path $chapterOutput | Out-Null
$chapterUnity='C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe'
$chapterLabel=if($TestFilter){$Action+'Filtered'}else{$Action}
$chapterArgs=@('-batchmode','-projectPath',('"'+$chapterRoot+'"'),'-logFile',('"'+(Join-Path $chapterOutput ($chapterLabel+'.log'))+'"'))
switch($Action){
 'Compile' {$chapterArgs+=@('-quit')}
 'Integrate' {$chapterArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.Chapter01ProductionBuilder.Build')}
 'Validate' {$chapterArgs+=@('-quit','-executeMethod','Gravivore.Editor.ProjectValidator.ValidateOrThrow')}
 'EditMode' {$chapterArgs+=@('-runTests','-testPlatform','EditMode','-testResults',('"'+(Join-Path $chapterOutput ($chapterLabel+'.xml'))+'"'))}
 'PlayMode' {$chapterArgs+=@('-runTests','-testPlatform','PlayMode','-testResults',('"'+(Join-Path $chapterOutput ($chapterLabel+'.xml'))+'"'))}
 'Build' {$chapterArgs+=@('-quit','-buildTarget','Android','-executeMethod','Gravivore.Editor.Build.AndroidBuild.BuildDev','-GravivoreVersion','0.1.0','-VersionCode','39')}
}
if($TestFilter){$chapterArgs+=@('-testFilter',$TestFilter)}
$chapterProcess=Start-Process -FilePath $chapterUnity -ArgumentList $chapterArgs -WorkingDirectory $chapterRoot -WindowStyle Hidden -PassThru
$chapterProcess.WaitForExit()
Write-Output ('UNITY_'+$Action+'_EXIT='+$chapterProcess.ExitCode)
exit $chapterProcess.ExitCode
