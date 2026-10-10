[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$ExpectedSourceSha,[Parameter(Mandatory=$true)][string]$BaselineApk)
$ErrorActionPreference='Stop'
$v47Root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$v47Output='docs/history/implementation-passes/chapter01-visual-replacement-v3/v47/verification'
$v47Apk=Join-Path $v47Root 'Builds/Android/gravivore-dev-0.1.0+47.apk'
& (Join-Path $v47Root 'Tools/chapter01-v3/verify_apk.ps1') -ExpectedSourceSha $ExpectedSourceSha -VersionCode 47 -OutputDirectory $v47Output
if($LASTEXITCODE){throw 'Package/provenance verification failed'}
# Keep copied legacy receipts usable under the deeper V47 evidence root on Windows.
foreach($v47Receipt in @(
 @('docs_history_visual-stages_chapter01-visual-passes_chapter01-production_verification_apk_packed_dependencies.json','apk_chapter01_production_dependencies.json'),
 @('docs_history_visual-stages_chapter01-visual-passes_concept-fidelity-v2_verification_apk_packed_dependencies.json','apk_concept_fidelity_dependencies.json'))){
 Move-Item -LiteralPath (Join-Path $v47Root ($v47Output+'/'+$v47Receipt[0])) -Destination (Join-Path $v47Root ($v47Output+'/'+$v47Receipt[1])) -Force
}
$v47Python='C:/Users/pamak/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
foreach($v47Inspection in @(@('Tools/visual-replacement-v3/inspect_apk.py','apk_accepted_production.json','--corrective'),@('Tools/surface-hero-v46/inspect_apk.py','apk_preserved_v46_pbr.json',''),@('Tools/concept-convergence-v47/inspect_apk.py','apk_v47_scene_skin_pbr.json',''))){
 $v47Args=@((Join-Path $v47Root $v47Inspection[0]),$v47Apk,'--output',(Join-Path $v47Root ($v47Output+'/'+$v47Inspection[1])))
 if($v47Inspection[2]){$v47Args+=$v47Inspection[2]}
 & $v47Python @v47Args
 if($LASTEXITCODE){throw ('Typed APK inspection failed: '+$v47Inspection[0])}
}
$v47Variants=Get-Content (Join-Path $v47Root 'docs/history/visual-stages/chapter01-visual-passes/render-hotfix/verification/compiled_shader_variants.txt')
foreach($v47Backend in @('Vulkan','GLES3x')){
 foreach($v47Keyword in @('_NORMALMAP','_OCCLUSIONMAP','_METALLICSPECGLOSSMAP','_EMISSION')){
  if(-not @($v47Variants | Where-Object {$_.StartsWith('Universal Render Pipeline/Lit | ') -and $_.Contains(' | '+$v47Backend+' | ') -and $_.Contains($v47Keyword)}).Count){throw ('PBR shader variant missing: '+$v47Backend+' '+$v47Keyword)}
 }
}
Copy-Item -LiteralPath (Join-Path $v47Root 'docs/history/visual-stages/chapter01-visual-passes/render-hotfix/verification/compiled_shader_variants.txt') -Destination (Join-Path $v47Root ($v47Output+'/compiled_shader_variants.txt'))
$v47Android='C:/Program Files/Unity/Hub/Editor/6000.3.0f1/Editor/Data/PlaybackEngines/AndroidPlayer'
$v47PriorSignature=& (Join-Path $v47Android 'OpenJDK/bin/java.exe') -jar (Join-Path $v47Android 'SDK/build-tools/36.0.0/lib/apksigner.jar') verify --print-certs $BaselineApk
if($LASTEXITCODE){throw 'V46 baseline signature verification failed'}
$v47PriorCert=@($v47PriorSignature | Where-Object {$_ -match '^Signer #1 certificate SHA-256 digest:'})
$v47Signature=Get-Content (Join-Path $v47Root ($v47Output+'/apk_signature.txt'))
$v47Cert=@($v47Signature | Where-Object {$_ -match '^Signer #1 certificate SHA-256 digest:'})
if($v47PriorCert.Count -ne 1 -or $v47Cert.Count -ne 1 -or $v47PriorCert[0] -ne $v47Cert[0]){throw 'V47 signer differs from distributed V46'}
$v47Evidence=[ordered]@{validated=$true;versionCode=47;baselineApk=$BaselineApk;sameSignerAsV46=$true;baselineSha256=(Get-FileHash -LiteralPath $BaselineApk -Algorithm SHA256).Hash.ToLowerInvariant();vulkanAndGlesPbrVariantsVerified=$true;typedV47SceneActorFacilityAndPbrReferencesVerified=$true;physicalAndroidExecution=$false}
$v47Evidence | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $v47Root ($v47Output+'/apk_upgrade_and_shader_proof.json')) -Encoding utf8
Write-Output 'V47_APK_PACKAGE_SIGNATURE_SCENE_SKIN_PBR_PASS'
