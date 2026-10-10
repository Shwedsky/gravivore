[CmdletBinding()]
param([ValidateSet('Compile','Baseline','Actors','Author','Audit','Validate','EditMode','PlayMode')][string]$Action,[string]$TestFilter='',[string]$Label='',[string]$CapturePhase='')
$ErrorActionPreference='Stop'
$convergenceRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$convergenceOutput=Join-Path $convergenceRoot 'docs/history/implementation-passes/chapter01-visual-replacement-v3/v47/verification'
New-Item -ItemType Directory -Force -Path $convergenceOutput | Out-Null
if($CapturePhase){$env:GRAVIVORE_V47_CAPTURE_PHASE=$CapturePhase}
$convergenceLabel=if($Label){$Label}else{$Action}
$convergenceArgs=@('-batchmode','-projectPath',('"'+$convergenceRoot+'"'),'-logFile',('"'+(Join-Path $convergenceOutput ($convergenceLabel+'.log'))+'"'))
if($Action -in @('Compile','Audit','Validate','EditMode','Baseline')){$convergenceArgs+='-nographics'}
switch($Action){
 'Compile' {$convergenceArgs+='-quit'}
 'Baseline' {$convergenceArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.ConceptConvergenceV47Builder.Baseline')}
 'Actors' {$convergenceArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.ConceptConvergenceV47Builder.BuildActors')}
 'Author' {$convergenceArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.ConceptConvergenceV47Builder.Build')}
 'Audit' {$convergenceArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.ConceptConvergenceV47Builder.Audit')}
 'Validate' {$convergenceArgs+=@('-quit','-executeMethod','Gravivore.Editor.ProjectValidator.ValidateOrThrow')}
 'EditMode' {$convergenceArgs+=@('-runTests','-testPlatform','EditMode','-testResults',('"'+(Join-Path $convergenceOutput ($convergenceLabel+'.xml'))+'"'))}
 'PlayMode' {$convergenceArgs+=@('-runTests','-testPlatform','PlayMode','-testResults',('"'+(Join-Path $convergenceOutput ($convergenceLabel+'.xml'))+'"'))}
}
if($TestFilter){$convergenceArgs+=@('-testFilter',$TestFilter)}
$convergenceProcess=Start-Process -FilePath 'C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe' -ArgumentList $convergenceArgs -WorkingDirectory $convergenceRoot -WindowStyle Hidden -PassThru
$convergenceProcess.WaitForExit()
Write-Output ('UNITY_'+$Action+'_EXIT='+$convergenceProcess.ExitCode)
exit $convergenceProcess.ExitCode
