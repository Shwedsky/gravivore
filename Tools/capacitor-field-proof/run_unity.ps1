[CmdletBinding()]
param([ValidateSet('Compile','Author','Validate','EditMode','PlayMode')][string]$Action,
      [string]$TestFilter='', [string]$Label='', [switch]$VideoCapture)
$ErrorActionPreference='Stop'
$proofRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$proofOutput=Join-Path $proofRoot 'docs/history/implementation-passes/capacitor-field-proof/verification'
New-Item -ItemType Directory -Force -Path $proofOutput | Out-Null
$proofLabel=if($Label){$Label}else{$Action}
$proofArgs=@('-batchmode','-projectPath',('"'+$proofRoot+'"'),'-logFile',('"'+(Join-Path $proofOutput ($proofLabel+'.log'))+'"'))
if($Action -in @('Compile','Author','Validate','EditMode')){$proofArgs+='-nographics'}
switch($Action){
 'Compile' {$proofArgs+='-quit'}
 'Author' {$proofArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.CapacitorFieldProofBuilder.Build')}
 'Validate' {$proofArgs+=@('-quit','-executeMethod','Gravivore.Editor.ProjectValidator.ValidateOrThrow')}
 'EditMode' {$proofArgs+=@('-runTests','-testPlatform','EditMode','-testResults',('"'+(Join-Path $proofOutput ($proofLabel+'.xml'))+'"'))}
 'PlayMode' {$proofArgs+=@('-runTests','-testPlatform','PlayMode','-testResults',('"'+(Join-Path $proofOutput ($proofLabel+'.xml'))+'"'))}
}
if($TestFilter){$proofArgs+=@('-testFilter',$TestFilter)}
if($VideoCapture){$env:GRAVIVORE_CAPACITOR_VIDEO='1'}
$proofProcess=Start-Process -FilePath 'C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe' -ArgumentList $proofArgs -WorkingDirectory $proofRoot -WindowStyle Hidden -PassThru
$proofProcess.WaitForExit()
Write-Output ('UNITY_'+$Action+'_EXIT='+$proofProcess.ExitCode)
exit $proofProcess.ExitCode
