[CmdletBinding()]
param([ValidateSet('Integrate','Validate','EditMode','PlayMode','Build')][string]$Action,[string]$TestFilter='')
$ErrorActionPreference='Stop'
$sliceRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$sliceOutput=Join-Path $sliceRoot 'docs\first-visual-slice\verification'
New-Item -ItemType Directory -Force -Path $sliceOutput | Out-Null
$sliceUnity='C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe'
$sliceLabel=if($TestFilter){$Action+'Filtered'}else{$Action}
$sliceArgs=@('-batchmode','-projectPath',('"'+$sliceRoot+'"'),'-logFile',('"'+(Join-Path $sliceOutput ($sliceLabel+'.log'))+'"'))
switch($Action){
 'Integrate' {$sliceArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.FirstVisualSliceBuilder.Build')}
 'Validate' {$sliceArgs+=@('-quit','-executeMethod','Gravivore.Editor.ProjectValidator.ValidateOrThrow')}
 'EditMode' {$sliceArgs+=@('-runTests','-testPlatform','EditMode','-testResults',('"'+(Join-Path $sliceOutput 'EditMode.xml')+'"'))}
 'PlayMode' {$sliceArgs+=@('-runTests','-testPlatform','PlayMode','-testResults',('"'+(Join-Path $sliceOutput ($sliceLabel+'.xml'))+'"'))}
 'Build' {$sliceArgs+=@('-quit','-buildTarget','Android','-executeMethod','Gravivore.Editor.Build.AndroidBuild.BuildDev','-GravivoreVersion','0.1.0','-VersionCode','36')}
}
if($TestFilter){$sliceArgs+=@('-testFilter',$TestFilter)}
$sliceProcess=Start-Process -FilePath $sliceUnity -ArgumentList $sliceArgs -WorkingDirectory $sliceRoot -WindowStyle Hidden -PassThru
$sliceProcess.WaitForExit()
Write-Output ('UNITY_'+$Action+'_EXIT='+$sliceProcess.ExitCode)
exit $sliceProcess.ExitCode
