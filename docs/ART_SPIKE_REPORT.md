# GRAVIVORE ART SPIKE report

**ART SPIKE VISUAL REVIEW: PENDING**

This is a stacked, visual-only proof. The reviewer decides whether these actual Unity compositions satisfy the intended style. No claim of commercial quality or completed human approval is made.

## Baseline and scope

- Repository: `Shwedsky/gravivore`.
- Base: `codex/s20-balance-vertical-slice`.
- Verified remote base SHA: `4394448a7f3950cb62df701e8969137ae4b4a01c`.
- Branch: `codex/art-spike-kitbash`.
- New isolated worktree: `C:\Users\pamak\Documents\ChatGPT\gravivore\.codex-worktrees\art-spike-kitbash`.
- Root checkout and all existing worktrees were left untouched. The S20 worktree was not reused.
- Production runtime/content impact: **NONE**. No camera balance, stats, thresholds, aggro, respawn, offline reward, menus, production catalog, evolution definition, elite/boss art, or canonical scene changes are part of this spike.

## Source selection and retained content

All five requested sources were investigated first. Quaternius's current site-wide QAL conflicts with the CC0 labels on the three requested pack pages, so those assets were excluded rather than assuming public raw-asset redistribution is permitted. Molten Maps publishes CC0 but uses an interactive itch.io free-download step; its exact manual archive/destination are in `ART_ASSET_SHORTLIST.md`.

Actual donors: 14 freshly acquired Kenney Factory Kit 3.0 FBX files and four Kenney Modular Space Kit 1.0 FBX files. Both official pages and both bundled licenses explicitly identify CC0. No paid content, unofficial mirrors, vendor code, or textures were imported. Exact files, archive URLs, dates, permissions, and attribution requirements are recorded in `ThirdPartyNotices.md`; every retained FBX hash is in `art-spike/ASSET_MANIFEST.json` and matches its official archive.

## G-0 compositions

All three roots have scale `(1,1,1)` and attack direction `+Z`. Each is a **true modular kitbash of industrial donor pieces plus project-owned primitive augmentation**, not a stack of complete robots. Vendor mesh geometry is unchanged. Transform normalization and intentionally nonuniform scaling occur under project-owned wrappers.

### G0_Tier0_ArtSpike

Hierarchy:

- `01_CoreChassis_CommonIdentity`: `machine-connection-hole` rotated into a low central casing; `cog-d` lower ring; a project-owned cyan spherical gravity core, dark cylindrical lens, and small energy lens; `screen-panel-flat` rear spine.
- `02_FourMechanicalSupports_Common`: LeftFront, LeftRear, RightFront, RightRear. Each contains a `piston-thin-square` support, a `box-long` contact skid, and a small cyan joint insert. The piston donor contains three mesh children.
- `03_ForwardGravityMandibles_Common`: two forward-facing `cone` donor prongs with small cyan emitter slits.

Materials: `Gravivore_DarkMetal`, `Gravivore_Armor`, `Gravivore_SecondaryMetal`, and `Gravivore_PlayerCore`. The core is an energy component inside a mechanical chassis, not the entire body.

### G0_Tier1_ArtSpike

Retains all Tier 0 structures and adds `04_Tier1_ArmorAndStabilizers`: two `screen-panel-flat` side plates, two elongated forward `cone` blades, two energy stabilizer strips, and a `cog-a` outer housing ring. The added side planes and forward modules expand the measured width from 1.43 to 1.76 metres. Core size, color, and prefab root scale are unchanged.

### G0_Tier2_ArtSpike

Retains Tier 0/1 and adds `05_Tier2_EmitterForksAndContainment`: two outer armor wings, two forward `piston-square` emitter housings with cyan cylinder muzzles, two extended donor blades, paired rear power struts/ports, a second `cog-b` containment ring, and small core anchors. The upper ring was positioned below the visible core crown so the common core stays exposed. Measured width is 2.38 metres; depth grows from Tier 0's 1.735 to 2.42 metres. Extra energy surfaces use the same shared core material; the evolution is also geometric.

## Ordinary enemy — exactly one archetype

`Cutter_ArtSpike.prefab`: a `hopper-square` body, `screen-panel-flat` dorsal shield, two `box-long` runners, two `piston-square` saw actuators, and unequal forward `cog-e` saw discs. Project-owned small primitives provide a rectangular dorsal red energy block and two red hubs. Shared dark/armor/secondary metals and `Gravivore_HostileCore` unify it with G-0 while its runners, rectangular center, and offset circular weapons differ from the four-support player. No Scout, elite, or boss prototype was added.

## Environment

`IndustrialBay_ArtSpike.prefab`: a 5×5 deck using `template-floor`; five low rear `template-wall-half` bulkheads; two `template-wall-detail-a` side frames; Factory Kit perimeter pipes and status accents; reactor plinth (`machine-fortified`) with donor containment rings and an amber primitive energy column; an auxiliary generator, sealed container, bent feed pipe, and `cables` bundle.

The central floor is clear. All scenery is decorative, with zero colliders. No high front wall hides the player. The floor uses shared `Gravivore_Floor`; surrounding machinery uses the restrained metal palette with small `Gravivore_IndustrialEnergy` accents.

