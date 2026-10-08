[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$ExpectedSourceSha)
$ErrorActionPreference='Stop'
$v3Root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$v3Apk=(Resolve-Path -LiteralPath (Join-Path $v3Root 'Builds/Android/gravivore-dev-0.1.0+42.apk')).Path
$v3Output=Join-Path $v3Root 'docs/chapter01-v3/verification'
$v3Android='C:/Program Files/Unity/Hub/Editor/6000.3.0f1/Editor/Data/PlaybackEngines/AndroidPlayer'
$v3Badging=& (Join-Path $v3Android 'SDK/build-tools/36.0.0/aapt.exe') dump badging $v3Apk
if($LASTEXITCODE -ne 0){throw 'aapt failed'}
$v3Package=$v3Badging | Where-Object {$_ -match '^package:'}
if($v3Package -notmatch "name='com.gravivore.mobile.dev'" -or $v3Package -notmatch "versionCode='42'" -or $v3Package -notmatch "versionName='0.1.0'"){throw ('Unexpected APK manifest: '+$v3Package)}
if(($v3Badging | Where-Object {$_ -match '^native-code:'}).Trim() -ne "native-code: 'arm64-v8a'"){throw 'ARM64-only build required'}
if(-not ($v3Badging | Where-Object {$_ -match '^application-debuggable'})){throw 'DEV debuggable flag missing'}
$v3Metadata=Get-Content -LiteralPath ([IO.Path]::ChangeExtension($v3Apk,'.build.json')) -Raw | ConvertFrom-Json
if($v3Metadata.gitCommitSha -ne $ExpectedSourceSha -or $v3Metadata.versionCode -ne 42 -or -not $v3Metadata.developmentBuild -or -not $v3Metadata.allowDebugging){throw 'Build provenance differs from committed source'}
Add-Type -AssemblyName System.IO.Compression.FileSystem
$v3Zip=[IO.Compression.ZipFile]::OpenRead($v3Apk)
try{$v3Libraries=@($v3Zip.Entries | Where-Object {$_.FullName -like 'lib/*/*.so'} | ForEach-Object {$_.FullName})
 if($v3Libraries -notcontains 'lib/arm64-v8a/libil2cpp.so' -or @($v3Libraries | Where-Object {$_ -notlike 'lib/arm64-v8a/*'}).Count){throw 'Unexpected IL2CPP architecture'}
}finally{$v3Zip.Dispose()}
$v3Java=Join-Path $v3Android 'OpenJDK/bin/java.exe'
$v3Signer=Join-Path $v3Android 'SDK/build-tools/36.0.0/lib/apksigner.jar'
$v3Signature=& $v3Java -jar $v3Signer verify --verbose --print-certs $v3Apk
if($LASTEXITCODE -ne 0){throw 'APK signature verification failed'}
$v3Baseline=Join-Path (Split-Path -Parent $v3Root) 'chapter01-concept-fidelity-v2/Builds/Android/gravivore-dev-0.1.0+41.apk'
$v3SameSigner=$null
if(Test-Path -LiteralPath $v3Baseline){
 $v3Prior=& $v3Java -jar $v3Signer verify --print-certs $v3Baseline
 if($LASTEXITCODE -ne 0){throw 'Baseline signature failed'}
 $v3Certificate=@($v3Signature | Where-Object {$_ -match '^Signer #1 certificate SHA-256 digest:'})
 $v3PriorCertificate=@($v3Prior | Where-Object {$_ -match '^Signer #1 certificate SHA-256 digest:'})
 if($v3Certificate[0] -ne $v3PriorCertificate[0]){throw 'Signer differs from accepted v41'}
 $v3SameSigner=$true
}
$v3Hash=(Get-FileHash -LiteralPath $v3Apk -Algorithm SHA256).Hash.ToLowerInvariant()
foreach($v3Proof in @('Builds/Android/gravivore-dev-0.1.0+42.visual-slice.json','Builds/Android/gravivore-dev-0.1.0+42.post-device.json','docs/chapter01-production/verification/apk_packed_dependencies.json','docs/concept-fidelity-v2/verification/apk_packed_dependencies.json')){
 $v3ProofPath=Join-Path $v3Root $v3Proof
 $v3Packing=Get-Content -LiteralPath $v3ProofPath -Raw | ConvertFrom-Json
 if(-not $v3Packing.validated -or $v3Packing.apkSha256 -ne $v3Hash){throw ('Packing mismatch: '+$v3Proof)}
 Copy-Item -LiteralPath $v3ProofPath -Destination (Join-Path $v3Output (($v3Proof -replace '[/:]','_')))
}
$v3Python='C:/Users/pamak/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
& $v3Python (Join-Path $v3Root 'Tools/chapter01-production/inspect_runtime_content.py') $v3Apk --output (Join-Path $v3Output 'accepted_runtime_content.json')
if($LASTEXITCODE -ne 0){throw 'Accepted runtime content verification failed'}
& $v3Python (Join-Path $v3Root 'Tools/chapter01-v3/inspect_runtime_content.py') $v3Apk --output (Join-Path $v3Output 'v3_runtime_content.json')
if($LASTEXITCODE -ne 0){throw 'V3 packed weapon/runtime verification failed'}
$v3Evidence=[ordered]@{validated=$true;apkPath=$v3Apk;bytes=(Get-Item -LiteralPath $v3Apk).Length;sha256=$v3Hash;
 versionCode=42;buildSourceSha=$v3Metadata.gitCommitSha;architecture='arm64-v8a';backend='IL2CPP';developmentBuild=$true;signatureVerified=$true;sameSignerAsV41=$v3SameSigner;nativeLibraries=$v3Libraries}
$v3Badging | Set-Content -LiteralPath (Join-Path $v3Output 'apk_badging.txt') -Encoding utf8
$v3Signature | Set-Content -LiteralPath (Join-Path $v3Output 'apk_signature.txt') -Encoding utf8
Copy-Item -LiteralPath ([IO.Path]::ChangeExtension($v3Apk,'.build.json')) -Destination (Join-Path $v3Output 'build_metadata.json')
$v3Evidence | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $v3Output 'apk_verification.json') -Encoding utf8
$v3Evidence | ConvertTo-Json -Depth 8
