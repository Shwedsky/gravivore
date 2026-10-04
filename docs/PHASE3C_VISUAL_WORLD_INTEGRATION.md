# Phase 3C — full visual world integration

Status: **PHASE 3C VISUAL WORLD INTEGRATION: DEVICE REVIEW PENDING**

## Base and scope

Merged foundation #38: `dd7392ff92755d29ee424bf3c8e9b61bdd4ada84`.
Current integration base / origin/main: `fdd57366eefd5958a083e32e256d262733d98cda`.
Verified #39 head: `2ea9ac9e80f94fb5efffa6570b8f6e01aa032327`.
At intake, its merge-base with then-current main was exactly the foundation merge SHA: two commits ahead, zero behind.
Integration branch: `codex/phase3c-visual-world-integration`, created directly from that head.
#39 was neither merged nor modified remotely; #37 was not used.

During Android verification, main advanced by five commits that add four future Chapter1 repeatable-loop design documents. The unpublished integration branch was rebased onto that main without file conflicts. `Assets`, `Packages`, `ProjectSettings` and `build-android.ps1` are byte-identical to the fully tested pre-sync tree; the new main documents are preserved and no loop implementation was added. Android was rebuilt from the rebased branch to record a reachable source commit.

This is the first integrated Chapter01 review build, not final visual acceptance. The chapter now has distinct machinery, a repair bay, travel infrastructure, an elite and a boss. The simple modular geometry, static actors and sparse transitions still prevent external visual acceptance. Device review and asset replacement remain real gates.

## Intake before binding

Phase3A `ArtIntakeTool.Inspect` ran on all 16 pack prefabs plus the accepted Cutter before runtime bindings changed. The first Unity import exposed rounded signed 64-bit FBX mesh fileIDs, despite the pack's static GUID audit: 53 references did not resolve to meshes.

Each repair retains the same committed GUID and resolves to a unique AssetDatabase mesh ID within 4096 of the rounded value. Ambiguous/missing matches throw; there is no geometry substitution. Repairs exist only on this integration branch. The original failed intake is preserved in [ART_INTAKE_BEFORE_REPAIR.json](phase3c/ART_INTAKE_BEFORE_REPAIR.json), and every old/new ID is recorded in [MESH_REFERENCE_REPAIRS.txt](phase3c/MESH_REFERENCE_REPAIRS.txt).

Final intake: **17/17 safe**, zero missing meshes/materials/scripts, zero broken texture references, zero incompatible shaders, zero colliders/rigidbodies/gameplay scripts/lights/cameras/playback components. All roots use finite positive unit scale. All meshes are static; there are no rigs/animation clips/LODs. Unsupported components: none. Optional unassigned shader texture properties are recorded separately from broken references.

Full per-candidate bounds (center/size), source paths, component counts, texture names/resolutions and warnings: [ART_INTAKE.json](phase3c/ART_INTAKE.json).
All four sampled material textures are existing 1024×1024 BlueMetalPlate diffuse/normal/metal-smoothness/occlusion assets. No new external asset, package or license was introduced. Provenance remains in [ThirdPartyNotices.md](../ThirdPartyNotices.md).

