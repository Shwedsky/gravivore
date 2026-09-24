# Project GRAVIVORE — Android Vertical Slice Specification

Status: **architecture frozen for Vertical Slice v0.1**
Target: **Android sideload APK**, portrait, single-player, anonymous local profile
Engine: **Unity 6.3 LTS + C# + URP**
Primary implementation agent: **Codex**
Repository: **GitHub**

> `GRAVIVORE` is a working codename. A public web search on 2026-09-24 did not surface a directly competing game with that exact name, but this is **not** legal/trademark clearance. The previously proposed `COREVORE` is not suitable as the public name because a separate Steam game already uses `CoreVore`.

## Product thesis

Create an original one-thumb mobile action/idle RPG inspired by the proven loop seen in games such as Butcher Hero, Devour Idle RPG, Alien Invasion and XP Hero, while avoiding their IP, assets, code and exact content.

The player controls a small escaped techno-organism with a gravitational core. It hunts autonomous machines and bio-mechanical creatures, pulls viable targets toward itself, destroys them and assimilates their cores. Every kill contributes to permanent growth. The hero visibly changes as its build develops.

Core loop:

`explore -> choose prey -> auto-engage in range -> destroy -> assimilate -> permanently grow -> unlock harder prey -> defeat elite/boss -> enter next chapter`

## Locked product decisions

- Production-ready architecture, vertical-slice scope.
- Android first.
- Portrait orientation.
- Sideloaded APK for the first test cycle.
- No required registration.
- Anonymous local profile at first launch.
- Cloud account only later for save/payment recovery.
- Original mechanics and IP; do not clone Butcher Hero one-for-one.
- Techno-organism + gravity core + assimilation theme.
- Visual evolution of the player is a core differentiator.
- Chapters + effectively unbounded stats + late prestige in future.
- Offline/AFK rewards exist but are secondary to active play.
- No auto-run, now or later.
- Auto-attack when a valid enemy is in range.
- Combat target: about 80% stat check / 20% movement skill.
- Future monetization may include one-time IAP, rewarded ads, ad-skip and time-limited/subscription-like resource benefits.
- RuStore/SBP integration is deliberately deferred from Vertical Slice v0.1.
- One vertical-slice zone has five enemy spots, each containing 3–5 concurrently active enemies.
- Only free assets are allowed for the initial project.
- GitHub repository.
- Codex is the default implementation agent.
- A Windows one-command Android build script is required.

## Read order for an implementation agent

1. `AGENTS.md`
2. `docs/DECISIONS.md`
3. `docs/PROJECT_BIBLE.md`
4. `docs/GAME_DESIGN.md`
5. `docs/ARCHITECTURE.md`
6. the requested file under `specs/`
7. `docs/TEST_STRATEGY.md`
8. `docs/BUILD_AND_RELEASE.md`

Do not implement later specifications opportunistically. Complete specs in dependency order.

## Bootstrap build commands

This repository is initialized as a Unity 6.3 LTS Android project. Install Unity with Android Build Support, Android SDK & NDK Tools, and OpenJDK.

Run the development Android build from the repository root:

```powershell
.\build-android.ps1
```

Optional arguments:

```powershell
.\build-android.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe" -Clean -Version 0.1.0
```

The build script invokes `Gravivore.Editor.Build.AndroidBuild.BuildDev`, writes `Builds/Logs/android-build.log`, and outputs APKs as `Builds/Android/gravivore-dev-<version>+<versionCode>.apk`.

Project validation is available in the Unity Editor menu at `Gravivore/Validation/Validate Project` and is also run before Android builds.

## Vertical Slice v0.1 success criterion

A tester can install the APK, launch without registration, understand movement with no explanation, clear five distinct enemy spots, feel permanent power growth, see the player model evolve visually at least twice, unlock an elite and a boss, dodge boss telegraphs, defeat the boss, close/reopen the app without losing progress, and receive a bounded offline reward after being away.

The slice should be fun for **30–45 minutes** without payments, ads, login or backend.
