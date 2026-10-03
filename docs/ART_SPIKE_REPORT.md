# GRAVIVORE ART SPIKE V3 — runtime preview report

**ART V3 RUNTIME DEVICE REVIEW: PENDING**

## RUNTIME APK PREVIEW

**ART V3 RUNTIME DEVICE REVIEW: PENDING**

- Required S20 **9f744b1fc08d9ed25d5a8f818513922a9d9c3ea4** integrated by normal merge **af442756cf5c2003fdba5289039ab9498d225026**, from previous S20 base **4394448a7f3950cb62df701e8969137ae4b4a01c**. Starting ART HEAD: **575d1905177613dc744050ccb23d3d74b25c1219**.
- Camera: offset **(0,14.8,-11.2)**, vertical FOV **46**, look-at height **0.9**, damping **0.18**, portrait **9:16**. Actual Chapter ground is **72 × 140**, center (0,0,30); latest ordinary spots, elite/boss positions and gate configuration retained. All ten comparison PNGs regenerated on this camera before runtime binding.
- Player: `S07_Evolution._tierPrefabs[0/1/2]` points to **G0_Tier0_ArtSpike / G0_Tier1_ArtSpike / G0_Tier2_ArtSpike**. Existing assimilation/presenter select whole forms at **20 / 42**. Exactly one form is active; old S15 body and primitive tier/accent additions are suppressed while whole-form overrides exist. Dominant stat is still tracked; proxy has no separate dominant-stat accent geometry.
- Cutter: only `S15_VisualCatalog` recipe **cutter-unit** receives `_presentationPrefab = Cutter_ArtSpike`. Existing ordinary pool and lifecycle build/cache/reset it; all four Cutting Floor instances use the new presentation. Original PBR material slots remain intact. Existing legacy parts are retained as the reversible fallback.
- Gameplay authority: player CharacterController **radius 0.42 / height 1.4 / center (0,0.7,0)** unchanged; Cutter collision radius **0.4**, HP **42** unchanged. No combat/stat/progression/save/movement/AI/spawn configuration is changed. Art prefab roots and visual instances use scale **(1,1,1)** and contain no scripts or physics.
- Gravity Lash: only VFX start is redirected to the presentation child socket **(0,0.68,1.15)** in front of the weapon. Combat selection, visibility tests, pull, ranges and damage still use the original gameplay root/target point. Existing VFX/audio/hit/death systems remain active.
- Old art retained: **Scout, Warden, Arc Drone, Carrier, Magnetar Guard, Custodian M-0 and full Chapter environment**. The review bay remains comparison-only and is absent from Android build scenes/packed assets.
- Animation: runtime forms remain **static**. No gait/IK/retargeting/controller or new cheat. Existing DEV god mode/stat grants speed normal farming; a fresh/reset profile shows Tier0, **20** ordinary kills show Tier1, **42** show Tier2. Stat grants alone do not increase assimilation. Existing “Open elite” raises score to 60 and should be avoided before Tier1 review.
- **Dev APK built successfully**, Unity exit **0**, ARM64 **IL2CPP**, portrait, Development + debugging, package **com.gravivore.mobile**, min SDK **26**, target **36**. Existing build script/flavor only; no Candidate APK.
- Filename: **gravivore-dev-0.1.0+1.apk**; size **57,966,186 bytes (55.28 MiB)**.
- Exact local APK: `C:\Users\pamak\Documents\ChatGPT\gravivore\.codex-worktrees\art-spike-kitbash\Builds\Android\gravivore-dev-0.1.0+1.apk`.
- Build log: `C:\Users\pamak\Documents\ChatGPT\gravivore\.codex-worktrees\art-spike-kitbash\Builds\Logs\android-dev-20261003-105531.log`.
- Build metadata: same APK basename with `.build.json`; code commit **232eed29bd33b157235501494a2b7bf44b0306eb**, UTC **2026-10-03T11:06:32.0080411Z**. Final report commit only changes documentation/evidence.
- APK SHA-256: **3B1754787C37D8DBED5F14FE8C884EE4C9E4F6B2D4260B94A73C899A711C393E**. ZIP integrity passes, `lib/arm64-v8a/libil2cpp.so` present, all four form names found in packed Unity assets; comparison scene name absent. [Build evidence](art-spike/verification/RuntimeAndroidBuild.json).
- Verification: compile passed, **265/265 EditMode**, **59/59 PlayMode**, standalone ProjectValidator passed, Dev Android build passed. Raw XML, logs and scope/source checks: [verification](art-spike/VERIFICATION.md).
- Runtime structural audit, actual visible **Tier2 + four Cutters**: **85 renderers, 158 material slots, 22,624 triangles, five shared materials, four 1K shared maps**. Android ASTC 6×6 including all mips estimates **2,505,152 bytes (2.39 MiB)**. Scene: **one realtime directional light, shadows disabled**; all 85 selected renderers are configured as potential shadow casters but no realtime shadow light renders them. Excludes environment/HUD/VFX/other enemies/inactive cached forms; slots are potential submissions before batching, not measured draw calls. [Runtime snapshot](art-spike/RUNTIME_PERFORMANCE.json).
- Manual checks: **launch, farther camera, expanded map, Tier0, movement, aligned attack VFX, Tier1, Tier2, V3 Cutter, Cutter movement/attack/death, gameplay readability, magenta materials, missing parts and several-minute FPS degradation**. Exact human instructions: [device checklist](ART_RUNTIME_DEVICE_CHECKLIST.md).
- Limits: original temporary proxy geometry/UVs/surfaces; no final-art approval; static supports may slide, wide side plates may intersect obstacles while the collider stays narrow; directional socket alignment needs side/back attack review. **Device launch, FPS, thermals and final touch-play appearance have not been measured.**
- Commits: S20 merge **af44275**, regenerated review **5bfde43**, clearly reversible runtime binding **232eed2**. Remove the four prefab references (or revert binding commit) to restore legacy presentation; the S20 merge remains separate. Same draft [PR #30](https://github.com/Shwedsky/gravivore/pull/30), base unchanged, unmerged. No full art replacement or next production spec started.


Temporary playable art preview on codex/art-spike-kitbash and the same draft PR #30:
https://github.com/Shwedsky/gravivore/pull/30
Base remains codex/s20-balance-vertical-slice. Previous base: 4394448a7f3950cb62df701e8969137ae4b4a01c; required S20 SHA 9f744b1fc08d9ed25d5a8f818513922a9d9c3ea4 was fetched, verified and integrated by normal merge af442756cf5c2003fdba5289039ab9498d225026. Starting ART HEAD: 575d1905177613dc744050ccb23d3d74b25c1219. Neither PR is merged or retargeted. The current human request authorizes the four temporary runtime visual bindings described below.

## Candidate evaluation

All five exact candidates were inspected first; detailed licensing/acquisition/source measurements and unknowns are recorded in [ART_ASSET_SHORTLIST.md](ART_ASSET_SHORTLIST.md).

- **Vanguard-Class Mech Titan:** official page/API CC BY 4.0, 42,295 triangles, animationCount 0. PBR map set described by author, but file/hierarchy/material/renderer/texture-resolution/rig data unmeasured because official download requires authentication. Skipped under the autonomous acquisition instruction.
- **Robot Warrior:** official CC BY 4.0, 35,798 triangles, animationCount 0, author describes rigged. Official download requires authentication; child/renderer/material/map/rig inspection unavailable. Skipped.
- **K3NY:** official CC BY 4.0, 39,072 triangles, animationCount 1, described textured/fully rigged. Official download requires authentication; file/hierarchy/renderer/material/rig detail unavailable. Skipped.
- **Corebreaker:** official CC0 ZIP acquired without login. Four robot mesh objects / 4,376 triangles, six material roles / 11 slots; 256×256 lens image and missing screen reference, no armor PBR set. Player skeleton 57 bones; three reset/test actions across player and first-person rigs. Body joins arms/legs/torso, unsuitable for separate four-support reuse without topology/skin editing. Rejected.
- **RetroStyle mech:** official FBX archive acquired. Active LOD0: four skinned mesh objects / 25,996 triangles / one material; LOD1 18,606. Actual five 2K PBR PNGs, 43 bones and seven animation FBXs. Six legs joined in one lower mesh. Free/royalty-free page does not establish public raw-source modification/redistribution terms; archive has no license. Also an identified Ocean Keeper protagonist. Rejected, no import.
- **Additional piacenti Robot:** official CC BY 3.0 ZIP acquired. Three complete alternate meshes, 23,857 triangles and one material each, 2K/4K color plus 2K normal/gloss; no armature/clips. Inseparable humanoid bodies rejected.

Rejected .blend/.fbx metrics are Blender source mesh-object counts, not asserted Unity renderer measurements. No archive with unresolved terms is committed.

## Selected donor strategy

The current instruction expressly permits a temporary mock/proxy rather than requiring user authentication or compromising the direction. Selected: original project-authored hard-surface proxy modules plus **Blue Metal Plate by Rob Tuytel / Poly Haven (CC0)**, downloaded from the official CDN without login. Kenney's existing CC0 subset remains scenery.

Eight original shared modules: Chassis, UpperSupport, LowerSupport, Foot, FlankPlate, WeaponHousing, EmitterFork, ShearBlade. No external character mesh or skin is loaded. Final replacement/refinement must preserve core position, four hip/knee/foot socket paths, units, root scale and tier silhouette. The proxy is not misrepresented as the unavailable Vanguard donor.

## G-0 V3

### Tier 0

Low chassis, central cyan core at (0,0.68,0) / diameter 0.30, four three-part mechanical supports, paired forward mandibles and fork tips. 19 renderers, 5,928 triangles. Original chamfered armor geometry, independent pivots and real surface maps replace unfinished donor planes.

### Tier 1

Adds flank plates, narrow cyan strips and outer containment ring. Common identity/locomotion/core preserved. 24 renderers, 6,936 triangles; width 1.961 m versus T0 1.500 m.

### Tier 2

Adds outer plates, heavier paired housing/fork weapons, supported upper ring/posts. 33 renderers, 9,216 triangles; width 2.498 m, height 0.862 m. Extra geometry establishes evolution without root scaling.

## Cutter V3

Low two-runner chassis, long/short asymmetric shear arms and tapered blades, offset shield and red spine. Same original proxy/PBR ecosystem; no humanoid head or cyan core. 13 renderers, 3,352 triangles, about 1.535 m wide / 0.614 m high, smaller/lower than T2. Static feet touch the review floor; no gait contact claim.

## Materials / PBR

Three unchanged CC0 source files at 1024×1024: diffuse JPG, GL normal PNG, ARM PNG. MD5 matches official API; original URLs/SHA-256/byte sizes in [manifest](art-spike/ASSET_MANIFEST.json).

Four maps are actually referenced by mechanical materials: diffuse, normal, derived metallic/smoothness and AO. Linear ARM.B goes to metallic R; 1-ARM.G goes to smoothness A; ARM.R goes to AO RGB. Original source is retained; output masks are 8-bit. Normal/masks linear, diffuse sRGB, mipmapped, 1K max size, Android ASTC 6x6 override. Project-owned cooler armor and darker structure tints retain the authored surface maps; cyan/red are independent energy materials.

This is a generic real surface donor, not baked final character texturing. Planar UVs and nonuniform module scaling need final refinement.

## Animation viability

No acquired rig is selected. A project-authored three-second transform-only idle clip independently moves four hip pivots and two common weapon pivots. Build-time sample at 0.75 seconds confirms all hips move; automated test verifies no Animator/Animation is added to the main character prefabs. Optional menu preview is a temporary DontSave clone in the isolated scene.

This proves articulation only, not a locomotion rig, skinning, IK, gait, floor contact under animation or gameplay binding. No animation integration blocks this visual review.

## Performance

Measured inactive-inclusive prefab counts:

- T0: 19 renderers, 36 material slots, 5,928 triangles, four unique materials.
- T1: 24 renderers, 43 slots, 6,936 triangles, four materials.
- T2: 33 renderers, 58 slots, 9,216 triangles, four materials.
- Cutter: 13 renderers, 25 slots, 3,352 triangles, three materials.

All character budgets pass. Four 1K texture maps are shared across the characters; no SkinnedMeshRenderer/Animator/Animation/collider/light/camera/gameplay script in character prefabs. Two deliberate material submeshes per mechanical module mean renderer counts alone understate draw cost. One review directional light, a static small cubemap and modest bloom; device profiling still required. No Android FPS/frame-time result is claimed.

V2 versus V3 counts, bounds and visual tradeoffs: [review](ART_SPIKE_REVIEW.md), exact [snapshots](art-spike/PERFORMANCE.md).

## Licensing / cleanup

**SHARE-ALIKE CHARACTER DEPENDENCY: NO**

Julius raw OBJ/MTL/license directory and metadata removed. Character prefabs/comparison scene and ten screenshots regenerated from independent geometry, without donor vertices. Old Julius-specific importer behavior removed. Notices/license scope/manifests updated; historical V2 remains under its original terms in ordinary Git history.

Current external visual assets are CC0. No paid/NC/SA/unclear/franchise asset, donor script or engine package imported. Attribution remains voluntarily documented for CC0.

## Visual outputs

All ten requested actual Unity outputs are listed and linked in [ART_SPIKE_REVIEW.md](ART_SPIKE_REVIEW.md), including surface closeup and documented exploded composition. Capture uses Unity 6000.3.0f1 / URP 17.3 on Direct3D11, AMD Radeon(TM) Graphics. Image 07 joins three actual full portrait panels; no synthetic render substitutes.

## Verification

- Unity compile: succeeded; runtime/editor/test assemblies built without C# errors.
- Full EditMode: **265/265 passed**, zero failed/skipped.
- Full PlayMode: **59/59 passed**, zero failed/skipped, including three canonical runtime preview tests.
- Standalone ProjectValidator: outcome recorded in [verification](art-spike/VERIFICATION.md).
- Performance snapshots and ten-image capture: executed.
- Android/APK: current Dev IL2CPP build and metadata are recorded in the RUNTIME APK PREVIEW section.
- Content diff against latest S20: only S07 presentation prefab references/offset and S15 Cutter presentation override. Camera, map, spawn spots, enemy/boss definitions and gameplay code are byte-identical to required S20; incidental Unity serialization is excluded.

Test XML/validator evidence: [verification artifacts](art-spike/verification/). Exact scope: [FILES_CHANGED.txt](art-spike/FILES_CHANGED.txt).

## Files / assumptions / limitations / next spec

Runtime changes: S07/S15 authored definitions; Presentation evolution/catalog/factory/composition/VFX; opt-in ArtRuntimePreview editor binder; binding and canonical smoke tests. Rebuilt comparison assets/images and art reports accompany them. build-android.ps1 now launches its batch Unity process with WindowStyle Hidden; its existing Dev flavor is used. Exact inventory: [FILES_CHANGED.txt](art-spike/FILES_CHANGED.txt). Rejected archives/tools and APK/build intermediates remain ignored. Root checkout and other worktrees are preserved.

Assumption: the explicit autonomous proxy fallback permits original temporary mesh authoring while preserving the V2 design language. It does not imply final visual approval or production readiness. Artist-quality geometry/UVs, a proper locomotion solution and device performance remain limitations.

No next production spec or full art replacement is started. The next stage is human review of this Dev APK using [ART_RUNTIME_DEVICE_CHECKLIST.md](ART_RUNTIME_DEVICE_CHECKLIST.md).

**ART V3 RUNTIME DEVICE REVIEW: PENDING**
