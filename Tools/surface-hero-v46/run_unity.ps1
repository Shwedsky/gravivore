[CmdletBinding()]
param([ValidateSet('Compile','Baseline','Author','Gallery','Validate','Audit','EditMode','PlayMode')][string]$Action,[string]$TestFilter='',[string]$Label='')
$ErrorActionPreference='Stop'
$surfaceRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$surfaceOutput=Join-Path $surfaceRoot 'docs/surface-hero-v46/verification'
New-Item -ItemType Directory -Force -Path $surfaceOutput | Out-Null
$surfaceLabel=if($Label){$Label}else{$Action}
$surfaceArgs=@('-batchmode','-projectPath',('"'+$surfaceRoot+'"'),'-logFile',('"'+(Join-Path $surfaceOutput ($surfaceLabel+'.log'))+'"'))
if($Action -in @('Compile','Validate','Audit','EditMode','Baseline')){$surfaceArgs+='-nographics'}
switch($Action){
 'Compile' {$surfaceArgs+='-quit'}
 'Baseline' {$surfaceArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.SurfaceHeroV46Builder.Baseline')}
 'Author' {$surfaceArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.SurfaceHeroV46Builder.Build')}
 'Gallery' {$surfaceArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.SurfaceHeroV46Builder.Gallery')}
 'Validate' {$surfaceArgs+=@('-quit','-executeMethod','Gravivore.Editor.ProjectValidator.ValidateOrThrow')}
 'Audit' {$surfaceArgs+=@('-quit','-executeMethod','Gravivore.Editor.VisualIntegration.SurfaceHeroV46Audit.ValidateOrThrow')}
 'EditMode' {$surfaceArgs+=@('-runTests','-testPlatform','EditMode','-testResults',('"'+(Join-Path $surfaceOutput ($surfaceLabel+'.xml'))+'"'))}
 'PlayMode' {$surfaceArgs+=@('-runTests','-testPlatform','PlayMode','-testResults',('"'+(Join-Path $surfaceOutput ($surfaceLabel+'.xml'))+'"'))}
}
if($TestFilter){$surfaceArgs+=@('-testFilter',$TestFilter)}
$surfaceProcess=Start-Process -FilePath 'C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe' -ArgumentList $surfaceArgs -WorkingDirectory $surfaceRoot -WindowStyle Hidden -PassThru
$surfaceProcess.WaitForExit()
Write-Output ('UNITY_'+$Action+'_EXIT='+$surfaceProcess.ExitCode)
exit $surfaceProcess.ExitCode
