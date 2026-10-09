# GRAVIVORE current visual target

Status: **CURRENT / AUTHORITATIVE**

## One-line target

GRAVIVORE is a **mobile-optimized premium hard-surface industrial sci-fi game** with dense functional machinery, readable top-down combat, authored PBR surfaces, strong silhouettes and restrained energy accents.

It is **not** a low-poly-aesthetic game, a primitive/proxy-art game, a toy-mech game, or a generic marketplace-kit showcase.

## Primary concept authority

The owner's approved GRAVIVORE concept board is the artistic target for world density, industrial composition, enemy hierarchy, lighting mood, materials, combat readability and presentation quality.

Current reference names used during development include:
- `Концепт GRAVIVORE: Мехи и окружение.png`
- the approved Scout / Cutter / Magnetar visual-target board when available

If a binary reference is not available inside a Codex execution environment, **do not replace the concept with an older phase document**. Use this file, `ART_DIRECTION.md`, the approved enemy target document, and the current explicit task. Record the missing binary as an evidence limitation.

### Explicit G-0 exception

The old concept-board G-0 anatomy is superseded. Current G-0 remains a **bipedal robotic combat mech** with torso, head/visor, arms, mechanical legs and visible weapon/equipment presentation.

Everything else in the concept board remains a target unless superseded by a newer explicit owner decision.

## Visual quality bar

Production presentation should read as authored game art at the real portrait gameplay camera:

- serious mechanical silhouettes, not cubes with attachments;
- functional industrial structures with believable support, access, service routing and purpose;
- layered hard-surface forms: primary mass, secondary structure, tertiary functional detail;
- clear material separation inside the same object;
- PBR response with base color, normal relief, metallic/roughness or smoothness variation, occlusion and localized emission where appropriate;
- restrained wear, edge exposure, recesses, seams, fasteners, vents, heat or damage where the object function supports them;
- cyan/blue energy for player/neutral/service technology;
- red/orange energy for hostile/dangerous machinery;
- no neon used as a substitute for geometry or material definition;
- no large clean default planes or sparse prop scattering as a finished environment solution;
- no stock-donor identity left obvious in hero actors or major landmarks.

A shared atlas/material vocabulary is encouraged, but it must preserve within-object surface richness. Material consolidation must not collapse an authored object into a flat single-color toy surface.

## World composition

Chapter spaces should feel like parts of a functioning or damaged industrial complex, not decorated spawn circles.

Use large meaningful structures when they improve the concept:
- fabrication / deployment facilities;
- maintenance bays and docks;
- reactors, generators and containment machinery;
- gates, wall systems and service superstructures;
- cranes, gantries, tanks, conduits, trenches and damaged hulls;
- large wrecks and industrial landmarks.

Gameplay anchors define behavior and topology; they do **not** cap the visual ambition of the surrounding presentation. A spawn point may visually live inside or beside a substantial facility while spawn authority remains data-driven elsewhere.

Keep combat centres, telegraphs, target readability and required traversal clear. Density should concentrate around edges, structures, transitions and functional landmarks rather than filling the fighting lane with clutter.

## Actor hierarchy

Enemy roles must be distinguishable by silhouette and function before color:

- ordinary enemies: clear role-specific massing and attack source;
- elite: materially and mechanically more complex than ordinary units, not merely scaled up;
- boss: arena-defining machinery with unique mass, weapon causality and readable telegraphs.

See `docs/visual-production-v2/ENEMY_VISUAL_TARGETS_APPROVED_V1.md` for the approved Scout/Cutter/Magnetar rules. Current G-0 bipedal anatomy remains separate from hostile radial/spider design language.

## Equipment and attack presentation

Equipped weapons/modules must be visibly present on G-0 at the gameplay camera and visibly affect attack source/presentation. Color-only or tiny invisible equipment changes are insufficient for meaningful equipment progression.

Source -> travel -> impact causality must remain readable against the denser world.

## UI target

UI must feel like the same modern industrial sci-fi product as the world:

- restrained frames and hierarchy;
- crisp readable typography;
- consistent inventory/equipment/minimap/boss treatment;
- limited luminous area;
- no broad plain prototype rectangles as final presentation;
- no visual treatment that evokes an old desktop/game UI merely because it is easy to build with stock uGUI shapes.

Interaction semantics, localization, safe areas and accessibility remain authoritative.

## Performance is a device budget, not an art style

Target 60 FPS on a reasonable mid-range Android device.

Use LOD, shared materials, atlases, batching, occlusion/culling, baked or emissive lighting, optimized shaders, texture compression and bounded VFX to meet that target.

Triangle/material/light numbers in old phases are historical integration guardrails, not permanent artistic ceilings. A hero structure or boss may use more geometry than an old prototype budget when the real device result remains healthy and the visual gain is material.

Prefer fewer meaningful authored forms over hundreds of tiny renderer objects.

## Lighting

Do not interpret a small dynamic-light budget as a requirement for a flat scene.

Use the cheapest suitable combination of:
- directional light;
- baked/lightmapped contribution where practical;
- emissive surfaces;
- reflection probes;
- decals/material gradients;
- bounded unshadowed local lights when justified;
- VFX lighting cues without permanent realtime-light spam.

Lighting must reveal form and material while preserving combat readability.

## Production vs proxy

Blockouts, primitive rigs, procedural proxy geometry and quick kitbashes are valid for exploration and technical proof.

They are **not acceptable as final production presentation** when the task asks to reach the current visual target. A production pass must replace or substantially refine obvious proxy language rather than merely recolor it.

## Acceptance gate

The final visual acceptance gate is the **real playable APK on device at the production portrait camera**.

Static renders, Blender views, automated audits and screenshots are supporting evidence only.

A visual pass is not A-level merely because it compiles, meets a triangle budget, has five different silhouettes, or removes primitive cubes. It must materially close the gap to the approved concept and stop reading as toy-like, sparse, generic, procedural or prototype art.
