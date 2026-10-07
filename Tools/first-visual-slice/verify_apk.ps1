[CmdletBinding()]
param([string]$ApkPath='Builds/Android/gravivore-dev-0.1.0+37.apk')
$ErrorActionPreference='Stop'
$sliceRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$sliceApk=(Resolve-Path -LiteralPath (Join-Path $sliceRoot $ApkPath)).Path
$sliceOutput=Join-Path $sliceRoot 'docs/device-correction/verification'
$sliceEditor='C:/Program Files/Unity/Hub/Editor/6000.3.0f1/Editor/Data/PlaybackEngines/AndroidPlayer'
$sliceAapt=Join-Path $sliceEditor 'SDK/build-tools/36.0.0/aapt.exe'
$sliceBadging=& $sliceAapt dump badging $sliceApk
if($LASTEXITCODE -ne 0){throw 'aapt failed to inspect the APK.'}
$slicePackage=$sliceBadging | Where-Object {$_ -match '^package:'}
if($slicePackage -notmatch "name='com.gravivore.mobile.dev'" -or $slicePackage -notmatch "versionCode='37'" -or $slicePackage -notmatch "versionName='0.1.0'"){
 throw ('Unexpected APK package/version: '+$slicePackage)
}
$sliceNative=$sliceBadging | Where-Object {$_ -match '^native-code:'}
if($sliceNative.Trim() -ne "native-code: 'arm64-v8a'"){throw ('APK is not ARM64 only: '+$sliceNative)}
Add-Type -AssemblyName System.IO.Compression.FileSystem
$sliceZip=[System.IO.Compression.ZipFile]::OpenRead($sliceApk)
try{
 $sliceLibraries=@($sliceZip.Entries | Where-Object {$_.FullName -like 'lib/*/*.so'} | ForEach-Object {$_.FullName})
 if($sliceLibraries -notcontains 'lib/arm64-v8a/libil2cpp.so'){throw 'APK is missing its ARM64 IL2CPP player.'}
 if(@($sliceLibraries | Where-Object {$_ -notlike 'lib/arm64-v8a/*'}).Count -ne 0){throw 'Unexpected non-ARM64 native library.'}
}finally{$sliceZip.Dispose()}
$sliceJava=Join-Path $sliceEditor 'OpenJDK/bin/java.exe'
$sliceSigner=Join-Path $sliceEditor 'SDK/build-tools/36.0.0/lib/apksigner.jar'
$sliceSignature=& $sliceJava -jar $sliceSigner verify --verbose $sliceApk
if($LASTEXITCODE -ne 0){throw 'APK signature verification failed.'}
$sliceMetadata=Get-Content -LiteralPath ([System.IO.Path]::ChangeExtension($sliceApk,'.build.json')) -Raw | ConvertFrom-Json
$slicePacking=Get-Content -LiteralPath ([System.IO.Path]::ChangeExtension($sliceApk,'.visual-slice.json')) -Raw | ConvertFrom-Json
if(-not $slicePacking.validated -or $slicePacking.required.Count -ne 8 -or $slicePacking.modelSources.Count -ne 8 -or $slicePacking.serializedArchiveEntries.Count -ne 12){throw 'Production correction packing evidence is incomplete.'}
$sliceFile=Get-Item -LiteralPath $sliceApk
$sliceHash=(Get-FileHash -LiteralPath $sliceApk -Algorithm SHA256).Hash.ToLowerInvariant()
if($slicePacking.apkSha256 -ne $sliceHash){throw 'Packing evidence does not match this APK.'}
$sliceEvidence=[ordered]@{
 apkPath=$sliceApk
 bytes=$sliceFile.Length
 mib=[Math]::Round($sliceFile.Length / 1MB,2)
 sha256=$sliceHash
 applicationId=$sliceMetadata.applicationId
 version=$sliceMetadata.version
 versionCode=$sliceMetadata.versionCode
 buildSourceSha=$sliceMetadata.gitCommitSha
 unityVersion=$sliceMetadata.unityVersion
 architecture='arm64-v8a'
 nativeLibraries=$sliceLibraries
 signatureVerified=$true
 allFiveVisualDependenciesValidated=$slicePacking.validated
 modelSourceDependencies=$slicePacking.modelSources
 serializedApkVisualEntries=$slicePacking.serializedArchiveEntries
}
New-Item -ItemType Directory -Force -Path $sliceOutput | Out-Null
$sliceBadging | Set-Content -LiteralPath (Join-Path $sliceOutput 'apk_badging.txt') -Encoding utf8
$sliceSignature | Set-Content -LiteralPath (Join-Path $sliceOutput 'apk_signature.txt') -Encoding utf8
$sliceEvidence | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $sliceOutput 'apk_verification.json') -Encoding utf8
$sliceEvidence | ConvertTo-Json -Depth 5
