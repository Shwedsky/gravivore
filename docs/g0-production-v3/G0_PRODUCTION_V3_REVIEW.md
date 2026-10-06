# G-0 Production V3 — isolated production asset review

Branch: `art/g0-production-v3`. Draft PR: https://github.com/Shwedsky/gravivore/pull/59. Frozen merged baseline: `467b43be836e7104aa13baa60af648f43fb453d0`. First real checkpoint: `30b99b0`; production checkpoints `c6e932f`, `37840b0`, `5361789`, `c450e7c` were committed and pushed. The exact final delivery HEAD is the PR head and completion response; asset hashes and executable verification results are in `data/`.

## What changed and what was preserved

The build opens the accepted `art/visual-production-v2/g0/G0_Bipedal_Blockout_V21.blend`, SHA256 `275705338e80dadb5db2ddf2ab8e7b4eae430ce4563688050b4dde7f98955ef0`. It does not rebuild the silhouette or load a new donor. The accepted broad shoulders, split chest and diamond/slit core, sensor blade, asymmetric lash/capture tools, open waist, mechanical legs, rear containment and two split feet remain identifiable.

Production geometry revises the sharp fan apices into supported ridge landings, adds broad inset service panels, replaces plain joint end caps with recessed bearing retainers, supports the hip/knee/shoulder/elbow bearings with collars, strengthens sole support and consolidates 142 source mesh objects into one rigid skin per LOD. Degenerate faces are removed, sharp/smooth boundaries are intentional and geometry normals are weighted. Layered parts remain disconnected rigid mesh islands where articulation or material construction requires it; consolidation is not destructive welding across moving joints.

LOD0 remains exactly **2.062500 wide × 1.753920 deep × 3.358425 high** in source meters, compared with V2.1 **2.062500 × 1.753808 × 3.358425**. The 0.000112 depth difference is edge construction, not a proportion revision. Width, height and ground plane are preserved. Source scale is not resized for gameplay. The review applies the frozen `.4585482776` fit, the accepted V2.1 normalization × **1.10**, giving **1.54 Unity units** of rest height. The actual camera capture independently asserts this height and ground contact.

No Chapter01 player prefab, movement, combat timing, collider, footprint, camera definition, encounter/gate/layout data, enemy/environment/UI/audio/VFX authoring or Chapter 2 work is included. Unity's built-in `com.unity.modules.animation` is enabled as the necessary prerequisite for importing and playing the requested rigged clips; it is not a new paid or third-party dependency.

## Deliverables

- Blender: `art/g0-production-v3/G0_Production_V3.blend`.
- FBX: `Assets/_Game/ArtReview/G0ProductionV3/Models/G0_Production_V3.fbx`.
- Isolated art prefab: `Assets/_Game/ArtReview/G0ProductionV3/G0_Production_V3_ArtReview.prefab`.
- Isolated review scene: `Assets/_Game/ArtReview/G0ProductionV3/G0_Production_V3_Review.unity`.
- Five-state visual review controller and one opaque URP atlas material alongside the prefab.
- Reproducible Blender production, independent validation, capture and evidence tools in `Tools/g0-production-v3/`.
- Complete changed-file list: `data/changed_files.txt`.

## LOD and mobile budget

- **LOD0: 23,264 triangles**, 12,154 Blender vertices; Unity splits this to 26,348 vertices at UV/normal discontinuities. This is 20.92% below the 29,417-triangle V2.1 evaluated blockout.
- **LOD1: 13,259 triangles**, 7,114 Blender vertices; **43.01% reduction** from LOD0.
- **LOD2: 6,978 triangles**, 3,939 Blender vertices; **70.01% reduction** from LOD0.

All share the same skeleton and atlas, one material/submesh and one renderer for the selected LOD. Collapse protection preserves outer tool/shoulder tips, upper silhouette, core and grounded sole contacts. Lower LODs keep width, height and ground contact; maximum depth growth at LOD2 is about 0.0021 source units. A handful of collapsed UV corner conflicts are repaired within their original atlas regions, and a separate 512×512 strict-interior raster check detects zero overlapping pixels in every LOD. This is the stated validation resolution, not an assertion of mathematically perfect UV intersections at arbitrary precision.

