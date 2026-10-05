# GRAVIVORE — Visual Replacement Proof V1

Branch scope: `art/visual-replacement-proof-v1`

> **STATUS: TECHNICAL CONCEPT ONLY — NO VISUAL QUALITY CLAIM**
>
> **VISUAL PROOF CANNOT BE RENDER-VERIFIED IN THIS ENVIRONMENT.**

This is an isolated art-direction technical proof, not a Chapter01 replacement pass. It does not modify production Chapter01 composition, Phase4 repeatable-loop/save work, map/minimap integration, or production Audio/VFX wiring.

## Verification boundary

The execution environment used for this branch does not contain Unity 6000.3.0f1. Therefore the following have **not** been executed and must not be presented as verified:

- Unity compilation of the proof scene;
- Play Mode execution;
- gameplay-camera rendering;
- generation of the ten required PNG evidence frames;
- generation of runtime `MobileMetrics.md`;
- DEV APK / standalone proof build;
- visual A/B/C quality judgement from rendered output.

No rendered evidence is committed under `Documentation/Art/VisualReplacementProofV1/Evidence/` because no genuine Unity render exists in this environment. Placeholder, synthetic, reconstructed, or non-Unity evidence must not be substituted.

## Asset sources / licenses

No external assets are used in Visual Replacement Proof V1.

No Sketchfab, Kenney, Asset Store, franchise/IP, mirrored, login-gated, or otherwise externally sourced mesh/texture/audio asset is introduced by this branch.

G-0, Scout, Cutter, industrial proof geometry and deterministic surface textures are project-authored from `VisualProofMeshFactory.cs` and the `VisualReplacementProof*.cs` composition code. They are fully modifiable inside the project and carry no third-party redistribution restriction.

## Construction approach

This branch is **primarily procedural/code-generated geometry**, not a production-quality authored/source-asset art pass.

Visual geometry is generated through multi-ring lofts, convex profile extrusions, routed tubes, toroidal mechanical rings and authored grate topology. The proof visual code does not call `GameObject.CreatePrimitive`, use `PrimitiveType`, reuse Phase3B/Phase3D actor meshes, or use `MeshCollider`.

That distinction is technically useful, but it does **not** establish that the rendered result escapes the cubic/toy-like/simple-kit/procedural visual language rejected by the owner. A more complicated procedural mesh is not automatically a visual improvement.

Simple `BoxCollider` / `CapsuleCollider` components are used only as collision proxies.

## G-0 technical candidate

The code constructs a tapered torso shell, separate chest armor and rear power spine, sensor cowl/lens, cyan core collar, broad shoulder pauldrons, visible shoulder/elbow/hip/knee joints, tapered limbs, directional feet and secondary armor fins.

Sockets: `Socket_Attack_Right`, `Socket_Hit_CenterMass`.

These are source-level construction facts only. No claim is made that the final gameplay-camera silhouette reaches the approved concept quality.

## Scout technical candidate

The code constructs a forward-raked hound-like hull with a projecting sensor prow, bright sensor/core, two reverse-joint mechanical legs, dorsal and flank armor, paired rear thruster forms and distinct leg guards.

Sockets: `Socket_Hit_Core`, `Socket_Death_Center`.

No rendered-quality claim is made.

## Second ordinary enemy — Cutter

The code constructs a broader, lower ram/cutting profile intended to contrast with Scout, with a frontal plow, paired cutting blades, lateral drive housings/rings, cooling hardware and warm core.

Sockets: `Socket_Hit_Core`, `Socket_Death_Center`.

No rendered-quality claim is made.

## Environment technical proof

The compact arena code includes nine layered/chamfered floor plates, circular combat-center seams, two grate lanes, a containment reactor/generator, routed cooling pipes and cables, a five-module structural wall/barrier family, a pressure-vessel/manifold/control-console prop cluster and peripheral service details. The combat center remains open.

This is still procedurally assembled/project-authored environment geometry rather than a production environment built from high-quality authored/source 3D assets.

## Materials and lighting

Shared URP/Lit families distinguish armor gunmetal, painted armor, armor edge metal, dark joints/rubber, structural metal, worn paint, floor, grate/edge metal, heat-stressed machinery, cable rubber, cyan functional energy and warm heat/danger energy.

Selected surfaces are intended to receive deterministic 128–256 px runtime surface variation. The proof also encodes a cool directional key, cyan reactor accent, warm machinery accent, dark ambient/fog and a local URP volume.

Because Unity rendering was not executed, the actual material response, texture mapping, post-processing, contrast and lighting quality are unverified.

## Physicality

Collision proxies are encoded for the arena floor (`BoxCollider`), reactor (`CapsuleCollider`), barrier modules (`BoxCollider`) and industrial prop cluster (`BoxCollider`). No `MeshCollider` is used.

## Attack / death compatibility

Production Audio/VFX remains outside this branch. `PROOF_ONLY_AttackReadabilityProxy` and `PROOF_ONLY_DeathReadabilityProxy` are review-space proxies only. They are not replacement production VFX.

## Camera and requested evidence contract

The proof encodes a portrait 1080×1920 review camera, offset `(0, 14.8, -11.2)`, FOV `46`, and requests these outputs from Unity:

1. `01_G0_GameCamera.png`
2. `02_Scout_GameCamera.png`
3. `03_SecondEnemy_GameCamera.png`
4. `04_G0_Vs_Scout.png`
5. `05_CombatArea_GameCamera.png`
6. `06_ReactorAndProps.png`
7. `07_WallBarrierFamily.png`
8. `08_Combat_Readability.png`
9. `09_Death_Readability.png`
10. `10_Wide_Proof_Area.png`
11. `MobileMetrics.md`

These files are **not currently available as verified evidence**.

## Source-level checks only

The branch can only claim source-level facts:

- proof remains isolated under `Assets/_Game/VisualReplacementProofV1` plus documentation;
- no production Chapter01 or Phase4 file is modified by the proof implementation;
- no external asset is introduced;
- no `CreatePrimitive` / `PrimitiveType` visual construction is used;
- no `MeshCollider` is used;
- mandatory camera values and requested capture names are encoded;
- screenshot and runtime metric generation code is encoded.

## Quality status

There is **no A/B/C visual self-grade** because no actual Unity gameplay-camera renders were produced in the available environment.

The previous provisional B assessment is withdrawn.

**TECHNICAL CONCEPT ONLY — NO VISUAL QUALITY CLAIM.**

## Recommendation

Do **not** merge or scale this branch into Chapter01 as the visual replacement solution.

Recommended disposition:

1. keep PR #49 only as research/reference for procedural construction, camera framing, collision/socket experiments and proof automation ideas;
2. do not treat it as evidence that the approved art direction has been achieved;
3. replace the visual proof effort with a real asset-sourcing/model-production proof using production-quality authored or properly licensed source meshes/materials, followed by actual Unity-rendered gameplay-camera evidence committed to the PR before human visual approval.

Given the owner's explicit rejection of cubic/toy-like, simple-kit and procedural-looking presentation, a source/model-production proof is the appropriate next visual-development path.