- **ScoutDrone_Phase3B:** 2,364 triangles; 6 renderers; 7 slots / 4 materials; 4 textures.
- **Cutter_ArtSpike:** 3,352 triangles; 13 renderers; 25 slots / 3 materials; 4 textures.
- **ArcDrone_Phase3B:** 2,312 triangles; 9 renderers; 10 slots / 5 materials; 4 textures.
- **Warden_Phase3B:** 3,240 triangles; 12 renderers; 13 slots / 5 materials; 4 textures.
- **Carrier_Phase3B:** 2,460 triangles; 9 renderers; 10 slots / 5 materials; 4 textures.
- **MagnetarGuard_Phase3B:** 4,168 triangles; 15 renderers; 16 slots / 6 materials; 4 textures.
- **CustodianM0_Phase3B:** 5,968 triangles; 23 renderers; 24 slots / 6 materials; 4 textures.
- **RepairHub_Phase3B:** 5,256 triangles; 16 renderers; 16 slots / 5 materials; 4 textures.
- **RelayYard_Phase3B:** 2,284 triangles; 9 renderers; 9 slots / 4 materials; 4 textures.
- **CuttingFloor_Phase3B:** 4,268 triangles; 10 renderers; 10 slots / 5 materials; 4 textures.
- **ShieldDump_Phase3B:** 2,096 triangles; 7 renderers; 7 slots / 4 materials; 4 textures.
- **CapacitorField_Phase3B:** 3,812 triangles; 13 renderers; 13 slots / 5 materials; 4 textures.
- **HaulerGraveyard_Phase3B:** 2,902 triangles; 8 renderers; 8 slots / 5 materials; 4 textures.
- **EliteApproach_Phase3B:** 3,652 triangles; 6 renderers; 6 slots / 3 materials; 4 textures.
- **EliteArena_Phase3B:** 1,048 triangles; 6 renderers; 6 slots / 3 materials; 4 textures.
- **BossApproach_Phase3B:** 3,468 triangles; 6 renderers; 6 slots / 3 materials; 4 textures.
- **BossArena_Phase3B:** 2,444 triangles; 8 renderers; 8 slots / 4 materials; 4 textures.

Arc, Warden and Carrier each exceed the ordinary four-material soft guardrail by one. No triangle/renderer guardrail is exceeded. Warnings do not reject candidates.

## Actor integration and offsets

Ordinary whole-prefab bindings live in `S15_VisualCatalog.asset`. Legacy parts remain as authored fallback data; accepted Cutter content is untouched. All rotations remain identity, offsets are local to visual roots, and authority roots retain unit scale.

- `scout-drone` → `ScoutDrone_Phase3B`; position (0,0,0), uniform scale .70. Its existing art-space hover clearance remains.
- `cutter-unit` → `Cutter_ArtSpike`; existing position (0,0,0), scale 1.
- `arc-drone` → `ArcDrone_Phase3B`; position (0,.014,0), scale .70.
- `warden` → `Warden_Phase3B`; position (0,.057,0), scale .70.
- `carrier` → `Carrier_Phase3B`; position (0,.039,0), scale .65.
- Chapter01 `_elite` → `MagnetarGuard_Phase3B`; position (0,.05,0), scale .80.
- Chapter01 `_boss` → `CustodianM0_Phase3B`; position (0,.085,0), scale 1.

Small vertical offsets place support geometry near ground. No mesh-derived gameplay radius/height is introduced. G-0's three forms, gait, attack cadence/charge/release, VFX/SFX and player capsule (.42 radius / 1.4 height) stay unchanged.

## Environment and camera composition

Nine validated `EnvironmentDressingBinding` entries attach to identity `Phase3C Dressing` anchors under matching existing regions; the tenth group (hub) uses the dedicated definition binding. All 11 foundation region roots remain, including `start-region`.

Relay Yard, Cutting Floor, Shield Dump and Hauler Graveyard dressing offsets are (0,-.05,3.5); Capacitor Field is (0,-.05,4.5). Elite/Boss approach and arena offsets are zero. All dressing scales/rotations are identity. These offsets affect art only; region roots, landmark anchors, mob spawns, encounter centers and gates do not move.

Two camera composition passes corrected imported mesh pivot/scale assumptions. FBX prop scales were reduced proportionally where their intrinsic largest dimension exceeded 2 m, mesh centers aligned with authored prop centers, and erroneous vertical crossbeam orientations corrected. This reduced, for example, the repair hub height from 20.8 m to 5.8 m and Cutting Floor from 15.6 m to 5.45 m. Exact per-node before/after transforms are in [COMPOSITION_OFFSETS.txt](phase3c/COMPOSITION_OFFSETS.txt) and [FINAL_COMPOSITION_OFFSETS.txt](phase3c/FINAL_COMPOSITION_OFFSETS.txt).

