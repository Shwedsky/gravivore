# G-0 Bipedal V2.1 proportion review

2026-10-06, Europe/Moscow. Review gate only. Derived by opening the actual saved V2 Blender source; the V2 file is preserved.

## Exact revision

- Ground-to-hip rest height: 2.075 -> 1.815625 authoring units, **12.5% shorter**. Geometry below that joint is compressed vertically; upper body is lowered by 0.259375 units without compressing chest/shoulders/tools.
- Pelvis mechanisms, hip fairings and ventral prow: **12% wider**. Thigh mechanisms/shields: **20% wider, 16% deeper** about their mechanical centerline. Lower-leg shin structure: **20% wider, 18% deeper**. Hock cover: **12% wider/deeper**. Lower-body centerlines move outward by 0.02 units each.
- Sensor: **12% wider, 10% deeper, 8% taller**, about the existing sensor carriage. Shoulders, split chest, recessed cyan core, open mechanical waist, rear containment and asymmetric gravity tools retain their identity.
- Existing split toe/foot components: **8% wider, 10% deeper**. Eight original parts added: heel stabilizer, instep bridge, forward sole brace and heel counterplate on each foot. Two supporting feet remain.
- No final textures, UVs, LODs or animation. Existing 16-bone rigid blockout skeleton retained; rest positions follow the height change. This is not production rig certification.

## Measured source

V2 evaluated dimensions, X width / Y depth / Z height: **2.062500 / 1.753956 / 3.605000**.

V2.1: **2.062500 / 1.753808 / 3.358425**. Overall height falls 6.84%; leg length falls 12.5%. Sensor enlargement slightly offsets the upper-body lowering. Overall width remains unchanged because the shoulders and unequal forearm tools still define the widest points.

V2.1: **142 mesh objects, 159 total hero objects, 29,417 evaluated triangles**. V2: 134 meshes, 151 hero objects, 24,705 evaluated triangles. Counts exclude hidden donor reference, studio and cameras. The added feet and fuller chamfered limb shapes account for the evaluated triangle increase. The 17 adapted Catfish mechanisms remain attributed to Jungle Jim under CC BY 4.0.

## Normalization and deliverables

- Blender 5.2.2 LTS; metric units, unit scale 1. **Z up, -Y forward**.
- Root at `(0,0,0)`; every hero object has scale `(1,1,1)`, zero object rotation and no negative scale. Evaluated feet contact Z=0.
- Source is not resized to Unity gameplay dimensions. Unity presentation fit is a separate transform, documented in the scale review.
- Editable source: `art/visual-production-v2/g0/G0_Bipedal_Blockout_V21.blend`.
- Static evaluated review export: `Assets/_Game/ArtReview/G0V21/Models/G0_Bipedal_V21_Review.fbx`. No rig/animation/camera/reference exported. Unity uses Y up, **-Z forward**, verified from the imported chest-center Z=-0.29 versus positive-Z rear containment. The review faces the camera with zero yaw.
- Reproduction: `Tools/g0-bipedal-v2/refine_v21.py`, `render_v21.py`, `verify_v21.py`.
- Exact metrics: `data-v21/proportion_metrics.json`; independent reopened-file checks: `data-v21/blend_reopen_validation.json`.

## Actual Blender captures

All five are 1200x1400 Cycles renders of the saved V2.1 source, 32 samples, denoised; no image generation or painted changes.

1. `evidence-v21/blender/01_G0_V21_front.png`
2. `evidence-v21/blender/02_G0_V21_side.png`
3. `evidence-v21/blender/03_G0_V21_back.png`
4. `evidence-v21/blender/04_G0_V21_threequarter.png`
5. `evidence-v21/blender/05_G0_V21_black_silhouette.png`

Visual judgment: the lower body now carries more of the upper body's mechanical weight while retaining an athletic bipedal contour. Split foot contacts and rear stabilizers make the support path more purposeful. Some actuator/shell intersections remain blockout compromises. No claim of final mesh, deforming gait or device performance approval.
