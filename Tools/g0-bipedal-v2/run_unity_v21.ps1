[CmdletBinding()]
param([Parameter(Mandatory=$true)][ValidateSet('Capture','EditMode','PlayMode','Validate','Build')][string]$Action)
$ErrorActionPreference='Stop'
$reviewRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$reviewOutput=Join-Path $reviewRoot 'docs\visual-production-v2\g0-bipedal-v2\verification-v21'
New-Item -ItemType Directory -Force -Path $reviewOutput | Out-Null
$reviewUnityPath='C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe'
$reviewArgs=@('-batchmode','-projectPath',('"'+$reviewRoot+'"'),'-logFile',('"'+(Join-Path $reviewOutput ($Action+'.log'))+'"'))
switch($Action) {
    'Capture' { $reviewArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.G0V21ScaleReview.BuildAndCapture') }
    'Validate' { $reviewArgs+=@('-quit','-executeMethod','Gravivore.Editor.ProjectValidator.ValidateOrThrow') }
    'EditMode' { $reviewArgs+=@('-runTests','-testPlatform','EditMode','-testResults',('"'+(Join-Path $reviewOutput 'EditMode.xml')+'"')) }
    'PlayMode' { $reviewArgs+=@('-runTests','-testPlatform','PlayMode','-testResults',('"'+(Join-Path $reviewOutput 'PlayMode.xml')+'"')) }
    'Build' { $reviewArgs+=@('-quit','-buildTarget','Android','-executeMethod','Gravivore.Editor.Build.AndroidBuild.BuildDev','-GravivoreVersion','0.1.0','-VersionCode','2') }
}
$reviewProcess=Start-Process -FilePath $reviewUnityPath -ArgumentList $reviewArgs -WorkingDirectory $reviewRoot -WindowStyle Hidden -PassThru -Wait
Write-Output ('UNITY_'+$Action+'_EXIT='+$reviewProcess.ExitCode)
exit $reviewProcess.ExitCode