The center capacitor pair was moved to the outer bank. Large frames use existing worn-metal surfaces instead of broad hostile neon. Decks sit above the visual floor and use the shared textured `Phase3C_WorkDeck` material; boss deck depth stops at the north boundary. Open centers and foreground approach routes remain available. Props have no collision, so visual/authority overlap must still be judged on device.

A four-vertex textured floor covers the existing 72×140 m bounds. Shared floor panels connect travel routes, with peripheral power trunks and service cabinets at x ±8/±10 between z -18 and 46. These remove some empty-plane exposure without placing physical obstacles in combat or travel paths. All added meshes/materials are presentation-only and reuse audited content.

`ChapterVisualEnvironment._showFallbackEnvironment` is false in Chapter01; the complete prototype layer remains available through that serialized toggle. Old S15 landmark visual anchors remain present but inactive where the new groups cover them. Their fallback data is retained. Gate/perimeter visuals are parented under `Structures` so hiding the covered prototype floor cannot make physical constraints invisible. Gameplay colliders/gate ownership are unchanged.

Lighting uses the existing single shadow-free directional light at intensity 1.6 and a neutral ambient probe. The project-authored studio reflection is copied into a separate runtime content asset, `Phase3C_IndustrialReflection`; the isolated art-review scene/scenery/lighting asset is still excluded from runtime dependencies. No extra realtime lights or postprocessing stack were added.

Acceptance camera is unchanged: offset (0,14.8,-11.2), look-at .9, FOV 46, portrait 9:16.

## Repair hub

`Chapter01_VisualIntegration._repairHub` → `RepairHub_Phase3B` under `repair-hub/MainPlatform`, local position (0,-.2,0), rotation identity, scale 1. The art `ServicePoint` at y .2 therefore matches the unchanged ground-level `PlayerDockPoint` / spawn.

Foundation presentation anchors align with the corresponding model references:

- ManipulatorLeft/Right → ManipulatorMount_L/R.
- RearManipulatorA/B → RearManipulatorMount_A/B.
- RepairBeamOriginLeft/Right → BeamEmitter_L/R.
- RepairBeamOriginRearA/B → BeamEmitter_RearA/B.
- AmbientFxRoot → AmbientFxVisualRoot.

Runtime tests compare all these world positions. Manipulators stay static. There are no new beams, repair animation, player docking, regen triggers or service authority.

## Runtime socket verification

All seven enemy bindings resolve cached sockets through `CharacterVisualBinding` / `PresentationSocketSet`; present transforms belong to the active model, and absent roles resolve the supplied existing authority TargetPoint/root fallback. Pool reuse clears old binding/socket state and switches from Scout's authored Sensor to Carrier's fallback without exposing the inactive old-life socket.

- Scout: Core, Sensor, AttackOrigin, HitCenter, GroundContact, VfxTop; VfxRear/TelegraphOrigin fallback.
- Arc/Warden/Carrier: Core, AttackOrigin, HitCenter, GroundContact, VfxTop; Sensor/VfxRear/TelegraphOrigin fallback.
- Cutter: existing safe fallback for all eight optional roles; no duplicate/wrapper model was needed.
- Elite/Boss: Core, AttackOrigin, HitCenter, GroundContact, VfxTop, VfxRear, TelegraphOrigin; Sensor fallback.
- Root always safely resolves to model root when omitted. WeaponLeft/Right remain optional.

Boss ConeAttackOrigin, LineAttackOrigin and CircleAttackOrigin remain model transforms only. No gameplay attack origin/dimension or authoritative telegraph event is changed. Cached sockets do not take ownership of existing Phase2 effects. Exact runtime authored/fallback lists are in [RUNTIME_REVIEW.json](phase3c/RUNTIME_REVIEW.json).

## Real Unity capture index

All files were rendered by `Camera.Render` in the integrated Chapter01 PlayMode scene, then converted losslessly from Unity RGB24 PPM to PNG. No paintover, external concept image, exposure edit, crop or asset preview render substitutes for gameplay views. Fixture uses a temporary isolated profile and freezes AI/movement for reproducible composition review; captures are not evidence of device FPS or animated art quality.

