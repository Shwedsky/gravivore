# Compact first-slice environment donor subset

Quaternius Standard, CC0. Inspect all 189 actual FBX models; approve only the following sixteen unique structural/component donors. Grade B donor subset; no whole-pack approval or final-art approval. Normals, trim texture relinking, meter scale/grid and pivots still need preparation for eventual Unity handoff.

The inspected free pack is much simpler than the approved concept machinery. Flat trim-driven panels are topology donors, not finished layered floors/walls. Add custom seam/edge/medium forms where needed. Neutral clay evidence makes this limitation visible.

All 189 modules have separate silhouette/topology/rig/animation/material/mobile/fit/donor grades in `data/environment_grades.json`. A B donor grade does not approve a weak stock piece as finished scenery.

## Floors

- `Platform_Metal2` — 24 triangles, 1 material(s); UVMap. Exact source: `.asset-intake-tmp/environment/Modular SciFi MegaKit[Standard]/Modular SciFi MegaKit[Standard]/FBX/Platforms/Platform_Metal2.fbx`.
- `Platform_Window_Thin` — 460 triangles, 4 material(s); UVMap. Exact source: `.asset-intake-tmp/environment/Modular SciFi MegaKit[Standard]/Modular SciFi MegaKit[Standard]/FBX/Platforms/Platform_Window_Thin.fbx`.

## Walls

- `WallAstra_Straight` — 355 triangles, 5 material(s); UVMap. Exact source: `.asset-intake-tmp/environment/Modular SciFi MegaKit[Standard]/Modular SciFi MegaKit[Standard]/FBX/Walls/WallAstra_Straight.fbx`.
- `WallAstra_Straight_Divided` — 328 triangles, 5 material(s); UVMap. Exact source: `.asset-intake-tmp/environment/Modular SciFi MegaKit[Standard]/Modular SciFi MegaKit[Standard]/FBX/Walls/WallAstra_Straight_Divided.fbx`.

## Corners

- `WallAstra_Corner_Square_Inner` — 730 triangles, 5 material(s); UVMap. Exact source: `.asset-intake-tmp/environment/Modular SciFi MegaKit[Standard]/Modular SciFi MegaKit[Standard]/FBX/Walls/WallAstra_Corner_Square_Inner.fbx`.
- `WallAstra_Corner_Square_Outer` — 686 triangles, 5 material(s); UVMap. Exact source: `.asset-intake-tmp/environment/Modular SciFi MegaKit[Standard]/Modular SciFi MegaKit[Standard]/FBX/Walls/WallAstra_Corner_Square_Outer.fbx`.

## Barriers

- `Prop_Rail_4` — 920 triangles, 1 material(s); UVMap. Exact source: `.asset-intake-tmp/environment/Modular SciFi MegaKit[Standard]/Modular SciFi MegaKit[Standard]/FBX/Props/Prop_Rail_4.fbx`.
- `Prop_Rail_Round_Small` — 1,592 triangles, 1 material(s); UVMap. Exact source: `.asset-intake-tmp/environment/Modular SciFi MegaKit[Standard]/Modular SciFi MegaKit[Standard]/FBX/Props/Prop_Rail_Round_Small.fbx`.

## Columns

- `Column_Hollow` — 2,848 triangles, 2 material(s); UVMap. Exact source: `.asset-intake-tmp/environment/Modular SciFi MegaKit[Standard]/Modular SciFi MegaKit[Standard]/FBX/Columns/Column_Hollow.fbx`.
- `Column_Large_Straight` — 924 triangles, 4 material(s); UVMap. Exact source: `.asset-intake-tmp/environment/Modular SciFi MegaKit[Standard]/Modular SciFi MegaKit[Standard]/FBX/Columns/Column_Large_Straight.fbx`.

## Pipes

- `Prop_PipeHolder` — 4,390 triangles, 3 material(s); UVMap. Exact source: `.asset-intake-tmp/environment/Modular SciFi MegaKit[Standard]/Modular SciFi MegaKit[Standard]/FBX/Props/Prop_PipeHolder.fbx`.

## Tanks

- `Prop_Barrel_Large` — 502 triangles, 2 material(s); UVMap. Exact source: `.asset-intake-tmp/environment/Modular SciFi MegaKit[Standard]/Modular SciFi MegaKit[Standard]/FBX/Props/Prop_Barrel_Large.fbx`.

## Machinery

- `Prop_Fan_Small` — 434 triangles, 3 material(s); UVMap. Exact source: `.asset-intake-tmp/environment/Modular SciFi MegaKit[Standard]/Modular SciFi MegaKit[Standard]/FBX/Props/Prop_Fan_Small.fbx`.
- `Prop_Vent_Big` — 328 triangles, 2 material(s); UVMap. Exact source: `.asset-intake-tmp/environment/Modular SciFi MegaKit[Standard]/Modular SciFi MegaKit[Standard]/FBX/Props/Prop_Vent_Big.fbx`.
- `Prop_AccessPoint` — 738 triangles, 4 material(s); UVMap. Exact source: `.asset-intake-tmp/environment/Modular SciFi MegaKit[Standard]/Modular SciFi MegaKit[Standard]/FBX/Props/Prop_AccessPoint.fbx`.

## Gate Structure

- `Column_MetalSupport` — 748 triangles, 2 material(s); UVMap. Exact source: `.asset-intake-tmp/environment/Modular SciFi MegaKit[Standard]/Modular SciFi MegaKit[Standard]/FBX/Columns/Column_MetalSupport.fbx`.
- `Column_Hollow` — 2,848 triangles, 2 material(s); UVMap. Exact source: `.asset-intake-tmp/environment/Modular SciFi MegaKit[Standard]/Modular SciFi MegaKit[Standard]/FBX/Columns/Column_Hollow.fbx`.

## Category limits and rejects

- Floors: Metal2 is an economical flat foundation; Window_Thin is an inset/trench frame requiring an opaque custom infill, not walkable glass. Do not approve featureless single plates as hero final surfaces.
- Tanks: Barrel_Large is only a small vessel/collar donor. **Full industrial tank CUSTOM REQUIRED**; no hero pressure vessel exists in this Standard ZIP.
- Pipes: PipeHolder is a compact caged pipe manifold. Dedicated bends/flanges and broader service routing are custom; do not pretend cables are pipes.
- Machinery: fan, vent and access-point shapes can form a service cluster. **Hero pump/reactor/generator CUSTOM REQUIRED**. Computer/Chest/Crate3/Crate4 are rejected from this first slice for weak/toy-like identity.
- Gate: hollow columns and structural truss only. **Hero leaf, lock/core, rails and actuator assemblies CUSTOM REQUIRED**. Flat Door_DarkMetal, Door_Simple, blocked frame and generic frame slabs are rejected as finished gates.
- Reject the aliens, all decal-only planes, thin ShortWall strips as standalone barriers, smooth basic columns, gratuitous alternate floor/wall families, crates and decorative neon duplication. Unselected modules do not receive implicit approval.
- Column_Pipes (6,480 triangles) is above repeated-module soft maximum; reject as delivered. Prefer PipeHolder (4,390) as a limited accent and lower-poly authored routing.

The subset remains local in the ignored extraction workspace. No third-party FBX is added to Unity or committed. No entire kit is promoted to production-ready status.
