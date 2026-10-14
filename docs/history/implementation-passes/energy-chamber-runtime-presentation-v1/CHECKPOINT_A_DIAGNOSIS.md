# Checkpoint A — runtime visibility diagnosis (before correction)

Human authority: the four supplied APK55 phone screenshots and Energy Chamber concept in `references/`. The architecture/modeling is accepted and frozen. This diagnosis uses equivalent player positions because the JPEGs contain no world-coordinate metadata. No runtime art, LOD membership, materials, lighting, scene, route, collision or gameplay code has been changed at this checkpoint.

## Observed cause

The root production LODGroup drops complete secondary assemblies at thresholds **.38 / .30 / .08**. Android defaults to **Medium, LOD bias .7**; the previous editor captures ran at editor quality Fantastic, bias **2**. Unity scales projected size by this bias. Nearby south/west views therefore select LOD1, and the wider approach selects LOD2. The same view rendered with diagnostic-only ForceLOD(0) immediately restores the absent parts. ForceLOD is reset to automatic after this comparison and is not a production fix.

All 45 Renderers remain enabled/active, with finite nonzero bounds; each absent component in the reproduction is still inside the camera frustum. GPU camera visibility agrees with the selected membership. There are **zero nested LODGroups**. All materials are opaque, queue 2000, with no transparent sorting. There is no runtime Renderer toggle or animation on the station. Fixed-yaw camera movement changes distance to the LOD reference point; this is distance/projected-size selection, not a camera-angle or bad-bounds defect. A component outside the frame in a west view remains ordinary frustum clipping.

## Reproduction

The real Chapter scene is loaded through its normal composition. Its live G-0 and actual initialized PortraitFollowCamera are used; SnapToTarget preserves the real offset, look-at, FOV 48 and portrait aspect 9:16. Only combat/population updates are paused in the capture fixture. Android quality is explicitly selected for the test.

- A_problematic_west_view: reference distance 25.44 m, bias-adjusted height 0.355, LOD1; 29 camera-visible Renderers; 8 in-frustum parts excluded by LOD.
- B_problematic_south_view: reference distance 26.04 m, bias-adjusted height 0.347, LOD1; 29 camera-visible Renderers; 16 in-frustum parts excluded by LOD.
- C_normal_approach: reference distance 29.63 m, bias-adjusted height 0.305, LOD1; 29 camera-visible Renderers; 16 in-frustum parts excluded by LOD.
- D_close_north_view: reference distance 18.31 m, bias-adjusted height 0.493, LOD0; 45 camera-visible Renderers; 0 in-frustum parts excluded by LOD.
- E_wider_chapter: reference distance 32.91 m, bias-adjusted height 0.274, LOD2; 15 camera-visible Renderers; 30 in-frustum parts excluded by LOD.

## Every affected assembly / Renderer

Each bullet below records the exact hierarchy path, baseline membership, observed root cause and proposed correction. All are distance/projected-size based; no bounds repair is justified.

