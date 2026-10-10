[CmdletBinding()]
param([ValidateSet('Compile','Author','Audit','Validate','EditMode','PlayMode')][string]$Action,
      [string]$TestFilter='', [string]$Label='', [switch]$StructuralCapture)
$ErrorActionPreference='Stop'
$r2Root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$r2Output=Join-Path $r2Root 'docs/history/implementation-passes/chapter01-blueprint-world-r2/verification'
New-Item -ItemType Directory -Force -Path $r2Output | Out-Null
$r2Label=if($Label){$Label}else{$Action}
$r2Args=@('-batchmode','-projectPath',('"'+$r2Root+'"'),'-logFile',('"'+(Join-Path $r2Output ($r2Label+'.log'))+'"'))
if($Action -in @('Compile','Audit','Validate','EditMode')){$r2Args+='-nographics'}
switch($Action){
 'Compile' {$r2Args+='-quit'}
 'Author' {$r2Args+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.Chapter01BlueprintWorldBuilder.BuildR2')}
 'Audit' {$r2Args+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.Chapter01BlueprintWorldBuilder.Audit')}
 'Validate' {$r2Args+=@('-quit','-executeMethod','Gravivore.Editor.ProjectValidator.ValidateOrThrow')}
 'EditMode' {$r2Args+=@('-runTests','-testPlatform','EditMode','-testResults',('"'+(Join-Path $r2Output ($r2Label+'.xml'))+'"'))}
 'PlayMode' {$r2Args+=@('-runTests','-testPlatform','PlayMode','-testResults',('"'+(Join-Path $r2Output ($r2Label+'.xml'))+'"'))}
}
if($TestFilter){$r2Args+=@('-testFilter',$TestFilter)}
if($StructuralCapture){$env:GRAVIVORE_VISUAL_INTEGRATION_QA=Join-Path $r2Output '../internal/integration-anchors'}
$r2Process=Start-Process -FilePath 'C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe' -ArgumentList $r2Args -WorkingDirectory $r2Root -WindowStyle Hidden -PassThru
$r2Process.WaitForExit()
Write-Output ('UNITY_'+$Action+'_EXIT='+$r2Process.ExitCode)
exit $r2Process.ExitCode