Unity LOD transitions are `.13 / .065 / .02` screen-relative height, without transparent crossfade. Native 360×640 captures of all three LODs and automatic selection are retained. Device GPU/FPS/thermal results have not been measured for the new asset.

## Rig and visual animations

The source and imported FBX have **18 bones**: ROOT, PELVIS, TORSO, SENSOR, paired HIP/KNEE/ANKLE, paired SHOULDER/ELBOW/TOOL and two capture-jaw hinges. Pivots align to the revised source bearings. Every vertex has exactly one rigid bone influence; armor does not bend like human skin. Root is at the origin. There are no face/finger bones, gameplay scripts, colliders, donor animation, cinematic constraints or runtime IK dependencies.

At 30 fps:

- **Idle:** frames 1–61, 2.0 s, loop; restrained torso/sensor/tool settling.
- **Run:** frames 1–25, 0.8 s, loop; alternating mechanical steps, planted stance feet, lifted swing feet, restrained body bob and opposite arm motion. A two-link authoring solve is baked into ordinary FK keys.
- **Attack:** frames 1–25, 0.8 s; anticipation, asymmetric lash-tool drive/capture articulation, brief hold and damped recovery.
- **Hit:** frames 1–16, 0.5 s; body/sensor recoil and recovery.
- **Death:** frames 1–46, 1.5 s; supported mechanical kneel/collapse and held terminal pose. It contains no explosion or VFX behavior.

All clips are in place, without animation events or root motion. Idle/Run source loop endpoints match; source foot-contact probes remain on/above the ground to numerical precision during Run/Death. Attack permits less than 0.002 source units of sole compression. FBX reimport independently demonstrates changing poses in all five takes. Unity's imported clips have the expected names, ranges and loop flags. The isolated Animator has independent review states; future presentation binding and stride-speed matching are intentionally outside this gate and cannot replace combat/movement authority.

## UV, surfaces and provenance

One non-overlapping UV0 atlas uses about 49% of the unit tile for LOD0, with explicit margins and bake gutters. It is packed once for the consolidated skin. The three **2048×2048** source maps are BaseColor, MetallicSmoothness (metallic RGB, smoothness alpha) and Emission. Android overrides cap each at **1024**, ASTC 6×6 with mipmaps. The configured texture footprint is approximately 1.9 MB including mipmaps; actual device residency is not profiled.

Muted graphite structure, dark painted titanium shell, machined bearing surfaces, restrained pale edge panels and small copper service marks separate construction roles. Roughness is higher than the blockout's glossy treatment; broad, low-amplitude surface variation is baked, not an expensive runtime procedural shader. Cyan is limited to the core, sensor and existing functional tool/containment indicators. The asset is opaque; no additional transparency, lights, particle systems or decorative glowing rings are introduced. Geometry normals and imported/calculated Mikk tangents supply the shading; no normal-map dependency is required.

The previously approved 17 low-identity Catfish pelvis/knee/ankle fragments retain **Jungle Jim / CC BY 4.0** provenance. Original source title, author, URL, license and modification notice remain in the blend and `ThirdPartyNotices.md`. The hidden donor comparison is excluded from FBX selection. G-0 exports neither donor textures nor donor animations.

## Actual camera evidence and method

The five studio PNGs reopen the saved production blend and render in Cycles, 1200×1400, 32 samples. Unity captures use the committed Chapter01 world/reference bindings and the unchanged production camera settings: offset **(0,14.8,-11.2)**, FOV **46**, look-at height **0.9**, portrait **9:16**. Review actor position is **(0,0,57)**, matching the approved V2.1 stage; existing Scout/Warden/Magnetar bindings keep their production scales.

Unity's batch editor does not advance the skinned draw cache between manual `Camera.Render` calls. The harness evaluates the actual imported Animator state, bakes the posed skin at source scale and renders a temporary mesh with the same URP material/transform. The temporary mesh is removed before saving the scene; the saved prefab and scene retain the real rigged asset. A physical rest-height/ground assertion prevents double application of the fixed presentation fit. Full original PNGs are retained, and movement/attack/death image hashes are distinct. Boards only add labels and explicitly described resizes/crops; they do not paint or generate character details.

