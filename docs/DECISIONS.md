# Frozen decisions — Vertical Slice v0.1

Date frozen: 2026-09-24.

## Product

Working codename: **GRAVIVORE**.  
Public brand name is not legally cleared and may change before store release.

Player fantasy: a small escaped techno-organism powered by an unstable gravity core. It assimilates cores/modules from enemies and visibly changes its body as it grows.

The game is inspired by the *genre/formula* of Butcher Hero / Alien Invasion / Devour / XP Hero, not by their protected IP or assets.

## Platform and engine

- Unity 6.3 LTS.
- Universal Render Pipeline.
- Android.
- Portrait-only.
- ARM64 required.
- Minimum Android API: 26 for v0.1.
- Target API for sideload build: highest supported installed SDK through Unity.
- Future store target API will be pinned to store policy at release time.
- Input: touch first; mouse simulation supported in Editor.

## Session and account

- No login at launch.
- Local anonymous profile.
- Save stored locally.
- Future account/cloud layer must be possible without rewriting gameplay.
- No backend in v0.1.

## Controls

- One-thumb movement.
- Floating joystick appears at the first valid touch in the lower gameplay area.
- No auto-run.
- No manual basic-attack button.
- Auto-target/auto-attack when a hostile target is in valid range.
- UI touches never move the player.
- Boss skill expression comes primarily from repositioning around telegraphed attacks.

## Combat

- Standard enemies may be displaced by the gravity attack.
- Elites have reduced displacement.
- Bosses cannot be pulled.
- Auto-attack pauses while no valid target exists.
- Player chooses engagements by positioning.
- Design objective: 80% progression/stat check, 20% movement skill.

## Vertical slice content

- One zone/chapter.
- Five enemy spots.
- Each spot maintains 3–5 live ordinary enemies, configurable per spot.
- Five ordinary enemy archetypes.
- One elite.
- One boss.
- Five core progression stats.
- At least two visible evolution milestones.
- 30–45 minute first-play progression.
- Death has no loss of permanent stats.

## Progression

- Chapters in the future.
- Effectively unbounded stat growth.
- Prestige/rebirth only in a later phase.
- Offline reward is secondary and capped.
- No auto-run.
- Full auto-battle is explicitly out of scope.

## Monetization

Not implemented in v0.1.

Future design allows:
- consumable/non-consumable purchases;
- rewarded ads;
- permanent ad skip;
- time-limited passes/subscription-like benefits;
- RuStore Pay SDK and SBP;
- cloud/payment recovery account.

All future payment providers are behind a platform abstraction.

## Art

Budget for initial assets: zero.
Allowed:
- original project-created geometry/materials;
- Unity built-ins;
- truly free assets whose licenses allow commercial use;
- preference for CC0.

Every imported external asset must be recorded in `ThirdPartyNotices.md`.

## Repository and delivery

- GitHub.
- Codex-oriented repository documentation.
- Windows `build-android.ps1`.
- Output APK under `Builds/Android/`.
- Dev APK may use debug signing.
- Release signing is deferred.
