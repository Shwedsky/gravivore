# GRAVIVORE Art Spike V3 — visual review

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


V2 established the approved silhouette direction, but its unfinished CC BY-SA donor was unsuitable for the selected V3 pipeline. The current V3 uses the expressly authorized autonomous fallback: **original temporary modular proxy geometry with real CC0 PBR surface maps**. This is not a claimed acquired Vanguard/game-ready character. All ten images below are new Unity/URP renders, without Julius geometry.

## Images

1. [01_G0_Evolution.png](art-spike/images/01_G0_Evolution.png) — 1920×1080, Tier 0 → Tier 1 → Tier 2 at equal root scale; common core and four supports, one-metre reference.
2. [02_G0_GameplayScale.png](art-spike/images/02_G0_GameplayScale.png) — 1080×1920, Tier 1 alone at the latest settled S20 camera (0,14.8,-11.2), FOV 46.
3. [03_Enemy_GameplayScale.png](art-spike/images/03_Enemy_GameplayScale.png) — 1080×1920, cyan G-0 left, red asymmetric Cutter upper right.
4. [04_Environment_Overview.png](art-spike/images/04_Environment_Overview.png) — 1920×1080, existing CC0 Kenney supporting bay.
5. [05_Gameplay_Mock.png](art-spike/images/05_Gameplay_Mock.png) — 1080×1920, Tier 2 and Cutter in the bay at S20 scale; no HUD/combat simulation.
6. [06_ScaleReference.png](art-spike/images/06_ScaleReference.png) — 1920×1080, closer same-scale player/enemy comparison with one-metre ruler.
7. [07_Evolution_S20Scale.png](art-spike/images/07_Evolution_S20Scale.png) — 3240×1920, three actual full portrait panels from the latest S20 camera.
8. [08_G0_CloseHero.png](art-spike/images/08_G0_CloseHero.png) — 1920×1080, Tier 2 three-quarter view showing armor, joints and forward weapon forks.
9. [09_G0_SurfaceDetail.png](art-spike/images/09_G0_SurfaceDetail.png) — 1920×1080, actual close PBR surface view: wear, normal relief, roughness variation, hard face bevels and containment.
10. [10_G0_DonorBreakdown.png](art-spike/images/10_G0_DonorBreakdown.png) — 1920×1080, actual exploded composition; read the source/group guide below. The filename is prescribed by the brief; the geometry groups are original proxy modules, not acquired character donors.

## Image 10 — composition/source guide

- Center: original Chassis module and its paired armor panels. Above it: common cyan primitive core, project-authored containment rings/posts, separated only for this diagnostic render.
- Four surrounding short assemblies: original UpperSupport, LowerSupport and Foot modules, with independent HipPivot/KneePivot/FootPivot transforms.
- Two raised plates with cyan strips: Tier 1 FlankPlate armor and project-owned power-strip primitives.
- Two farther outer plates without strips: Tier 2 HeavyOuterArmor, using the same independently authored plate module.
- Four fork-ended assemblies in front: two common WeaponHousing/EmitterFork mandibles and two larger Tier 2 weapon modules.
- Surface source on the mechanical modules: Blue Metal Plate by Rob Tuytel / Poly Haven, CC0; project-owned URP tints and derived AO/metallic-smoothness maps. No external character mesh, skeleton or clip is present.
- Supporting floor: the existing CC0 Kenney deck. Temporary exploded transforms are restored and never saved to the comparison scene.

## V2 versus V3

- **Silhouette:** common core position/diameter and low four-support/front-weapon language retained. V3 uses purposeful paired fork tips and progressive flank/outer armor. Exact V2 contours are not copied from its donor vertices; wider modules change measured bounds. Human review determines whether the direction is preserved well enough.
- **Surface:** V2 had zero character texture maps. V3 uses four shared 1K PBR maps (diffuse, GL normal, metallic/smoothness, AO), with worn paint and cooler gray material tints. These are real surface donor maps, not bespoke character baking.
- **Joints:** V2 used nonuniformly transformed whole limb child meshes, with static source joints. V3 has separate hip/knee/foot pivots, cylindrical joints and piston shins. These remain simplified proxy mechanics; an idle sample does not establish walking or load-bearing ground contact.
- **Gameplay readability:** image 07 now uses the latest S20 camera. Conservative projected bounding boxes are about 193×233 / 253×236 / 325×265 pixels for T0/T1/T2 at 1080×1920. Forks/width differentiate progression; fine scratches and pistons are primarily close-view details.
- **Renderers:** V2 T0/T1/T2/Cutter = 13/18/25/9. V3 = **19/24/33/13**, below 30/35/40/25 targets. More independent modules increase submissions; V3 is not presented as a renderer-count optimization.
- **Triangles:** V2 = 6,252/6,900/8,292/4,104; V3 = **5,928/6,936/9,216/3,352**. T2 adds 924 triangles; each remains below 50K.
- **License:** selected V2 character adaptations/images carried CC BY-SA 3.0. Current V3 geometry is independently authored and external surfaces/scenery are CC0, with no share-alike character dependency. Historical V2 remains under its original terms in Git.

## Limitations and replacement plan

The character geometry is still a temporary proxy, not production-ready final art. Repeated modules, planar UV scale/distortion, simple hard bevels and a generic surface texture are visible at close range. A final artist pass or obtainable permissive donor must provide refined chassis/limb/weapon geometry and authored UVs while preserving the core and socket contract. The three-second isolated idle proves independent pivot motion only; gait, IK, ground contact and combat animation are unimplemented.

No Android frame-time claim is made. Main prefabs are static and collider-free; this authorized ART-branch preview binds them through existing S07/S15 presentation in the real Chapter. Other enemies and environment retain current art. Full art replacement remains pending human device review.

See [candidate evidence](ART_ASSET_SHORTLIST.md), [performance](art-spike/PERFORMANCE.md), [license scope](art-spike/ART_LICENSE.md), [verification](art-spike/VERIFICATION.md) and [report](ART_SPIKE_REPORT.md).

**SHARE-ALIKE CHARACTER DEPENDENCY: NO**

**ART V3 RUNTIME DEVICE REVIEW: PENDING**
