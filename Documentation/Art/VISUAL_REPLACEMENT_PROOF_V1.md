# GRAVIVORE — Visual Replacement Proof V1

Branch scope: `art/visual-replacement-proof-v1`

This is an isolated art-direction proof, not a Chapter01 replacement pass. It does not modify production Chapter01 composition, Phase4 repeatable-loop/save work, map/minimap integration, or production Audio/VFX wiring.

## Asset sources / licenses

No external assets are used in Visual Replacement Proof V1.

No Sketchfab, Kenney, Asset Store, franchise/IP, mirrored, login-gated, or otherwise externally sourced mesh/texture/audio asset is introduced by this branch.

G-0, Scout, Cutter, industrial proof geometry and deterministic surface textures are project-authored from `VisualProofMeshFactory.cs` and the `VisualReplacementProof*.cs` composition code. They are fully modifiable inside the project and carry no third-party redistribution restriction.

## Construction rule

Visual geometry is authored through multi-ring lofts, convex profile extrusions, routed tubes, toroidal mechanical rings and authored grate topology. The proof visual code does not call `GameObject.CreatePrimitive`, use `PrimitiveType`, reuse Phase3B/Phase3D actor meshes, or use `MeshCollider`.

Simple `BoxCollider` / `CapsuleCollider` components are used only as collision proxies.

## G-0 candidate

The G-0 candidate uses a tapered torso shell, separate chest armor and rear power spine, sensor cowl/lens, cyan core collar, broad shoulder pauldrons, visible shoulder/elbow/hip/knee joints, tapered limbs, directional feet and secondary armor fins. It is designed to read as an articulated mech at the gameplay camera rather than as a rectangular body block.

Sockets: `Socket_Attack_Right`, `Socket_Hit_CenterMass`.

## Scout candidate

Scout uses a forward-raked hound-like hull with a projecting sensor prow, bright sensor/core, two reverse-joint mechanical legs, dorsal and flank armor, paired rear thruster forms and distinct leg guards. It is intentionally not a spider assembled from blocks.

Sockets: `Socket_Hit_Core`, `Socket_Death_Center`.

## Second ordinary enemy — Cutter

Cutter was chosen to demonstrate clear contrast with Scout. Scout is narrower/taller and pursuit/sensor-led; Cutter is broad, low and ram-like, with a frontal plow, paired cutting blades, lateral drive housings/rings, cooling hardware and warm core. Both use the same material/mechanical vocabulary.

Sockets: `Socket_Hit_Core`, `Socket_Death_Center`.

## Environment proof

The compact arena includes nine layered/chamfered floor plates, circular combat-center seams, two grate lanes, a substantial containment reactor/generator, routed cooling pipes and cables, a five-module structural wall/barrier family, a pressure-vessel/manifold/control-console prop cluster and peripheral service details. The combat center remains deliberately open.

## Materials

Shared URP/Lit families distinguish armor gunmetal, painted armor, armor edge metal, dark joints/rubber, structural metal, worn paint, floor, grate/edge metal, heat-stressed machinery, cable rubber, cyan functional energy and warm heat/danger energy.

Selected surfaces receive deterministic 128–256 px runtime surface variation for grain, seams, scuffs, paint chips, brushing or heat banding. There are zero imported texture bytes in V1.

## Lighting

The proof uses one cool directional key with soft shadows, one cyan reactor point accent without shadows, one warm machinery spot with soft shadows, dark ambient/fog, plus a local URP volume using ACES, restrained bloom, contrast and a light vignette. No realtime-light explosion is introduced.

## Physicality

Collision proxies are included for the arena floor (`BoxCollider`), reactor (`CapsuleCollider`), each barrier module (`BoxCollider`) and the industrial prop cluster (`BoxCollider`). No `MeshCollider` is used.

## Attack / death compatibility

Production Audio/VFX remains outside this branch. `PROOF_ONLY_AttackReadabilityProxy` and `PROOF_ONLY_DeathReadabilityProxy` are disabled during normal presentation and enabled only for screenshots 08/09 to prove socket placement, line-of-action visibility and contrast/clearance for existing production effects. They are not replacement production VFX.

## Camera

Mandatory review camera: portrait 1080×1920, offset `(0, 14.8, -11.2)`, FOV `46`. The target point varies per review frame but the offset and FOV do not.

## Mobile metrics

The proof scene writes exact runtime metrics to `Artifacts/VisualReplacementProofV1/MobileMetrics.md` for G-0, Scout and Cutter: triangles, renderer count, material slots, unique materials, procedural texture sizes, mesh mode and animation clip count.

V1 uses static MeshFilter/MeshRenderer presentation meshes and no animation clips. Polygon count is intentionally kept in the low-thousands regime and is not used as the primary quality lever.

## Gameplay-camera screenshots

Entering Play Mode in `Assets/_Game/VisualReplacementProofV1/VisualReplacementProofV1.unity` writes:

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

## Verification state

Source-level checks completed:

- proof remains under `Assets/_Game/VisualReplacementProofV1` plus this document;
- no production Chapter01 or Phase4 file is modified;
- no external asset is introduced;
- no `CreatePrimitive` / `PrimitiveType` visual construction;
- no `MeshCollider`;
- mandatory camera values and requested capture names are encoded;
- screenshot and runtime metric generation are encoded.

The current execution environment does not contain Unity 6000.3.0f1, so compile, Play Mode render, screenshot generation and DEV/standalone build cannot honestly be reported as executed here. The repository itself targets Unity 6000.3.0f1 and URP 17.3.0.

## A / B / C self-grade

**B — provisional, human review required.**

The construction language has changed rather than polishing the old primitive inventory; silhouettes are intentionally different across G-0, Scout and Cutter; environment composition has structural hierarchy and routed utilities; material/light hierarchy follows graphite + cyan + restrained warm accents; collision and effect sockets are designed in; complexity remains mobile-oriented.

This is not upgraded to A because A requires viewing the actual ten gameplay-camera frames on the target renderer/device. If those generated frames still read toy-like, the correct outcome is C and scale-up must stop.

## Review procedure

Open the proof scene, enter Play Mode, confirm no compile/runtime exception, inspect `Camera_Proof_GameplayPortrait`, then review the generated files under `Artifacts/VisualReplacementProofV1/` against the approved concept direction—not against Phase3D. No Chapter01 wiring is required for this review.
