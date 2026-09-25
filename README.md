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

The build script invokes `Gravivore.Editor.Build.AndroidBuild.BuildDev`, applies the project configuration, writes `Builds/Logs/android-build.log`, and outputs APKs as `Builds/Android/gravivore-dev-<version>+<versionCode>.apk`.

After a clean clone, the Editor bootstrap automatically generates the canonical URP pipeline and Universal Renderer Data assets with Unity's URP APIs before validation or tests run. `Gravivore/Configuration/Configure Project` remains available for an explicit rerun. Project validation is available at `Gravivore/Validation/Validate Project`; it is read-only and is also run after configuration before Android builds.

## S01 input, movement, and camera

The Bootstrap scene loads Chapter 01, whose composition root wires the S01 player, uGUI floating joystick, portrait follow camera, and safe-area HUD root from project-owned configuration assets under `Assets/_Game/Content/Definitions`. The HUD uses a 1080x1920 portrait reference resolution so joystick size and drag radius scale consistently across Android resolutions.

- On Android, the first valid touch in the lower gameplay area places the floating joystick; releasing it stops movement.
- In the Editor, hold and drag the left mouse button to simulate the joystick.
- Touches inside registered HUD exclusion regions do not feed movement input.
- Turn rate, joystick response, and camera follow values are configured in the S01 definition assets rather than in runtime control flow.

## S02 player stats

The five integer player stat levels and their derived values are owned by `PlayerStatsState`. Power, Hull, Armor, Flux, and Mobility curves are authored as ScriptableObject definitions. Flux is constrained by a hard minimum attack interval, and Mobility by a maximum move speed. Runtime modifier snapshots can be replaced or recalculated without changing base levels, with typed notifications for actual derived-value changes. Locomotion consumes only the derived move-speed provider and does not depend on progression concerns.

## S03 targeting and Gravity Lash

The player automatically selects hostile capability-based targets using distance/front scoring, acquisition/release radii, and sticky switching. Targeting, line of sight, and lash VFX use `ITargetable.TargetPoint`; pull geometry and movement use the independent `IDisplaceable.DisplacementRoot`. Gravity Lash cadence and raw damage come from `PlayerStatsState`; standard targets receive full safe pull, elites a configured fraction, and bosses none. Hard blockers use the `HardBlocker` layer. Each entity must have exactly one dedicated sensing collider on the `CombatTarget` layer; body and hitbox colliders must remain on other layers so the fixed non-alloc scan capacity counts entities. The placeholder lash renderer is prewarmed and reused from a fixed pool.

## Vertical Slice v0.1 success criterion

A tester can install the APK, launch without registration, understand movement with no explanation, clear five distinct enemy spots, feel permanent power growth, see the player model evolve visually at least twice, unlock an elite and a boss, dodge boss telegraphs, defeat the boss, close/reopen the app without losing progress, and receive a bounded offline reward after being away.

The slice should be fun for **30–45 minutes** without payments, ads, login or backend.