01–10, 11 and 15 use the settled current gameplay camera relationship. 11 temporarily stages five existing ordinary family representatives near Capacitor Field and commits an attack through `GravityAttackController`; authored spawn data remains unchanged. 12 is a supplementary wide view. 13 stages all five ordinary families plus elite/boss in the actual Chapter scene, using an explicitly supplementary overview camera. 14 is supplementary boss detail. Snapshot staging exists only in the test fixture and is restored/cleaned up.

1. [01_RepairHub.png](phase3c/screenshots/01_RepairHub.png)
2. [02_CapacitorField.png](phase3c/screenshots/02_CapacitorField.png)
3. [03_HaulerGraveyard.png](phase3c/screenshots/03_HaulerGraveyard.png)
4. [04_RelayYard.png](phase3c/screenshots/04_RelayYard.png)
5. [05_ShieldDump.png](phase3c/screenshots/05_ShieldDump.png)
6. [06_CuttingFloor.png](phase3c/screenshots/06_CuttingFloor.png)
7. [07_EliteApproach.png](phase3c/screenshots/07_EliteApproach.png)
8. [08_MagnetarGuard.png](phase3c/screenshots/08_MagnetarGuard.png)
9. [09_BossApproach.png](phase3c/screenshots/09_BossApproach.png)
10. [10_CustodianM0_Arena.png](phase3c/screenshots/10_CustodianM0_Arena.png)
11. [11_OrdinaryCombat_MultipleFamilies.png](phase3c/screenshots/11_OrdinaryCombat_MultipleFamilies.png)
12. [12_WideChapterTraversal.png](phase3c/screenshots/12_WideChapterTraversal.png)
13. [13_AllEnemyFamilies_Review.png](phase3c/screenshots/13_AllEnemyFamilies_Review.png)
14. [14_Boss_CloseReview.png](phase3c/screenshots/14_Boss_CloseReview.png)
15. [15_Player_In_NewWorld.png](phase3c/screenshots/15_Player_In_NewWorld.png)

## Integrated visual grades

A = acceptable for prototype external test; B = temporary, needs polish/replacement; C = replace before external test. These are independent real-camera assessments, not subjective automated assertions.

- **Cutter — A:** accepted compact asymmetric cutter silhouette; readable against the new floor. Still temporary art, with socket fallback.
- **Scout — B:** hovering sensor silhouette differs from ground robots; small details disappear and the shell remains simple.
- **Arc Drone — B:** twin-prong electrical identity reads; repeated glowing ring kitbash still feels toy-like.
- **Warden — B:** broad armored quadruped reads defensive; plates repeat the G-0 surface language too closely.
- **Carrier — C:** cargo blocks with legs read like a stock logistics toy, not a convincing combat hauler.
- **Magnetar Guard — B:** increased mass/paired generators establish elite hierarchy; static pose, generic rings and repeated metal require a hero pass.
- **Custodian M-0 — B:** substantially larger asymmetric reactor/crane mass gives boss presence in the unchanged camera; ring outline, static supports and blunt surfaces remain replaceable. Wide silhouette exceeds authority capsule.
- **Repair Hub — B:** four service directions and a semi-hangar read immediately; simple bright emitter blocks and static manipulators remain obvious proxies.
- **Relay Yard — B:** mast/cabinets establish a recognizable communications spot; stock modules and thin mast need refinement.
- **Cutting Floor — B:** corrected gantry and paired hanging blades explain function; clean repeated frame geometry still exposes kitbash construction.
- **Capacitor Field — C (downgraded from B):** bank layout is readable and combat is open, but repeated large shiny rings dominate and feel like a toy obstacle array.
- **Shield Dump — C:** large clean rings and generic generators remain abstract; the cyan ring competes with player diagnostics.
- **Hauler Graveyard — C:** differentiated low masses are useful structurally; they still look like boxes with arcs rather than broken hauler wrecks.
- **Elite Approach — B:** framed narrowing/power infrastructure signals escalation; insufficient damaged industrial vocabulary.
- **Elite Arena — C:** combat center is readable, but sparse straight perimeter pieces do not give a premium elite destination.
- **Boss Approach — B:** taller worn framing escalates from ordinary regions; needs bespoke damage/superstructure.
- **Boss Arena — B:** asymmetrical crane and open center support the boss hierarchy; broad repeated deck and primitive-looking perimeter remain weak.

