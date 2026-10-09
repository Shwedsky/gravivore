[CmdletBinding()]
param([string]$ApkPath='Builds/Android/gravivore-dev-0.1.0+38.apk')
$ErrorActionPreference='Stop'
$taskRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$taskApk=(Resolve-Path -LiteralPath (Join-Path $taskRoot $ApkPath)).Path
$taskOutput=Join-Path $taskRoot 'docs/history/visual-stages/chapter01-visual-passes/post-device-combat-readability/verification'
$taskEditor='C:/Program Files/Unity/Hub/Editor/6000.3.0f1/Editor/Data/PlaybackEngines/AndroidPlayer'
$taskAapt=Join-Path $taskEditor 'SDK/build-tools/36.0.0/aapt.exe'
$taskBadging=& $taskAapt dump badging $taskApk
if($LASTEXITCODE -ne 0){throw 'aapt failed.'}
$taskPackage=$taskBadging | Where-Object {$_ -match '^package:'}
if($taskPackage -notmatch "name='com.gravivore.mobile.dev'" -or $taskPackage -notmatch "versionCode='38'" -or $taskPackage -notmatch "versionName='0.1.0'"){
 throw ('Unexpected package/version: '+$taskPackage)
}
$taskNative=$taskBadging | Where-Object {$_ -match '^native-code:'}
if($taskNative.Trim() -ne "native-code: 'arm64-v8a'"){throw 'APK must be ARM64 only.'}
$taskMetadata=Get-Content -LiteralPath ([IO.Path]::ChangeExtension($taskApk,'.build.json')) -Raw | ConvertFrom-Json
if($taskMetadata.flavor -ne 'Dev' -or -not $taskMetadata.developmentBuild -or $taskMetadata.versionCode -ne 38 -or $taskMetadata.applicationId -ne 'com.gravivore.mobile.dev'){
 throw 'Build metadata differs from the DEV manifest.'
}
Add-Type -AssemblyName System.IO.Compression.FileSystem
$taskZip=[IO.Compression.ZipFile]::OpenRead($taskApk)
try{
 $taskLibraries=@($taskZip.Entries | Where-Object {$_.FullName -like 'lib/*/*.so'} | ForEach-Object {$_.FullName})
 if($taskLibraries -notcontains 'lib/arm64-v8a/libil2cpp.so'){throw 'Missing ARM64 IL2CPP runtime.'}
 if(@($taskLibraries | Where-Object {$_ -notlike 'lib/arm64-v8a/*'}).Count){throw 'Unexpected native architecture.'}
 $taskIl2cppMetadata=$taskZip.GetEntry('assets/bin/Data/Managed/Metadata/global-metadata.dat')
 if($null -eq $taskIl2cppMetadata){throw 'Missing IL2CPP type metadata.'}
 $taskStream=$taskIl2cppMetadata.Open(); $taskMemory=[IO.MemoryStream]::new()
 try{
  $taskStream.CopyTo($taskMemory)
  $taskTypes=[Text.Encoding]::UTF8.GetString($taskMemory.ToArray())
  foreach($taskType in @('EnemyCombatReadabilityPresenter','PresentationHitchDiagnostics','PlayerAttackResolvedEvent')){
   if(-not $taskTypes.Contains($taskType)){throw ('Missing runtime type in APK: '+$taskType)}
  }
 }finally{$taskStream.Dispose(); $taskMemory.Dispose()}
}finally{$taskZip.Dispose()}
$taskJava=Join-Path $taskEditor 'OpenJDK/bin/java.exe'
$taskSigner=Join-Path $taskEditor 'SDK/build-tools/36.0.0/lib/apksigner.jar'
$taskSignature=& $taskJava -jar $taskSigner verify --verbose --print-certs $taskApk
if($LASTEXITCODE -ne 0){throw 'APK signature verification failed.'}
$taskBaseline=Join-Path (Split-Path -Parent $taskRoot) 'first-visual-slice-apk-v1/Builds/Android/gravivore-dev-0.1.0+37.apk'
$taskPriorSignature=& $taskJava -jar $taskSigner verify --print-certs $taskBaseline
if($LASTEXITCODE -ne 0){throw 'Accepted APK signature verification failed.'}
$taskCertificate=@($taskSignature | Where-Object {$_ -match '^Signer #1 certificate SHA-256 digest:'})
$taskPriorCertificate=@($taskPriorSignature | Where-Object {$_ -match '^Signer #1 certificate SHA-256 digest:'})
if($taskCertificate.Count -ne 1 -or $taskPriorCertificate.Count -ne 1 -or $taskCertificate[0] -ne $taskPriorCertificate[0]){
 throw 'New APK signer differs from the accepted APK; update installation would fail.'
}
$taskVisual=Get-Content -LiteralPath ([IO.Path]::ChangeExtension($taskApk,'.visual-slice.json')) -Raw | ConvertFrom-Json
$taskCombat=Get-Content -LiteralPath ([IO.Path]::ChangeExtension($taskApk,'.post-device.json')) -Raw | ConvertFrom-Json
if(-not $taskVisual.validated -or $taskVisual.required.Count -ne 9 -or $taskVisual.modelSources.Count -ne 9 -or $taskVisual.serializedArchiveEntries.Count -ne 14){throw 'Accepted visual/music dependency checks are incomplete.'}
if(-not $taskCombat.validated -or $taskCombat.serializedEntries.Count -ne 10){throw 'New combat/settings/audio content checks are incomplete.'}
$taskFile=Get-Item -LiteralPath $taskApk
$taskHash=(Get-FileHash -LiteralPath $taskApk -Algorithm SHA256).Hash.ToLowerInvariant()
if($taskVisual.apkSha256 -ne $taskHash -or $taskCombat.apkSha256 -ne $taskHash){throw 'Dependency evidence does not match the delivered APK.'}
$taskEvidence=[ordered]@{
 apkPath=$taskApk; bytes=$taskFile.Length; mib=[Math]::Round($taskFile.Length/1MB,2); sha256=$taskHash
 applicationId=$taskMetadata.applicationId; version=$taskMetadata.version; versionCode=$taskMetadata.versionCode
 buildSourceSha=$taskMetadata.gitCommitSha; unityVersion=$taskMetadata.unityVersion; architecture='arm64-v8a'
 nativeLibraries=$taskLibraries; signatureVerified=$true; sameSignerAsAcceptedApk=$true
 certificateSha256=($taskCertificate[0] -replace '^Signer #1 certificate SHA-256 digest: ','')
 acceptedVisualDependenciesValidated=$true; postDeviceContentValidated=$true; devHitchTelemetryPacked=$true
 modelSourceDependencies=$taskVisual.modelSources; serializedApkVisualEntries=$taskVisual.serializedArchiveEntries
 serializedApkCombatEntries=$taskCombat.serializedEntries
}
New-Item -ItemType Directory -Force -Path $taskOutput | Out-Null
$taskBadging | Set-Content -LiteralPath (Join-Path $taskOutput 'apk_badging.txt') -Encoding utf8
$taskSignature | Set-Content -LiteralPath (Join-Path $taskOutput 'apk_signature.txt') -Encoding utf8
$taskEvidence | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $taskOutput 'apk_verification.json') -Encoding utf8
$taskEvidence | ConvertTo-Json -Depth 5
