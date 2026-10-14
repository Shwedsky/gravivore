[CmdletBinding()]
param([ValidateSet('Compile','Inspect','Survey','FixVisibility','LookDev','Validate','EditMode','PlayMode','Android')][string]$Action,
      [string]$TestFilter='', [string]$Label='', [string]$Stage='diagnosis', [int]$VersionCode=56)
$ErrorActionPreference='Stop'
$presentationRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$presentationOutput=Join-Path $presentationRoot 'docs/history/implementation-passes/energy-chamber-runtime-presentation-v1/verification'
New-Item -ItemType Directory -Force -Path $presentationOutput | Out-Null
$presentationLabel=if($Label){$Label}else{$Action}
$env:GRAVIVORE_ENERGY_PRESENTATION_STAGE=$Stage
$presentationArgs=@('-batchmode','-projectPath',('"'+$presentationRoot+'"'),'-logFile',('"'+(Join-Path $presentationOutput ($presentationLabel+'.log'))+'"'))
if($Action -in @('Compile','Inspect','Survey','FixVisibility','LookDev','Validate','EditMode')){$presentationArgs+='-nographics'}
switch($Action){
 'Compile' {$presentationArgs+='-quit'}
 'Inspect' {$presentationArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.EnergyChamberRuntimePresentation.Inspect')}
 'Survey' {$presentationArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.EnergyChamberRuntimePresentation.SurveySurfaces')}
 'FixVisibility' {$presentationArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.EnergyChamberRuntimePresentation.FixVisibility')}
 'LookDev' {$presentationArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.EnergyChamberRuntimePresentation.AuthorLookDev')}
 'Validate' {$presentationArgs+=@('-quit','-executeMethod','Gravivore.Editor.ProjectValidator.ValidateOrThrow')}
 'EditMode' {$presentationArgs+=@('-runTests','-testPlatform','EditMode','-testResults',('"'+(Join-Path $presentationOutput ($presentationLabel+'.xml'))+'"'))}
 'PlayMode' {$presentationArgs+=@('-runTests','-testPlatform','PlayMode','-testResults',('"'+(Join-Path $presentationOutput ($presentationLabel+'.xml'))+'"'))}
 'Android' {
    $presentationSdk=Join-Path (Split-Path -Parent (Split-Path -Parent $presentationRoot)) '.utmp/architecture-sdk55'
    if(-not (Test-Path -LiteralPath $presentationSdk -PathType Container)){throw 'The genuine installed SDK mirror is unavailable'}
    $presentationApk=Join-Path $presentationRoot ('Builds/Android/gravivore-dev-0.1.0+'+$VersionCode+'.apk')
    if(Test-Path -LiteralPath $presentationApk){throw ('Refusing to overwrite '+$presentationApk)}
    $env:GRAVIVORE_ARCHITECTURE_OFFLINE_SDK=$presentationSdk
    $presentationArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.ArchitectureAndroidBuild.BuildWithInstalledSdk','-GravivoreVersion','0.1.0','-VersionCode',([string]$VersionCode))
 }
}
if($TestFilter){$presentationArgs+=@('-testFilter',$TestFilter)}
$presentationProcess=Start-Process -FilePath 'C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe' -ArgumentList $presentationArgs -WorkingDirectory $presentationRoot -WindowStyle Hidden -PassThru
Write-Output ('UNITY_PID='+$presentationProcess.Id)
$presentationProcess.WaitForExit()
Write-Output ('UNITY_'+$Action+'_EXIT='+$presentationProcess.ExitCode)
exit $presentationProcess.ExitCode
