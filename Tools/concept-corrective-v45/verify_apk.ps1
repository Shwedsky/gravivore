[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$ExpectedSourceSha)
$ErrorActionPreference='Stop'
$correctiveRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$correctiveOutput=Join-Path $correctiveRoot 'docs/history/implementation-passes/chapter01-visual-replacement-v3/v45/verification'
& (Join-Path $correctiveRoot 'Tools/chapter01-v3/verify_apk.ps1') -ExpectedSourceSha $ExpectedSourceSha -VersionCode 45 -OutputDirectory 'docs/history/implementation-passes/chapter01-visual-replacement-v3/v45/verification'
if($LASTEXITCODE){throw 'Base V45 manifest/signature/accepted content verification failed'}
$correctiveApk=Join-Path $correctiveRoot 'Builds/Android/gravivore-dev-0.1.0+45.apk'
$correctivePython='C:/Users/pamak/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
& $correctivePython (Join-Path $correctiveRoot 'Tools/visual-replacement-v3/inspect_apk.py') $correctiveApk --corrective --output (Join-Path $correctiveOutput 'apk_typed_production.json')
if($LASTEXITCODE){throw 'V45 typed scene/shader/UI/motion verification failed'}
$correctiveHash=(Get-FileHash -LiteralPath $correctiveApk -Algorithm SHA256).Hash.ToLowerInvariant()
foreach($proofPath in @('docs/history/implementation-passes/chapter01-visual-replacement-v3/v44/verification/apk_production_dependencies.json','docs/history/implementation-passes/chapter01-visual-replacement-v3/v45/verification/apk_corrective_dependencies.json')){
 $proof=Get-Content (Join-Path $correctiveRoot $proofPath) -Raw | ConvertFrom-Json
 if(-not $proof.validated -or $proof.apkSha256 -ne $correctiveHash){throw ('Packing receipt differs from delivered APK: '+$proofPath)}
 if($proofPath -like '*visual-replacement*'){Copy-Item -LiteralPath (Join-Path $correctiveRoot $proofPath) -Destination (Join-Path $correctiveOutput 'apk_production_dependencies.json')}
}
$correctiveVariants=Get-Content (Join-Path $correctiveRoot 'docs/history/visual-stages/chapter01-visual-passes/render-hotfix/verification/compiled_shader_variants.txt')
foreach($shader in @('UI/Default','UI/DefaultETC1','Universal Render Pipeline/Lit','Universal Render Pipeline/Particles/Unlit')){
 foreach($backend in @('Vulkan','GLES3x')){
  if(-not @($correctiveVariants | Where-Object {$_.StartsWith($shader+' | ') -and $_.Contains(' | '+$backend+' | ')}).Count){throw ('Target shader variant missing: '+$shader+' / '+$backend)}
 }
}
Copy-Item -LiteralPath (Join-Path $correctiveRoot 'docs/history/visual-stages/chapter01-visual-passes/render-hotfix/verification/compiled_shader_variants.txt') -Destination (Join-Path $correctiveOutput 'compiled_shader_variants.txt')
Copy-Item -LiteralPath (Join-Path $correctiveRoot 'docs/history/visual-stages/chapter01-visual-passes/render-hotfix/verification/serialized_scene_renderers.txt') -Destination (Join-Path $correctiveOutput 'serialized_scene_renderers.txt')
$correctiveAndroid='C:/Program Files/Unity/Hub/Editor/6000.3.0f1/Editor/Data/PlaybackEngines/AndroidPlayer'
$correctivePrior=Join-Path $correctiveRoot 'Builds/Android/gravivore-dev-0.1.0+44.apk'
$correctivePriorCert=@(& (Join-Path $correctiveAndroid 'OpenJDK/bin/java.exe') -jar (Join-Path $correctiveAndroid 'SDK/build-tools/36.0.0/lib/apksigner.jar') verify --print-certs $correctivePrior | Where-Object {$_ -match '^Signer #1 certificate SHA-256 digest:'})
if($LASTEXITCODE){throw 'V44 signer verification failed'}
$correctiveNewCert=@(Get-Content (Join-Path $correctiveOutput 'apk_signature.txt') | Where-Object {$_ -match '^Signer #1 certificate SHA-256 digest:'})
if($correctivePriorCert[0] -ne $correctiveNewCert[0]){throw 'V45 signer differs from V44'}
$correctiveEvidence=Get-Content (Join-Path $correctiveOutput 'apk_verification.json') -Raw | ConvertFrom-Json
$correctiveEvidence | Add-Member -NotePropertyName typedProductionAndRenderingVerified -NotePropertyValue $true
$correctiveEvidence | Add-Member -NotePropertyName targetShaderVariantsVerified -NotePropertyValue $true
$correctiveEvidence | Add-Member -NotePropertyName sameSignerAsV44 -NotePropertyValue $true
$correctiveEvidence | Add-Member -NotePropertyName executedOnAndroid -NotePropertyValue $false
$correctiveEvidence | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $correctiveOutput 'apk_verification.json') -Encoding utf8
$correctiveEvidence | ConvertTo-Json -Depth 8
