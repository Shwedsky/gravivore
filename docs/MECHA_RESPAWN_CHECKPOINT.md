# G-0 mecha and adaptive respawn checkpoint

## Scope and baseline

This package starts from ART V3 `efecf5f1f7be0f95b9521ffb9f2233aa9391d83c`, including S20 spatial/camera tuning, manual gameplay fixes and the shared HP-bar fix. The primary project checkout was older PRE-S13; the final changes are isolated in `codex/mecha-adaptive-respawn`.

Exact package scope: [files changed](MECHA_RESPAWN_FILES_CHANGED.txt).

The camera remains offset (0, 14.8, -11.2), FOV 46, look-at height 0.9, damping 0.18. Spot positions, world size, movement, regeneration, player respawn, boss reset, rewards, progression thresholds (20 / 42) and save schema are retained.

## Player presentation

- Replace the low four-support chassis with a biped combat robot: beveled torso, separate abdomen/pelvis, sensor head/visor, articulated arm/leg pivots, piston shins, armored feet and cyan chest core.
- Reuse original project-authored mechanical meshes and the existing licensed CC0 PBR surfaces/materials. No new download, package or external asset.
- Keep the three existing whole-form prefab paths and runtime tier selection. Tier1 adds shoulder pauldrons and core containment; Tier2 adds heavy shoulder armor and rear vector fins.
- Retain the authoritative CharacterController: height 1.4, radius 0.42, center (0, 0.7, 0). Visual prefabs contain no physics or gameplay scripts.
- Move only the existing lash presentation socket to (0, 1.10, 0.35), near the chest core. Target sensing, attack distance, pull resolution and damage still use the player gameplay root.
- `ArtSpikeBuilder.RebuildPlayerMecha` rebuilds the player forms and rebinds them without rebuilding Cutter or the environment. The isolated articulation clip is updated for two hip pivots; there is no runtime walk/IK animation.

## Adaptive ordinary-mob respawn

Each spawn spot owns an independent session-local `AdaptiveRespawnState`.

- Preserve each authored baseline range: current S20 spots use 8–12 seconds, sampled by the existing seeded random source.
- Every fourth death on the same spot adds one penalty step. Partial chain pressure counts toward the next step.
- Each step adds 8 seconds; cap is 4 steps / +32 seconds. Delay ranges are therefore 8–12, 16–20, 24–28, 32–36, 40–44 seconds.
- Every death restarts a 60-second idle grace period. After that, each further 30 seconds without a death removes one step worth of pressure, including partial chains. First recovery is 90 seconds after the last death; complete recovery from cap takes 180 seconds.
- Count death before publishing reward events, and schedule replacement after recycle. Repeated damage on the same dead life does not add another death or reward.
- An already assigned respawn time remains fixed. Recovery changes delays for future deaths. Initial population is immediate; minimum player distance, pool reset and global live-enemy cap remain authoritative.
- Timer and pressure use scaled gameplay time. Paused gameplay does not advance recovery. Session restart resets transient pressure, consistent with existing non-persistent spawn timers; no save migration.
- Legacy callers that construct runtime configurations without an adaptive policy retain baseline behavior. All five canonical data assets explicitly enable adaptive tuning.

## Future marker contract

`IWorldMarkerSource` provides on-demand value snapshots for the player, five regular spots, elite and boss: stable id, kind, position, available/locked/respawning/defeated state, population, pending respawns, next timer and penalty step.

Elite defeat also reads restored world state, so a defeated saved elite is not shown as locked. A zero timer can still be blocked by the existing spawn distance/global cap. No minimap UI, automatic route, scene searches, subscriptions or new persisted state.

## Validation

Executed on Unity 6000.3.0f1 against the current ART/S20 baseline:

- Compilation / player prefab authoring: PASS, exit 0.
- Full EditMode: **277/277 passed**, zero failed/skipped. Includes adaptive policy edges, recovery partition independence, existing deterministic gameplay/save tests, whole-form safety and existing per-character mobile budgets.
- Full PlayMode: **61/61 passed**, zero failed/skipped. Includes canonical scene wiring, mecha tiers/authority, death/reward/pool/respawn recovery, map snapshots/restored elite defeat and the existing regen, save, HP-bar, boss-reset and progression coverage.
- Separate ProjectValidator: PASS, exit 0.
- Actual active Tier2 + four Cutters structural audit: **87 renderers, 158 material slots, 23,012 triangles**, five shared materials/four shared textures, one realtime light/no shadow lights. [Audit](art-spike/RUNTIME_PERFORMANCE.json). These are structural measurements, not draw-call/FPS measurements.
- Native Unity camera capture: all three tiers at the unchanged gameplay camera, plus front detail views. [Tier0 detail](mecha-respawn/images/mecha-Tier0-detail.png), [Tier1 detail](mecha-respawn/images/mecha-Tier1-detail.png), [Tier2 detail](mecha-respawn/images/mecha-Tier2-detail.png), [Tier2 gameplay scale](mecha-respawn/images/mecha-Tier2-gameplay.png). Captures omit overlay HUD.
- Android IL2CPP/ARM64 development APK: **PASS**, Unity exit 0. Output: `Builds/Android/gravivore-dev-0.1.0+1.apk`. Package `com.gravivore.mobile`, version 0.1.0 / code 1, min SDK 26 / target SDK 36, portrait. ZIP contains `lib/arm64-v8a/libil2cpp.so`; APK Signature Scheme v2 verifies with the existing Android Debug certificate.
- ADB device inventory: no device connected; device installation, FPS, heat, background/resume and manual acceptance were not performed.

Logs/results remain under `Builds/Logs/mecha-*`. Preliminary PRE-S13 runs are excluded from final acceptance. Existing generated import/settings changes are not included in this package.

## Manual acceptance and deferred items

- Judge the robot silhouette and Tier0/1/2 upgrades at the current camera on the phone.
- Farm one spot through 4 / 8 / 12 / 16 deaths; check the capped 40–44-second range. Visit another spot and verify it retains its own baseline/pressure. Return after 90–180 seconds without killing on the original spot.
- Smoke-check movement, gravity attacks, HP/regeneration, player respawn, mob pool reuse, save/relaunch and boss reset below/above three seconds.
- Profile steady combat and respawn on Android; desktop smoke and structural budgets do not establish 60 FPS or heat behavior.

Deferred: minimap/markers UI, gait/IK/attack animation, bespoke final model, respawn-pressure persistence, wall-clock/offline cooldown, complete S20 pacing rebalance. Final balance constants remain provisional pending device feedback.

No next specification is defined after S20 in `specs/INDEX.md`; this package is an explicit post-S20 correction, followed by device acceptance.
