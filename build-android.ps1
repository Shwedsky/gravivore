[CmdletBinding()]
param(
    [string]$UnityPath,
    [switch]$Clean,
    [ValidateSet("Dev", "Candidate")]
    [string]$Flavor = "Dev",
    [string]$Version,
    [int]$VersionCode
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$logDir = Join-Path $projectRoot "Builds\Logs"
$timestamp = [DateTime]::UtcNow.ToString("yyyyMMdd-HHmmss")
$flavorLabel = $Flavor.ToLowerInvariant()
$logPath = Join-Path $logDir "android-$flavorLabel-$timestamp.log"

if ($Version -and $Version -notmatch '^\d+\.\d+\.\d+(?:\.\d+)?$') {
    throw "Version must contain three or four numeric components, for example 0.1.0."
}

if ($PSBoundParameters.ContainsKey("VersionCode") -and $VersionCode -le 0) {
    throw "VersionCode must be a positive integer."
}

if ($Flavor -eq "Candidate" -and -not $PSBoundParameters.ContainsKey("VersionCode")) {
    throw "Candidate builds require an explicit -VersionCode. Choose a value greater than every previously distributed Candidate/production build."
}

if ($Flavor -eq "Candidate" -and $PSBoundParameters.ContainsKey("VersionCode")) {
    $existingMetadata = Get-ChildItem -LiteralPath (Join-Path $projectRoot "Builds\Android") -Filter "*.build.json" -File -ErrorAction SilentlyContinue
    $priorCandidateCodes = @()
    foreach ($metadataFile in $existingMetadata) {
        try {
            $priorMetadata = Get-Content -LiteralPath $metadataFile.FullName -Raw | ConvertFrom-Json
            if ($priorMetadata.flavor -eq "Candidate" -and $null -ne $priorMetadata.versionCode) {
                $priorCandidateCodes += [int]$priorMetadata.versionCode
            }
        }
        catch {
            Write-Warning "Ignoring unreadable prior build metadata: $($metadataFile.FullName)"
        }
    }

    if ($priorCandidateCodes.Count -gt 0) {
        $highestPriorCode = ($priorCandidateCodes | Measure-Object -Maximum).Maximum
        if ($VersionCode -le $highestPriorCode) {
            throw "Candidate VersionCode $VersionCode is not monotonic. Highest local Candidate versionCode is $highestPriorCode."
        }
    }
}

function Find-Unity {
    param([string]$ConfiguredPath)

    if ($ConfiguredPath) {
        if (Test-Path -LiteralPath $ConfiguredPath) {
            return (Resolve-Path -LiteralPath $ConfiguredPath).Path
        }

        throw "Configured UnityPath does not exist: $ConfiguredPath"
    }

    $candidates = @(
        "C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe",
        "C:\Program Files\Unity\Hub\Editor\6000.3.1f1\Editor\Unity.exe",
        "C:\Program Files\Unity\Hub\Editor\6000.3.2f1\Editor\Unity.exe"
    )

    $hubEditorRoot = "C:\Program Files\Unity\Hub\Editor"
    if (Test-Path -LiteralPath $hubEditorRoot) {
        $hubCandidates = Get-ChildItem -LiteralPath $hubEditorRoot -Directory |
            Where-Object { $_.Name -like "6000.3.*" } |
            Sort-Object Name -Descending |
            ForEach-Object { Join-Path $_.FullName "Editor\Unity.exe" }
        $candidates = @($hubCandidates) + $candidates
    }

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate) {
            return $candidate
        }
    }

    throw "Unity 6.3 LTS was not found. Install Unity 6.3 with Android Build Support, or pass -UnityPath."
}

function ConvertTo-ProcessArgument {
    param([AllowEmptyString()][string]$Value)

    if ($Value.Contains('"')) {
        throw "Unity arguments cannot contain double quotes: $Value"
    }

    if ($Value.Length -eq 0 -or $Value -match '\s') {
        $escapedValue = [regex]::Replace(
            $Value,
            '\\+$',
            { param($match) $match.Value + $match.Value })
        return '"' + $escapedValue + '"'
    }

    return $Value
}

