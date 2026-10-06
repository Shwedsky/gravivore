# Actual source inspection results

Inspection: 2026-10-06. Blender 5.2.2 LTS (`d13f752e3b9c`). All thirteen ZIPs passed CRC integrity checks and were SHA256 hashed before safe extraction into ignored `.asset-intake-tmp/`. Originals remain in the user's intake directory. Two nested ZIPs and one RAR were also extracted and hashed. No source archive or donor geometry is committed.

Reference commits: PR #53 `478cd3e7cf0f28ffccd227a0ce9c60273f567455`; PR #54 `bfe989529a8e4057ceaade967fa45d7a8edfd6fb`; PR #55 `135a9acd9c699a79200adde19cfbe0edc76e95c7`.

Evidence: `data/archive_inventory.json` records every archive member and nested hash; `data/candidates_audit.json` and `data/environment_audit.json` record every imported object, parent, UV layer, material slot, modifier, rig, action and mesh topology. `data/physical_texture_inventory.json` records actual supplied bitmap sizes. `source-review/*contact_sheet.png` are neutral renders of these actual sources, not marketplace thumbnails.

Eight character sources and all 189 distinct non-Unity FBX modules were opened. The Standard ZIP contains 378 FBX copies, 190 glTF/BIN pairs and 191 OBJ/MTL files, not the complete 277-model paid/source pack advertised online. Equivalent format copies are not added together as unique models. The native `wm.fbx_import` handles both binary and ASCII FBX; legacy Python FBX import rejects Mirandanimator's ASCII file. At least one glTF refers to nonexistent `Decal_Line_90_001.bin`; several relative textures also fail. Use inspected FBX geometry and explicit texture relinking for any later handoff.

Boundary counts alone do not prove holes: exported glTF meshes split vertices at UV/normal seams. Degenerate faces and edges with more than two attached faces are reported separately. Audits use source geometry and evaluated modifiers at frame zero. Neutral renders expose shape, not final material quality. The COLLADA-only model uses a documented static XML adapter because native COLLADA import is unavailable; source UV/controller counts are reported separately from converted data. No rig/UV conversion is claimed for it.

Sketchfab provenance/CC Attribution follows PR #55's exact title/author/URL matching; seven live page retries returned HTTP 403, so this pass does not claim new license-page verification for those. Stalenhag's accessible page reconfirmed CC Attribution and the underlying-design credit; it is excluded. Quaternius CC0 and Poly Haven CC0 were independently reconfirmed on official pages. No licenses were bundled in the character ZIPs. Future extracted parts must carry exact object/component identity plus archive hash and attribution. Earlier PR #54's assertion that CC BY inherently prohibits unmodified redistribution is incorrect; avoiding raw archive commits is project policy.

## scorpion-mech.zip

