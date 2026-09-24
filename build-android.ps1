[CmdletBinding()]
param(
    [string]$UnityPath,
    [switch]$Clean,
    [string]$Version
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$logDir = Join-Path $projectRoot "Builds\Logs"
$logPath = Join-Path $logDir "android-build.log"

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

New-Item -ItemType Directory -Force -Path $logDir | Out-Null
$unity = Find-Unity -ConfiguredPath $UnityPath

$unityArgs = @(
    "-batchmode",
    "-quit",
    "-projectPath", $projectRoot,
    "-buildTarget", "Android",
    "-executeMethod", "Gravivore.Editor.Build.AndroidBuild.BuildDev",
    "-logFile", $logPath
)

if ($Clean) {
    $unityArgs += "-Clean"
}

if ($Version) {
    $unityArgs += @("-Version", $Version)
}

Write-Host "Using Unity: $unity"
Write-Host "Writing build log: $logPath"

$process = Start-Process -FilePath $unity -ArgumentList $unityArgs -Wait -PassThru -NoNewWindow
if ($process.ExitCode -ne 0) {
    throw "Android build failed with exit code $($process.ExitCode). See $logPath"
}

Write-Host "Android build completed. APK output is under Builds\Android."
