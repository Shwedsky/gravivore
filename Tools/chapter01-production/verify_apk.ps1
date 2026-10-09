[CmdletBinding()]
param([string]$ApkPath='Builds/Android/gravivore-dev-0.1.0+39.apk', [Parameter(Mandatory=$true)][string]$ExpectedSourceSha)
$ErrorActionPreference='Stop'
$chapterRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$chapterApk=(Resolve-Path -LiteralPath (Join-Path $chapterRoot $ApkPath)).Path
$chapterOutput=Join-Path $chapterRoot 'docs/history/visual-stages/chapter01-visual-passes/chapter01-production/verification'
$chapterEditor='C:/Program Files/Unity/Hub/Editor/6000.3.0f1/Editor/Data/PlaybackEngines/AndroidPlayer'
$chapterAapt=Join-Path $chapterEditor 'SDK/build-tools/36.0.0/aapt.exe'
$chapterBadging=& $chapterAapt dump badging $chapterApk
if($LASTEXITCODE -ne 0){throw 'aapt inspection failed.'}
$chapterPackage=$chapterBadging | Where-Object {$_ -match '^package:'}
if($chapterPackage -notmatch "name='com.gravivore.mobile.dev'" -or $chapterPackage -notmatch "versionCode='39'" -or $chapterPackage -notmatch "versionName='0.1.0'"){throw ('Unexpected manifest: '+$chapterPackage)}
$chapterNative=$chapterBadging | Where-Object {$_ -match '^native-code:'}
if($chapterNative.Trim() -ne "native-code: 'arm64-v8a'"){throw 'APK must be ARM64 only.'}
if(-not ($chapterBadging | Where-Object {$_ -match '^application-debuggable'})){throw 'APK is not a debuggable DEV build.'}
$chapterMetadata=Get-Content -LiteralPath ([IO.Path]::ChangeExtension($chapterApk,'.build.json')) -Raw | ConvertFrom-Json
if($chapterMetadata.flavor -ne 'Dev' -or -not $chapterMetadata.developmentBuild -or -not $chapterMetadata.allowDebugging -or $chapterMetadata.versionCode -ne 39 -or $chapterMetadata.gitCommitSha -ne $ExpectedSourceSha){throw 'Build metadata differs from the intended committed DEV source.'}
Add-Type -AssemblyName System.IO.Compression.FileSystem
$chapterZip=[IO.Compression.ZipFile]::OpenRead($chapterApk)
try{
 $chapterLibraries=@($chapterZip.Entries | Where-Object {$_.FullName -like 'lib/*/*.so'} | ForEach-Object {$_.FullName})
 if($chapterLibraries -notcontains 'lib/arm64-v8a/libil2cpp.so'){throw 'ARM64 IL2CPP runtime is missing.'}
 if(@($chapterLibraries | Where-Object {$_ -notlike 'lib/arm64-v8a/*'}).Count){throw 'Unexpected native architecture.'}
}finally{$chapterZip.Dispose()}
$chapterJava=Join-Path $chapterEditor 'OpenJDK/bin/java.exe'
$chapterSigner=Join-Path $chapterEditor 'SDK/build-tools/36.0.0/lib/apksigner.jar'
$chapterSignature=& $chapterJava -jar $chapterSigner verify --verbose --print-certs $chapterApk
if($LASTEXITCODE -ne 0){throw 'APK signature verification failed.'}
$chapterBaseline=Join-Path (Split-Path -Parent $chapterRoot) 'post-device-combat-readability-v1/Builds/Android/gravivore-dev-0.1.0+38.apk'
$chapterPriorSignature=& $chapterJava -jar $chapterSigner verify --print-certs $chapterBaseline
if($LASTEXITCODE -ne 0){throw 'Accepted v38 signature verification failed.'}
$chapterCertificate=@($chapterSignature | Where-Object {$_ -match '^Signer #1 certificate SHA-256 digest:'})
$chapterPriorCertificate=@($chapterPriorSignature | Where-Object {$_ -match '^Signer #1 certificate SHA-256 digest:'})
if($chapterCertificate.Count -ne 1 -or $chapterPriorCertificate.Count -ne 1 -or $chapterCertificate[0] -ne $chapterPriorCertificate[0]){throw 'APK signer differs from accepted v38.'}
$chapterVisual=Get-Content -LiteralPath ([IO.Path]::ChangeExtension($chapterApk,'.visual-slice.json')) -Raw | ConvertFrom-Json
$chapterCombat=Get-Content -LiteralPath ([IO.Path]::ChangeExtension($chapterApk,'.post-device.json')) -Raw | ConvertFrom-Json
$chapterProduction=Get-Content -LiteralPath (Join-Path $chapterOutput 'apk_packed_dependencies.json') -Raw | ConvertFrom-Json
if(-not $chapterVisual.validated -or $chapterVisual.modelSources.Count -ne 9 -or $chapterVisual.serializedArchiveEntries.Count -ne 14){throw 'Accepted visual/music packing evidence is incomplete.'}
if(-not $chapterCombat.validated -or $chapterCombat.serializedEntries.Count -ne 10){throw 'Accepted audio/settings packing evidence is incomplete.'}
if(-not $chapterProduction.validated -or $chapterProduction.required.Count -ne 32 -or $chapterProduction.serializedEntries.Count -ne 23){throw 'Full Chapter 01 serialized packing evidence is incomplete.'}
$chapterHash=(Get-FileHash -LiteralPath $chapterApk -Algorithm SHA256).Hash.ToLowerInvariant()
foreach($chapterProof in @($chapterVisual,$chapterCombat,$chapterProduction)){if($chapterProof.apkSha256 -ne $chapterHash){throw 'Packing evidence does not match delivered APK SHA256.'}}
$chapterPython='C:/Users/pamak/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
& $chapterPython (Join-Path $PSScriptRoot 'inspect_runtime_content.py') $chapterApk --output (Join-Path $chapterOutput 'apk_runtime_content.json')
if($LASTEXITCODE -ne 0){throw 'Compiled UI and named serialized footstep settings inspection failed.'}
$chapterRuntime=Get-Content -LiteralPath (Join-Path $chapterOutput 'apk_runtime_content.json') -Raw | ConvertFrom-Json
$chapterFile=Get-Item -LiteralPath $chapterApk
$chapterEvidence=[ordered]@{
 validated=$true; apkPath=$chapterApk; bytes=$chapterFile.Length; mib=[Math]::Round($chapterFile.Length/1MB,2); sha256=$chapterHash
 applicationId=$chapterMetadata.applicationId; version=$chapterMetadata.version; versionCode=$chapterMetadata.versionCode
 buildSourceSha=$chapterMetadata.gitCommitSha; unityVersion=$chapterMetadata.unityVersion; architecture='arm64-v8a'; backend='IL2CPP'
 developmentBuild=$true; allowDebugging=$true; nativeLibraries=$chapterLibraries; signatureVerified=$true; sameSignerAsAcceptedV38=$true
 certificateSha256=($chapterCertificate[0] -replace '^Signer #1 certificate SHA-256 digest: ','')
 acceptedVisualAndMusicPacking=$chapterVisual.serializedArchiveEntries; acceptedAudioPacking=$chapterCombat.serializedEntries
 completeChapterPacking=$chapterProduction.serializedEntries; runtimeContent=$chapterRuntime
}
$chapterBadging | Set-Content -LiteralPath (Join-Path $chapterOutput 'apk_badging.txt') -Encoding utf8
$chapterSignature | Set-Content -LiteralPath (Join-Path $chapterOutput 'apk_signature.txt') -Encoding utf8
Copy-Item -LiteralPath ([IO.Path]::ChangeExtension($chapterApk,'.build.json')) -Destination (Join-Path $chapterOutput 'build_metadata.json')
Copy-Item -LiteralPath ([IO.Path]::ChangeExtension($chapterApk,'.visual-slice.json')) -Destination (Join-Path $chapterOutput 'accepted_visual_packing.json')
Copy-Item -LiteralPath ([IO.Path]::ChangeExtension($chapterApk,'.post-device.json')) -Destination (Join-Path $chapterOutput 'accepted_audio_packing.json')
$chapterEvidence | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $chapterOutput 'apk_verification.json') -Encoding utf8
$chapterEvidence | ConvertTo-Json -Depth 8
