[CmdletBinding()]
param([ValidateSet('Compile','Author','Capture','Validate','ProjectValidate','EditMode','PlayMode','Android')][string]$Action)
$ErrorActionPreference='Stop'
$proofRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$proofOutput=Join-Path $proofRoot 'docs/history/implementation-passes/energy-chamber-hybrid-proof-v1/verification'
New-Item -ItemType Directory -Force -Path $proofOutput | Out-Null
$proofArgs=@('-batchmode','-projectPath',('"'+$proofRoot+'"'),'-logFile',('"'+(Join-Path $proofOutput ($Action+'.log'))+'"'))
if($Action -in @('Compile','Validate','ProjectValidate','EditMode')){$proofArgs+='-nographics'}
switch($Action){
 'Compile' {$proofArgs+='-quit'}
 'Author' {$proofArgs+=@('-quit','-executeMethod','Gravivore.ArtProof.EnergyChamber.EnergyChamberProofBuilder.Build')}
 'Capture' {$proofArgs+=@('-quit','-executeMethod','Gravivore.ArtProof.EnergyChamber.EnergyChamberProofBuilder.Capture')}
 'Validate' {$proofArgs+=@('-quit','-executeMethod','Gravivore.ArtProof.EnergyChamber.EnergyChamberProofBuilder.Validate')}
 'ProjectValidate' {$proofArgs+=@('-quit','-executeMethod','Gravivore.Editor.ProjectValidator.ValidateOrThrow')}
 'EditMode' {$proofArgs+=@('-runTests','-testPlatform','EditMode','-testResults',('"'+(Join-Path $proofOutput 'EditMode.xml')+'"'))}
 'PlayMode' {$proofArgs+=@('-runTests','-testPlatform','PlayMode','-testFilter','EnergyChamberProof','-testResults',('"'+(Join-Path $proofOutput 'PlayMode.xml')+'"'))}
 'Android' {$proofArgs+=@('-quit','-buildTarget','Android','-executeMethod','Gravivore.ArtProof.EnergyChamber.EnergyChamberProofBuilder.BuildAndroid')}
}
$proofProcess=Start-Process -FilePath 'C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe' -ArgumentList $proofArgs -WorkingDirectory $proofRoot -WindowStyle Hidden -PassThru
$proofProcess.WaitForExit()
Write-Output ('UNITY_'+$Action+'_EXIT='+$proofProcess.ExitCode)
exit $proofProcess.ExitCode
