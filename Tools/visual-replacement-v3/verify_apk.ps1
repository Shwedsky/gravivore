[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$ExpectedSourceSha)
$ErrorActionPreference='Stop'
$replacementRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$replacementOutput=Join-Path $replacementRoot 'docs/history/implementation-passes/chapter01-visual-replacement-v3/v44/verification'
& (Join-Path $replacementRoot 'Tools/chapter01-v3/verify_apk.ps1') -ExpectedSourceSha $ExpectedSourceSha -VersionCode 44 -OutputDirectory 'docs/history/implementation-passes/chapter01-visual-replacement-v3/v44/verification'
if($LASTEXITCODE){throw 'Base v44 manifest/signature/content verification failed'}
$replacementApk=Join-Path $replacementRoot 'Builds/Android/gravivore-dev-0.1.0+44.apk'
$replacementPython='C:/Users/pamak/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
& $replacementPython (Join-Path $PSScriptRoot 'inspect_apk.py') $replacementApk --output (Join-Path $replacementOutput 'apk_typed_production.json')
if($LASTEXITCODE){throw 'V44 typed shader/UI/rank/model verification failed'}
$replacementHash=(Get-FileHash -LiteralPath $replacementApk -Algorithm SHA256).Hash.ToLowerInvariant()
$replacementPacking=Get-Content (Join-Path $replacementOutput 'apk_production_dependencies.json') -Raw | ConvertFrom-Json
if(-not $replacementPacking.validated -or $replacementPacking.apkSha256 -ne $replacementHash){throw 'Production packing proof differs from APK'}
$replacementVariants=Get-Content (Join-Path $replacementRoot 'docs/render-hotfix/verification/compiled_shader_variants.txt')
foreach($replacementShader in @('UI/Default','UI/DefaultETC1','Universal Render Pipeline/Lit','Universal Render Pipeline/Particles/Unlit')){
 foreach($replacementBackend in @('Vulkan','GLES3x')){
  if(-not @($replacementVariants | Where-Object {$_.StartsWith($replacementShader+' | ') -and $_.Contains(' | '+$replacementBackend+' | ')}).Count){throw ('Target variant missing: '+$replacementShader+' / '+$replacementBackend)}
 }
}
$replacementSceneAudit=Get-Content (Join-Path $replacementRoot 'docs/render-hotfix/verification/serialized_scene_renderers.txt') -Raw
foreach($replacementScene in @('Bootstrap','Chapter01_ScrapExclusion')){
 if($replacementSceneAudit -notmatch ($replacementScene+'\.unity: renderers=\d+; v3=\d+; missing/error materials=0')){throw ('Serialized renderer audit missing: '+$replacementScene)}
}
Copy-Item -LiteralPath (Join-Path $replacementRoot 'docs/render-hotfix/verification/compiled_shader_variants.txt') -Destination (Join-Path $replacementOutput 'compiled_shader_variants.txt')
Copy-Item -LiteralPath (Join-Path $replacementRoot 'docs/render-hotfix/verification/serialized_scene_renderers.txt') -Destination (Join-Path $replacementOutput 'serialized_scene_renderers.txt')
$replacementEvidence=Get-Content (Join-Path $replacementOutput 'apk_verification.json') -Raw | ConvertFrom-Json
$replacementEvidence | Add-Member -NotePropertyName typedProductionAndRenderingVerified -NotePropertyValue $true
$replacementEvidence | Add-Member -NotePropertyName targetShaderVariantsVerified -NotePropertyValue $true
$replacementEvidence | Add-Member -NotePropertyName executedOnAndroid -NotePropertyValue $false
$replacementPrior=Join-Path (Split-Path -Parent (Split-Path -Parent $replacementRoot)) 'Builds/Android/gravivore-dev-0.1.0+43.apk'
if(Test-Path -LiteralPath $replacementPrior){
 $replacementAndroid='C:/Program Files/Unity/Hub/Editor/6000.3.0f1/Editor/Data/PlaybackEngines/AndroidPlayer'
 $replacementCert=@(& (Join-Path $replacementAndroid 'OpenJDK/bin/java.exe') -jar (Join-Path $replacementAndroid 'SDK/build-tools/36.0.0/lib/apksigner.jar') verify --print-certs $replacementPrior | Where-Object {$_ -match '^Signer #1 certificate SHA-256 digest:'})
 if($LASTEXITCODE){throw 'v43 signer verification failed'}
 $replacementNewCert=@(Get-Content (Join-Path $replacementOutput 'apk_signature.txt') | Where-Object {$_ -match '^Signer #1 certificate SHA-256 digest:'})
 if($replacementCert[0] -ne $replacementNewCert[0]){throw 'V44 signer differs from v43'}
 $replacementEvidence | Add-Member -NotePropertyName sameSignerAsV43 -NotePropertyValue $true
}
$replacementEvidence | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $replacementOutput 'apk_verification.json') -Encoding utf8
$replacementEvidence | ConvertTo-Json -Depth 8
