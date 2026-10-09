[CmdletBinding()]
param([ValidateSet('Compile','Author','Validate','EditMode','PlayMode')][string]$Action,[string]$TestFilter='',[string]$Label='')
$ErrorActionPreference='Stop'
$correctiveRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$correctiveOutput=Join-Path $correctiveRoot 'docs/concept-corrective-v45/verification'
New-Item -ItemType Directory -Force -Path $correctiveOutput | Out-Null
$correctiveLabel=if($Label){$Label}else{$Action}
$correctiveArgs=@('-batchmode','-projectPath',('"'+$correctiveRoot+'"'),'-logFile',('"'+(Join-Path $correctiveOutput ($correctiveLabel+'.log'))+'"'))
if($Action -in @('Compile','Validate','EditMode')){$correctiveArgs+='-nographics'}
switch($Action){
 'Compile' {$correctiveArgs+='-quit'}
 'Author' {$correctiveArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.ConceptCorrectiveV45Builder.Build')}
 'Validate' {$correctiveArgs+=@('-quit','-executeMethod','Gravivore.Editor.ProjectValidator.ValidateOrThrow')}
 'EditMode' {$correctiveArgs+=@('-runTests','-testPlatform','EditMode','-testResults',('"'+(Join-Path $correctiveOutput ($correctiveLabel+'.xml'))+'"'))}
 'PlayMode' {$correctiveArgs+=@('-runTests','-testPlatform','PlayMode','-testResults',('"'+(Join-Path $correctiveOutput ($correctiveLabel+'.xml'))+'"'))}
}
if($TestFilter){$correctiveArgs+=@('-testFilter',$TestFilter)}
$correctiveProcess=Start-Process -FilePath 'C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe' -ArgumentList $correctiveArgs -WorkingDirectory $correctiveRoot -WindowStyle Hidden -PassThru
$correctiveProcess.WaitForExit()
Write-Output ('UNITY_'+$Action+'_EXIT='+$correctiveProcess.ExitCode)
exit $correctiveProcess.ExitCode
