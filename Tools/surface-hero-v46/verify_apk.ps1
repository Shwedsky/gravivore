[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$ExpectedSourceSha)
$ErrorActionPreference='Stop'
$surfaceRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$surfaceOutput=Join-Path $surfaceRoot 'docs/history/implementation-passes/chapter01-visual-replacement-v3/v46/verification'
& (Join-Path $surfaceRoot 'Tools/chapter01-v3/verify_apk.ps1') -ExpectedSourceSha $ExpectedSourceSha -VersionCode 46 -OutputDirectory 'docs/history/implementation-passes/chapter01-visual-replacement-v3/v46/verification'
if($LASTEXITCODE){throw 'V46 base package verification failed'}
$surfaceApk=Join-Path $surfaceRoot 'Builds/Android/gravivore-dev-0.1.0+46.apk'
$surfacePython='C:/Users/pamak/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
& $surfacePython (Join-Path $surfaceRoot 'Tools/visual-replacement-v3/inspect_apk.py') $surfaceApk --corrective --output (Join-Path $surfaceOutput 'apk_accepted_production.json')
if($LASTEXITCODE){throw 'Preserved accepted runtime/URP/UI/weapon audit failed'}
& $surfacePython (Join-Path $surfaceRoot 'Tools/surface-hero-v46/inspect_apk.py') $surfaceApk --output (Join-Path $surfaceOutput 'apk_surface_production.json')
if($LASTEXITCODE){throw 'V46 typed PBR/machinery verification failed'}
$surfaceVariants=Get-Content (Join-Path $surfaceRoot 'docs/render-hotfix/verification/compiled_shader_variants.txt')
foreach($surfaceBackend in @('Vulkan','GLES3x')){
 foreach($surfaceKeyword in @('_NORMALMAP','_OCCLUSIONMAP','_METALLICSPECGLOSSMAP','_EMISSION')){
  if(-not @($surfaceVariants | Where-Object {$_.StartsWith('Universal Render Pipeline/Lit | ') -and $_.Contains(' | '+$surfaceBackend+' | ') -and $_.Contains($surfaceKeyword)}).Count){throw ('V46 PBR keyword missing on '+$surfaceBackend+': '+$surfaceKeyword)}
 }
}
Copy-Item -LiteralPath (Join-Path $surfaceRoot 'docs/render-hotfix/verification/compiled_shader_variants.txt') -Destination (Join-Path $surfaceOutput 'compiled_shader_variants.txt')
Copy-Item -LiteralPath (Join-Path $surfaceRoot 'docs/render-hotfix/verification/serialized_scene_renderers.txt') -Destination (Join-Path $surfaceOutput 'serialized_scene_renderers.txt')
$surfaceAndroid='C:/Program Files/Unity/Hub/Editor/6000.3.0f1/Editor/Data/PlaybackEngines/AndroidPlayer'
$surfacePrevious=Join-Path $surfaceRoot 'Builds/Android/gravivore-dev-0.1.0+45.apk'
$surfacePreviousCert=@(& (Join-Path $surfaceAndroid 'OpenJDK/bin/java.exe') -jar (Join-Path $surfaceAndroid 'SDK/build-tools/36.0.0/lib/apksigner.jar') verify --print-certs $surfacePrevious | Where-Object {$_ -match '^Signer #1 certificate SHA-256 digest:'})
if($LASTEXITCODE){throw 'V45 certificate verification failed'}
$surfaceNewCert=@(Get-Content (Join-Path $surfaceOutput 'apk_signature.txt') | Where-Object {$_ -match '^Signer #1 certificate SHA-256 digest:'})
if($surfacePreviousCert[0] -ne $surfaceNewCert[0]){throw 'V46 signer differs from V45'}
Write-Output 'V46_APK_SIGNATURE_MANIFEST_SHADER_PBR_PASS'