Required captures:

1. `evidence/blender/01_front.png`
2. `evidence/blender/02_side.png`
3. `evidence/blender/03_rear.png`
4. `evidence/blender/04_threequarter.png`
5. `evidence/blender/05_black_silhouette.png`
6. `evidence/unity/06_gameplay_camera.png`
7. `evidence/unity/07_phone_size.png`
8. `evidence/unity/08_ordinary_enemy.png`
9. `evidence/unity/09_magnetar.png`
10. `evidence/unity/10_movement.png`
11. `evidence/unity/11_attack.png`
12. `evidence/unity/12_death.png`

Review boards: `evidence/01_v21_v3_comparison.png`, `02_lod_phone_board.png`, `03_animation_pose_board.png`. Per-clip native camera samples and individual LOD phone captures accompany them. Exact image/asset hashes are in `data/evidence_manifest.json`.

## Visual assessment

G-0 still reads as the approved bipedal machine: shoulder/core hierarchy, open waist, paired legs and asymmetric tools remain. V3 improves supported panel construction, joint finish, material restraint and grounded articulation over the accepted blockout. The central cyan identity survives at native phone size, with recognizable arm/leg separation and a clear elite size difference next to Magnetar. Small service panels/bearing recesses naturally disappear at phone resolution; the silhouette and core carry identity there. The pose board makes stride, attack extension and kneeling death visibly distinct without camera changes.

The aesthetic follows restrained stylized mobile hard-surface treatment. The terminal death pose is a mechanical collapse; incoming death VFX/audio remains separately owned.

## Executed verification and final gate

Executable results, including full EditMode/PlayMode counts, project validation and the dev Android regression build, are recorded in **`data/verification_summary.json`** and original logs/XML under `verification/`. Reopened source and FBX validation pass; the Unity importer/capture pass records complete UV/normal/tangent channels, 18-bone skins, five clips, no review model in canonical dependencies and disabled root motion. Do not infer an APK/device result from a successful FBX import.

Unity **6000.3.0f1** compilation and full project validation passed. **EditMode: 392 passed, zero failed/skipped. PlayMode: 94 passed, zero failed, one skipped** out of 95. The skip is the existing opt-in `CaptureStructuralFoundationWhenRequested` capture test; the new V3 rig/clip/isolation checks passed. Native logs retain Unity's original formatting; authored-file whitespace checks pass.

The dev Android build passed at tested code checkpoint **`c450e7cadf7995071af086ba335162288623a5ce`**. APK: `C:\Users\pamak\Documents\ChatGPT\gravivore\.codex-worktrees\g0-production-v3\Builds\Android\gravivore-dev-0.1.0+2.apk`, **59,573,853 bytes**, SHA256 `8835e58b151943d831d65a5a86544f266bf46748312d26223c69ccc6bdd4760d`. Independent Android badging confirms `com.gravivore.mobile.dev`, version **0.1.0+2**, **ARM64**, minimum SDK 26 and target SDK 36. The final delivery commit adds the report and native verification records without changing the tested production source/model/editor/test code.

The final build is the existing runtime regression APK. The new review scene/model is excluded from production dependencies and build scenes. No live integration or PR merge is performed. Generated Unity serialization/configuration noise is removed from this isolated worktree before final delivery; the dirty root checkout is untouched.

Assumptions: keep the accepted V2.1 source-meter normalization and use rigid mechanical skinning; author clip lengths as visual assets, with no combat-timing changes; use the committed V2.1 review stage to preserve the exact camera/world/reference conditions. No new character art direction is inferred.

Known limitations: Android device performance and human in-motion/device acceptance remain unmeasured; live presentation event binding, stride matching and death VFX are future integration work. The art gate stops here. **Next spec id: none authorized.** Next gate: human review of G-0 V3, followed by separately authorized production integration.

Recommendation: **PASS FOR PRODUCTION INTEGRATION**, subject to the human asset/device review gate. This recommendation approves the delivered asset for the next integration review; it does not authorize replacing the live Chapter01 player.
