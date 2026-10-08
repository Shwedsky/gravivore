[CmdletBinding()]
param([string]$ApkPath='Builds/Android/gravivore-dev-0.1.0+41.apk',[Parameter(Mandatory=$true)][string]$ExpectedSourceSha)
$ErrorActionPreference='Stop'
$fidelityRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$fidelityApk=(Resolve-Path -LiteralPath (Join-Path $fidelityRoot $ApkPath)).Path
$fidelityOutput=Join-Path $fidelityRoot 'docs/concept-fidelity-v2/verification'
$fidelityAndroid='C:/Program Files/Unity/Hub/Editor/6000.3.0f1/Editor/Data/PlaybackEngines/AndroidPlayer'
$fidelityBadging=& (Join-Path $fidelityAndroid 'SDK/build-tools/36.0.0/aapt.exe') dump badging $fidelityApk
if($LASTEXITCODE -ne 0){throw 'aapt inspection failed.'}
$fidelityPackage=$fidelityBadging | Where-Object {$_ -match '^package:'}
if($fidelityPackage -notmatch "name='com.gravivore.mobile.dev'" -or $fidelityPackage -notmatch "versionCode='41'" -or $fidelityPackage -notmatch "versionName='0.1.0'"){throw ('Unexpected manifest: '+$fidelityPackage)}
if(($fidelityBadging | Where-Object {$_ -match '^native-code:'}).Trim() -ne "native-code: 'arm64-v8a'"){throw 'APK must be ARM64 only.'}
if(-not ($fidelityBadging | Where-Object {$_ -match '^application-debuggable'})){throw 'APK is not a debuggable DEV build.'}
$fidelityMetadata=Get-Content -LiteralPath ([IO.Path]::ChangeExtension($fidelityApk,'.build.json')) -Raw | ConvertFrom-Json
if($fidelityMetadata.flavor -ne 'Dev' -or -not $fidelityMetadata.developmentBuild -or -not $fidelityMetadata.allowDebugging -or $fidelityMetadata.versionCode -ne 41 -or $fidelityMetadata.gitCommitSha -ne $ExpectedSourceSha){throw 'Build metadata differs from committed DEV source.'}
Add-Type -AssemblyName System.IO.Compression.FileSystem
$fidelityZip=[IO.Compression.ZipFile]::OpenRead($fidelityApk)
try{
 $fidelityLibraries=@($fidelityZip.Entries | Where-Object {$_.FullName -like 'lib/*/*.so'} | ForEach-Object {$_.FullName})
 if($fidelityLibraries -notcontains 'lib/arm64-v8a/libil2cpp.so'){throw 'ARM64 IL2CPP runtime is missing.'}
 if(@($fidelityLibraries | Where-Object {$_ -notlike 'lib/arm64-v8a/*'}).Count){throw 'Unexpected native architecture.'}
}finally{$fidelityZip.Dispose()}
$fidelityJava=Join-Path $fidelityAndroid 'OpenJDK/bin/java.exe'
$fidelitySigner=Join-Path $fidelityAndroid 'SDK/build-tools/36.0.0/lib/apksigner.jar'
$fidelitySignature=& $fidelityJava -jar $fidelitySigner verify --verbose --print-certs $fidelityApk
if($LASTEXITCODE -ne 0){throw 'APK signature verification failed.'}
$fidelityBaseline=Join-Path (Split-Path -Parent $fidelityRoot) 'chapter01-respawn-map-repair-v1/Builds/Android/gravivore-dev-0.1.0+40.apk'
$fidelityPriorSignature=& $fidelityJava -jar $fidelitySigner verify --print-certs $fidelityBaseline
if($LASTEXITCODE -ne 0){throw 'Merged v40 signature verification failed.'}
$fidelityCertificate=@($fidelitySignature | Where-Object {$_ -match '^Signer #1 certificate SHA-256 digest:'})
$fidelityPriorCertificate=@($fidelityPriorSignature | Where-Object {$_ -match '^Signer #1 certificate SHA-256 digest:'})
if($fidelityCertificate.Count -ne 1 -or $fidelityPriorCertificate.Count -ne 1 -or $fidelityCertificate[0] -ne $fidelityPriorCertificate[0]){throw 'APK signer differs from v40.'}
$fidelityHash=(Get-FileHash -LiteralPath $fidelityApk -Algorithm SHA256).Hash.ToLowerInvariant()
$fidelityChapterProofPath=Join-Path $fidelityRoot 'docs/chapter01-production/verification/apk_packed_dependencies.json'
$fidelityChapterProof=Get-Content -LiteralPath $fidelityChapterProofPath -Raw | ConvertFrom-Json
if($fidelityChapterProof.apkSha256 -ne $fidelityHash){
 # Historical chapter reports may be restored after their current-build evidence
 # is copied into this milestone's immutable delivery folder.
 $fidelityChapterProofPath=Join-Path $fidelityOutput 'complete_chapter_packing.json'
}
$fidelityProofs=@(
 [IO.Path]::ChangeExtension($fidelityApk,'.visual-slice.json'),
 [IO.Path]::ChangeExtension($fidelityApk,'.post-device.json'),
 $fidelityChapterProofPath,
 (Join-Path $fidelityOutput 'apk_packed_dependencies.json'))
