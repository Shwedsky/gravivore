[CmdletBinding()]
param([ValidateSet('Compile','Author','Validate','EditMode','PlayMode')][string]$Action,
      [string]$TestFilter='',[string]$Label='',[switch]$VideoCapture,[switch]$CompleteChapter)
$ErrorActionPreference='Stop'
$sectorProject=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$sectorOutput=Join-Path $sectorProject $(if($CompleteChapter){'docs/history/implementation-passes/chapter01-complete-concept/verification'}else{'docs/history/implementation-passes/connected-concept-sectors/verification'})
New-Item -ItemType Directory -Force -Path $sectorOutput | Out-Null
$sectorLabel=if($Label){$Label}else{$Action}
$sectorArgs=@('-batchmode','-projectPath',('"'+$sectorProject+'"'),'-logFile',('"'+(Join-Path $sectorOutput ($sectorLabel+'.log'))+'"'))
if($Action -in @('Compile','Author','Validate','EditMode')){$sectorArgs+='-nographics'}
switch($Action){
'Compile'{$sectorArgs+='-quit'}
'Author'{$sectorArgs+=@('-quit','-executeMethod',$(if($CompleteChapter){'Gravivore.Editor.VisualIntegration.Chapter01CompleteConceptBuilder.Build'}else{'Gravivore.Editor.VisualIntegration.ConnectedConceptSectorsBuilder.Build'}))}
'Validate'{$sectorArgs+=@('-quit','-executeMethod','Gravivore.Editor.ProjectValidator.ValidateOrThrow')}
'EditMode'{$sectorArgs+=@('-runTests','-testPlatform','EditMode','-testResults',('"'+(Join-Path $sectorOutput ($sectorLabel+'.xml'))+'"'))}
'PlayMode'{$sectorArgs+=@('-runTests','-testPlatform','PlayMode','-testResults',('"'+(Join-Path $sectorOutput ($sectorLabel+'.xml'))+'"'))}
}
if($TestFilter){$sectorArgs+=@('-testFilter',$TestFilter)}
if($VideoCapture){$env:GRAVIVORE_CONNECTED_VIDEO='1';$env:GRAVIVORE_CAPACITOR_VIDEO='1';$env:GRAVIVORE_CHAPTER_VIDEO='1'}
$sectorProcess=Start-Process -FilePath 'C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe' -ArgumentList $sectorArgs -WorkingDirectory $sectorProject -WindowStyle Hidden -PassThru
$sectorProcess.WaitForExit()
Write-Output ('UNITY_'+$Action+'_EXIT='+$sectorProcess.ExitCode)
exit $sectorProcess.ExitCode
