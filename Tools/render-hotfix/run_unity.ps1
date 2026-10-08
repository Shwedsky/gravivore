[CmdletBinding()]
param([ValidateSet('Compile','Validate','EditMode','PlayMode','Build')][string]$Action,[string]$TestFilter='')
$ErrorActionPreference='Stop'
$renderRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$renderOutput=Join-Path $renderRoot 'docs/render-hotfix/verification'
New-Item -ItemType Directory -Force -Path $renderOutput | Out-Null
$renderLabel=if($TestFilter){$Action+'Filtered'}else{$Action}
$renderArgs=@('-batchmode','-projectPath',('"'+$renderRoot+'"'),'-logFile',('"'+(Join-Path $renderOutput ($renderLabel+'.log'))+'"'))
switch($Action){
 'Compile' {$renderArgs+=@('-quit')}
 'Validate' {$renderArgs+=@('-quit','-executeMethod','Gravivore.Editor.ProjectValidator.ValidateOrThrow')}
 'EditMode' {$renderArgs+=@('-runTests','-testPlatform','EditMode','-testResults',('"'+(Join-Path $renderOutput ($renderLabel+'.xml'))+'"'))}
 'PlayMode' {$renderArgs+=@('-runTests','-testPlatform','PlayMode','-testResults',('"'+(Join-Path $renderOutput ($renderLabel+'.xml'))+'"'))}
 'Build' {$renderArgs+=@('-quit','-buildTarget','Android','-executeMethod','Gravivore.Editor.Build.AndroidBuild.BuildDev','-GravivoreVersion','0.1.0','-VersionCode','43')}
}
if($TestFilter){$renderArgs+=@('-testFilter',$TestFilter)}
$renderProcess=Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/6000.3.0f1/Editor/Unity.exe' -ArgumentList $renderArgs -WorkingDirectory $renderRoot -WindowStyle Hidden -PassThru
$renderProcess.WaitForExit()
Write-Output ('UNITY_'+$Action+'_EXIT='+$renderProcess.ExitCode)
exit $renderProcess.ExitCode
