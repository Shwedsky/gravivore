# GRAVIVORE Visual Production V2 — First Slice Asset Manifest

Status: sourcing checkpoint / production contract initialized.

Source of truth: Visual Replacement Production V2 preproduction package (PR #53) and approved GRAVIVORE prototype board.

This manifest intentionally tracks production maturity explicitly. No web thumbnail is sufficient to mark an asset READY. Required state progression for important donors: visual candidate -> source inspected -> license verified -> downloaded -> imported -> topology inspected -> approved for production.

| Production ID | Role | Preferred source/path | Source URL | License | Acquisition state | Blender work required | Target output | Target tris | Materials | Rig | Animation | Sockets | LOD | Owner/tool | Status | Blocking issue |
|---|---|---|---|---|---|---|---|---:|---:|---|---|---|---|---|---|---|
| G0-T0 | Player G-0 Tier 0 | Pending research; custom/hybrid favored unless donor fit is exceptional | TBD | TBD | not sourced | silhouette design, kitbash/custom modeling, retopo, UV, bake, rig, LOD, export validation | FBX + textures | TBD from production brief | <=4 target | yes | yes | weapon/VFX/interaction as required | required | Blender | sourcing | donor identity risk |
| EN-SCOUT | Scout enemy | Pending donor/AI/custom comparison | TBD | TBD | not sourced | silhouette edit, retopo/cleanup, UV/material consolidation, rig, LOD | FBX + textures | TBD | <=3 target | yes if articulated | gameplay set | attack/VFX as required | required | Blender | sourcing | candidate search |
| EN-CUTTER | Cutter enemy | Pending donor/AI/custom comparison | TBD | TBD | not sourced | blade-language edit, retopo/cleanup, UV/material consolidation, rig, LOD | FBX + textures | TBD | <=3 target | yes if articulated | gameplay set | attack/VFX as required | required | Blender | sourcing | candidate search |
| EN-MAG | Magnetar Guard elite | Pending donor + custom kitbash comparison | TBD | TBD | not sourced | heavy silhouette construction, layered armor, focal-energy assembly, retopo, bake, rig, LOD | FBX + textures | TBD | <=4 target | yes | yes | attack/VFX/core as required | required | Blender | sourcing | elite uniqueness |
| ENV-FLOOR | Modular industrial floor kit | Pending coherent environment-family selection | TBD | TBD | not sourced | modular cleanup, scale/pivot normalization, UV/material consolidation, LOD/collider proxy plan | FBX + textures | TBD | shared | no | no | none | as needed | Blender | sourcing | family coherence |
| ENV-WALL | Wall/barrier kit | Pending coherent environment-family selection | TBD | TBD | not sourced | modular cleanup, trim alignment, scale/pivot normalization, UV/material consolidation | FBX + textures | TBD | shared | no | no | none | as needed | Blender | sourcing | family coherence |
| ENV-GATE | Hero gate | Pending donor/custom comparison | TBD | TBD | not sourced | silhouette enhancement, moving-part separation if needed, pivots, UV/bake, LOD | FBX + textures | TBD | <=3 target | optional | optional | VFX/interaction if used | required | Blender | sourcing | landmark quality |
| ENV-REACTOR | Hero reactor | Custom/hybrid likely | TBD | TBD | not sourced | landmark silhouette, kitbash/custom modeling, UV/bake, material consolidation, LOD | FBX + textures | TBD | <=4 target | no unless moving | optional | core/VFX sockets | required | Blender | sourcing | generic-cylinder rejection |
| ENV-PROP-01..06 | Supporting industrial props | Pending coherent pack selection | TBD | TBD | not sourced | scale/pivot cleanup, material consolidation, light decimation/LOD where needed | FBX + textures | TBD | shared | no | no | none | as needed | Blender | sourcing | pack selection |
| MAT-STRUCT | Structural/armor/floor material families | CC0/permissive PBR preferred | TBD | TBD | not sourced | channel packing/conversion for Unity URP, 1K/2K outputs | Unity textures/materials | n/a | shared | no | no | n/a | n/a | Blender/Unity | sourcing | source verification |

## First checkpoint rules

- No runtime Unity code changes belong in this branch.
- Reject unclear licenses.
- Prefer coherent environment families over unrelated asset collage.
- Mobile targets are reviewed from gameplay camera, not portfolio close-up.
- G-0 identity has priority over procurement convenience.
