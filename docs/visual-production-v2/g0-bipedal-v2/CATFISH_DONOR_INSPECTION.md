# Catfish donor inspection

2026-10-06. Imported and measured in Blender **5.2.2 LTS**, build d13f752e3b9c. Source import succeeded; no substitute source was used.

## Actual archive

- Filename: `catfish-mech-low-poly-animated.zip`.
- Absolute path: `C:/Users/pamak/Documents/GRAVIVORE_ASSET_INTAKE/g0/catfish-mech-low-poly-animated.zip`.
- Bytes: 175998644.
- SHA256: `3c20dd24a1611d8391c4d430d8f920dc67969d641cbb93811514cb627ccf98c6`.
- Contents: one binary FBX 7200, `source/Mech long legs Army CC export.fbx` (87818720 bytes), plus 24 JPEG texture maps under `textures/`. No license/readme is bundled. The full entry list, sizes, object hierarchy, transforms, material/image nodes, UV layers and bone hierarchy are in [catfish_inspection.json](data/catfish_inspection.json).
- Local extraction and imported source blend are ignored under `.local-g0-v2/`; the raw archive is not committed.

## Provenance

[Catfish Mech low-poly (animated)](https://sketchfab.com/3d-models/catfish-mech-low-poly-animated-ad9bc16464744935b1ac9b7768a17474) by [Jungle Jim](https://sketchfab.com/jungle_jim), **CC BY 4.0**, https://creativecommons.org/licenses/by/4.0/.

The original listing was retrieved through search. Direct page rendering returned HTTP 403. The original public API was successfully retrieved on 2026-10-06 and explicitly identifies the author, title, downloadable state and BY 4.0 URL. Exact response: [catfish_official_metadata.json](data/catfish_official_metadata.json). This is attribution evidence, not an endorsement. Modifications and exact retained components are documented in the design decision and ThirdPartyNotices.

## Measured source

- Objects: **54**: 53 meshes and one `Armature`.
- Mesh triangles: **14650**; imported vertices: **8141**. Official listing/API also reports 14650 triangles and 8127 vertices. The vertex difference is consistent with import splitting; its exact cause was not independently proven.
- Seven assigned materials: `camo_metal_armor_30_98`, `green_metal_armor_30_1100`, `green_metal_armor_30_99`, `green_painted_metal_30_26`, `gun_black_metal_30_05`, `scratched_metal_30_28`, `Material.001`.
- 24 provided diffuse/normal/metallic/AO/roughness JPEG maps, largely 4096 square. FBX carries 18 image references with stale author-machine `D:/Temp folder Reallusion/...` paths and embedded data. The inspection/preview remaps used diffuse/normal nodes to actual archive textures by matching basename stems, including `.jpg`/`.jpeg` differences. Stale unused packed references emit save warnings; source import itself works.
- All **53** meshes have UV layer(s). UVs are inherited authoring data, not repacked/polished here.
- Armature: **71 bones**, CC/Reallusion-style names; root `RL_BoneRoot`, hip/pelvis, thigh/calf/twist/foot/toe chains, spine, clavicle/arm chains, and unnecessary facial/eye/tongue/breast bones. Full heads/tails/parents are recorded in JSON.
- Meshes are parented to `Armature`, with vertex groups and Armature modifiers. Large combined armor mesh `mesh_rep_0_ori_repair_106`: 7912 triangles. Objects have opaque repair-number naming.
- Animation: **one skeletal take**, `Armature|2666562703104_TempMotion`, frames **1–912**, imported **60 fps** (~15.18 seconds between endpoints). Additionally **53 shape-key actions** share the take/range. Do not call those 54 independent locomotion clips. No named walk/run/idle split is present. No retarget/playability claim is made for G-0.
- Initial imported animated-frame bounds in Blender world units: min (-0.346013, -0.366018, -0.023358), max (0.373670, 0.208088, 0.940480), ~0.964 height. Imported scale conversion places centimeter-style bone coordinates into sub-meter world coordinates; it is not assumed production meter scale.
- Orientation: Z up, front toward **-Y**, as verified in the actual donor render. Review source is put in rest pose, normalized by its actual rest-pose bounds and grounded; no source silhouette redesign in comparison.

## Topology and mobile suitability

Source mesh analysis: **1673 boundary edges**, **1703 nonmanifold edges** (includes boundaries), **2 near-zero-area faces**, **0 loose vertices**. Open hard-surface panels and intersecting mechanical parts are normal for this donor but require selective cleanup. The mesh is not a watertight or deformation-certified production character.

14650 triangles is manageable as a hero starting point; 53 mesh objects, seven materials, 4K textures, excess bones and dense imported animation/shape-key data are unsuitable as an untouched mobile runtime import. No Android performance test is claimed. G-0 keeps only small mechanisms and replaces the texture palette, armor and rig strategy. Final batching, atlas, LOD and deformation work are deferred by the requested gate.

## Keep / discard

Keep the **17** components / **1499 source triangles** identified below, transformed into custom mechanical joint assemblies. No whole donor leg armor, feet or stock silhouette is kept.

- Pelvis: `mesh_rep_0_ori_repair_134` (405 triangles).
- Knee linkages: suffixes **135,136,137,138,139,140,152,153** (586 triangles).
- Ankle linkages: suffixes **141,142,143,144,145,146,147,148** (508 triangles).

Discard from G-0: stock combined armor 106, military torso, guns, head/body identity, shoulders/arms, back tanks, feet 149/150, other detail objects, camouflage/textures, shape keys and imported rig/actions. The unchanged donor remains comparison evidence, attributed separately from G-0's custom geometry.
