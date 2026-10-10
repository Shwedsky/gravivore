[CmdletBinding()]
param([ValidateSet('Compile','Author','Audit','Validate','EditMode','PlayMode')][string]$Action,
      [string]$TestFilter='', [string]$Label='', [switch]$StructuralCapture)
$ErrorActionPreference='Stop'
$blueprintRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$blueprintOutput=Join-Path $blueprintRoot 'docs/history/implementation-passes/chapter01-blueprint-world-r1/verification'
New-Item -ItemType Directory -Force -Path $blueprintOutput | Out-Null
$blueprintLabel=if($Label){$Label}else{$Action}
$blueprintArgs=@('-batchmode','-projectPath',('"'+$blueprintRoot+'"'),'-logFile',('"'+(Join-Path $blueprintOutput ($blueprintLabel+'.log'))+'"'))
if($Action -in @('Compile','Audit','Validate','EditMode')){$blueprintArgs+='-nographics'}
switch($Action){
 'Compile' {$blueprintArgs+='-quit'}
 'Author' {$blueprintArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.Chapter01BlueprintWorldBuilder.Build')}
 'Audit' {$blueprintArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.Chapter01BlueprintWorldBuilder.Audit')}
 'Validate' {$blueprintArgs+=@('-quit','-executeMethod','Gravivore.Editor.ProjectValidator.ValidateOrThrow')}
 'EditMode' {$blueprintArgs+=@('-runTests','-testPlatform','EditMode','-testResults',('"'+(Join-Path $blueprintOutput ($blueprintLabel+'.xml'))+'"'))}
 'PlayMode' {$blueprintArgs+=@('-runTests','-testPlatform','PlayMode','-testResults',('"'+(Join-Path $blueprintOutput ($blueprintLabel+'.xml'))+'"'))}
}
if($TestFilter){$blueprintArgs+=@('-testFilter',$TestFilter)}
if($StructuralCapture){$env:GRAVIVORE_VISUAL_INTEGRATION_QA=Join-Path $blueprintOutput '../internal/integration-anchors'}
$blueprintProcess=Start-Process -FilePath 'C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe' -ArgumentList $blueprintArgs -WorkingDirectory $blueprintRoot -WindowStyle Hidden -PassThru
$blueprintProcess.WaitForExit()
Write-Output ('UNITY_'+$Action+'_EXIT='+$blueprintProcess.ExitCode)
exit $blueprintProcess.ExitCode
