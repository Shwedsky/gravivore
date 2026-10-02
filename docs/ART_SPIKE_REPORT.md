# GRAVIVORE ART SPIKE V2 report

**ART SPIKE V2 VISUAL REVIEW: PENDING**

V1 passed the technical gate but human review rejected its Kenney-only factory-prop appearance. This revision keeps the isolated authoring, material, comparison, capture and audit pipeline; replaces character donors with intact armored mech parts; and provides actual Unity images for another human review. No visual approval or Android performance claim is made.

## Same stack and scope

- Repository: Shwedsky/gravivore; same draft [PR #30](https://github.com/Shwedsky/gravivore/pull/30).
- Same branch: `codex/art-spike-kitbash`; revision starts at `42dfbe83d1a28127cc1bf1eb192dca2092dfe0c3`.
- Same base: `codex/s20-balance-vertical-slice`, pinned SHA `4394448a7f3950cb62df701e8969137ae4b4a01c`.
- Same isolated worktree: `C:\Users\pamak\Documents\ChatGPT\gravivore\.codex-worktrees\art-spike-kitbash`.
- Production runtime/content impact: **NONE**. S20 balance, Chapter 01 scene/catalog/evolution bindings, gameplay, other enemy archetypes, elites and bosses are unchanged. Root checkout and other worktrees were not modified.
- No merge, new PR, main retargeting or full Chapter art replacement.

## Donors and permissions

Character parts come from **Unfinished mech sketch by Julius**: [original author upload](https://opengameart.org/content/unfinished-mech-sketch), [author](https://opengameart.org/users/julius). The original OBJ, MTL and bundled license are retained. The archive offers GPLv2+ or CC BY-SA 3.0+; this spike selects **CC BY-SA 3.0 Unported**, permitting commercial use, modification and source redistribution with attribution, modification notice and share-alike. Adapted character visuals, scene visual content and review PNGs carry that license; independent project code and CC0 source assets retain their own terms. Exact scope and credits: [ART_LICENSE.md](art-spike/ART_LICENSE.md), [ThirdPartyNotices.md](../ThirdPartyNotices.md).

The source contains nine existing mesh objects, 3,944 total triangles, no textures or rig. Existing torso, legs, arms, shoulders and pelvis objects are selected independently. The head and complete humanoid are not instantiated. Source bytes/topology are unchanged; Unity normal import uses 15° smoothing. Composition uses transforms, shared materials and small project-owned primitives/rings. No mesh splitting, source geometry edit, destructive Blender work or blind mesh combine.

Kenney is now supporting scenery only: four Factory Kit FBX and five Modular Space Kit FBX, both CC0. Ten obsolete Factory/cog character donors were removed. No G-0 or Cutter contains Kenney geometry. Archive/file hashes and unchanged bundled licenses: [ASSET_MANIFEST.json](art-spike/ASSET_MANIFEST.json). Expanded original-source research, exact licenses, rejected expensive/unclear candidates and optional manual downloads: [ART_ASSET_SHORTLIST.md](ART_ASSET_SHORTLIST.md).

## Character compositions

**Tier 0:** horizontal torso armor chassis, unchanged cyan gravity core, a small containment ring, four intact armored leg/support assemblies with hip sockets, two forward mech weapon arms. Root scale is one; +Z is forward.

**Tier 1:** retains Tier 0 and adds shoulder-derived side armor, narrow cyan stabilizers and an outer containment ring. Width increases from 1.27 to 1.56 metres; the core stays identical.

**Tier 2:** retains the common structure and adds outer armor wings, larger forward weapon arms, upper containment ring and anchors. Width is 2.00 metres, depth 2.07 metres. Evolution adds geometry and silhouette; it does not enlarge the core or root.

**Cutter:** one ordinary archetype, with armored torso, two supports, unequal articulated weapon arms, shoulder shield, thin pelvis-derived cutting blades and a red dorsal energy spine. There are no cog weapons. It shares the restrained metal palette while differing in core shape, support count and asymmetric arm reach.

All four prefabs are decorative/static with no gameplay component, colliders, animation or rig.

## Environment and presentation

A quiet 5×5 deck now includes nine low-contrast panel insets. Low bulkheads, conduits and small service props remain peripheral. The reactor was reduced and its gear rings replaced with small project-owned containment rings. No decorative foreground wall blocks the central combat space.

Opaque URP materials provide dark metal, lighter armor and restrained cyan/red/amber accents. A shared 32×32-per-face mipmapped RGBAHalf studio reflection cubemap helps reveal armor planes; it adds no realtime light. The scene keeps one directional light and modest bloom.

## Eight actual Unity review images

The comparison scene preserves Areas A–D. The added close camera shows Tier 2 from three quarters, hiding the neighboring Cutter/reference during that capture only. S20-scale views use the actual settled camera offset, FOV and look-at height. Rendering executed in Unity URP on Direct3D11 / AMD Radeon(TM) Graphics. No generated concept image or composited fake geometry is used.

- [01_G0_Evolution.png](art-spike/images/01_G0_Evolution.png), 1920×1080: Tier 0/1/2 left to right, same scale.
- [02_G0_GameplayScale.png](art-spike/images/02_G0_GameplayScale.png), 1080×1920: Tier 1, S20 camera.
- [03_Enemy_GameplayScale.png](art-spike/images/03_Enemy_GameplayScale.png), 1080×1920: Tier 1 and Cutter, S20 camera.
- [04_Environment_Overview.png](art-spike/images/04_Environment_Overview.png), 1920×1080: bay overview.
- [05_Gameplay_Mock.png](art-spike/images/05_Gameplay_Mock.png), 1080×1920: Tier 2, Cutter and bay, S20 camera.
- [06_ScaleReference.png](art-spike/images/06_ScaleReference.png), 1920×1080: Tier 2 and Cutter, same scale.
- [07_Evolution_S20Scale.png](art-spike/images/07_Evolution_S20Scale.png), 3240×1920: three actual portrait renders, Tier 0/1/2.
- [08_G0_CloseHero.png](art-spike/images/08_G0_CloseHero.png), 1920×1080: clean three-quarter Tier 2 surface review.

Review guide: [ART_SPIKE_REVIEW.md](ART_SPIKE_REVIEW.md). Close-camera surface quality must be judged separately from actual gameplay-scale readability; the mock has no HUD or combat animation.

## Measured structural snapshot

- Tier 0: **13 renderers**, 6,252 triangles, four shared materials, 13 slots.
- Tier 1: **18 renderers**, 6,900 triangles, four shared materials, 18 slots.
- Tier 2: **25 renderers**, 8,292 triangles, four shared materials, 25 slots.
- Cutter: **9 renderers**, 4,104 triangles, four shared materials, nine slots.
- All: MeshRenderers only; zero SkinnedMeshRenderer, Animator, Animation, collider or character texture maps. The shared scene reflection cubemap is separate.

Tier 2 renderer count falls **58.3% from V1's 60**; triangles rise from 5,116 to 8,292 because the selected mech parts include actual armor/joint geometry. No combining was used. Submission counts are before batching, not a measured draw-call or FPS result. Full bounds, mesh/slot counts and method: [PERFORMANCE.md](art-spike/PERFORMANCE.md) and adjacent per-prefab JSON.

## Verification

Unity **6000.3.0f1** compile/import and final eight-image capture completed with return code 0. Full EditMode: **254/254 passed**, zero failed/skipped. Full PlayMode: **56/56 passed**, zero failed/skipped. Standalone ProjectValidator: explicit pass and process return code 0. The source hash audit, staged production isolation audit and raw XML are linked in [VERIFICATION.md](art-spike/VERIFICATION.md).

Android build was **not run**, as requested. No APK produced.

## Limits, assumptions and next gate

This donor is an unfinished untextured mesh. Armor has no PBR surface maps; compression/nonuniform transforms affect proportions; joins and overlaps remain visible in close review; some inner mechanics lie in shadow. Limbs are static and provide no engineered gait or combat articulation. Fewer renderers do not prove mobile performance; repeated units, shadows, batching and future animation still need device profiling.

Assumptions: the explicitly allowed other free commercial licenses include CC BY-SA with its documented share-alike obligations; intact mechanical parts from a humanoid donor are acceptable; one Unity unit is one metre; static posing and settled S20 camera are sufficient for this visual spike. No user Blender work is required.

Next spec ID: **none defined after S20**. Next gate: human ART SPIKE V2 visual review. No production adoption or Chapter replacement starts from technical pass alone.
