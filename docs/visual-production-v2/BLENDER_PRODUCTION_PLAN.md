# Blender Production Plan — First Visual Slice

## Global delivery standard

All sourced meshes enter Blender before Unity handoff. No marketplace asset goes directly to runtime. Apply metric scale, transforms, deterministic naming, clean normals/tangents, sane pivots, bounded material slots, mobile LODs where useful, and separate collider proxies. Final export format: FBX for animated/rigged production meshes unless the project standard requires otherwise; textures delivered separately in Unity-ready PBR channels.

## G-0 Tier 0
- custom blockout from approved board;
- donor parts limited to low-identity hinges/pistons/feet/greebles;
- rebuild torso/core/front weapons manually;
- retopo to ~22–35k tris LOD0;
- unified UV/bake; 3–4 materials max;
- full mechanical rig with explicit hinge axes;
- sockets: core/VFX, left/right attack, hit/interaction anchors as runtime requires;
- LOD1 ~50–60%, LOD2 ~20–30% of LOD0;
- simple capsule/box/convex proxy plan, not render-mesh collision.

## Scout
- inspect donor hierarchy/rig first;
- shrink/simplify torso while preserving radial legs;
- replace top shell and central core housing to remove stock identity;
- reduce to ~8–18k tris LOD0;
- one body atlas + optional emissive;
- verify leg clearance during fast scuttle;
- sockets: core/hit, melee/contact VFX if needed;
- LOD1 + optional LOD2 depending screen occupancy.

## Cutter
- use donor chassis only if its locomotion chain is clean;
- delete donor tail/guns/recognizable stock front end;
- author long custom metallic blades/claws and blade-root housings;
- shift visual balance rearward to support forward blade reach;
- ~14–24k tris LOD0;
- rig blades as explicit bones/hinges if attack poses articulate them;
- sockets at blade roots/tips and core;
- bake into shared hostile material family.

## Magnetar Guard
- donor limbs/joint housings only after source inspection;
- custom torso, armor layers and dominant amber/orange containment assembly;
- broaden stance and deepen overlap layers beyond ordinary enemies;
- ~35–55k tris LOD0;
- 3–4 materials max after consolidation;
- custom rig; test sweep/area poses before final skin/weights;
- LOD1 and LOD2 mandatory;
- sockets: central core, attack emitters, weak-state exposure, impact anchors.

## Floor kit
- select only modules required for 15×20 m combat area;
- normalize to project grid;
- remove invisible underside complexity;
- unify tiling material coordinates;
- author edge trims/insets where top-down silhouette needs breakup;
- collider proxies should follow actual blocking surfaces only.

## Wall/barrier kit
- rebuild pivots for snapping;
- tune heights for camera visibility and combat readability;
- merge excessive material slots;
- add shared trim/decal support rather than unique textures per piece;
- simple box/convex colliders for walls and solid-looking barriers.

## Gate
- separate leaf/rail/piston pieces if animated;
- author frame thickness and mechanical transition so it does not read as a rectangular placeholder;
- set hinge/slide pivots precisely;
- add open/closed collision proxy variants if runtime requires them;
- ~8–18k tris target depending hero detail.

## Hero reactor
- custom/hybrid build;
- create at least three macro masses: structural outer frame, central containment/core, service/conduit layer;
- avoid single-cylinder silhouette;
- ~20–35k tris LOD0;
- shared environment metal materials + one controlled energy material;
- sockets/empties for VFX/lighting accents only where integration needs them;
- LOD1/LOD2 mandatory because it is a landmark with large screen-size range.

## Supporting props (4–6)
Choose coherent categories such as tank, pipe manifold, service console, pump/compressor, junction cabinet, maintenance barrier.
- scale/pivot cleanup;
- decimate only if source cost is excessive;
- consolidate to shared materials;
- add grime/contact wear consistent with placement;
- simple physical colliders where silhouettes imply solidity.

## Material production
- use 1K/2K CC0 PBR source maps;
- convert roughness/smoothness and pack masks for project shader;
- bake donor-specific detail into shared atlases where needed;
- no runtime displacement/tessellation requirement;
- maintain one calibrated roughness/metal response across environment and enemies.

## QA before Unity handoff
- transforms applied;
- scale verified against G-0 and environment meter reference;
- zero orphan textures/material slots;
- no hidden high-poly helpers exported;
- no negative scale in animated hierarchies;
- normals/tangents valid;
- UV0 valid, UV1 only if the runtime pipeline needs it;
- rigs contain only required deform/control bones for export;
- animation clips named consistently;
- sockets/empties deterministic;
- LOD meshes preserve silhouette at gameplay camera;
- source/license/provenance recorded in candidate register.