Current G-0 remains a context-review prototype, not a new accepted character: mechanical gait is preserved, but its small blocky form is still visible beside richer enemies. Added floor/routes are temporary B surfaces; chapter-wide density and legacy bright gate bars remain C presentation risks. **The entire chapter is not external-test visually accepted while these C items remain.**

## Performance

See [RUNTIME_REVIEW.json](phase3c/RUNTIME_REVIEW.json) for final runtime samples. Estimates use active/enabled renderers whose world AABBs intersect the camera frustum. Mesh triangles count index topology, not occlusion-correct GPU submissions; LineRenderer beam/pulse geometry is included in renderer counts but excluded from mesh triangle totals. No FPS, draw-call count or device timing is inferred from these estimates.

- Ordinary Relay Yard view with all **20 ordinary enemies** alive across five spots: **17,158 estimated visible mesh triangles; 69 renderers; 90 slots / 9 unique materials**.
- Mixed-family attack view: **30,610 triangles; 127 renderers; 163 slots / 12 materials**. Five representatives are staged for comparison in addition to the other live spot population; this is a denser review sample, not a new encounter layout.
- Boss encounter view: **13,814 triangles; 63 renderers; 81 slots / 10 materials**. Boss model itself: **5,968 triangles / 23 renderers**, included in this view.
- Active environment: **150 renderers** (covered prototype/old landmark renderers excluded).
- Active ParticleSystems: **0**. Active/enabled AudioSources: **4**; two were playing in the sampled mixed-family attack, zero in frozen static views.

Renderer/material-slot cost is the more relevant warning than triangle count for this kitbash. There is no claim of mobile batching efficiency or 60 FPS. Sustained device measurement remains pending.

**Pressure assessment: materially increased art inventory versus the sparse foundation/prototype world.** The integrated environment has 150 active renderers, and the boss model adds 23 renderers / 24 slots / 6 unique materials for 5,968 triangles. The scene snapshots above include the world and actors together. No matched before/after camera benchmark was taken, so these figures establish current pressure, not an exact delta or draw-call count. Shared materials do not by themselves prove batching. Device review must measure CPU/GPU frame time and sustained FPS in ordinary combat and the boss encounter before this art load is accepted.

## Regression verification

Unity 6000.3.0f1. Compile and ProjectValidator executed successfully. Full EditMode: **318/318** (baseline 312 + six binding/intake cases). Full PlayMode: **78/78** (baseline 75 + three integration tests). Zero failed/skipped tests, including all capture fixtures. No subjective quality tests were added.

New runtime tests cover actual canonical bindings, safe socket fallbacks, hub alignment, collision authority dimensions, visible locked gates with prototype visuals hidden, and pooled Scout→Carrier model/life/socket reuse. Existing full suites retain combat timing/cadence, observer-failure safety, first attack, target loss, recycled lives, save/progression, regen, offline return, respawn, gates and encounter reset checks.

Legacy tests were adapted only where they asserted an empty foundation or the pre-integration world: new empty-definition fallback safety remains independently tested; legitimate Phase3B runtime dependencies are allowed while comparison-bay scenery/lighting remain forbidden; Cutter and S15 fallback data checks stay intact. Review scenes remain absent from build scenes. No gameplay/Core/Persistence/Platform code, balance/config, camera setting, player model or save schema changed.

## External audit compatibility

