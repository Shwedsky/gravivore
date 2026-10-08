[CmdletBinding()]
param([ValidateSet('Compile','Integrate','Validate','EditMode','PlayMode','Build')][string]$Action,[string]$TestFilter='')
$ErrorActionPreference='Stop'
$fidelityRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$fidelityOutput=Join-Path $fidelityRoot 'docs\concept-fidelity-v2\verification'
New-Item -ItemType Directory -Force -Path $fidelityOutput | Out-Null
$fidelityUnity='C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe'
$fidelityLabel=if($TestFilter){$Action+'Filtered'}else{$Action}
$fidelityArgs=@('-batchmode','-projectPath',('"'+$fidelityRoot+'"'),'-logFile',('"'+(Join-Path $fidelityOutput ($fidelityLabel+'.log'))+'"'))
if($Action -in @('Compile','Validate','EditMode')) {$fidelityArgs+=@('-nographics')}
switch($Action){
 'Compile' {$fidelityArgs+=@('-quit')}
 'Integrate' {$fidelityArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.ConceptFidelityBuilder.Build')}
 'Validate' {$fidelityArgs+=@('-quit','-executeMethod','Gravivore.Editor.ProjectValidator.ValidateOrThrow')}
 'EditMode' {$fidelityArgs+=@('-runTests','-testPlatform','EditMode','-testResults',('"'+(Join-Path $fidelityOutput ($fidelityLabel+'.xml'))+'"'))}
 'PlayMode' {$fidelityArgs+=@('-runTests','-testPlatform','PlayMode','-testResults',('"'+(Join-Path $fidelityOutput ($fidelityLabel+'.xml'))+'"'))}
 'Build' {$fidelityArgs+=@('-quit','-buildTarget','Android','-executeMethod','Gravivore.Editor.Build.AndroidBuild.BuildDev','-GravivoreVersion','0.1.0','-VersionCode','41')}
}
if($TestFilter){$fidelityArgs+=@('-testFilter',$TestFilter)}
$fidelityProcess=Start-Process -FilePath $fidelityUnity -ArgumentList $fidelityArgs -WorkingDirectory $fidelityRoot -WindowStyle Hidden -PassThru
$fidelityProcess.WaitForExit()
Write-Output ('UNITY_'+$Action+'_EXIT='+$fidelityProcess.ExitCode)
exit $fidelityProcess.ExitCode