## Comparison scene and renders

Scene: `Assets/_Game/ArtSpike/Scenes/ArtSpike_Comparison.unity`.

- Area A: all three tiers side by side, same ground and a one-metre reference bar (20 cm tick spacing).
- Area B: Tier 1 and Cutter at the actual settled S20 camera pose/FOV in portrait.
- Area C: industrial bay with Tier 2 and Cutter, an overview camera and an actual S20 portrait camera.
- Area D: Tier 2 and Cutter side by side at intended scale.

Every PNG is an actual Unity URP render on Direct3D11, AMD Radeon(TM) Graphics. Close review uses a shorter camera offset with the same approximate pitch; it is not used as proof of gameplay-scale readability. Images 02/03/05 and each panel of 07 use the S20 offset/FOV/look-at height directly. No gameplay HUD is rendered; the mock is a static art composition.

Required outputs:

- `docs/art-spike/images/01_G0_Evolution.png` — 1920×1080, Tier 0/1/2 left to right.
- `docs/art-spike/images/02_G0_GameplayScale.png` — 1080×1920, Tier 1 alone.
- `docs/art-spike/images/03_Enemy_GameplayScale.png` — 1080×1920, Tier 1 and Cutter.
- `docs/art-spike/images/04_Environment_Overview.png` — 1920×1080, isolated bay overview.
- `docs/art-spike/images/05_Gameplay_Mock.png` — 1080×1920, Tier 2/Cutter/environment at S20 camera.
- Supplementary `06_ScaleReference.png` — 1920×1080, same-scale player/enemy close review.
- Supplementary `07_Evolution_S20Scale.png` — 3240×1920, three actual S20 portrait-camera panels, Tier 0/1/2 left to right.

## Measured performance snapshot

Detailed machine-readable per-prefab JSON and measurement method: `art-spike/PERFORMANCE.md` and the four adjacent JSON files.

- Tier 0: 30 child renderers, all MeshRenderer; 0 SkinnedMeshRenderer; 4 shared materials, 30 slots; 11 unique meshes, 30 mesh instances; 2,724 triangles.
- Tier 1: 37 child renderers, all MeshRenderer; 0 SkinnedMeshRenderer; 4 shared materials, 37 slots; 12 unique meshes, 37 instances; 3,316 triangles.
- Tier 2: 60 child renderers, all MeshRenderer; 0 SkinnedMeshRenderer; 4 shared materials, 60 slots; 16 unique meshes, 60 instances; 5,116 triangles.
- Cutter: 15 child renderers, all MeshRenderer; 0 SkinnedMeshRenderer; 4 shared materials, 15 slots; 9 unique meshes, 15 instances; 1,362 triangles.

All four: maximum texture size 0 (no textures), no Animator/Animation, no colliders, no lights. The scene has one directional light and a modest bloom pass. Triangle sums count geometry instances using mesh index counts, including primitives. The audit's projected bounding boxes are conservative bounds rather than silhouette pixel counts.

**Obvious risk:** 60 renderers on Tier 2 and three meshes per piston are many submissions despite modest triangle/material counts. This static proof makes no Android FPS claim. No optimization was performed solely to hit an arbitrary number. Animation and batching need separate measured work before adoption.

## Observable goals and limitations

Implemented: shared core/housing/support identity across all tiers; added geometric armor/weapon modules; different player/enemy locomotion and center shapes; unified opaque URP palette; low industrial framing; verified CC0 imports; reviewable Unity screenshots; isolated production content.

Human review still decides evolution visibility, shape readability, direction match, aesthetic improvement over S15, and acceptability of the scale. Increasing geometric bounds is not a substitute for that judgment.

Known weaknesses: familiar cog/industrial-prop forms remain visible; some nonuniform scaling stretches bevel proportions; supports are static rather than an engineered animated gait; joints are assembled and may appear discontinuous from certain angles; the floor is deliberately simple; broad shadows hide some inner mechanics. There is no complete donor character mesh and no stacked-robot composition.

Transform-only composition with this donor set cannot provide seamless bespoke armor surfaces, a newly retopologized character body, new UVs, or a custom skeletal rig/gait. Those would require additional licensed modular/rigged donors or custom asset work in a later task. No user Blender work is required for this proof. Quaternius acquisitions and the optional Molten archive remain unresolved as recorded in the shortlist.

## Verification and next gate

Unity compile/capture executed successfully. Full EditMode, full PlayMode, standalone ProjectValidator, and the final production-diff audit are recorded in `art-spike/VERIFICATION.md` after execution. Android build is intentionally omitted per this task's explicit instruction; no APK was produced.

Assumptions: Factory Kit is an acceptable fallback mechanical donor while preferred Quaternius licensing is unresolved; one Unity unit is one metre; +Z is forward; static posing is sufficient for this visual proof; portrait captures use a settled camera rather than simulating follow damping. No production authority or balance changed.

Next spec ID: none defined after S20 in the current spec index. Next gate is human ART SPIKE visual review, alongside the separate pending S20 fresh-profile review. Full Chapter 01 art replacement is not started. PR remains stacked against S20 and must not be merged or retargeted to main before the authorized review/merge sequence.
