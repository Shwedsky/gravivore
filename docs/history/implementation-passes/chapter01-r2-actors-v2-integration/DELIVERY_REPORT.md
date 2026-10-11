# Chapter01 R2 + Actor Production V2 delivery

Status: **FULL CHAPTER01 VISUAL CONVERGENCE CANDIDATE**.
Final visual/device acceptance belongs to the owner. No merge is authorized or performed.

## Branch and pinned sources

- Integration branch: `integration/chapter01-r2-actors-v2`.
- Draft PR: https://github.com/Shwedsky/gravivore/pull/75 (base main).
- Exact World R2 / PR #71 head: `1f8084daf1b9a42a9bd5115d4d9580833f57867b`.
- Exact Actor Production V2 / PR #74 head: `a3b5c279f48a932127a1d3cebe63069bedc7f6f5`.
- Real pre-implementation checkpoint: `14c0b2fc6364a301331c92c4566ac89295b75053`, pushed before integration work.
- Actor authority PNG SHA256: `67960c1254133c48b1de97d2dd635a5969cea49b949a7e0e8981afd34738099f`; PNG unchanged.

## What changed

All seven hostile presentations are bound in actual playable Chapter01: Scout_V2, Cutter_V2,
Warden_V2, ArcDrone_V2, Carrier_V2, Magnetar_V2 and Custodian_V2. Clean production wrappers,
original rigs/controllers/PBR maps and three LODs are promoted under
`Assets/_Game/Content/ActorProductionV2/`. Editable project-owned Blender sources are preserved
under `art/chapter01-actor-production-v2/`. The source report records zero reused donor bytes;
this pass introduces no external raw download or new third-party license requirement.

Presentation code now maps real movement/combat events to role clips, resets pooled lives,
preserves shutdown poses while authority recycles, and samples new weapon/hit/UI/death sockets.
Carrier Bank loops and turns as a vehicle. Warden retains two separate arm-controlled shields.
Magnetar Charge/Vent and Custodian phase poses follow existing authoritative timing.
Custodian basic Windup/Attack remain distinct from Telegraph and phase attacks.
Localized emission is enabled with modest actor-only material gains; authored PBR maps are retained.

No gameplay collider is changed. Bounded cosmetic wall separation preserves the source model
scale/envelope and leaves controller/sensor/ground telegraph authority unchanged. Full technical
mapping and configuration are in [ACTOR_BINDINGS](ACTOR_BINDINGS.md) and
[COLLIDER_CLEARANCE_REPORT](COLLIDER_CLEARANCE_REPORT.md).

## Preservation

`verify_preservation.py` confirms 588 protected tracked files match the pinned
World R2 source after LF normalization. These include gameplay/core/persistence/platform code,
gameplay definitions, world models/materials/lights/layout, world/map presentation, production scenes
and G-0 assets. Player/landmark sections of the shared visual catalog remain identical; only hostile
bindings and the hostile presentation profile are added. All seven FBX model bytes, source controllers,
PBR texture payloads and seven editable Blend sources match pinned Actor V2.

HP, damage, cadence, movement, aggro, rewards/loot, cooldowns, respawn, equipment, saves, quest flow,
elite/boss progression, population and minimap authority are unchanged. Relevant behavior regressions
are exercised by the complete suites. Unity import/serialization noise and unrelated historical
test captures are restored before committing. See [preservation.json](verification/preservation.json).

## Executed validation

- Explicit Unity 6000.3.0f1 compilation gate: exit 0.
- ProjectValidator: exit 0. Production actor dependency/socket validation also runs before Android packaging.
- EditModeCandidate: 510 passed, 0 failed, 0 skipped, total 510; 12,8161971 seconds. Raw [NUnit XML](verification/EditModeCandidate.xml).
- PlayModeCandidate: 148 passed, 0 failed, 1 skipped, total 149; 789,601523 seconds. Raw [NUnit XML](verification/PlayModeCandidate.xml).
  - Gravivore.Tests.PlayMode.VisualIntegrationSmokeTests.CaptureStructuralFoundationWhenRequested: Skipped; Set GRAVIVORE_VISUAL_INTEGRATION_QA to export structural review captures.
