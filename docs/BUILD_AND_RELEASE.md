# Build and Release — Private Android APK

## Engine

Unity 6.3 LTS.

Install via Unity Hub with:
- Android Build Support;
- Android SDK & NDK Tools;
- OpenJDK.

## Player settings

- Product name: `GRAVIVORE`
- Company name: temporary `Project Gravivore`
- Package id: `com.gravivore.mobile`
- Orientation: Portrait only
- Architecture: ARM64
- Scripting backend: IL2CPP for Android candidate builds
- Minimum API: 26
- Target API: highest installed supported target for private build
- Development Build: enabled for internal dev flavor; disabled for candidate performance build
- Internet permission: do not require unless a feature genuinely uses it
- Write permission: no broad external storage permission

## Versioning

Use:
- semantic app version, e.g. `0.1.0`;
- monotonically increasing Android version code.

APK example:
`Builds/Android/gravivore-dev-0.1.0+12.apk`

## Windows build entry point

Repository root must contain:

`build-android.ps1`

Behavior:
1. locate configured Unity executable or Unity Hub install;
2. fail with a clear message if Unity 6.3 LTS is absent;
3. invoke Unity in batch mode;
4. execute `Gravivore.Editor.AndroidBuild.BuildDev`;
5. write log to `Builds/Logs/android-build.log`;
6. place APK under `Builds/Android/`;
7. return non-zero exit code on build failure.

Allow optional:
`-UnityPath`
`-Clean`
`-Version`

Do not embed secrets/signing passwords.

## Signing

v0.1 sideload:
- debug/development signing is acceptable.

Before public release:
- create release keystore outside repository;
- secrets supplied through secure environment/CI;
- never commit keystore passwords.

## Git ignores

Ignore:
- Library/
- Temp/
- Obj/
- Logs/
- UserSettings where appropriate;
- Builds/ binaries unless explicitly attaching a release;
- local IDE files;
- signing secrets.

Commit:
- ProjectSettings;
- Packages manifests/lock;
- Assets + `.meta`;
- build/editor scripts;
- specs/docs.
