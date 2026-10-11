[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$ExpectedSourceSha)
$ErrorActionPreference='Stop'
$renderRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$renderOutput=Join-Path $renderRoot 'docs/history/visual-stages/chapter01-visual-passes/render-hotfix/verification'
& (Join-Path $renderRoot 'Tools/chapter01-v3/verify_apk.ps1') -ExpectedSourceSha $ExpectedSourceSha -VersionCode 43 -OutputDirectory 'docs/history/visual-stages/chapter01-visual-passes/render-hotfix/verification'
if($LASTEXITCODE){throw 'Base v43 APK verification failed'}
$renderApk=Join-Path $renderRoot 'Builds/Android/gravivore-dev-0.1.0+43.apk'
$renderPython='C:/Users/pamak/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
& $renderPython (Join-Path $PSScriptRoot 'inspect_rendering.py') $renderApk --output (Join-Path $renderOutput 'apk_rendering.json')
if($LASTEXITCODE){throw 'Typed APK shader/material/pipeline verification failed'}
$renderDependencies=Get-Content -LiteralPath (Join-Path $renderOutput 'render_dependency_report.json') -Raw | ConvertFrom-Json
if(-not $renderDependencies.validated -or $renderDependencies.required.Count -lt 6){throw 'Incomplete build report rendering audit'}
foreach($renderDependency in $renderDependencies.required){
 if($renderDependencies.packed -notcontains $renderDependency){throw ('Render dependency missing from build report: '+$renderDependency)}
}
$renderSceneAudit=Get-Content -LiteralPath (Join-Path $renderOutput 'serialized_scene_renderers.txt') -Raw
foreach($renderScene in @('Bootstrap','Chapter01_ScrapExclusion')){
 if($renderSceneAudit -notmatch ($renderScene+'\.unity: renderers=\d+; v3=\d+; missing/error materials=0')){throw ('Serialized renderer audit missing: '+$renderScene)}
}
$renderVariants=Get-Content -LiteralPath (Join-Path $renderOutput 'compiled_shader_variants.txt')
foreach($renderShader in @('UI/Default','UI/DefaultETC1','Universal Render Pipeline/Lit','Universal Render Pipeline/Particles/Unlit')){
 foreach($renderBackend in @('Vulkan','GLES3x')){
  $renderMatches=@($renderVariants | Where-Object {$_.StartsWith($renderShader+' | ') -and $_.Contains(' | '+$renderBackend+' | ')})
  if(-not $renderMatches.Count){throw ('Target shader variant not recorded: '+$renderShader+' / '+$renderBackend)}
 }
}
$renderAndroid='C:/Program Files/Unity/Hub/Editor/6000.3.0f1/Editor/Data/PlaybackEngines/AndroidPlayer'
$renderJava=Join-Path $renderAndroid 'OpenJDK/bin/java.exe'
$renderSigner=Join-Path $renderAndroid 'SDK/build-tools/36.0.0/lib/apksigner.jar'
$renderPrevious=Join-Path $renderRoot 'Builds/Android/gravivore-dev-0.1.0+42.apk'
$renderPriorCert=@(& $renderJava -jar $renderSigner verify --print-certs $renderPrevious | Where-Object {$_ -match '^Signer #1 certificate SHA-256 digest:'})
if($LASTEXITCODE){throw 'v42 signer check failed'}
$renderCurrentCert=@(Get-Content -LiteralPath (Join-Path $renderOutput 'apk_signature.txt') | Where-Object {$_ -match '^Signer #1 certificate SHA-256 digest:'})
if($renderPriorCert[0] -ne $renderCurrentCert[0]){throw 'v43 signer differs from v42'}
$renderEvidence=Get-Content -LiteralPath (Join-Path $renderOutput 'apk_verification.json') -Raw | ConvertFrom-Json
$renderEvidence | Add-Member -NotePropertyName sameSignerAsV42 -NotePropertyValue $true
$renderEvidence | Add-Member -NotePropertyName typedRenderingDependenciesVerified -NotePropertyValue $true
$renderEvidence | Add-Member -NotePropertyName targetShaderVariantsVerified -NotePropertyValue $true
$renderEvidence | Add-Member -NotePropertyName serializedRendererMaterialsVerified -NotePropertyValue $true
$renderEvidence | Add-Member -NotePropertyName executedOnAndroid -NotePropertyValue $false
$renderEvidence | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $renderOutput 'apk_verification.json') -Encoding utf8
$renderEvidence | ConvertTo-Json -Depth 6