foreach($fidelityProofPath in $fidelityProofs){
 $fidelityProof=Get-Content -LiteralPath $fidelityProofPath -Raw | ConvertFrom-Json
 if(-not $fidelityProof.validated -or $fidelityProof.apkSha256 -ne $fidelityHash){throw ('Packing evidence mismatch: '+$fidelityProofPath)}
}
$fidelityPack=Get-Content -LiteralPath (Join-Path $fidelityOutput 'apk_packed_dependencies.json') -Raw | ConvertFrom-Json
if($fidelityPack.required.Count -ne 21 -or $fidelityPack.serializedEntries.Count -ne 21){throw 'Authored 18-mesh pack/config/atlas/route evidence incomplete.'}
$fidelityChapter=Get-Content -LiteralPath $fidelityProofs[2] -Raw | ConvertFrom-Json
if($fidelityChapter.serializedEntries.Count -ne 23){throw 'Complete Chapter 01 packing evidence is incomplete.'}
$fidelityPython='C:/Users/pamak/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
& $fidelityPython (Join-Path $fidelityRoot 'Tools/chapter01-production/inspect_runtime_content.py') $fidelityApk --output (Join-Path $fidelityOutput 'apk_runtime_content.json')
if($LASTEXITCODE -ne 0){throw 'Compiled UI and named serialized footstep settings inspection failed.'}
$fidelityFile=Get-Item -LiteralPath $fidelityApk
$fidelityEvidence=[ordered]@{
 validated=$true; apkPath=$fidelityApk; bytes=$fidelityFile.Length; mib=[Math]::Round($fidelityFile.Length/1MB,2); sha256=$fidelityHash
 applicationId=$fidelityMetadata.applicationId; version=$fidelityMetadata.version; versionCode=41; buildSourceSha=$fidelityMetadata.gitCommitSha
 unityVersion=$fidelityMetadata.unityVersion; architecture='arm64-v8a'; backend='IL2CPP'; developmentBuild=$true; allowDebugging=$true
 signatureVerified=$true; sameSignerAsV40=$true; certificateSha256=($fidelityCertificate[0] -replace '^Signer #1 certificate SHA-256 digest: ','')
 authoredContent=$fidelityPack.serializedEntries; completeChapterContent=$fidelityChapter.serializedEntries; nativeLibraries=$fidelityLibraries
}
$fidelityBadging | Set-Content -LiteralPath (Join-Path $fidelityOutput 'apk_badging.txt') -Encoding utf8
$fidelitySignature | Set-Content -LiteralPath (Join-Path $fidelityOutput 'apk_signature.txt') -Encoding utf8
Copy-Item -LiteralPath ([IO.Path]::ChangeExtension($fidelityApk,'.build.json')) -Destination (Join-Path $fidelityOutput 'build_metadata.json')
Copy-Item -LiteralPath $fidelityProofs[0] -Destination (Join-Path $fidelityOutput 'accepted_visual_packing.json')
Copy-Item -LiteralPath $fidelityProofs[1] -Destination (Join-Path $fidelityOutput 'accepted_audio_packing.json')
$fidelityChapterCopy=Join-Path $fidelityOutput 'complete_chapter_packing.json'
if([IO.Path]::GetFullPath($fidelityProofs[2]) -ne [IO.Path]::GetFullPath($fidelityChapterCopy)){
 Copy-Item -LiteralPath $fidelityProofs[2] -Destination $fidelityChapterCopy
}
$fidelityEvidence | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $fidelityOutput 'apk_verification.json') -Encoding utf8
$fidelityEvidence | ConvertTo-Json -Depth 8