- Category: cutter. Size: 651,730 bytes.
- SHA256: `24f42cadf310ccb89215709e8f2624da226703fa7966252a558f011344da98fc`.
- Contained formats: .fbx: 1.
- Provenance: [Scorpion mech](https://sketchfab.com/3d-models/scorpion-mech-db3225ec5dcf431d8b3b8dc328a42bbb) by gwim; CC Attribution per #55.
- Meshes: 1; base triangles: 23,808; evaluated triangles: 23,808.
- Rig: none supplied. Actions: none supplied.
- Materials: Alien Metal, JSP1, JSP2. UV: UVMap.
- Grades (silhouette / topology / rig / animation / materials / mobile / GRAVIVORE fit / donor): B / Reject / Reject / Reject / C / C / C / C.
- Review: Reject as preferred chassis: one fused Crawler mesh, 2,040 zero-area faces and 74 non-manifold edges, missing referenced textures, no rig. Forward gun/tail identity needs replacement.
- Supplied external bitmaps: 0; sizes: none. Packed/embedded maps are additionally recorded in Blender audit.

## scorpion-robot.zip

- Category: cutter. Size: 7,401,867 bytes.
- SHA256: `11c5759c11460ec2bcf5eabbf90bb526f335f219f31886cf170d615ccbce8c4b`.
- Contained formats: .fbx: 1.
- Provenance: [Scorpion Robot](https://sketchfab.com/3d-models/scorpion-robot-e44be9a622af4e85b4b10cf59b25de1a) by Mirandanimator; CC Attribution per #55.
- Meshes: 8; base triangles: 29,274; evaluated triangles: 29,274.
- Rig: none supplied. Actions: none supplied.
- Materials: Mat. UV: UVW.
- Grades (silhouette / topology / rig / animation / materials / mobile / GRAVIVORE fit / donor): B / B / Reject / Reject / B / C / C / B.
- Review: Preferred Cutter chassis donor. Eight named rigid mesh objects, UVW and one Mat material, no degenerate/non-manifold faces in audit. Delete raised tail and replace front claws/core/shell. Requires component separation and a new rig; raw 29,274 triangles exceed ordinary-enemy targets.
- Supplied external bitmaps: 0; sizes: none. Packed/embedded maps are additionally recorded in Blender audit.

## Modular SciFi MegaKit[Standard].zip

- Category: environment. Size: 48,559,316 bytes.
- SHA256: `6fae60cf5189e44dff0bd91097f094a765acc6d57d64a85a0cc0dd56e03035e3`.
- Contained formats: .fbx: 378, .bin: 190, .gltf: 190, .txt: 1, .mtl: 191, .obj: 191, .png: 24.
- Quaternius CC0. Inspected 189 unique FBX modules: 113,517 aggregate triangles (not a runtime scene budget). Per-module UV/material/hierarchy and rigs/animations are in the audit. The pack contains animated organic aliens, a chest and a fan; these do not provide character locomotion.
- Silhouette B as structural donors, topology B with open modular surfaces, rig/animation Reject for this static subset, material structure B after relinking, mobile A for selected compact modules, GRAVIVORE fit C as a complete pack, donor value B. Only the explicit compact subset advances.
- Supplied external bitmaps: 24; sizes: (1921, 1081), (2000, 2000), (2048, 2048). Packed/embedded maps are additionally recorded in Blender audit.

## gunslinger-crab-mech-v12.zip

- Category: magnetar. Size: 133,016,988 bytes.
- SHA256: `6667c8ed7c2013a611caed755d19f0c3e08599ce22d14ade6a09ef949428d231`.
- Contained formats: .zip: 1, .jpeg: 13, .png: 8.
- Provenance: [Gunslinger Crab Mech v1.2](https://sketchfab.com/3d-models/gunslinger-crab-mech-v12-5147619f337a45b0974f349eac27b34a) by Vaportrash; CC Attribution per #55.
- Meshes: 11; base triangles: 91,496; evaluated triangles: 91,496.
- Rig: none supplied. Actions: none supplied.
- Materials: arm_MAT.002, crab_main.001, labels_eyes_MAT.001, labels_front_MAT.001, labels_mouth_side_MAT.001, labels_rear_MAT.001, labels_top_MAT.001, legs_MAT.002, m_launcher_label_MAT.001, m_launcher_MAT.002. UV: UVMap.
- Grades (silhouette / topology / rig / animation / materials / mobile / GRAVIVORE fit / donor): B / B / Reject / Reject / C / Reject / C / B.
- Review: Mechanics only: crab_legs (15,612 triangles) has robust pivots/feet. Do not reuse full body, launcher, gun arms, decals or stock identity. 4K source maps and ten materials are unsuitable as delivered. Split glTF vertices require welding before topology cleanup.
- Supplied external bitmaps: 32; sizes: (512, 512), (1024, 1024), (2048, 2048), (4096, 4096). Packed/embedded maps are additionally recorded in Blender audit.

## mechanical-spider.zip

- Category: magnetar. Size: 8,964,705 bytes.
- SHA256: `7b22e2f0fea91cbced07cc685cf0e5ab0f2eb59ffca9fe59184bd38e7a0986f1`.
- Contained formats: .fbx: 1, .jpg: 2, .jpeg: 1.
- Provenance: [Mechanical Spider](https://sketchfab.com/3d-models/mechanical-spider-d1f67d2e995d4c81a056242df1b97390) by Preview_Tempest; CC Attribution per #55.
- Meshes: 9; base triangles: 101,298; evaluated triangles: 101,298.
- Rig: none supplied. Actions: none supplied.
- Materials: 01 - Default, 02 - Default, 07 - Default, Material #25, Material #26, Material #27, Material #28, Material #29, Material #30, Material #31. UV: UVChannel_1.
- Grades (silhouette / topology / rig / animation / materials / mobile / GRAVIVORE fit / donor): B / C / Reject / Reject / C / Reject / C / B.
- Review: Mechanics only: Cylinder001 representative leg/joint chain. Each repeated chain is 10,277 triangles and contains 44 degenerate faces and three non-manifold edges. Rebuild/reduce around hinges; no delivered rig despite articulated appearance.
- Supplied external bitmaps: 3; sizes: (512, 512), (626, 417), (3264, 2448). Packed/embedded maps are additionally recorded in Blender audit.

## stalenhag-environment-project-spider-mech.zip

- Category: magnetar. Size: 67,202,696 bytes.
- SHA256: `dfda45d14c0547d850c2ce6747269c98da611b53c6003ca30bed1a440061d7b2`.
- Contained formats: .zip: 1, .jpeg: 16, .png: 4.
- Provenance: [Stalenhag Environment Project: Spider Mech](https://sketchfab.com/3d-models/stalenhag-environment-project-spider-mech-ec5914b53b6a4cde8de4820050bc46c5) by Enrico Labarile; CC Attribution per #55.
- Meshes: 4; base triangles: 103,396; evaluated triangles: 103,396.
- Rig: none supplied. Actions: none supplied.
- Materials: not converted; see COLLADA limit. UV: not converted; see source UV inputs.
- Grades (silhouette / topology / rig / animation / materials / mobile / GRAVIVORE fit / donor): C / B / Reject / Reject / B / Reject / Reject / Reject.
- Review: Reject production reuse. Creator explicitly credits Simon Stalenhag designs; uploader CC BY record does not establish rights to the underlying design. Static COLLADA XML import preserves geometry/scene transforms only; source has four UV inputs, no controllers/animation, but this inspection adapter does not transfer UVs/materials.
- Supplied external bitmaps: 39; sizes: (512, 512), (2048, 2048). Packed/embedded maps are additionally recorded in Blender audit.

## blue_metal_plate_2k.blend.zip

- Category: materials. Size: 6,880,008 bytes.
- SHA256: `7738ea91c0192dc37afd7a3b4a8b67a4082f9fecdbd5ed07a6385022cc96d94a`.
- Contained formats: .blend: 1, .exr: 2, .png: 1, .jpg: 1.
- Poly Haven CC0; actual material-only Blender library opened and shader nodes recorded. No mesh/rig/animation hierarchy in these material libraries.
- Prepared channels: basecolor, normal_gl, roughness, all 2048×2048. Displacement discarded. No AO map supplied. Metallic absent in blue/grid sets; do not invent it from diffuse.
- Grade B as reusable surface sources; final color/tiling calibration and Android compression remain later work.
- Supplied external bitmaps: 2; sizes: (2048, 2048). Packed/embedded maps are additionally recorded in Blender audit.

## metal_grate_rusty_2k.blend.zip

- Category: materials. Size: 23,899,342 bytes.
- SHA256: `7ea004b1b74e42a17cc40920d2e1f6af68c0c592900e33f12ca6e7695caf8e73`.
- Contained formats: .blend: 1, .exr: 3, .jpg: 1, .png: 1.
- Poly Haven CC0; actual material-only Blender library opened and shader nodes recorded. No mesh/rig/animation hierarchy in these material libraries.
- Prepared channels: basecolor, normal_gl, roughness, metallic, all 2048×2048. Displacement discarded. No AO map supplied. Metallic absent in blue/grid sets; do not invent it from diffuse.
- Grade B as reusable surface sources; final color/tiling calibration and Android compression remain later work.
- Supplied external bitmaps: 2; sizes: (2048, 2048). Packed/embedded maps are additionally recorded in Blender audit.

## metal_plate_2k.blend.zip

- Category: materials. Size: 24,586,768 bytes.
- SHA256: `1847929a34a4dd51b77f5e36760e2a7c009caf159bee4a0936a5b869cb26fb20`.
- Contained formats: .blend: 1, .png: 1, .exr: 3, .jpg: 1.
- Poly Haven CC0; actual material-only Blender library opened and shader nodes recorded. No mesh/rig/animation hierarchy in these material libraries.
- Prepared channels: basecolor, normal_gl, roughness, metallic, all 2048×2048. Displacement discarded. No AO map supplied. Metallic absent in blue/grid sets; do not invent it from diffuse.
- Grade B as reusable surface sources; final color/tiling calibration and Android compression remain later work.
- Supplied external bitmaps: 2; sizes: (2048, 2048). Packed/embedded maps are additionally recorded in Blender audit.

## rusty_metal_grid_2k.blend.zip

- Category: materials. Size: 15,042,216 bytes.
- SHA256: `a62a081fd4644af029320269e6fd4351515834d5ccae35d798dbd5f795574d58`.
- Contained formats: .blend: 1, .jpg: 1, .exr: 2, .png: 1.
- Poly Haven CC0; actual material-only Blender library opened and shader nodes recorded. No mesh/rig/animation hierarchy in these material libraries.
- Prepared channels: basecolor, normal_gl, roughness, all 2048×2048. Displacement discarded. No AO map supplied. Metallic absent in blue/grid sets; do not invent it from diffuse.
- Grade B as reusable surface sources; final color/tiling calibration and Android compression remain later work.
- Supplied external bitmaps: 2; sizes: (2048, 2048). Packed/embedded maps are additionally recorded in Blender audit.

## robot-spider.zip

- Category: scout. Size: 73,790,124 bytes.
- SHA256: `4892ce139072491a31aca8f15169e3e89475f2511a0b53a3f112f0a7e94d7218`.
- Contained formats: .glb: 1, .png: 3, .jpg: 9.
- Provenance: [Robot spider](https://sketchfab.com/3d-models/robot-spider-c9c7188c7f9e4504b8499f1131693b72) by PAndras; CC Attribution per #55.
- Meshes: 2; base triangles: 18,754; evaluated triangles: 18,754.
- Rig: none supplied. Actions: none supplied.
- Materials: spidey_Baked, weapon_Baked_Baked. UV: UVMap, UVMap.001, UVMap.002, UVMap.003, UVMap.004.
- Grades (silhouette / topology / rig / animation / materials / mobile / GRAVIVORE fit / donor): B / B / Reject / Reject / B / C / C / B.
- Review: Six-legged gun drone has useful tapered hinges/feet; no rig or clips. Body and cannon must be replaced. Many boundary edges reflect split glTF vertices, not automatic holes.
- Supplied external bitmaps: 12; sizes: (2048, 2048), (4096, 4096). Packed/embedded maps are additionally recorded in Blender audit.

## spider-robot-rigged.zip

- Category: scout. Size: 140,877 bytes.
- SHA256: `e3f43b8c1f9ccb689f67feda9a868ee2fab009a1b1425dbfc8e343697f6745e4`.
- Contained formats: .rar: 1.
- Provenance: [Spider Robot (Rigged)](https://sketchfab.com/3d-models/spider-robot-rigged-419632470d3b4328b8d8ea7ae0462ce7) by Keralt; CC Attribution per #55.
- Meshes: 1; base triangles: 4,603; evaluated triangles: 4,603.
- Rig: Armature / 39 bones. Actions: none supplied.
- Materials: Material, Material.001, Material.002, Material.003, Material.004, Material.005. UV: UVMap.
- Grades (silhouette / topology / rig / animation / materials / mobile / GRAVIVORE fit / donor): Reject / C / B / Reject / C / A / Reject / C.
- Review: Tall literal box body fails GRAVIVORE. 39-bone rig but no actions; 150 non-manifold edges and six material slots. Reject as preferred donor.
- Supplied external bitmaps: 0; sizes: none. Packed/embedded maps are additionally recorded in Blender audit.

## spider-robot.zip

- Category: scout. Size: 1,569,493 bytes.
- SHA256: `b5a5ffb4dfdec67c3240495eb5631fe35e0cd9bdefbb8233010c27a3847b9dd0`.
- Contained formats: .blend: 1, .png: 1.
- Provenance: [Spider Robot](https://sketchfab.com/3d-models/spider-robot-7ea58c2e0e7e48f281d7a840f02aa7b1) by Muhammad Hasan Alasady; CC Attribution per #55.
- Meshes: 1; base triangles: 1,962; evaluated triangles: 7,848.
- Rig: Spider / 33 bones. Actions: _Default, BigBoom, Fire, Idle, Walk.
- Materials: Dots Stroke, Material. UV: UVMap.
- Grades (silhouette / topology / rig / animation / materials / mobile / GRAVIVORE fit / donor): B / C / B / B / C / A / C / B.
- Review: Preferred Scout donor. Low articulated four-support anatomy and actual Idle/Walk/Fire/BigBoom actions. Base 1,962 triangles becomes 7,848 through subdivision. Repair 14 degenerate faces, four non-manifold edges and palette reference. Fire and explosion are reference-only after weapon redesign.
- Supplied external bitmaps: 1; sizes: (256, 256). Packed/embedded maps are additionally recorded in Blender audit.