- **Changes save schema: NO.** `SaveSchema.CurrentVersion` remains 1; DTOs, restore validation, migrations and save/offline configuration are unchanged. Presentation mesh repairs/bindings do not rename or remove persisted gameplay content IDs.
- **Changes applicationId/build flavor behavior: NO.** `AndroidBuild`, `GravivoreVersion`, `build-android.ps1` and committed PlayerSettings are unchanged. Existing Dev/Candidate applicationId and persistentDataPath sharing remains; no release isolation is implemented here.
- **Changes encounter persistence: NO.** Elite/boss gameplay, one-time completion state, rewards and save contracts are unchanged. No repeatable encounters or save schema v2 implementation was added.
- **Materially increases renderer/material pressure: YES.** The integrated content adds substantial renderable inventory; ordinary, mixed-family and boss pressure is reported above. Exact comparative GPU cost and device FPS remain unmeasured.
- **Creates new conflicts with MJ-1/MJ-2/MJ-3: none identified.** The diff contains no changes to the relevant persistence/build/encounter implementations, and full regression suites passed. MJ-1 strict restore and MJ-2 shared storage remain inherited deferred findings. MJ-3 is addressed at design level by merged [Phase4 specification #40](https://github.com/Shwedsky/gravivore/pull/40); its design documents are preserved unchanged and implementation remains outside Phase3C.

This audit addendum updates reporting only. Existing compile, full EditMode/PlayMode, ProjectValidator and Android results still apply to identical runtime/assets/build files; they were not rerun for this documentation-only addition. The APK, checksum and captures remain unchanged. No Phase3C correctness regression was identified that requires persistence/release hardening.

## Android and device review

**Passed:** existing `build-android.ps1 -Flavor Dev` pipeline, Unity exit 0 / successful build report. DEV + debugging enabled, ARM64 only, IL2CPP. APK ZIP inspection confirms `lib/arm64-v8a/libil2cpp.so` and no other ABI directory.

- APK: `C:\Users\pamak\Documents\ChatGPT\gravivore\.codex-worktrees\phase3c-visual-world-integration\Builds\Android\gravivore-dev-0.1.0+1.apk`.
- Size: **58,325,390 bytes** (55.62 MiB).
- SHA256: `0565232bc56754fc0bb82883240bc7db8e3bccc0bc90f7863f7b29b6165ae3a0` (computed with `Get-FileHash`; the existing pipeline supplies metadata, not a checksum).
- Build metadata source HEAD: `28a5622ef8e0c9e48a2578a30b5778500f9470fd`.
- Metadata: `Builds/Android/gravivore-dev-0.1.0+1.build.json`; log: `Builds/Logs/android-dev-20261004-150328.log`.

The subsequent report commit changes documentation only; runtime/source assets are identical to the APK source HEAD. Portable artifact evidence is in [ANDROID_BUILD.json](phase3c/ANDROID_BUILD.json). The APK remains a local build artifact and is not committed.

The first incremental package after rebase retained about 35.8 MB of stale ZIP overhead. Its APK and generated Gradle launcher output were archived under `Builds/Logs/phase3c-android-packaging-review`, and the same existing pipeline regenerated a fresh package. The final APK has 82,972 bytes of ZIP/signing overhead; no source or build-pipeline settings changed for this packaging correction.

Human review still needs traversal/input, attack → beam → HP causality, readable telegraphs around new scenery, visual/capsule mismatch, repair-bay exit, camera overlap near tall props, cold start/save/offline return, and sustained mid-range-device performance/thermal behavior. Screenshots and Editor tests cannot substitute for those checks. On a profile that already defeated the elite/boss, use a separate review profile or the existing DEV review workflow to inspect encounters; no repeatability/reset behavior was added.

## Deferred items and assumptions

Replace C candidates (Carrier, Shield Dump, Hauler Graveyard, Elite Arena and now Capacitor Field) before external visual testing. Refine B hero destinations/actors, material variation, grounded supports, peripheral density and transition dressing after device composition feedback. New actor gait/rigging, docking/repair animation, dedicated socket-driven effect polish and legacy gate visual replacement are deferred.

No repeatable elite/boss, stronger farming spots, minimap, economy/reward/save changes, Chapter2, final audio/VFX or G-0 redesign. No replacement-asset search or new package was performed. The smallest safe interpretation is to use these B/C candidates to expose the whole world, preserve accepted Phase2 authority and report failures of the visual bar explicitly.

Next gate: Phase3C device review, followed by the visual refinement/replacement decisions in the locked Phase3 roadmap.
