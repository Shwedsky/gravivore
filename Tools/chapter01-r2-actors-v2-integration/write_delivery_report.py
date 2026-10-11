"""Write a reviewable report from executed gates and verified delivery metadata."""
import json, subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
BASE=ROOT/"docs/history/implementation-passes/chapter01-r2-actors-v2-integration"
OUT=BASE/"verification"
g=json.loads((OUT/"final-gates.json").read_text());p=json.loads((OUT/"preservation.json").read_text())
assert p["validated"]
perf=g["editorPerformance"];soak=g["fiveMinuteSoak"];c=g["clearance"]
apk_path=OUT/"apk-metadata.json";apk=json.loads(apk_path.read_text()) if apk_path.exists() else None
test_lines=[]
for label,result in g["tests"].items():
    test_lines.append(f"- {label}: {result['passed']} passed, {result['failed']} failed, {result['skipped']} skipped, total {result['total']}; {result['duration']} seconds. Raw [NUnit XML](verification/{label}.xml).")
    for case in result["nonPassingCases"]:test_lines.append(f"  - {case['name']}: {case['result']}; {case['reason']}")
apk_text=(f"- Delivered APK: `{apk['ownerApkPath']}`.\n- Version: 0.1.0, versionCode **50**; {apk['apkBytes']:,} bytes.\n"
    f"- SHA256: `{apk['apkSha256']}`.\n- Build source commit: `{apk['metadata']['gitCommitSha']}`.\n"
    "- Verified actual manifest: `com.gravivore.mobile.dev`, portrait, debuggable, ARM64 only. IL2CPP native library is present.\n"
    "- APK signature verifies and matches the Android Debug certificate of APK 49.\n"
    "- Serialized APK contents contain all seven production wrappers and all 21 actor LOD names plus World R2.\n"
    "- Build scenes: Bootstrap, then Chapter01_ScrapExclusion. Actor review scenes are absent from startup/dependencies.\n"
    f"- All {apk['previousOwnerFilesPreserved']} previous delivery files retain their baseline bytes/hash, including APK 49.\n"
    "- [Manifest](verification/apk-manifest.txt), [signer](verification/apk-signer.txt), [packing](verification/apk-actor-world-packing.json), [metadata](verification/apk-metadata.json).") if apk else "Android build/delivery pending. This implementation commit freezes tested production content before building."
text=f'''# Chapter01 R2 + Actor Production V2 delivery

Status: **FULL CHAPTER01 VISUAL CONVERGENCE CANDIDATE**.
Final visual/device acceptance belongs to the owner. No merge is authorized or performed.

## Branch and pinned sources

- Integration branch: `integration/chapter01-r2-actors-v2`.
- Draft PR: https://github.com/Shwedsky/gravivore/pull/75 (base main).
- Exact World R2 / PR #71 head: `{p['worldSource']}`.
- Exact Actor Production V2 / PR #74 head: `{p['actorSource']}`.
- Real pre-implementation checkpoint: `14c0b2fc6364a301331c92c4566ac89295b75053`, pushed before integration work.
- Actor authority PNG SHA256: `{p['actorAuthoritySha256']}`; PNG unchanged.

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

`verify_preservation.py` confirms {p['protectedFileCount']} protected tracked files match the pinned
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
{chr(10).join(test_lines)}
- All ordinary roles: actual attack events, death/recycle, fresh animation/socket state on new life, repeat wave.
- Save/reload, equipment-derived stats, legitimate boss loot, ranking, exactly-once rewards, first-clear/repeat
  Magnetar/Custodian cooldown loops and boss HUD/reset/death are covered by existing full-scene regressions.
- Animated clearance: {c['samples']} samples, {c['penetratingVertices']} penetrating LOD0 vertices;
  lowest sampled vertex {c['minimumFloorHeight']:.5f} m above the floor. Finite samples, not an exhaustive proof.
- Ordinary actor traversal: 120 route segments; original player traversal: all 24 segments.
- Real encounter events: Charge/Vent, Telegraph/Line/Circle/Cone, reset/death/exactly-once completion.
- Five-minute actual combat/traversal soak completed: {soak['seconds']:.2f} seconds,
  {soak['reachedWaypoints']} waypoints, peak {soak['maxLiveEnemies']} live enemies,
  peak {soak['maxParticles']} sampled particles. Save flush succeeds.

## Performance observations

The 240-frame playable Editor observation measured {perf['meanEditorFrameMs']:.3f} ms mean wall-frame time
({perf['editorFps']:.1f} reciprocal FPS), {perf['meanMainThreadMs']:.3f} ms mean recorded main-thread time,
peak {perf['peakEnemyPopulation']} live enemies and peak {perf['peakVfx']} pooled VFX instances.
Inventory: {perf['activeRendererObjects']} enabled renderer objects, {perf['activeSkinObjects']} skinned objects,
{perf['inventoryTriangles']:,} triangles across enabled LOD objects, {perf['materials']} material objects,
{perf['lights']} lights and at most {perf['maxPotentialLightsAtSkin']} light bounding-sphere candidates at a skin.
These are object/frustum inventories, not submitted draw/triangle counts or measured per-pixel lights.

Editor GC recorder: {perf['totalEditorGcBytes']:,} total bytes, {perf['maxEditorGcBytes']:,} maximum per frame;
both CPU/GC recorders valid. The soak records {soak['gen0Collections']} generation-0 collections and
{soak['maxEditorFrameMs']:.2f} ms maximum Editor frame. Editor/test-runner overhead is included.
This evidence establishes neither Android 60 FPS nor zero-allocation gameplay. High skin geometry,
CPU-readable source meshes, pooled corpses, local lighting and particle overdraw remain device GPU,
memory/thermal and GC profiling risks. See [final-gates](verification/final-gates.json) and per-sector cost files.

## APK

{apk_text}

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
'''
(BASE/"DELIVERY_REPORT.md").write_text(text,encoding="utf-8")
print("DELIVERY_REPORT_WRITTEN", "APK_VERIFIED" if apk else "APK_PENDING")
