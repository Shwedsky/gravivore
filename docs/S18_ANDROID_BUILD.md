# S18 Android Builds

The Windows build runner produces private Android APKs with Unity 6000.3.x. It does not publish to a store or use a production signing key.

## Commands

Development APK (default flavor):

```powershell
.\build-android.ps1 -Flavor Dev
```

Private candidate APK:

```powershell
.\build-android.ps1 -Flavor Candidate
```

Optional arguments can be combined:

```powershell
.\build-android.ps1 -Flavor Dev -Clean -Version 0.1.1 -VersionCode 2
.\build-android.ps1 -Flavor Candidate -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe"
```

`-Clean` removes `Builds/Android` outputs only. It does not delete `Library`, `PackageCache`, or global Unity caches. `-Version` accepts three or four numeric components. `-VersionCode` must be a positive integer. Without overrides, values come from `GravivoreVersion`.

## Flavors

`Dev` uses `BuildOptions.Development | BuildOptions.AllowDebugging`; `DEVELOPMENT_BUILD` tooling, the S16 overlay, and local development telemetry are available.

`Candidate` uses `BuildOptions.None`. Development tooling is not compiled or composed and debugging is not enabled. Candidate is for private tester distribution only. It may still use Unity's default/debug signing and is not a production store release.

Both flavors run `ProjectConfigurator.ConfigureOrThrow()` and `ProjectValidator.ValidateOrThrow()` before `BuildPipeline.BuildPlayer`.

## Outputs

- DEV APK: `Builds/Android/gravivore-dev-<version>+<versionCode>.apk`
- Candidate APK: `Builds/Android/gravivore-candidate-<version>+<versionCode>.apk`
- Metadata: same basename with `.build.json`
- Logs: `Builds/Logs/android-<flavor>-<UTC timestamp>.log`

Metadata records flavor, version, versionCode, UTC timestamp, Git commit when available, Unity version, development-build state, and APK filename.

## Prerequisites

- Unity 6000.3.x
- Android Build Support installed from Unity Hub
- Unity Android SDK, NDK, and OpenJDK modules

The runner exits non-zero for missing Unity, invalid arguments, configuration/validation failure, Unity build failure, missing artifact markers, missing APKs, or missing metadata.