New-Item -ItemType Directory -Force -Path $logDir | Out-Null
$unity = Find-Unity -ConfiguredPath $UnityPath
$executeMethod = if ($Flavor -eq "Candidate") {
    "Gravivore.Editor.Build.AndroidBuild.BuildCandidate"
}
else {
    "Gravivore.Editor.Build.AndroidBuild.BuildDev"
}

$unityArgs = @(
    "-batchmode",
    "-quit",
    "-projectPath", $projectRoot,
    "-buildTarget", "Android",
    "-executeMethod", $executeMethod,
    "-logFile", $logPath
)

if ($Clean) {
    $unityArgs += "-Clean"
}

if ($Version) {
    # Unity reserves -version and exits before executing the build method.
    $unityArgs += @("-GravivoreVersion", $Version)
}

if ($PSBoundParameters.ContainsKey("VersionCode")) {
    $unityArgs += @("-VersionCode", $VersionCode.ToString([Globalization.CultureInfo]::InvariantCulture))
}

Write-Host "Using Unity: $unity"
Write-Host "Project root: $projectRoot"
Write-Host "Build flavor: $Flavor"
Write-Host "Writing build log: $logPath"

$processArguments = @($unityArgs | ForEach-Object { ConvertTo-ProcessArgument $_ })
try {
    $unityProcess = Start-Process `
        -FilePath $unity `
        -ArgumentList $processArguments `
        -WorkingDirectory $projectRoot `
        -WindowStyle Hidden `
        -Wait `
        -PassThru `
        -ErrorAction Stop
}
catch {
    throw "Unable to start Unity process '$unity': $($_.Exception.Message)"
}

$unityExitCode = $unityProcess.ExitCode
Write-Host "Unity process exit code: $unityExitCode"
if ($unityExitCode -ne 0) {
    if (Test-Path -LiteralPath $logPath -PathType Leaf) {
        Write-Host "Relevant build log tail:"
        Select-String -LiteralPath $logPath -Pattern 'error|exception|failed' -CaseSensitive:$false |
            Select-Object -Last 20 |
            ForEach-Object { Write-Host $_.Line }
    }
    throw "Android build failed with exit code $unityExitCode. See $logPath"
}

if (-not (Test-Path -LiteralPath $logPath -PathType Leaf)) {
    throw "Unity exited with code 0, but the build log does not exist: $logPath"
}

$artifactLogEntry = Select-String `
    -LiteralPath $logPath `
    -Pattern 'Android (?:Dev|Candidate) build written to (?<path>.+\.apk)\s*$' |
    Select-Object -Last 1
if ($null -eq $artifactLogEntry) {
    throw "Unity exited with code 0, but $executeMethod did not report an APK path in $logPath."
}

$reportedApkPath = $artifactLogEntry.Matches[0].Groups['path'].Value.Trim()
$apkPath = if ([IO.Path]::IsPathRooted($reportedApkPath)) {
    $reportedApkPath
}
else {
    Join-Path $projectRoot $reportedApkPath
}

if (-not (Test-Path -LiteralPath $apkPath -PathType Leaf)) {
    throw "Unity exited with code 0, but the expected APK does not exist: $apkPath"
}

$apkPath = (Resolve-Path -LiteralPath $apkPath).Path
$metadataPath = [IO.Path]::ChangeExtension($apkPath, ".build.json")
if (-not (Test-Path -LiteralPath $metadataPath -PathType Leaf)) {
    throw "Unity exited with code 0 and produced an APK, but build metadata is missing: $metadataPath"
}

$metadata = Get-Content -LiteralPath $metadataPath -Raw | ConvertFrom-Json
$expectedApplicationId = if ($Flavor -eq "Candidate") { "com.gravivore.mobile" } else { "com.gravivore.mobile.dev" }
if ($metadata.flavor -ne $Flavor) {
    throw "Build metadata flavor mismatch. Expected $Flavor, got $($metadata.flavor)."
}
if ($metadata.applicationId -ne $expectedApplicationId) {
    throw "Build metadata applicationId mismatch. Expected $expectedApplicationId, got $($metadata.applicationId)."
}
if ($PSBoundParameters.ContainsKey("VersionCode") -and [int]$metadata.versionCode -ne $VersionCode) {
    throw "Build metadata versionCode mismatch. Expected $VersionCode, got $($metadata.versionCode)."
}
if ($metadata.targetArchitecture -ne "ARM64") {
    throw "Build metadata architecture mismatch. Expected ARM64, got $($metadata.targetArchitecture)."
}
if ($Flavor -eq "Candidate" -and ($metadata.developmentBuild -or $metadata.allowDebugging)) {
    throw "Candidate metadata indicates development/debugging options."
}
if ($Flavor -eq "Dev" -and (-not $metadata.developmentBuild -or -not $metadata.allowDebugging)) {
    throw "Dev metadata must indicate Development Build and AllowDebugging."
}

$editorDir = Split-Path -Parent $unity
$sdkBuildTools = Join-Path $editorDir "Data\PlaybackEngines\AndroidPlayer\SDK\build-tools"
$aapt = $null
if (Test-Path -LiteralPath $sdkBuildTools) {
    $aapt = Get-ChildItem -LiteralPath $sdkBuildTools -Filter "aapt.exe" -File -Recurse -ErrorAction SilentlyContinue |
        Sort-Object FullName -Descending |
        Select-Object -First 1
}

if ($null -ne $aapt) {
    $badging = & $aapt.FullName dump badging $apkPath 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "aapt failed to inspect APK badging: $($badging -join [Environment]::NewLine)"
    }

    $packageLine = $badging | Where-Object { $_ -match "^package:" } | Select-Object -First 1
    $applicationIdMatch = [regex]::Match([string]$packageLine, "name='(?<id>[^']+)'")
    if (-not $applicationIdMatch.Success) {
        throw "aapt did not report an applicationId."
    }
    $actualApplicationId = $applicationIdMatch.Groups['id'].Value
    if ($actualApplicationId -ne $expectedApplicationId) {
        throw "APK applicationId mismatch. Expected $expectedApplicationId, got $actualApplicationId."
    }

    $versionCodeMatch = [regex]::Match([string]$packageLine, "versionCode='(?<code>\d+)'")
    if (-not $versionCodeMatch.Success) {
        throw "aapt did not report APK versionCode."
    }
    $actualVersionCode = [int]$versionCodeMatch.Groups['code'].Value
    if ($actualVersionCode -ne [int]$metadata.versionCode) {
        throw "APK versionCode does not match build metadata."
    }

    $nativeCodeLine = $badging | Where-Object { $_ -match "^native-code:" } | Select-Object -First 1
    if ($null -eq $nativeCodeLine -or $nativeCodeLine -notmatch "'arm64-v8a'" -or $nativeCodeLine -match "'(?:armeabi-v7a|x86|x86_64)'") {
        throw "APK native-code is not ARM64-only: $nativeCodeLine"
    }

    if ($Flavor -eq "Candidate") {
        $manifestTree = & $aapt.FullName dump xmltree $apkPath AndroidManifest.xml 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "aapt failed to inspect AndroidManifest.xml."
        }

        $debuggableLine = $manifestTree | Where-Object { $_ -match "android:debuggable" } | Select-Object -First 1
        if ($null -ne $debuggableLine -and $debuggableLine -notmatch "0x0(?:\s|$)") {
            throw "Candidate manifest is debuggable: $debuggableLine"
        }
    }

    $permissionDump = & $aapt.FullName dump permissions $apkPath 2>&1
    if ($LASTEXITCODE -eq 0) {
        $hasInternetPermission = [bool]($permissionDump | Where-Object { $_ -match "android\.permission\.INTERNET" })
        Write-Host "Manifest INTERNET permission present: $hasInternetPermission"
    }

    Write-Host "APK manifest/package verification: passed via $($aapt.FullName)"
}
else {
    Write-Warning "aapt.exe was not found in the Unity Android SDK. Build intent/metadata checks passed, but manifest-level package/debuggable verification was skipped."
}

Write-Host "APK path: $apkPath"
Write-Host "Metadata path: $metadataPath"
Write-Host "Build log: $logPath"
Write-Host "Verified applicationId: $expectedApplicationId"
Write-Host "Verified versionCode: $($metadata.versionCode)"
Write-Host "Android build completed successfully."
