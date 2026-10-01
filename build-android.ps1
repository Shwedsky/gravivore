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
    $unityArgs += @("-Version", $Version)
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

Write-Host "APK path: $apkPath"
Write-Host "Metadata path: $metadataPath"
Write-Host "Build log: $logPath"
Write-Host "Android build completed successfully."
