# Checkpoint B — automatic runtime visibility correction

The early, Android-bias-adjusted transitions and assembly-level membership diagnosed in checkpoint A are corrected on the same production prefab. No architecture, mesh, UV, source, transform, bounds, collider, Chapter scene, route or gameplay changes were made.

- Transition heights: **.38 / .30 / .08 → .26 / .13 / .035**. Automatic LOD selection remains enabled; no runtime ForceLOD or global quality override.
- LOD0: **45 Renderers / 279,694 triangles / 156 slots / 22 materials** (unchanged).
- LOD1: **40 / 253,846 / 135 / 22**. Retains the central guard rails, chamber top/caps, outer node details, major side machines and support. Fine foundation and conduit polish drops at range.
- LOD2: **26 / 215,286 / 115 / 21**. Retains the chamber top, guard silhouette, stairs, outer node detail and the primary machinery arrangement. Auxiliary donors, bridge supports and tiny asymmetric node variants drop at long range.
- LODGroup size/reference and every Renderer bound are unchanged. No bounds inflation or frustum workaround.
- Added geometry, Renderers and realtime lights: **0**. Far cost still falls by 9.2% / 23.0% relative to complete near geometry; this deliberately spends more distant geometry than APK55 to retain the accepted identity.

At Android default quality/bias .7, all five captured ordinary gameplay views now retain full LOD0. The equivalent west/south views have their rail, top and auxiliary assemblies restored. The 22/30/50/80 m sweep at six bias values (.3/.4/.7/1/1.5/2) verifies that in-frustum identity assemblies remain camera-visible and that Android's default genuinely selects both LOD1 and LOD2 at range. It is not an always-LOD0 policy.

Executed validation:

- Unity author/compile: exit 0.
- Actual Chapter camera and 24 quality/distance combinations: **2 PlayMode tests passed, 0 failed** (`verification/VisibilityCamera.xml`).
- Relevant architecture/visibility asset EditMode checks: see `verification/VisibilityEditMode.xml`.
- Frozen source/scene/layout and all 248 prefab records checked by `verify_freeze.py`; only LOD membership/threshold changes permitted.
- Equivalent before/after gameplay captures under `screenshots/diagnosis/` and `screenshots/visibility/`; camera and GPU visibility receipts under `verification/visibility/`.

LookDev and operational presentation follow in checkpoint C. Human device acceptance remains pending.