- `Energy Chamber / production/EC_FoundationPolish/EC_FoundationPolish(Clone)` — 16,360 triangles; LOD 0. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: keep complete floor interfaces through nearby LOD0; drop this finer plinth assembly only at range.
- `Energy Chamber / production/EC_ChamberPolish/EC_ChamberPolish(Clone)` — 40,946 triangles; LOD 0. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain this identity/silhouette-critical assembly in LOD1 and the far silhouette level.
- `Energy Chamber / production/Distribution node 1/EC_NodePolish/EC_NodePolish(Clone)` — 5,742 triangles; LOD 0. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain this identity/silhouette-critical assembly in LOD1 and the far silhouette level.
- `Energy Chamber / production/Distribution node 1/VR3_generator/VR3_generator(Clone)` — 4,132 triangles; LOD 0, 1. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain the side-machine/support assembly in LOD1; remove only at long range.
- `Energy Chamber / production/Distribution node 1/VR3_centrifuge/VR3_centrifuge(Clone)` — 788 triangles; LOD 0, 1. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain the side-machine/support assembly in LOD1; remove only at long range.
- `Energy Chamber / production/Core-to-node route 1/EC_ConnectorPolish/EC_ConnectorPolish(Clone)` — 2,372 triangles; LOD 0. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: keep connector interfaces through nearby LOD0; allow fine conduit extras to drop at range.
- `Energy Chamber / production/Core-to-node route 1/Existing pipe-holder / carrier bridge support/VR3_proppipeholder(Clone)` — 4,390 triangles; LOD 0, 1. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain the side-machine/support assembly in LOD1; remove only at long range.
- `Energy Chamber / production/Distribution node 2/EC_NodePolish/EC_NodePolish(Clone)` — 5,742 triangles; LOD 0. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain this identity/silhouette-critical assembly in LOD1 and the far silhouette level.
- `Energy Chamber / production/Distribution node 2/VR3_generator/VR3_generator(Clone)` — 4,132 triangles; LOD 0, 1. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain the side-machine/support assembly in LOD1; remove only at long range.
- `Energy Chamber / production/Distribution node 2/VR3_centrifuge/VR3_centrifuge(Clone)` — 788 triangles; LOD 0, 1. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain the side-machine/support assembly in LOD1; remove only at long range.
- `Energy Chamber / production/Distribution node 2/EC_NodeVariant/EC_NodeVariant(Clone)` — 660 triangles; LOD 0. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain the side-machine/support assembly in LOD1; remove only at long range.
- `Energy Chamber / production/Core-to-node route 2/EC_ConnectorPolish/EC_ConnectorPolish(Clone)` — 2,372 triangles; LOD 0. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: keep connector interfaces through nearby LOD0; allow fine conduit extras to drop at range.
- `Energy Chamber / production/Core-to-node route 2/Existing pipe-holder / carrier bridge support/VR3_proppipeholder(Clone)` — 4,390 triangles; LOD 0, 1. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain the side-machine/support assembly in LOD1; remove only at long range.
- `Energy Chamber / production/Distribution node 3/EC_NodePolish/EC_NodePolish(Clone)` — 5,742 triangles; LOD 0. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain this identity/silhouette-critical assembly in LOD1 and the far silhouette level.
- `Energy Chamber / production/Distribution node 3/VR3_generator/VR3_generator(Clone)` — 4,132 triangles; LOD 0, 1. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain the side-machine/support assembly in LOD1; remove only at long range.
- `Energy Chamber / production/Distribution node 3/VR3_centrifuge/VR3_centrifuge(Clone)` — 788 triangles; LOD 0, 1. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain the side-machine/support assembly in LOD1; remove only at long range.
- `Energy Chamber / production/Core-to-node route 3/EC_ConnectorPolish/EC_ConnectorPolish(Clone)` — 2,372 triangles; LOD 0. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: keep connector interfaces through nearby LOD0; allow fine conduit extras to drop at range.
- `Energy Chamber / production/Core-to-node route 3/Existing pipe-holder / carrier bridge support/VR3_proppipeholder(Clone)` — 4,390 triangles; LOD 0, 1. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain the side-machine/support assembly in LOD1; remove only at long range.
- `Energy Chamber / production/Distribution node 4/EC_NodePolish/EC_NodePolish(Clone)` — 5,742 triangles; LOD 0. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain this identity/silhouette-critical assembly in LOD1 and the far silhouette level.
- `Energy Chamber / production/Distribution node 4/VR3_generator/VR3_generator(Clone)` — 4,132 triangles; LOD 0, 1. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain the side-machine/support assembly in LOD1; remove only at long range.
- `Energy Chamber / production/Distribution node 4/VR3_centrifuge/VR3_centrifuge(Clone)` — 788 triangles; LOD 0, 1. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain the side-machine/support assembly in LOD1; remove only at long range.
- `Energy Chamber / production/Distribution node 4/EC_NodeVariant/EC_NodeVariant(Clone)` — 660 triangles; LOD 0. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain the side-machine/support assembly in LOD1; remove only at long range.
- `Energy Chamber / production/Core-to-node route 4/EC_ConnectorPolish/EC_ConnectorPolish(Clone)` — 2,372 triangles; LOD 0. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: keep connector interfaces through nearby LOD0; allow fine conduit extras to drop at range.
- `Energy Chamber / production/Core-to-node route 4/Existing pipe-holder / carrier bridge support/VR3_proppipeholder(Clone)` — 4,390 triangles; LOD 0, 1. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain the side-machine/support assembly in LOD1; remove only at long range.
- `Energy Chamber / production/Service architecture / source stairs and rails/Service stair 1/Platform_Stairs_4(Clone)` — 524 triangles; LOD 0, 1. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain this identity/silhouette-critical assembly in LOD1 and the far silhouette level.
- `Energy Chamber / production/Service architecture / source stairs and rails/Service stair 2/Platform_Stairs_4(Clone)` — 524 triangles; LOD 0, 1. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain this identity/silhouette-critical assembly in LOD1 and the far silhouette level.
- `Energy Chamber / production/Service architecture / source stairs and rails/Service quadrant safety rail 1/Prop_Rail_Round_Big(Clone)` — 1,872 triangles; LOD 0. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain this identity/silhouette-critical assembly in LOD1 and the far silhouette level.
- `Energy Chamber / production/Service architecture / source stairs and rails/Service quadrant safety rail 2/Prop_Rail_Round_Big(Clone)` — 1,872 triangles; LOD 0. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain this identity/silhouette-critical assembly in LOD1 and the far silhouette level.
- `Energy Chamber / production/Service architecture / source stairs and rails/Service quadrant safety rail 3/Prop_Rail_Round_Big(Clone)` — 1,872 triangles; LOD 0. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain this identity/silhouette-critical assembly in LOD1 and the far silhouette level.
- `Energy Chamber / production/Service architecture / source stairs and rails/Service quadrant safety rail 4/Prop_Rail_Round_Big(Clone)` — 1,872 triangles; LOD 0. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: retain this identity/silhouette-critical assembly in LOD1 and the far silhouette level.

## Checkpoint B proposal

Rebalance local thresholds to **.26 / .13 / .035**, keeping automatic LOD selection and the existing bounds. LOD1 retains the guard rails, full top/cap assembly, node outer detail, asymmetric side details and all major machinery/support. LOD2 retains guard silhouette, stairs, chamber top/cap and the node outer frames. Fine plinth/conduit dressing may disappear at range. At Android default bias .7, estimated reference distances become 34.7 / 69.4 / 257.8 m versus 23.7 / 30.1 / 112.8 m before. Camera far clipping still applies. Actual captured membership will be verified after correction. No global quality override is proposed.

## Executed evidence

- Unity inspection/compile exit 0 (`DiagnosisInventory.log`, local ignored log).
- Real Chapter camera PlayMode sweep: 1 passed, 0 failed (`verification/DiagnosisCamera.xml`).
- Machine-readable per-Renderer bounds/material/LOD inventory and actual camera visibility receipts under `verification/diagnosis/`.
- Eight normal captures plus two diagnostic LOD0 controls under `screenshots/diagnosis/`.
- Original FBX/source/scene/layout SHA-256 freeze manifest stored before the fix.
- Phone and concept copies carry SHA-256/source receipts; original files in Builds/Android are untouched.

This checkpoint is diagnosis only. APK56 and human acceptance remain pending.
