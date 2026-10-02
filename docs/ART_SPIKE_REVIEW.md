# GRAVIVORE Art Spike V3 — visual review

**ART SPIKE V3 VISUAL REVIEW: PENDING**

V2 established the approved silhouette direction, but its unfinished CC BY-SA donor was unsuitable for the selected V3 pipeline. The current V3 uses the expressly authorized autonomous fallback: **original temporary modular proxy geometry with real CC0 PBR surface maps**. This is not a claimed acquired Vanguard/game-ready character. All ten images below are new Unity/URP renders, without Julius geometry.

## Images

1. [01_G0_Evolution.png](art-spike/images/01_G0_Evolution.png) — 1920×1080, Tier 0 → Tier 1 → Tier 2 at equal root scale; common core and four supports, one-metre reference.
2. [02_G0_GameplayScale.png](art-spike/images/02_G0_GameplayScale.png) — 1080×1920, Tier 1 alone at the unchanged settled S20 camera.
3. [03_Enemy_GameplayScale.png](art-spike/images/03_Enemy_GameplayScale.png) — 1080×1920, cyan G-0 left, red asymmetric Cutter upper right.
4. [04_Environment_Overview.png](art-spike/images/04_Environment_Overview.png) — 1920×1080, existing CC0 Kenney supporting bay.
5. [05_Gameplay_Mock.png](art-spike/images/05_Gameplay_Mock.png) — 1080×1920, Tier 2 and Cutter in the bay at S20 scale; no HUD/combat simulation.
6. [06_ScaleReference.png](art-spike/images/06_ScaleReference.png) — 1920×1080, closer same-scale player/enemy comparison with one-metre ruler.
7. [07_Evolution_S20Scale.png](art-spike/images/07_Evolution_S20Scale.png) — 3240×1920, three actual full portrait panels from the unchanged S20 camera.
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
- **Gameplay readability:** image 07 preserves the exact S20 camera. Conservative projected bounding boxes are about 215×258 / 281×261 / 361×294 pixels for T0/T1/T2 at 1080×1920. Forks/width differentiate progression; fine scratches and pistons are primarily close-view details.
- **Renderers:** V2 T0/T1/T2/Cutter = 13/18/25/9. V3 = **19/24/33/13**, below 30/35/40/25 targets. More independent modules increase submissions; V3 is not presented as a renderer-count optimization.
- **Triangles:** V2 = 6,252/6,900/8,292/4,104; V3 = **5,928/6,936/9,216/3,352**. T2 adds 924 triangles; each remains below 50K.
- **License:** selected V2 character adaptations/images carried CC BY-SA 3.0. Current V3 geometry is independently authored and external surfaces/scenery are CC0, with no share-alike character dependency. Historical V2 remains under its original terms in Git.

## Limitations and replacement plan

The character geometry is still a temporary proxy, not production-ready final art. Repeated modules, planar UV scale/distortion, simple hard bevels and a generic surface texture are visible at close range. A final artist pass or obtainable permissive donor must provide refined chassis/limb/weapon geometry and authored UVs while preserving the core and socket contract. The three-second isolated idle proves independent pivot motion only; gait, IK, ground contact and combat animation are unimplemented.

No Android frame-time claim is made. Main prefabs are static, collider-free and unbound to production S15/S07/Chapter 01. Full Chapter integration and other enemies remain future work after human approval.

See [candidate evidence](ART_ASSET_SHORTLIST.md), [performance](art-spike/PERFORMANCE.md), [license scope](art-spike/ART_LICENSE.md), [verification](art-spike/VERIFICATION.md) and [report](ART_SPIKE_REPORT.md).

**SHARE-ALIKE CHARACTER DEPENDENCY: NO**

**ART SPIKE V3 VISUAL REVIEW: PENDING**