- DamageAnchorRegression: 1 passed, 0 failed, 0 skipped, total 1; 1,4232998 seconds. Raw [NUnit XML](verification/DamageAnchorRegression.xml).
- All ordinary roles: actual attack events, death/recycle, fresh animation/socket state on new life, repeat wave.
- Save/reload, equipment-derived stats, legitimate boss loot, ranking, exactly-once rewards, first-clear/repeat
  Magnetar/Custodian cooldown loops and boss HUD/reset/death are covered by existing full-scene regressions.
- Animated clearance: 766 samples, 0 penetrating LOD0 vertices;
  lowest sampled vertex 0.02426 m above the floor. Finite samples, not an exhaustive proof.
- Ordinary actor traversal: 120 route segments; original player traversal: all 24 segments.
- Real encounter events: Charge/Vent, Telegraph/Line/Circle/Cone, reset/death/exactly-once completion.
- Five-minute actual combat/traversal soak completed: 300.00 seconds,
  63 waypoints, peak 20 live enemies,
  peak 122 sampled particles. Save flush succeeds.

## Performance observations

The 240-frame playable Editor observation measured 1.116 ms mean wall-frame time
(896.2 reciprocal FPS), 1.107 ms mean recorded main-thread time,
peak 20 live enemies and peak 3 pooled VFX instances.
Inventory: 226 enabled renderer objects, 69 skinned objects,
2,409,694 triangles across enabled LOD objects, 11 material objects,
19 lights and at most 4 light bounding-sphere candidates at a skin.
These are object/frustum inventories, not submitted draw/triangle counts or measured per-pixel lights.

Editor GC recorder: 2,737,292 total bytes, 18,942 maximum per frame;
both CPU/GC recorders valid. The soak records 45 generation-0 collections and
402.60 ms maximum Editor frame. Editor/test-runner overhead is included.
This evidence establishes neither Android 60 FPS nor zero-allocation gameplay. High skin geometry,
CPU-readable source meshes, pooled corpses, local lighting and particle overdraw remain device GPU,
memory/thermal and GC profiling risks. See [final-gates](verification/final-gates.json) and per-sector cost files.

## APK

Android build/delivery pending. This implementation commit freezes tested production content before building.

## Gallery and remaining visual issues

[GALLERY](GALLERY.md) links the actual portrait production-camera captures: all eight sectors,
each actor’s normal/combat/attack/death/near-wall context, four ordinary headings, Warden Block,
Magnetar Charge/Vent and Custodian Telegraph/phase/Windup poses, plus combat/HUD regression captures.
No beauty/review camera or isolated ActorReview scene substitutes for the playable chapter.
Some controlled fixtures move an actor to a legal wall face and freeze an authored pose; actual-event
captures and the separate live soak provide complementary evidence.

The chapter now combines R2 facilities/light pools with the seven distinct V2 roles. This is a technical
integration candidate, not a declaration that it feels finished on device. Dark rear-facing mechanics
and Scout blade detail can lose contrast against the floor; warm/cool light intensity, core/emission
readability, LOD transitions and encounter scale need owner portrait/device assessment. Custodian and
Magnetar can visibly separate from their collision centre near machinery (caps 2.00 m / 1.48 m), so
attack/target feel there deserves particular attention.

Material differences from the concepts are inherited from the pinned implementations: V2 forms are
cleaner and more angular with less layered surface wear/detail than the approved illustration; R2 uses
its authored modular facility/deck interpretation. This pass preserves those forms, world composition
and G-0, rather than treating implementation assets as new visual authority. The brighter localized
energy maps improve gameplay visibility but are still a candidate tuning choice. No new redesign is claimed.

## Assumptions, files and next gate

The current source heads were deliberately pinned; later changes to source PRs are not silently inherited.
VersionCode 50 was free in the normal owner delivery directory when its 22-file baseline was captured.
G-0’s existing historical review-named dependencies remain unchanged; new Actor V2 review scaffold is excluded.
No Android device is connected, so install/startup, Android FPS/GPU/thermal and human visual acceptance
could not run. No new spec ID is defined for this integration; the next explicit task is owner device review
of the complete Chapter01 candidate. Files are listed in [changed-files.txt](verification/changed-files.txt).
