# Measured actor and mechanical donor inventory

78 evidence rows: four mech characters each measured in original Blender and FBX, three Essentials actors, nine RTS models, twenty-six Striker models, one Tower Defence turret, four UnityFan vehicles, one Voodoo drone, twelve Seed Hunter candidate/rejection checks, and fourteen WarZone models. All other package model/member names and export formats are in ARCHIVE_MANIFEST. Environment/VFX/UI and modular gun parts were inventoried at package/member level; they are not represented as measured actor rigs. Plain/textured alternate exports were not all imported separately.

Counts are raw datablock totals including imported parts. Evaluated counts include active modifiers. Mesh/object count is not a draw-call guarantee. Imported action counts may include object tracks/duplicates; no retargeted gameplay clip or locomotion was tested. `resolved_texture_images` means filename/material-reference matching, not a validated shader hookup; `additional_texture_candidates` is explicitly only a candidate set. Package image counts include previews/variants. Every raw action/frame range and bone name is retained in MODEL_METRICS.json.

## Quaternius Animated Mech Pack (March 2021)

Canonical archive: `ExternalAssetIntake/Current/quaternius-animated-mech-pack/Animated Mech Pack - March 2021-20261008T180817Z-1-001.zip`. [Official source](https://quaternius.com/packs/animatedmech.html). Licence: CC0 1.0 in payload and item pages; conflicting current general QAL v1.0 (2026-08-28).

### George.fbx вЂ” A

- Exact path: `Animated Mech Pack - March 2021/Textured/FBX/George.fbx`; FBX; SHA-256 `57c01cdca2d56ace4ddb4b8cdec8828e5b0a78af6db1a2ef1905e8b0b88fda70`. Method: Blender import/source read.
- Objects 2; meshes 1; vertices 4,124; polygons 3,816; raw triangles 7,864; evaluated triangles 7864; materials 1 (George_Texture).
- Mesh objects: George (7,864 tris).
- Rig: RobotArmature: 47 bones. Imported action records: 20. Normalized take labels: Dance, Death, Hello, HitRecieve_1, HitRecieve_2, Idle, Jump, Kick, No, Pickup, Punch, Run, Run_Holding, Run_Tall, Shoot, SwordSlash, Walk, Walk_Holding, Walk_Tall, Yes.
- Exact filename/material-linked image files (1): Animated Mech Pack - March 2021/Textured/Textures/George_Texture.png (2048x2048).
- Additional local texture candidates (4, not proven shader bindings): Animated Mech Pack - March 2021/Textured/Textures/Color Variations/George_1_Texture.png (2048x2048); Animated Mech Pack - March 2021/Textured/Textures/Color Variations/George_2_Texture.png (2048x2048); Animated Mech Pack - March 2021/Textured/Textures/Color Variations/George_3_Texture.png (2048x2048); Animated Mech Pack - March 2021/Textured/Textures/Color Variations/George_4_Texture.png (2048x2048). Whole package readable raster images: 21.
- Selected roles: Scout. Parts: Light articulated biped with usable arm and three-segment leg chains. Original blend has 61 disconnected geometry islands within one skinned mesh; separating islands still requires checking skin weights.
- Blender kitbash: Easy original-Blender editing; moderate rig-preserving shell replacement. Long blade arms, narrow aggressive armour, core enclosure and attack poses must be authored.
- Unity integration: Generic rig candidate; bake constraints/control bones, relink 2K base colour, export FBX and verify root/feet. Existing SwordSlash is a starting reference, not approved blade combat.
- Mobile/visual gap: 7,864 triangles/one material is a plausible starting cost; no device evidence. Stock simple shading and rounded/toy proportions miss the current layered industrial finish.

### Leela.fbx вЂ” C

- Exact path: `Animated Mech Pack - March 2021/Textured/FBX/Leela.fbx`; FBX; SHA-256 `6e1fafa6ca62f59ac8f5f504320d4c35d74a68c90cdc846285aa73d752ec6b19`. Method: Blender import/source read.
- Objects 2; meshes 1; vertices 1,262; polygons 1,143; raw triangles 2,368; evaluated triangles 2368; materials 1 (Leela_Texture).
- Mesh objects: Leela (2,368 tris).
- Rig: RobotArmature: 17 bones. Imported action records: 18. Normalized take labels: Dance, Death, Hello, HitRecieve_1, HitRecieve_2, Idle, Jump, Kick, No, Pickup, Punch, Run, Run_Tall, Shoot, SwordSlash, Walk, Walk_Tall, Yes.
- Exact filename/material-linked image files (1): Animated Mech Pack - March 2021/Textured/Textures/Leela_Texture.png (2048x2048).
- Additional local texture candidates (4, not proven shader bindings): Animated Mech Pack - March 2021/Textured/Textures/Color Variations/Leela_1_Texture.png (2048x2048); Animated Mech Pack - March 2021/Textured/Textures/Color Variations/Leela_2_Texture.png (2048x2048); Animated Mech Pack - March 2021/Textured/Textures/Color Variations/Leela_3_Texture.png (2048x2048); Animated Mech Pack - March 2021/Textured/Textures/Color Variations/Leela_4_Texture.png (2048x2048). Whole package readable raster images: 21.
- Selected roles: Scout leg mechanics only. Parts: Biped leg chain but NO arm bones; not a complete blade-armed Scout rig. Source has 30 geometry islands in one skinned mesh.
- Blender kitbash: Easy legs/components extraction from original blend; constructing and animating arm chains is substantial work.
- Unity integration: Generic export candidate; inherited Punch/SwordSlash names do not supply missing arm anatomy.
- Mobile/visual gap: 2,368 triangles/one material; cheap component donor, incomplete anatomy and stock visual finish.

### Mike.fbx вЂ” B

- Exact path: `Animated Mech Pack - March 2021/Textured/FBX/Mike.fbx`; FBX; SHA-256 `8f441cd81e3679cab1fc0bcc35fbdaeb1f09748e1a2f98dc25d3cd5de538b880`. Method: Blender import/source read.
- Objects 2; meshes 1; vertices 3,158; polygons 2,893; raw triangles 6,062; evaluated triangles 6062; materials 1 (Mike_Texture).
- Mesh objects: Mike (6,062 tris).
- Rig: RobotArmature: 43 bones. Imported action records: 18. Normalized take labels: Dance, Death, Hello, HitRecieve_1, HitRecieve_2, Idle, Jump, Kick, No, Pickup, Punch, Run, Run_Holding, Shoot, SwordSlash, Walk, Walk_Holding, Yes.
- Exact filename/material-linked image files (1): Animated Mech Pack - March 2021/Textured/Textures/Mike_Texture.png (2048x2048).
- Additional local texture candidates (4, not proven shader bindings): Animated Mech Pack - March 2021/Textured/Textures/Color Variations/Mike_1_Texture.png (2048x2048); Animated Mech Pack - March 2021/Textured/Textures/Color Variations/Mike_2_Texture.png (2048x2048); Animated Mech Pack - March 2021/Textured/Textures/Color Variations/Mike_3_Texture.png (2048x2048); Animated Mech Pack - March 2021/Textured/Textures/Color Variations/Mike_4_Texture.png (2048x2048). Whole package readable raster images: 21.
- Selected roles: G-0 optional mechanics; Scout alternate; Warden secondary. Parts: Mike has articulated arms/hands and biped legs; single skinned mesh. Original blend has 49 geometry islands.
- Blender kitbash: Easy original-Blender access; moderate parts extraction and reskinning. Chunkier torso is less suitable than George for the lightest hostile biped.
- Unity integration: Generic rig and existing locomotion references; bake/export/relink, tune foot contacts and authored combat poses.
- Mobile/visual gap: About 6K triangles/one material. Substantial armour/silhouette/PBR work; do not replace the current G-0 direction with unchanged stock art.

### Stan.fbx вЂ” B

- Exact path: `Animated Mech Pack - March 2021/Textured/FBX/Stan.fbx`; FBX; SHA-256 `be5dd03dbd6f601dc55e9987f2efd54b43ed54b72b3d69146bba79698ed00829`. Method: Blender import/source read.
- Objects 2; meshes 1; vertices 3,185; polygons 2,924; raw triangles 6,110; evaluated triangles 6110; materials 1 (Stan_Texture).
- Mesh objects: Stan (6,110 tris).
- Rig: RobotArmature: 43 bones. Imported action records: 18. Normalized take labels: Dance, Death, Hello, HitRecieve_1, HitRecieve_2, Idle, Jump, Kick, No, Pickup, Punch, Run, Run_Holding, Shoot, SwordSlash, Walk, Walk_Holding, Yes.
- Exact filename/material-linked image files (1): Animated Mech Pack - March 2021/Textured/Textures/Stan_Texture.png (2048x2048).
- Additional local texture candidates (4, not proven shader bindings): Animated Mech Pack - March 2021/Textured/Textures/Color Variations/Stan_1_Texture.png (2048x2048); Animated Mech Pack - March 2021/Textured/Textures/Color Variations/Stan_2_Texture.png (2048x2048); Animated Mech Pack - March 2021/Textured/Textures/Color Variations/Stan_3_Texture.png (2048x2048); Animated Mech Pack - March 2021/Textured/Textures/Color Variations/Stan_4_Texture.png (2048x2048). Whole package readable raster images: 21.
- Selected roles: G-0 optional mechanics; Scout alternate; Warden secondary. Parts: Stan has articulated arms/hands and biped legs; single skinned mesh. Original blend has 52 geometry islands.
- Blender kitbash: Easy original-Blender access; moderate parts extraction and reskinning. Chunkier torso is less suitable than George for the lightest hostile biped.
- Unity integration: Generic rig and existing locomotion references; bake/export/relink, tune foot contacts and authored combat poses.
- Mobile/visual gap: About 6K triangles/one material. Substantial armour/silhouette/PBR work; do not replace the current G-0 direction with unchanged stock art.

### George.blend вЂ” A

- Exact path: `Animated Mech Pack - March 2021/Textured/Blends/George.blend`; BLEND; SHA-256 `55e24fbae28736f87684129d2f3e27e0e3ba246cc8e91668822d0b3faf155b0c`. Method: Blender import/source read.
- Objects 2; meshes 1; vertices 4,124; polygons 3,816; raw triangles 7,864; evaluated triangles 7864; materials 1 (George_Texture).
- Mesh objects: George (7,864 tris, 61 islands).
- Rig: RobotArmature: 47 bones. Imported action records: 20. Normalized take labels: Dance, Death, Hello, HitRecieve_1, HitRecieve_2, Idle, Jump, Kick, No, Pickup, Punch, Run, Run_Holding, Run_Tall, Shoot, SwordSlash, Walk, Walk_Holding, Walk_Tall, Yes.
- Exact filename/material-linked image files (1): Animated Mech Pack - March 2021/Textured/Textures/George_Texture.png (2048x2048).
- Additional local texture candidates (4, not proven shader bindings): Animated Mech Pack - March 2021/Textured/Textures/Color Variations/George_1_Texture.png (2048x2048); Animated Mech Pack - March 2021/Textured/Textures/Color Variations/George_2_Texture.png (2048x2048); Animated Mech Pack - March 2021/Textured/Textures/Color Variations/George_3_Texture.png (2048x2048); Animated Mech Pack - March 2021/Textured/Textures/Color Variations/George_4_Texture.png (2048x2048). Whole package readable raster images: 21.
- Selected roles: Scout. Parts: Light articulated biped with usable arm and three-segment leg chains. Original blend has 61 disconnected geometry islands within one skinned mesh; separating islands still requires checking skin weights.
- Blender kitbash: Easy original-Blender editing; moderate rig-preserving shell replacement. Long blade arms, narrow aggressive armour, core enclosure and attack poses must be authored.
- Unity integration: Generic rig candidate; bake constraints/control bones, relink 2K base colour, export FBX and verify root/feet. Existing SwordSlash is a starting reference, not approved blade combat.
- Mobile/visual gap: 7,864 triangles/one material is a plausible starting cost; no device evidence. Stock simple shading and rounded/toy proportions miss the current layered industrial finish.

### Leela.blend вЂ” C

- Exact path: `Animated Mech Pack - March 2021/Textured/Blends/Leela.blend`; BLEND; SHA-256 `88d30e923c41ed307d3eeb1eea1587e7f52c818ea37672053ea67d1091552884`. Method: Blender import/source read.
- Objects 2; meshes 1; vertices 1,262; polygons 1,143; raw triangles 2,368; evaluated triangles 2368; materials 1 (Leela_Texture).
- Mesh objects: Leela (2,368 tris, 30 islands).
- Rig: RobotArmature: 17 bones. Imported action records: 18. Normalized take labels: Dance, Death, Hello, HitRecieve_1, HitRecieve_2, Idle, Jump, Kick, No, Pickup, Punch, Run, Run_Tall, Shoot, SwordSlash, Walk, Walk_Tall, Yes.
- Exact filename/material-linked image files (1): Animated Mech Pack - March 2021/Textured/Textures/Leela_Texture.png (2048x2048).
- Additional local texture candidates (4, not proven shader bindings): Animated Mech Pack - March 2021/Textured/Textures/Color Variations/Leela_1_Texture.png (2048x2048); Animated Mech Pack - March 2021/Textured/Textures/Color Variations/Leela_2_Texture.png (2048x2048); Animated Mech Pack - March 2021/Textured/Textures/Color Variations/Leela_3_Texture.png (2048x2048); Animated Mech Pack - March 2021/Textured/Textures/Color Variations/Leela_4_Texture.png (2048x2048). Whole package readable raster images: 21.
- Selected roles: Scout leg mechanics only. Parts: Biped leg chain but NO arm bones; not a complete blade-armed Scout rig. Source has 30 geometry islands in one skinned mesh.
- Blender kitbash: Easy legs/components extraction from original blend; constructing and animating arm chains is substantial work.
- Unity integration: Generic export candidate; inherited Punch/SwordSlash names do not supply missing arm anatomy.
- Mobile/visual gap: 2,368 triangles/one material; cheap component donor, incomplete anatomy and stock visual finish.

### Mike.blend вЂ” B

- Exact path: `Animated Mech Pack - March 2021/Textured/Blends/Mike.blend`; BLEND; SHA-256 `f4bebbc1a2201c0c1895d1f0124295ee864155f9fa73f3c76887e04d172c8f92`. Method: Blender import/source read.
- Objects 2; meshes 1; vertices 3,158; polygons 2,893; raw triangles 6,062; evaluated triangles 6062; materials 1 (Mike_Texture).
- Mesh objects: Mike (6,062 tris, 49 islands).
- Rig: RobotArmature: 43 bones. Imported action records: 18. Normalized take labels: Dance, Death, Hello, HitRecieve_1, HitRecieve_2, Idle, Jump, Kick, No, Pickup, Punch, Run, Run_Holding, Shoot, SwordSlash, Walk, Walk_Holding, Yes.
- Exact filename/material-linked image files (1): Animated Mech Pack - March 2021/Textured/Textures/Mike_Texture.png (2048x2048).
- Additional local texture candidates (4, not proven shader bindings): Animated Mech Pack - March 2021/Textured/Textures/Color Variations/Mike_1_Texture.png (2048x2048); Animated Mech Pack - March 2021/Textured/Textures/Color Variations/Mike_2_Texture.png (2048x2048); Animated Mech Pack - March 2021/Textured/Textures/Color Variations/Mike_3_Texture.png (2048x2048); Animated Mech Pack - March 2021/Textured/Textures/Color Variations/Mike_4_Texture.png (2048x2048). Whole package readable raster images: 21.
- Selected roles: G-0 optional mechanics; Scout alternate; Warden secondary. Parts: Mike has articulated arms/hands and biped legs; single skinned mesh. Original blend has 49 geometry islands.
- Blender kitbash: Easy original-Blender access; moderate parts extraction and reskinning. Chunkier torso is less suitable than George for the lightest hostile biped.
- Unity integration: Generic rig and existing locomotion references; bake/export/relink, tune foot contacts and authored combat poses.
- Mobile/visual gap: About 6K triangles/one material. Substantial armour/silhouette/PBR work; do not replace the current G-0 direction with unchanged stock art.

### Stan.blend вЂ” B

- Exact path: `Animated Mech Pack - March 2021/Textured/Blends/Stan.blend`; BLEND; SHA-256 `6cdbd9abd2fc8d1747b8e3121e7ae324d70624a91dbf56c4d30cbed60d0b220a`. Method: Blender import/source read.
- Objects 2; meshes 1; vertices 3,185; polygons 2,924; raw triangles 6,110; evaluated triangles 6110; materials 1 (Stan_Texture).
- Mesh objects: Stan (6,110 tris, 52 islands).
- Rig: RobotArmature: 43 bones. Imported action records: 18. Normalized take labels: Dance, Death, Hello, HitRecieve_1, HitRecieve_2, Idle, Jump, Kick, No, Pickup, Punch, Run, Run_Holding, Shoot, SwordSlash, Walk, Walk_Holding, Yes.
- Exact filename/material-linked image files (1): Animated Mech Pack - March 2021/Textured/Textures/Stan_Texture.png (2048x2048).
- Additional local texture candidates (4, not proven shader bindings): Animated Mech Pack - March 2021/Textured/Textures/Color Variations/Stan_1_Texture.png (2048x2048); Animated Mech Pack - March 2021/Textured/Textures/Color Variations/Stan_2_Texture.png (2048x2048); Animated Mech Pack - March 2021/Textured/Textures/Color Variations/Stan_3_Texture.png (2048x2048); Animated Mech Pack - March 2021/Textured/Textures/Color Variations/Stan_4_Texture.png (2048x2048). Whole package readable raster images: 21.
- Selected roles: G-0 optional mechanics; Scout alternate; Warden secondary. Parts: Stan has articulated arms/hands and biped legs; single skinned mesh. Original blend has 52 geometry islands.
- Blender kitbash: Easy original-Blender access; moderate parts extraction and reskinning. Chunkier torso is less suitable than George for the lightest hostile biped.
- Unity integration: Generic rig and existing locomotion references; bake/export/relink, tune foot contacts and authored combat poses.
- Mobile/visual gap: About 6K triangles/one material. Substantial armour/silhouette/PBR work; do not replace the current G-0 direction with unchanged stock art.

## Quaternius Sci-Fi Essentials Kit Standard

Canonical archive: `ExternalAssetIntake/Current/quaternius-scifi-essentials/Sci-Fi Essentials Kit[Standard].zip`. [Official source](https://quaternius.com/packs/scifiessentialskit.html). Licence: CC0 1.0 in payload and item pages; conflicting current general QAL v1.0 (2026-08-28).

### Enemy_EyeDrone.fbx вЂ” A

- Exact path: `FBX (Unity)/Enemy_EyeDrone.fbx`; FBX; SHA-256 `2ed79c0e1bf99052cbc769942033a637d63afb89496c1a46ff3031f3ef4916f2`. Method: Blender import/source read.
- Objects 2; meshes 1; vertices 1,829; polygons 1,711; raw triangles 3,530; evaluated triangles 3530; materials 1 (MI_Enemies).
- Mesh objects: Enemies_EyeDrone (3,530 tris).
- Rig: Rig: 14 bones. Imported action records: 6. Normalized take labels: Attack, BackFlip, Charging, Hit, Idle, Look.
- Exact filename/material-linked image files (3): glTF/T_Enemies_Normal.png (2048x2048); Textures/T_Enemies_BaseColor.png (2048x2048); Textures/T_Enemies_Normal.png (2048x2048).
- Additional local texture candidates (2, not proven shader bindings): Textures/T_Enemies_Emissive.png (1024x1024); Textures/T_Enemies_ORM.png (2048x2048). Whole package readable raster images: 69.
- Selected roles: Arc Drone. Parts: Floating eye sphere with eyelid/piston articulation; no walking chassis. Single mesh, 14-bone rig.
- Blender kitbash: Moderate shell replacement while preserving aerial articulation. Add ranged emitter, contained energy housing and aggressive fins/nozzles.
- Unity integration: Generic FBX rig; six named takes provide idle/hit/charge/attack references. Rebuild URP material from shared BaseColor/Normal/ORM/Emissive maps.
- Mobile/visual gap: 3,530 triangles/one material; reasonable starting geometry. Round toy eye identity and shared kit finish need substantial custom production work.

### Enemy_QuadShell.fbx вЂ” A

- Exact path: `FBX (Unity)/Enemy_QuadShell.fbx`; FBX; SHA-256 `823ff8d076f4f99be412cf6854e8e85763efb06cc8e82f4bca8fd93b8a7407a0`. Method: Blender import/source read.
- Objects 3; meshes 2; vertices 4,182; polygons 3,641; raw triangles 7,494; evaluated triangles 7494; materials 1 (MI_Enemies).
- Mesh objects: QuadShell_Body (2,150 tris); QuadShell_Legs (5,344 tris).
- Rig: Rig: 28 bones. Imported action records: 8. Normalized take labels: Attack, Charge, Hit, Idle, Look, Run, TurnOff, Walk.
- Exact filename/material-linked image files (3): glTF/T_Enemies_Normal.png (2048x2048); Textures/T_Enemies_BaseColor.png (2048x2048); Textures/T_Enemies_Normal.png (2048x2048).
- Additional local texture candidates (2, not proven shader bindings): Textures/T_Enemies_Emissive.png (1024x1024); Textures/T_Enemies_ORM.png (2048x2048). Whole package readable raster images: 69.
- Selected roles: Cutter; Magnetar support mechanics. Parts: Separate body/legs meshes, quadruped support chains and Gun.L/Gun.R bones. Forward guns are not cutting blades.
- Blender kitbash: Moderate rig-preserving body refit; author paired cutting assemblies, exposed drive mechanisms and low aggressive shell.
- Unity integration: Generic FBX, eight takes including Walk/Run/Attack. Rebuild URP PBR channels; verify feet and new cutter sweep before reuse.
- Mobile/visual gap: 7,494 triangles/two meshes/one material; useful starting rig. Stock silhouette/materials are not current production art.

### Enemy_Trilobite.fbx вЂ” C

- Exact path: `FBX (Unity)/Enemy_Trilobite.fbx`; FBX; SHA-256 `015e1424f20afacd7fcd57a93b95418fd0745f849ee81790ea3853facc19f5e8`. Method: Blender import/source read.
- Objects 3; meshes 2; vertices 4,300; polygons 4,041; raw triangles 8,338; evaluated triangles 8338; materials 1 (MI_Enemies_Large).
- Mesh objects: Trilobite_Body (3,242 tris); Trilobite_Legs (5,096 tris).
- Rig: Rig: 20 bones. Imported action records: 9. Normalized take labels: Attack, AttackAuto, Hanging, Hit, Idle, Look, Run, TurnOff, Walk.
- Exact filename/material-linked image files (4): glTF/T_Enemies_Large_BaseColor.png (2048x2048); glTF/T_Enemies_Large_Normal.png (2048x2048); Textures/T_Enemies_Large_BaseColor.png (2048x2048); Textures/T_Enemies_Large_Normal.png (2048x2048).
- Additional local texture candidates (2, not proven shader bindings): Textures/T_Enemies_Large_Emissive.png (1024x1024); Textures/T_Enemies_Large_ORM.png (2048x2048). Whole package readable raster images: 69.
- Selected roles: Magnetar joints; Custodian support mechanics. Parts: Separate disk body and leg mesh; two-support articulated machine, not a heavy multi-support elite or arena boss.
- Blender kitbash: Moderate extraction of joints/support chains. Entire Magnetar/Custodian architecture and reactor containment remain custom.
- Unity integration: 20-bone generic rig and nine takes are articulation references; new multi-support skeleton and phase animation required.
- Mobile/visual gap: 8,338 triangles/one material. Reusing the whole creature or scaling it up fails the requested silhouette.

## RTS Sci-Fi Game Assets v1

Canonical archive: `ExternalAssetIntake/Current/rts-scifi-assets-v1/RTS Sci-Fi Game Assets v1.unitypackage`. [Official source](https://assetstore.unity.com/packages/3d/environments/sci-fi/rts-sci-fi-game-assets-v1-112251). Licence: Standard Unity Asset Store EULA.

### Structure_v1.FBX вЂ” C

- Exact path: `Assets/RTS_Scifi_game_assets/Models/Structures/Structure_v1/Source/Structure_v1.FBX`; FBX; SHA-256 `587cc1e33ce0dc4da1835a819234abf6410b31f76b712ab374891afc5515acec`. Method: Blender import/source read.
- Objects 3; meshes 3; vertices 4,937; polygons 9,414; raw triangles 9,414; evaluated triangles 9414; materials 1 (24 - Default).
- Mesh objects: Structure_v1_body (7,976 tris); Structure_v1_satellite_dish (1,028 tris); Structure_v1_satellite_dish_stand (410 tris).
- Rig: none. Imported action records: 2. Normalized take labels: BaseLayer.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 70.
- Selected roles: Magnetar pivots; Custodian doors/rotors; Carrier optional moving hardware. Parts: Separate rigid objects named below. Imported BaseLayer actions are object tracks and can be channels of the SAME take; no skeletal rig.
- Blender kitbash: Easy-to-moderate doors/rotor/turret/track components extraction. Vehicle_v3 is a tall dish vehicle, rejected as the whole Carrier silhouette.
- Unity integration: Relink stale absolute source-image paths. Inspect and bake object animations/pivots; importer action count is not a gameplay-clip count.
- Mobile/visual gap: 822-9,414 triangles across these candidates; select components, rebake materials. Buildings/dish vehicles cannot become final bosses by scaling.

### Structure_v2.FBX вЂ” C

- Exact path: `Assets/RTS_Scifi_game_assets/Models/Structures/Structure_v2/Source/Structure_v2.FBX`; FBX; SHA-256 `c4ddd65b93898ec2aa227b749a1ee4ab887e11e0904ce31fbca3fd6e19105577`. Method: Blender import/source read.
- Objects 2; meshes 2; vertices 1,792; polygons 2,822; raw triangles 2,822; evaluated triangles 2822; materials 1 (24 - Default).
- Mesh objects: Structure_v2_body (2,662 tris); Structure_v2_rotor (160 tris).
- Rig: none. Imported action records: 1. Normalized take labels: BaseLayer.
- Exact filename/material-linked image files (1): Assets/RTS_Scifi_game_assets/Models/Structures/Structure_v2/Source/Structure_v2.tga (2048x2048).
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 70.
- Selected roles: Magnetar pivots; Custodian doors/rotors; Carrier optional moving hardware. Parts: Separate rigid objects named below. Imported BaseLayer actions are object tracks and can be channels of the SAME take; no skeletal rig.
- Blender kitbash: Easy-to-moderate doors/rotor/turret/track components extraction. Vehicle_v3 is a tall dish vehicle, rejected as the whole Carrier silhouette.
- Unity integration: Relink stale absolute source-image paths. Inspect and bake object animations/pivots; importer action count is not a gameplay-clip count.
- Mobile/visual gap: 822-9,414 triangles across these candidates; select components, rebake materials. Buildings/dish vehicles cannot become final bosses by scaling.

### Structure_v2_doors.FBX вЂ” C

- Exact path: `Assets/RTS_Scifi_game_assets/Models/Structures/Structure_v2/Source/Structure_v2_doors.FBX`; FBX; SHA-256 `f9d9afa8058e45fb4e38db944e36a9f949993eb9d84dbecb3091eacfb42a947c`. Method: Blender import/source read.
- Objects 3; meshes 2; vertices 84; polygons 152; raw triangles 152; evaluated triangles 152; materials 1 (24 - Default).
- Mesh objects: Structure_v2_door_s1 (72 tris); Structure_v2_door_s2 (80 tris).
- Rig: none. Imported action records: 2. Normalized take labels: BaseLayer.
- Exact filename/material-linked image files (1): Assets/RTS_Scifi_game_assets/Models/Structures/Structure_v2/Source/Structure_v2.tga (2048x2048).
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 70.
- Selected roles: Magnetar pivots; Custodian doors/rotors; Carrier optional moving hardware. Parts: Separate rigid objects named below. Imported BaseLayer actions are object tracks and can be channels of the SAME take; no skeletal rig.
- Blender kitbash: Easy-to-moderate doors/rotor/turret/track components extraction. Vehicle_v3 is a tall dish vehicle, rejected as the whole Carrier silhouette.
- Unity integration: Relink stale absolute source-image paths. Inspect and bake object animations/pivots; importer action count is not a gameplay-clip count.
- Mobile/visual gap: 822-9,414 triangles across these candidates; select components, rebake materials. Buildings/dish vehicles cannot become final bosses by scaling.

### Structure_v3.FBX вЂ” C

- Exact path: `Assets/RTS_Scifi_game_assets/Models/Structures/Structure_v3/Source/Structure_v3.FBX`; FBX; SHA-256 `43287f7f4accb5b56ade5c9efe9b8ef7bf4d42e7d111381e4ab58c9632315166`. Method: Blender import/source read.
- Objects 7; meshes 7; vertices 3,846; polygons 5,580; raw triangles 5,580; evaluated triangles 5580; materials 1 (24 - Default).
- Mesh objects: Structure_v3_body (4,820 tris); Structure_v3_door_stand (342 tris); Structure_v3_sec_door (4 tris); Structure_v3_vent_b1 (68 tris); Structure_v3_vent_b2 (68 tris); Structure_v3_vent_s1 (139 tris); Structure_v3_vent_s2 (139 tris).
- Rig: none. Imported action records: 4. Normalized take labels: BaseLayer.
- Exact filename/material-linked image files (1): Assets/RTS_Scifi_game_assets/Models/Structures/Structure_v3/Source/Structure_v3.tga (2048x2048).
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 70.
- Selected roles: Magnetar pivots; Custodian doors/rotors; Carrier optional moving hardware. Parts: Separate rigid objects named below. Imported BaseLayer actions are object tracks and can be channels of the SAME take; no skeletal rig.
- Blender kitbash: Easy-to-moderate doors/rotor/turret/track components extraction. Vehicle_v3 is a tall dish vehicle, rejected as the whole Carrier silhouette.
- Unity integration: Relink stale absolute source-image paths. Inspect and bake object animations/pivots; importer action count is not a gameplay-clip count.
- Mobile/visual gap: 822-9,414 triangles across these candidates; select components, rebake materials. Buildings/dish vehicles cannot become final bosses by scaling.

### Structure_v3_doors.FBX вЂ” C

- Exact path: `Assets/RTS_Scifi_game_assets/Models/Structures/Structure_v3/Source/Structure_v3_doors.FBX`; FBX; SHA-256 `bb3b62c3f8fe192d8133e20c939f2afd620f838483882f974ab0928b5ccef803`. Method: Blender import/source read.
- Objects 2; meshes 2; vertices 238; polygons 438; raw triangles 438; evaluated triangles 438; materials 1 (24 - Default).
- Mesh objects: Structure_v3_door_D (215 tris); Structure_v3_door_T (223 tris).
- Rig: none. Imported action records: 2. Normalized take labels: BaseLayer.
- Exact filename/material-linked image files (1): Assets/RTS_Scifi_game_assets/Models/Structures/Structure_v3/Source/Structure_v3.tga (2048x2048).
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 70.
- Selected roles: Magnetar pivots; Custodian doors/rotors; Carrier optional moving hardware. Parts: Separate rigid objects named below. Imported BaseLayer actions are object tracks and can be channels of the SAME take; no skeletal rig.
- Blender kitbash: Easy-to-moderate doors/rotor/turret/track components extraction. Vehicle_v3 is a tall dish vehicle, rejected as the whole Carrier silhouette.
- Unity integration: Relink stale absolute source-image paths. Inspect and bake object animations/pivots; importer action count is not a gameplay-clip count.
- Mobile/visual gap: 822-9,414 triangles across these candidates; select components, rebake materials. Buildings/dish vehicles cannot become final bosses by scaling.

### Turret_v1.FBX вЂ” C

- Exact path: `Assets/RTS_Scifi_game_assets/Models/Turrets/Source/Turret_v1.FBX`; FBX; SHA-256 `842ebf413babb81bf7f5917366088705febf6824f3d46c0719935a8ca364a746`. Method: Blender import/source read.
- Objects 6; meshes 6; vertices 945; polygons 1,190; raw triangles 1,327; evaluated triangles 1327; materials 1 (345Default).
- Mesh objects: Turret_v1_doorL (9 tris); Turret_v1_doorR (9 tris); Turret_v1_guns (252 tris); Turret_v1_hole (7 tris); Turret_v1_stand (256 tris); Turret_v1_tower (794 tris).
- Rig: none. Imported action records: 5. Normalized take labels: BaseLayer.
- Exact filename/material-linked image files (1): Assets/RTS_Scifi_game_assets/Models/Turrets/Source/Turret_v1.tga (2048x2048).
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 70.
- Selected roles: Magnetar pivots; Custodian doors/rotors; Carrier optional moving hardware. Parts: Separate rigid objects named below. Imported BaseLayer actions are object tracks and can be channels of the SAME take; no skeletal rig.
- Blender kitbash: Easy-to-moderate doors/rotor/turret/track components extraction. Vehicle_v3 is a tall dish vehicle, rejected as the whole Carrier silhouette.
- Unity integration: Relink stale absolute source-image paths. Inspect and bake object animations/pivots; importer action count is not a gameplay-clip count.
- Mobile/visual gap: 822-9,414 triangles across these candidates; select components, rebake materials. Buildings/dish vehicles cannot become final bosses by scaling.

### Vehicle_v1.FBX вЂ” B

- Exact path: `Assets/RTS_Scifi_game_assets/Models/Vehicle/Vehicle_v1/Source/Vehicle_v1.FBX`; FBX; SHA-256 `4f50c89ae9e43e67905607c22d957c0d6aaecb88ae3915cabc0920ffaeeede99`. Method: Blender import/source read.
- Objects 1; meshes 1; vertices 1,132; polygons 1,357; raw triangles 2,212; evaluated triangles 2212; materials 1 (28 - Default).
- Mesh objects: Vehicle_v1_body (2,212 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 70.
- Selected roles: Carrier secondary chassis. Parts: Low armoured wheeled buggy, but one combined mesh; no separate wheel rig and no locomotion clips.
- Blender kitbash: Moderate chassis extraction; less streamlined than UnityFan 012. Cutting out wheels/panels would require geometry work.
- Unity integration: Relink publisher-machine texture references from local maps; supply all locomotion and running gear animation.
- Mobile/visual gap: 2,212 triangles/one material. Stock buggy silhouette and worn diffuse style require a major assault-hovercraft refit.

### Laser_tower_v1.FBX вЂ” C

- Exact path: `Assets/RTS_Scifi_game_assets/Models/Vehicle/Vehicle_v1/Source/Weapon/Laser_tower_v1/Laser_tower_v1.FBX`; FBX; SHA-256 `266496bae5b6baf20460216f0337593eb65f352767c7953918f517e3a8260c67`. Method: Blender import/source read.
- Objects 3; meshes 3; vertices 566; polygons 759; raw triangles 822; evaluated triangles 822; materials 1 (Rocket_launcher).
- Mesh objects: Laser_tower_v1_arm (152 tris); Laser_tower_v1_laser (552 tris); Laser_tower_v1_stand (118 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 70.
- Selected roles: Magnetar pivots; Custodian doors/rotors; Carrier optional moving hardware. Parts: Separate rigid objects named below. Imported BaseLayer actions are object tracks and can be channels of the SAME take; no skeletal rig.
- Blender kitbash: Easy-to-moderate doors/rotor/turret/track components extraction. Vehicle_v3 is a tall dish vehicle, rejected as the whole Carrier silhouette.
- Unity integration: Relink stale absolute source-image paths. Inspect and bake object animations/pivots; importer action count is not a gameplay-clip count.
- Mobile/visual gap: 822-9,414 triangles across these candidates; select components, rebake materials. Buildings/dish vehicles cannot become final bosses by scaling.

### Vehicle_v3.FBX вЂ” C

- Exact path: `Assets/RTS_Scifi_game_assets/Models/Vehicle/Vehicle_v3/Source/Vehicle_v3.FBX`; FBX; SHA-256 `c615047ddf30866e1f57439bb0b8ab10655b2de9ee7535e46cb7c1402f10a1b8`. Method: Blender import/source read.
- Objects 8; meshes 8; vertices 2,038; polygons 2,616; raw triangles 3,024; evaluated triangles 3024; materials 2 (23 - Default, 24 - Default).
- Mesh objects: Vehicle_v3_ATN_body (308 tris); Vehicle_v3_ATN_LB (38 tris); Vehicle_v3_ATN_LT (38 tris); Vehicle_v3_ATN_RB (38 tris); Vehicle_v3_ATN_RT (38 tris); Vehicle_v3_ATN_stand (180 tris); Vehicle_v3_body (1,904 tris); Vehicle_v3_track (480 tris).
- Rig: none. Imported action records: 5. Normalized take labels: BaseLayer.
- Exact filename/material-linked image files (1): Assets/RTS_Scifi_game_assets/Models/Universal_textures/Tank_track_v1/Tank_track_v1.tga (512x256).
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 70.
- Selected roles: Magnetar pivots; Custodian doors/rotors; Carrier optional moving hardware. Parts: Separate rigid objects named below. Imported BaseLayer actions are object tracks and can be channels of the SAME take; no skeletal rig.
- Blender kitbash: Easy-to-moderate doors/rotor/turret/track components extraction. Vehicle_v3 is a tall dish vehicle, rejected as the whole Carrier silhouette.
- Unity integration: Relink stale absolute source-image paths. Inspect and bake object animations/pivots; importer action count is not a gameplay-clip count.
- Mobile/visual gap: 822-9,414 triangles across these candidates; select components, rebake materials. Buildings/dish vehicles cannot become final bosses by scaling.

## MSGDI Medium Mech Striker

Canonical archive: `ExternalAssetIntake/Current/msgdi-medium-mech-striker/Medium Mech Striker.unitypackage`. [Official source](https://assetstore.unity.com/packages/3d/characters/robots/medium-mech-striker-124342). Licence: Standard Unity Asset Store EULA.

### MediumMechStriker.fbx вЂ” A

- Exact path: `Assets/MediumMechStriker/FBX/MediumMechStriker/MediumMechStriker.fbx`; FBX; SHA-256 `b0cea6976225126369264bb4ace54f52b4d3f46f2ebaec9b997703ccafc17e91`. Method: Blender import/source read.
- Objects 4; meshes 3; vertices 4,148; polygons 3,584; raw triangles 6,472; evaluated triangles 6472; materials 1 (MediumMechStrikerBlue).
- Mesh objects: MediumMechStrikerArmLeft (440 tris); MediumMechStrikerArmRight (440 tris); MediumMechStrikerChassis (5,592 tris).
- Rig: MediumMechHumanoidSkeleton: 17 bones. Imported action records: 102. Normalized take labels: a1ShutdownPose, a2ShutdownOverheat, a3StartupSequence, a4IdlePose, a5WalkCycle, a5WalkCycleLeftLegCrippled, a5WalkCycleLeftLegCrippledRootMotion, a5WalkCycleRightLegCrippled, a5WalkCycleRightLegCrippledR_3e0d755, a5WalkCycleRightLegCrippledRootMotion, a5WalkCycleRootMotion, a5walkCycleLeftLegCrippled, a6WalkCycleBack, a6WalkCycleBackRootMotion, a7RunCycle, a7RunCycleRootMotion, a8Falling, a8FallingRootMotion, a8Jump, a8JumpJetFlying, a8JumpJetFlyingRootMotion, a8JumpRootMotion, a8Landing, a9TurningSteps, b1HitBack1, b1HitBack2, b1HitBack3, b1HitFront1, b1HitFront2, b1HitFront3, b2DeathFallDown1, b2DeathFallDown2, b2DeathPose1, b2DeathPose2, b2StandUp1, b2StandUp2, c1HeadRam, c1KickLeft, c1KickRight, c1PunchLeft, c1PunchRight, c1StompVehicle, xxxDefaultPose.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: Warden; G-0 optional mechanics. Parts: Heavy layered biped; separate left arm/right arm/chassis meshes. Seventeen bones ending at LowerArm_L/R; NO wrist/finger bones, NO shields.
- Blender kitbash: Moderate heavy-body refit. Separate hand FBXs can supply rigid grips; author shields, grip/socket placement, shield collision presentation and new wrist joints if needed.
- Unity integration: Generic rig; raw importer reports 102 actions with duplicate/suffixed names, not 102 distinct motions. Reuse inspected locomotion as references; custom shield brace/raise/break clips required. Convert 4K maps and private EULA workflow.
- Mobile/visual gap: 6,472 triangles/one material before hands/shields. Stock military markings, silhouette and surface treatment need substantial production changes.

### MediumMechStrikerDestroyedArmLeft.fbx вЂ” C

- Exact path: `Assets/MediumMechStriker/FBX/MediumMechStriker/MediumMechStrikerDestroyedArmLeft.fbx`; FBX; SHA-256 `ffaf59e56384f6ef47dd9812da13db28a30b5d3621103ce09e1a0a045088e736`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 587; polygons 571; raw triangles 1,024; evaluated triangles not measured; materials 1 (MediumMechStrikerBlue).
- Mesh objects: MediumMechStrikerDestroyedArmLeft (1,024 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: Warden hands/damage parts. Parts: Standalone rigid mesh; hands are separate geometry, not a hand skeleton. Weapon/damage parts have no rig or clips.
- Blender kitbash: Moderate: Blender 5.2 rejects this FBX 6.1 ASCII. Counts came from Objects-section array parsing; convert with a trusted private DCC/importer before kitbash.
- Unity integration: Unity import has not been tested. Preserve donor licence, convert privately, create sockets/pivots and rebuild materials.
- Mobile/visual gap: Small parts can be economical; appended hands/weapons add cost. Reauthor styling and atlas with actor materials.

### MediumMechStrikerDestroyedArmRight.fbx вЂ” C

- Exact path: `Assets/MediumMechStriker/FBX/MediumMechStriker/MediumMechStrikerDestroyedArmRight.fbx`; FBX; SHA-256 `7409395d0fb3757904a1d61147254d515065bbf0e91edf9bd9ba4011982d67f8`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 587; polygons 571; raw triangles 1,024; evaluated triangles not measured; materials 1 (MediumMechStrikerBlue).
- Mesh objects: MediumMechStrikerDestroyedArmRight (1,024 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: Warden hands/damage parts. Parts: Standalone rigid mesh; hands are separate geometry, not a hand skeleton. Weapon/damage parts have no rig or clips.
- Blender kitbash: Moderate: Blender 5.2 rejects this FBX 6.1 ASCII. Counts came from Objects-section array parsing; convert with a trusted private DCC/importer before kitbash.
- Unity integration: Unity import has not been tested. Preserve donor licence, convert privately, create sockets/pivots and rebuild materials.
- Mobile/visual gap: Small parts can be economical; appended hands/weapons add cost. Reauthor styling and atlas with actor materials.

### MediumMechStrikerHandLeft.fbx вЂ” C

- Exact path: `Assets/MediumMechStriker/FBX/MediumMechStriker/MediumMechStrikerHandLeft.fbx`; FBX; SHA-256 `23fdd6502b9e10dd9b74247579c1784d36b38f9f735a5984ae4a8c940f0273c2`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 648; polygons 545; raw triangles 999; evaluated triangles not measured; materials 1 (MediumMechStrikerBlue).
- Mesh objects: MediumMechStrikerHandLeft (999 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: Warden hands/damage parts. Parts: Standalone rigid mesh; hands are separate geometry, not a hand skeleton. Weapon/damage parts have no rig or clips.
- Blender kitbash: Moderate: Blender 5.2 rejects this FBX 6.1 ASCII. Counts came from Objects-section array parsing; convert with a trusted private DCC/importer before kitbash.
- Unity integration: Unity import has not been tested. Preserve donor licence, convert privately, create sockets/pivots and rebuild materials.
- Mobile/visual gap: Small parts can be economical; appended hands/weapons add cost. Reauthor styling and atlas with actor materials.

### MediumMechStrikerHandRight.fbx вЂ” C

- Exact path: `Assets/MediumMechStriker/FBX/MediumMechStriker/MediumMechStrikerHandRight.fbx`; FBX; SHA-256 `38dca1e2a606f1aefa22459d2bd62352354cf733f7588111668d19eaf817bdd4`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 648; polygons 545; raw triangles 999; evaluated triangles not measured; materials 1 (MediumMechStrikerBlue).
- Mesh objects: MediumMechStrikerHandRight (999 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: Warden hands/damage parts. Parts: Standalone rigid mesh; hands are separate geometry, not a hand skeleton. Weapon/damage parts have no rig or clips.
- Blender kitbash: Moderate: Blender 5.2 rejects this FBX 6.1 ASCII. Counts came from Objects-section array parsing; convert with a trusted private DCC/importer before kitbash.
- Unity integration: Unity import has not been tested. Preserve donor licence, convert privately, create sockets/pivots and rebuild materials.
- Mobile/visual gap: Small parts can be economical; appended hands/weapons add cost. Reauthor styling and atlas with actor materials.

### ScifiMechDestroyedJoint.fbx вЂ” C

- Exact path: `Assets/MediumMechStriker/FBX/MediumMechStriker/ScifiMechDestroyedJoint.fbx`; FBX; SHA-256 `5b848b85ee220fc37423a57eb55638febb2eb0162091448e66e22e144062efae`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 327; polygons 320; raw triangles 584; evaluated triangles not measured; materials 1 (MediumMechStrikerBlue).
- Mesh objects: ScifiMechDestroyedJoint (584 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: Warden hands/damage parts. Parts: Standalone rigid mesh; hands are separate geometry, not a hand skeleton. Weapon/damage parts have no rig or clips.
- Blender kitbash: Moderate: Blender 5.2 rejects this FBX 6.1 ASCII. Counts came from Objects-section array parsing; convert with a trusted private DCC/importer before kitbash.
- Unity integration: Unity import has not been tested. Preserve donor licence, convert privately, create sockets/pivots and rebuild materials.
- Mobile/visual gap: Small parts can be economical; appended hands/weapons add cost. Reauthor styling and atlas with actor materials.

### ScifiMechHeavyCanonCartridge.fbx вЂ” C

- Exact path: `Assets/MediumMechStriker/FBX/ScifiMechAmmunition/ScifiMechHeavyCanonCartridge.fbx`; FBX; SHA-256 `54228e26db91bc4a357a3e226b52fab806a5245149e4560c2f5a398f011bed34`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 162; polygons 176; raw triangles 320; evaluated triangles not measured; materials 1 (ScifiMechAmmunition).
- Mesh objects: ScifiMechHeavyCanonCartridge (320 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: Cutter mechanisms; Magnetar/Custodian weapon parts. Parts: Standalone rigid mesh; hands are separate geometry, not a hand skeleton. Weapon/damage parts have no rig or clips.
- Blender kitbash: Moderate: Blender 5.2 rejects this FBX 6.1 ASCII. Counts came from Objects-section array parsing; convert with a trusted private DCC/importer before kitbash.
- Unity integration: Unity import has not been tested. Preserve donor licence, convert privately, create sockets/pivots and rebuild materials.
- Mobile/visual gap: Small parts can be economical; appended hands/weapons add cost. Reauthor styling and atlas with actor materials.

### ScifiMechHeavyCanonProjectile.fbx вЂ” C

- Exact path: `Assets/MediumMechStriker/FBX/ScifiMechAmmunition/ScifiMechHeavyCanonProjectile.fbx`; FBX; SHA-256 `2e2c79517b4102d9039a72e922807fad1590b91cc150a55854723b53ccf7df0e`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 162; polygons 176; raw triangles 320; evaluated triangles not measured; materials 1 (ScifiMechAmmunition).
- Mesh objects: ScifiMechHeavyCanonProjectile (320 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: Cutter mechanisms; Magnetar/Custodian weapon parts. Parts: Standalone rigid mesh; hands are separate geometry, not a hand skeleton. Weapon/damage parts have no rig or clips.
- Blender kitbash: Moderate: Blender 5.2 rejects this FBX 6.1 ASCII. Counts came from Objects-section array parsing; convert with a trusted private DCC/importer before kitbash.
- Unity integration: Unity import has not been tested. Preserve donor licence, convert privately, create sockets/pivots and rebuild materials.
- Mobile/visual gap: Small parts can be economical; appended hands/weapons add cost. Reauthor styling and atlas with actor materials.

### ScifiMechHeavyCanonShell.fbx вЂ” C

- Exact path: `Assets/MediumMechStriker/FBX/ScifiMechAmmunition/ScifiMechHeavyCanonShell.fbx`; FBX; SHA-256 `dccbb58de951c08cf4ed41be211a9435d119eb9ac8d524c3256e4d452c2925a5`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 226; polygons 224; raw triangles 416; evaluated triangles not measured; materials 1 (ScifiMechAmmunition).
- Mesh objects: ScifiMechHeavyCanonShell (416 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: Cutter mechanisms; Magnetar/Custodian weapon parts. Parts: Standalone rigid mesh; hands are separate geometry, not a hand skeleton. Weapon/damage parts have no rig or clips.
- Blender kitbash: Moderate: Blender 5.2 rejects this FBX 6.1 ASCII. Counts came from Objects-section array parsing; convert with a trusted private DCC/importer before kitbash.
- Unity integration: Unity import has not been tested. Preserve donor licence, convert privately, create sockets/pivots and rebuild materials.
- Mobile/visual gap: Small parts can be economical; appended hands/weapons add cost. Reauthor styling and atlas with actor materials.

### ScifiMechLighCanonCartridge.fbx вЂ” C

- Exact path: `Assets/MediumMechStriker/FBX/ScifiMechAmmunition/ScifiMechLighCanonCartridge.fbx`; FBX; SHA-256 `5d01cd0d3e92f8a39a8f927ea69c7f0dabcaf3f9f1f61e0c28f5f40e525f00fa`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 162; polygons 176; raw triangles 320; evaluated triangles not measured; materials 1 (ScifiMechAmmunition).
- Mesh objects: ScifiMechLighCanonCartridge (320 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: Cutter mechanisms; Magnetar/Custodian weapon parts. Parts: Standalone rigid mesh; hands are separate geometry, not a hand skeleton. Weapon/damage parts have no rig or clips.
- Blender kitbash: Moderate: Blender 5.2 rejects this FBX 6.1 ASCII. Counts came from Objects-section array parsing; convert with a trusted private DCC/importer before kitbash.
- Unity integration: Unity import has not been tested. Preserve donor licence, convert privately, create sockets/pivots and rebuild materials.
- Mobile/visual gap: Small parts can be economical; appended hands/weapons add cost. Reauthor styling and atlas with actor materials.

### ScifiMechLighCanonProjectile.fbx вЂ” C

- Exact path: `Assets/MediumMechStriker/FBX/ScifiMechAmmunition/ScifiMechLighCanonProjectile.fbx`; FBX; SHA-256 `94b1a628f90ad2a107ba58b3af54967ebea229534f00cbc47fdc1439d5237c9f`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 98; polygons 112; raw triangles 192; evaluated triangles not measured; materials 1 (ScifiMechAmmunition).
- Mesh objects: ScifiMechLighCanonProjectile (192 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: Cutter mechanisms; Magnetar/Custodian weapon parts. Parts: Standalone rigid mesh; hands are separate geometry, not a hand skeleton. Weapon/damage parts have no rig or clips.
- Blender kitbash: Moderate: Blender 5.2 rejects this FBX 6.1 ASCII. Counts came from Objects-section array parsing; convert with a trusted private DCC/importer before kitbash.
- Unity integration: Unity import has not been tested. Preserve donor licence, convert privately, create sockets/pivots and rebuild materials.
- Mobile/visual gap: Small parts can be economical; appended hands/weapons add cost. Reauthor styling and atlas with actor materials.

### ScifiMechLighCanonShell.fbx вЂ” C

- Exact path: `Assets/MediumMechStriker/FBX/ScifiMechAmmunition/ScifiMechLighCanonShell.fbx`; FBX; SHA-256 `847724e470676a618cc707a8eea1ea008dd56cbfac64b43d2b6d40dd67089dec`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 226; polygons 224; raw triangles 416; evaluated triangles not measured; materials 1 (ScifiMechAmmunition).
- Mesh objects: ScifiMechLighCanonShell (416 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: Cutter mechanisms; Magnetar/Custodian weapon parts. Parts: Standalone rigid mesh; hands are separate geometry, not a hand skeleton. Weapon/damage parts have no rig or clips.
- Blender kitbash: Moderate: Blender 5.2 rejects this FBX 6.1 ASCII. Counts came from Objects-section array parsing; convert with a trusted private DCC/importer before kitbash.
- Unity integration: Unity import has not been tested. Preserve donor licence, convert privately, create sockets/pivots and rebuild materials.
- Mobile/visual gap: Small parts can be economical; appended hands/weapons add cost. Reauthor styling and atlas with actor materials.

### ScifiMechStandardLRM.fbx вЂ” C

- Exact path: `Assets/MediumMechStriker/FBX/ScifiMechAmmunition/ScifiMechStandardLRM.fbx`; FBX; SHA-256 `9c180d61bce24328b5c865e8e0ae2d6f0fe60cf99664320308ec862576a1ae90`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 226; polygons 240; raw triangles 448; evaluated triangles not measured; materials 1 (ScifiMechAmmunition).
- Mesh objects: ScifiMechStandardLRM (448 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: Cutter mechanisms; Magnetar/Custodian weapon parts. Parts: Standalone rigid mesh; hands are separate geometry, not a hand skeleton. Weapon/damage parts have no rig or clips.
- Blender kitbash: Moderate: Blender 5.2 rejects this FBX 6.1 ASCII. Counts came from Objects-section array parsing; convert with a trusted private DCC/importer before kitbash.
- Unity integration: Unity import has not been tested. Preserve donor licence, convert privately, create sockets/pivots and rebuild materials.
- Mobile/visual gap: Small parts can be economical; appended hands/weapons add cost. Reauthor styling and atlas with actor materials.

### ScifiMechStandardSRM.fbx вЂ” C

- Exact path: `Assets/MediumMechStriker/FBX/ScifiMechAmmunition/ScifiMechStandardSRM.fbx`; FBX; SHA-256 `d7b3354dd1a25a68de2e2a4a724f063fb8b7f6ae195d35e513d25afd6a3640ac`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 194; polygons 208; raw triangles 384; evaluated triangles not measured; materials 1 (ScifiMechAmmunition).
- Mesh objects: ScifiMechStandardSRM (384 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: Cutter mechanisms; Magnetar/Custodian weapon parts. Parts: Standalone rigid mesh; hands are separate geometry, not a hand skeleton. Weapon/damage parts have no rig or clips.
- Blender kitbash: Moderate: Blender 5.2 rejects this FBX 6.1 ASCII. Counts came from Objects-section array parsing; convert with a trusted private DCC/importer before kitbash.
- Unity integration: Unity import has not been tested. Preserve donor licence, convert privately, create sockets/pivots and rebuild materials.
- Mobile/visual gap: Small parts can be economical; appended hands/weapons add cost. Reauthor styling and atlas with actor materials.

### Decal02.fbx вЂ” D

- Exact path: `Assets/MediumMechStriker/FBX/ScifiMechDecals/Decal02.fbx`; FBX; SHA-256 `b7fbac3ff9ee8cad97494f6d145e00fddfd8d712f8da107ae0c8cb2252ea671a`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 4; polygons 1; raw triangles 2; evaluated triangles not measured; materials 1 (ScifiMechDecals).
- Mesh objects: Decal02 (2 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: no actor production role. Parts: Flat decals/display podium geometry, not mechanical anatomy.
- Blender kitbash: No selected actor use; omit supplier decoration.
- Unity integration: ASCII FBX measured by a read-only parser; Blender did not import it. No rig or animation.
- Mobile/visual gap: Reject decorative branding/motif and presentation stand from final actors.

### DecalAncientEagle.fbx вЂ” D

- Exact path: `Assets/MediumMechStriker/FBX/ScifiMechDecals/DecalAncientEagle.fbx`; FBX; SHA-256 `69ebbce8cff58aefe8c7aa56d97276ce03e8a3cfa6c07d56e959c07ad1af3b28`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 4; polygons 1; raw triangles 2; evaluated triangles not measured; materials 1 (ScifiMechDecals).
- Mesh objects: DecalAncientEagle (2 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: no actor production role. Parts: Flat decals/display podium geometry, not mechanical anatomy.
- Blender kitbash: No selected actor use; omit supplier decoration.
- Unity integration: ASCII FBX measured by a read-only parser; Blender did not import it. No rig or animation.
- Mobile/visual gap: Reject decorative branding/motif and presentation stand from final actors.

### DecalHeavyHit.fbx вЂ” D

- Exact path: `Assets/MediumMechStriker/FBX/ScifiMechDecals/DecalHeavyHit.fbx`; FBX; SHA-256 `be3bf7aa98d1f6b8c2608b763af96bf0164b1ea6b35cd1a1f3c6989acacb7b03`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 4; polygons 1; raw triangles 2; evaluated triangles not measured; materials 1 (ScifiMechDecals).
- Mesh objects: DecalHeavyHit (2 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: no actor production role. Parts: Flat decals/display podium geometry, not mechanical anatomy.
- Blender kitbash: No selected actor use; omit supplier decoration.
- Unity integration: ASCII FBX measured by a read-only parser; Blender did not import it. No rig or animation.
- Mobile/visual gap: Reject decorative branding/motif and presentation stand from final actors.

### DecalSkull.fbx вЂ” D

- Exact path: `Assets/MediumMechStriker/FBX/ScifiMechDecals/DecalSkull.fbx`; FBX; SHA-256 `665d3df658002dbcda086c696969cd6caa1c179437b36e27f1c5fec4ee14e82f`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 4; polygons 1; raw triangles 2; evaluated triangles not measured; materials 1 (ScifiMechDecals).
- Mesh objects: DecalSkull (2 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: no actor production role. Parts: Flat decals/display podium geometry, not mechanical anatomy.
- Blender kitbash: No selected actor use; omit supplier decoration.
- Unity integration: ASCII FBX measured by a read-only parser; Blender did not import it. No rig or animation.
- Mobile/visual gap: Reject decorative branding/motif and presentation stand from final actors.

### ScifiMechPresentation.fbx вЂ” D

- Exact path: `Assets/MediumMechStriker/FBX/ScifiMechPresentation/ScifiMechPresentation.fbx`; FBX; SHA-256 `278342af72a1fa2b7155da0d06d1da0699b954f469a639e58d6584ed6b65904c`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 295; polygons 272; raw triangles 448; evaluated triangles not measured; materials 1 (ScifiMechPresentation).
- Mesh objects: ScifiMechPresentation (448 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: no actor production role. Parts: Flat decals/display podium geometry, not mechanical anatomy.
- Blender kitbash: No selected actor use; omit supplier decoration.
- Unity integration: ASCII FBX measured by a read-only parser; Blender did not import it. No rig or animation.
- Mobile/visual gap: Reject decorative branding/motif and presentation stand from final actors.

### ScifiMechPresentationFrontHalf.fbx вЂ” D

- Exact path: `Assets/MediumMechStriker/FBX/ScifiMechPresentation/ScifiMechPresentationFrontHalf.fbx`; FBX; SHA-256 `cefd424efcdb67d0870d462d399a020ee40e9d9e2f19e8ab08e049f0840543c9`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 68; polygons 48; raw triangles 96; evaluated triangles not measured; materials 1 (ScifiMechPresentation).
- Mesh objects: ScifiMechPresentationFrontHalf (96 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: no actor production role. Parts: Flat decals/display podium geometry, not mechanical anatomy.
- Blender kitbash: No selected actor use; omit supplier decoration.
- Unity integration: ASCII FBX measured by a read-only parser; Blender did not import it. No rig or animation.
- Mobile/visual gap: Reject decorative branding/motif and presentation stand from final actors.

### ScifiMech20cmCanonBarrel.fbx вЂ” C

- Exact path: `Assets/MediumMechStriker/FBX/ScifiMechStandardWeapons/ScifiMech20cmCanonBarrel.fbx`; FBX; SHA-256 `da27598dd355fb6fa26945c865d64c6957096bd10ae7e8304b6ecc45a587defe`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 289; polygons 260; raw triangles 488; evaluated triangles not measured; materials 1 (ScifiMechStandardWeapons).
- Mesh objects: ScifiMech20cmCanonBarrel (488 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: Cutter mechanisms; Magnetar/Custodian weapon parts. Parts: Standalone rigid mesh; hands are separate geometry, not a hand skeleton. Weapon/damage parts have no rig or clips.
- Blender kitbash: Moderate: Blender 5.2 rejects this FBX 6.1 ASCII. Counts came from Objects-section array parsing; convert with a trusted private DCC/importer before kitbash.
- Unity integration: Unity import has not been tested. Preserve donor licence, convert privately, create sockets/pivots and rebuild materials.
- Mobile/visual gap: Small parts can be economical; appended hands/weapons add cost. Reauthor styling and atlas with actor materials.

### ScifiMechLRMRackX5.fbx вЂ” C

- Exact path: `Assets/MediumMechStriker/FBX/ScifiMechStandardWeapons/ScifiMechLRMRackX5.fbx`; FBX; SHA-256 `dcfb3e8a39e9cecf18cd3ada249551c885cdb35c55f3ddc922cf2cac4430a68a`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 104; polygons 76; raw triangles 152; evaluated triangles not measured; materials 1 (ScifiMechStandardWeapons).
- Mesh objects: ScifiMechLRMRackX5 (152 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: Cutter mechanisms; Magnetar/Custodian weapon parts. Parts: Standalone rigid mesh; hands are separate geometry, not a hand skeleton. Weapon/damage parts have no rig or clips.
- Blender kitbash: Moderate: Blender 5.2 rejects this FBX 6.1 ASCII. Counts came from Objects-section array parsing; convert with a trusted private DCC/importer before kitbash.
- Unity integration: Unity import has not been tested. Preserve donor licence, convert privately, create sockets/pivots and rebuild materials.
- Mobile/visual gap: Small parts can be economical; appended hands/weapons add cost. Reauthor styling and atlas with actor materials.

### ScifiMechLRMRackX5Half.fbx вЂ” C

- Exact path: `Assets/MediumMechStriker/FBX/ScifiMechStandardWeapons/ScifiMechLRMRackX5Half.fbx`; FBX; SHA-256 `4ae914179433219950f75c7c0a2189e67912818f4bfafa67ebba3c7b3197c640`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 64; polygons 46; raw triangles 92; evaluated triangles not measured; materials 1 (ScifiMechStandardWeapons).
- Mesh objects: ScifiMechLRMRackX5Half (92 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: Cutter mechanisms; Magnetar/Custodian weapon parts. Parts: Standalone rigid mesh; hands are separate geometry, not a hand skeleton. Weapon/damage parts have no rig or clips.
- Blender kitbash: Moderate: Blender 5.2 rejects this FBX 6.1 ASCII. Counts came from Objects-section array parsing; convert with a trusted private DCC/importer before kitbash.
- Unity integration: Unity import has not been tested. Preserve donor licence, convert privately, create sockets/pivots and rebuild materials.
- Mobile/visual gap: Small parts can be economical; appended hands/weapons add cost. Reauthor styling and atlas with actor materials.

### ScifiMechMediumStandardLaser.fbx вЂ” C

- Exact path: `Assets/MediumMechStriker/FBX/ScifiMechStandardWeapons/ScifiMechMediumStandardLaser.fbx`; FBX; SHA-256 `c1111aa7da86fe8f833c4163980d00626830c32f67d70c1efcf5e5d74922c06d`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 325; polygons 312; raw triangles 570; evaluated triangles not measured; materials 1 (ScifiMechStandardWeapons).
- Mesh objects: ScifiMechMediumStandardLaser (570 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: Cutter mechanisms; Magnetar/Custodian weapon parts. Parts: Standalone rigid mesh; hands are separate geometry, not a hand skeleton. Weapon/damage parts have no rig or clips.
- Blender kitbash: Moderate: Blender 5.2 rejects this FBX 6.1 ASCII. Counts came from Objects-section array parsing; convert with a trusted private DCC/importer before kitbash.
- Unity integration: Unity import has not been tested. Preserve donor licence, convert privately, create sockets/pivots and rebuild materials.
- Mobile/visual gap: Small parts can be economical; appended hands/weapons add cost. Reauthor styling and atlas with actor materials.

### ScifiMechMGBarrel.fbx вЂ” C

- Exact path: `Assets/MediumMechStriker/FBX/ScifiMechStandardWeapons/ScifiMechMGBarrel.fbx`; FBX; SHA-256 `6dd912c869b488b2a7ed32d0c4097b9a9206d7e5a20c14940908c4c6d8df136c`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 97; polygons 84; raw triangles 156; evaluated triangles not measured; materials 1 (ScifiMechStandardWeapons).
- Mesh objects: ScifiMechMGBarrel (156 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: Cutter mechanisms; Magnetar/Custodian weapon parts. Parts: Standalone rigid mesh; hands are separate geometry, not a hand skeleton. Weapon/damage parts have no rig or clips.
- Blender kitbash: Moderate: Blender 5.2 rejects this FBX 6.1 ASCII. Counts came from Objects-section array parsing; convert with a trusted private DCC/importer before kitbash.
- Unity integration: Unity import has not been tested. Preserve donor licence, convert privately, create sockets/pivots and rebuild materials.
- Mobile/visual gap: Small parts can be economical; appended hands/weapons add cost. Reauthor styling and atlas with actor materials.

### ScifiMechSRMRackX2.fbx вЂ” C

- Exact path: `Assets/MediumMechStriker/FBX/ScifiMechStandardWeapons/ScifiMechSRMRackX2.fbx`; FBX; SHA-256 `415cd1fd137967bb799e991e857a0fbb06b8f57dde2fbd7d74871602d4994460`. Method: ASCII FBX Objects-array parser ONLY; Blender import failed.
- Objects 1; meshes 1; vertices 120; polygons 100; raw triangles 184; evaluated triangles not measured; materials 1 (ScifiMechStandardWeapons).
- Mesh objects: ScifiMechSRMRackX2 (184 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 33.
- Selected roles: Cutter mechanisms; Magnetar/Custodian weapon parts. Parts: Standalone rigid mesh; hands are separate geometry, not a hand skeleton. Weapon/damage parts have no rig or clips.
- Blender kitbash: Moderate: Blender 5.2 rejects this FBX 6.1 ASCII. Counts came from Objects-section array parsing; convert with a trusted private DCC/importer before kitbash.
- Unity integration: Unity import has not been tested. Preserve donor licence, convert privately, create sockets/pivots and rebuild materials.
- Mobile/visual gap: Small parts can be economical; appended hands/weapons add cost. Reauthor styling and atlas with actor materials.

## Tower Defence Sci-Fi Turret FREE

Canonical archive: `ExternalAssetIntake/Current/tower-defence-turret-free/Tower Defence Sci-Fi Turret FREE.unitypackage`. [Official source](https://assetstore.unity.com/packages/3d/environments/sci-fi/tower-defence-sci-fi-turret-free-246331). Licence: Standard Unity Asset Store EULA.

### turret1.fbx вЂ” C

- Exact path: `Assets/TD_Sci-Fi_Turret1_Example/Models/turret1.fbx`; FBX; SHA-256 `0907664d2701d2ee8b5cc2b88554d4f5cceac5c365619f765082e5aee2b541bc`. Method: Blender import/source read.
- Objects 9; meshes 9; vertices 3,230; polygons 2,983; raw triangles 5,784; evaluated triangles 5784; materials 3 (turret_base1, turret_head1, turret_mount1).
- Mesh objects: turret_base1 (638 tris); turret_head1 (2,212 tris); turret_head1_armor_plate_l (184 tris); turret_head1_barrel_l (616 tris); turret_head1_barrel_r (616 tris); turret_head_1_armor_plate_r (184 tris); turret_mount1 (604 tris); turret_mount1_mid (20 tris); turret_mount1_top (710 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 18.
- Selected roles: Cutter paired mechanisms; Magnetar pivots; Custodian articulation. Parts: Nine separate meshes: base/head/mount sections, paired barrels and armour plates. No skeleton or baked animation.
- Blender kitbash: Easy-to-moderate rigid assembly extraction. Useful paired mounting hardware; armour plates are NOT handheld shields.
- Unity integration: Create named pivots and authored joint motion; material-name binding is unresolved by the FBX inspection. Relink package maps and rebuild URP materials.
- Mobile/visual gap: 5,784 triangles/three materials; selective modules only. Custom cutting tools, shields, reactor and boss structure remain necessary.

## UnityFan Free Sci-Fi Vehicle 011-02

Canonical archive: `ExternalAssetIntake/Current/unityfan-scifi-vehicle-011-02/free-sci-fi-vehicle-011-02-public-domain-cc0.zip`. [Official source](https://sketchfab.com/3d-models/free-sci-fi-vehicle-011-02-public-domain-cc0-3887e33fd0514df795739c24bfc137c8). Licence: Author CC0 public-domain dedication on exact Sketchfab item; platform Free Standard label also displayed.

### sci-fi_vehicle_012_02.fbx вЂ” B

- Exact path: `source/sci-fi_vehicle_012_02.fbx`; FBX; SHA-256 `cba723dc2898e731d40003c00098bf2ff82d48f897297479d23d155b73da9a7d`. Method: Blender import/source read.
- Objects 17; meshes 17; vertices 8,146; polygons 7,991; raw triangles 15,519; evaluated triangles 15519; materials 6 (Material, Material.001, Material.002, Material.003, Material.004, chrome.001).
- Mesh objects: Cube (264 tris); Cube.001 (268 tris); Cube.002 (1,040 tris); Cube.004 (920 tris); Cube.014 (392 tris); Cube.015 (724 tris); Cube.016 (216 tris); Cube.017 (592 tris); Cube.018 (1,032 tris); Cube.019 (1,248 tris); Cube.020 (696 tris); Cube.021 (152 tris); Cube.022 (3,956 tris); Cube.023 (104 tris); Cube.029 (176 tris); Cube.033 (1,728 tris); Unity_Fan_logo.004 (2,011 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (1, not proven shader bindings): textures/internal_ground_ao_texture.jpeg (512x512). Whole package readable raster images: 1.
- Selected roles: Carrier. Parts: Low streamlined vehicle shell with separate rigid mesh objects; no bones, no clips and no animated running gear. Object separation alone does not prove hinges/pivots are usable.
- Blender kitbash: Easy editable Blender 012/012-01; moderate imported FBX 011-02/022. 012 is the best low wedge/hover shell. Retain a deterministic modifier/export copy; remove supplier logos where present.
- Unity integration: New root/pivot rig and hover/idle/bank/drift/death clips needed. Ground AO JPEG is not a complete vehicle PBR texture set; author UVs/atlas/materials and enclosed assault body fittings.
- Mobile/visual gap: Use evaluated triangles: 012 is 9,248, not its 485 control-cage triangles; 012-01 is 21,753, not 4,942. 011-02 is 15,519 and 022 is 36,916. Five to seven materials need consolidation. Refitted shell still needs significant custom geometry and device profiling.

## UnityFan Free Sci-Fi Vehicle 012

Canonical archive: `ExternalAssetIntake/Current/unityfan-scifi-vehicle-012/free-sci-fi-vehicle-012-public-domain-cc0.zip`. [Official source](https://sketchfab.com/3d-models/free-sci-fi-vehicle-012-public-domain-cc0-d69b22aa84304ecb8098138baa67c8dd). Licence: Author CC0 public-domain dedication on exact Sketchfab item; platform Free Standard label also displayed.

### sci-fi_vehicle_013_2.blend вЂ” A

- Exact path: `source/sci-fi_vehicle_013_2.blend`; BLEND; SHA-256 `c3b6164b80876271a2569f41e2661c7fc030effc4b6d8558819dd40dee9393c6`. Method: Blender import/source read.
- Objects 9; meshes 7; vertices 415; polygons 163; raw triangles 485; evaluated triangles 9248; materials 5 (blocker, body, glass, headlight, tailight).
- Mesh objects: Cube (40 tris); Cube.001 (32 tris); Cube.002 (255 tris); Cube.003 (112 tris); Cube.004 (32 tris); Cube.005 (4 tris); Cube.006 (10 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (1, not proven shader bindings): textures/internal_ground_ao_texture.jpeg (512x512). Whole package readable raster images: 1.
- Selected roles: Carrier. Parts: Low streamlined vehicle shell with separate rigid mesh objects; no bones, no clips and no animated running gear. Object separation alone does not prove hinges/pivots are usable.
- Blender kitbash: Easy editable Blender 012/012-01; moderate imported FBX 011-02/022. 012 is the best low wedge/hover shell. Retain a deterministic modifier/export copy; remove supplier logos where present.
- Unity integration: New root/pivot rig and hover/idle/bank/drift/death clips needed. Ground AO JPEG is not a complete vehicle PBR texture set; author UVs/atlas/materials and enclosed assault body fittings.
- Mobile/visual gap: Use evaluated triangles: 012 is 9,248, not its 485 control-cage triangles; 012-01 is 21,753, not 4,942. 011-02 is 15,519 and 022 is 36,916. Five to seven materials need consolidation. Refitted shell still needs significant custom geometry and device profiling.

## UnityFan Free Sci-Fi Vehicle 012-01

Canonical archive: `ExternalAssetIntake/Current/unityfan-scifi-vehicle-012-01/free-sci-fi-vehicle-012-01-public-domain-cc0.zip`. [Official source](https://sketchfab.com/3d-models/free-sci-fi-vehicle-012-01-public-domain-cc0-be5907569dec439881bcaf91284de555). Licence: Author CC0 public-domain dedication on exact Sketchfab item; platform Free Standard label also displayed.

### sci-fi_vehicle_011_01.blend вЂ” B

- Exact path: `source/sci-fi_vehicle_011_01.blend`; BLEND; SHA-256 `4c62d3c9a1a242164aba92c0bbf02c8f8deea5b36c7de06d87fa8d56e04a0ee1`. Method: Blender import/source read.
- Objects 11; meshes 9; vertices 3,715; polygons 2,409; raw triangles 4,942; evaluated triangles 21753; materials 7 (Material, Material.001, Material.002, Material.003, Material.004, Material.005, chrome.001).
- Mesh objects: Cube (132 tris); Cube.001 (409 tris); Cube.002 (44 tris); Cube.003 (10 tris); Cube.004 (23 tris); Unity_Fan_logo (1,081 tris); Unity_Fan_logo.001 (1,081 tris); Unity_Fan_logo.002 (1,081 tris); Unity_Fan_logo.003 (1,081 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 0.
- Selected roles: Carrier. Parts: Low streamlined vehicle shell with separate rigid mesh objects; no bones, no clips and no animated running gear. Object separation alone does not prove hinges/pivots are usable.
- Blender kitbash: Easy editable Blender 012/012-01; moderate imported FBX 011-02/022. 012 is the best low wedge/hover shell. Retain a deterministic modifier/export copy; remove supplier logos where present.
- Unity integration: New root/pivot rig and hover/idle/bank/drift/death clips needed. Ground AO JPEG is not a complete vehicle PBR texture set; author UVs/atlas/materials and enclosed assault body fittings.
- Mobile/visual gap: Use evaluated triangles: 012 is 9,248, not its 485 control-cage triangles; 012-01 is 21,753, not 4,942. 011-02 is 15,519 and 022 is 36,916. Five to seven materials need consolidation. Refitted shell still needs significant custom geometry and device profiling.

## UnityFan Free Sci-Fi Vehicle 022

Canonical archive: `ExternalAssetIntake/Current/unityfan-scifi-vehicle-022/free-sci-fi-vehicle-022-public-domain-cc0.zip`. [Official source](https://sketchfab.com/3d-models/free-sci-fi-vehicle-022-public-domain-cc0-eba7965b617d4b97a79dbb0a673934c4). Licence: Author CC0 public-domain dedication on exact Sketchfab item; platform Free Standard label also displayed.

### sci-fi_vehicle_022_02.fbx вЂ” B

- Exact path: `source/sci-fi_vehicle_022_02.fbx`; FBX; SHA-256 `876566b1629a64b393f179afdccc1d96b163d85d1e24071da373807baf5252eb`. Method: Blender import/source read.
- Objects 6; meshes 6; vertices 18,518; polygons 18,652; raw triangles 36,916; evaluated triangles 36916; materials 5 (Material, body, glass, headlight, taillights).
- Mesh objects: Cube (17,840 tris); Cube.002 (2,212 tris); Cube.003 (472 tris); Cube.004 (1,160 tris); Cube.005 (1,208 tris); Cube.007 (14,024 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (1, not proven shader bindings): textures/internal_ground_ao_texture.jpeg (512x512). Whole package readable raster images: 1.
- Selected roles: Carrier. Parts: Low streamlined vehicle shell with separate rigid mesh objects; no bones, no clips and no animated running gear. Object separation alone does not prove hinges/pivots are usable.
- Blender kitbash: Easy editable Blender 012/012-01; moderate imported FBX 011-02/022. 012 is the best low wedge/hover shell. Retain a deterministic modifier/export copy; remove supplier logos where present.
- Unity integration: New root/pivot rig and hover/idle/bank/drift/death clips needed. Ground AO JPEG is not a complete vehicle PBR texture set; author UVs/atlas/materials and enclosed assault body fittings.
- Mobile/visual gap: Use evaluated triangles: 012 is 9,248, not its 485 control-cage triangles; 012-01 is 21,753, not 4,942. 011-02 is 15,519 and 022 is 36,916. Five to seven materials need consolidation. Refitted shell still needs significant custom geometry and device profiling.

## VoodooPlay Low Poly Combat Drone

Canonical archive: `ExternalAssetIntake/Current/voodooplay-low-poly-combat-drone/Low poly combat drone.unitypackage`. [Official source](https://assetstore.unity.com/packages/3d/characters/robots/low-poly-combat-drone-82234). Licence: Standard Unity Asset Store EULA.

### Drone.FBX вЂ” B

- Exact path: `Assets/VoodooPlay/Models/Drone/Drone.FBX`; FBX; SHA-256 `447f649d27eb87c808d7d8f5d776da107a92108b1d9d6edbe99e40ff91e85393`. Method: Blender import/source read.
- Objects 1; meshes 1; vertices 709; polygons 1,362; raw triangles 1,362; evaluated triangles 1362; materials 1 (01 - Default).
- Mesh objects: Cylinder001 (1,362 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (4): Assets/VoodooPlay/Models/Materials/Drone_Albedo.png (2048x2048); Assets/VoodooPlay/Models/Materials/Drone_Emission.png (2048x2048); Assets/VoodooPlay/Models/Materials/Drone_NormalMap.png (2048x2048); Assets/VoodooPlay/Models/Materials/Drone_Specular.tga (1024x1024).
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 7.
- Selected roles: Arc Drone secondary shell. Parts: One combined static aerial fighter/thruster mesh; no armature or animation. Low-poly is the supplier name, not the target art style.
- Blender kitbash: Moderate shell/pod extraction and reauthoring; useful aerial proportions, not a replacement for EyeDrone articulation.
- Unity integration: Rebuild 2K specular workflow maps for URP metallic/smoothness; author flight pivots and energy attack animation.
- Mobile/visual gap: 1,362 triangles/one material. Detail, mechanical layering and contained ranged emitter need substantial custom work.

## Seed Hunter

Canonical archive: `ExternalAssetIntake/Current/seed-hunter/Seed Hunter.unitypackage`. [Official source](https://assetstore.unity.com/packages/3d/environments/seed-hunter-143414). Licence: Standard Unity Asset Store EULA.

### coll_Hallway_GateFrame.FBX вЂ” D

- Exact path: `Assets/Polygonautic/SeedHunter/Art Environment/Hallway_GateFrame/coll_Hallway_GateFrame.FBX`; FBX; SHA-256 `020132aa0fd5a86eed778240b418345ca9e92c2ee5ecb73bb6530cd0ee0e2833`. Method: Blender import/source read.
- Objects 1; meshes 1; vertices 56; polygons 92; raw triangles 92; evaluated triangles 92; materials 0 ().
- Mesh objects: coll_Hallway_GateFrame (92 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 162.
- Selected roles: no actor mechanics role. Parts: Collision mesh, organic snake/roots sculpture or HUD panels; no mechanical rig or clips.
- Blender kitbash: Not selected for actor anatomy. Serpent is actual ornamental snakes, not an articulated machine.
- Unity integration: No actor import recommended; package remains environment/secondary drone reference.
- Mobile/visual gap: Roots are 74,276/147,404 triangles and Serpent is 28,296; expensive organic forms and wrong anatomy. Collision/HUD meshes are not production body donors.

### env_Hallway_GateFrame.FBX вЂ” C

- Exact path: `Assets/Polygonautic/SeedHunter/Art Environment/Hallway_GateFrame/env_Hallway_GateFrame.FBX`; FBX; SHA-256 `0b6e90495f95f23408bc7c1a2c719ebea18735948a9ff89e9b60b131901efae2`. Method: Blender import/source read.
- Objects 1; meshes 1; vertices 316; polygons 485; raw triangles 485; evaluated triangles 485; materials 1 (env_Hallway_GateFrame).
- Mesh objects: env_Hallway_GateFrame (485 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 162.
- Selected roles: Custodian containment frames; Magnetar optional housing. Parts: Static gate/frame pieces; gate has separate left/right/bottom geometry, eye has sensor/body. No embedded model clips or rig.
- Blender kitbash: Moderate rigid hardware extraction; replace ornate/organic source motifs with industrial containment.
- Unity integration: HDRP-to-URP material conversion and new authored pivots/motion required. Package has one .anim but that does not establish actor animation.
- Mobile/visual gap: Small frames can help containment; 10-material gate-frame step requires consolidation. No central reactor or complete boss architecture supplied.

### env_Hallway_GateFrame_hole.FBX вЂ” C

- Exact path: `Assets/Polygonautic/SeedHunter/Art Environment/Hallway_GateFrame/env_Hallway_GateFrame_hole.FBX`; FBX; SHA-256 `c61fa089124790d742d5f60f7899337f09d9d71af4fbd62a6fabfa7abbff6b7d`. Method: Blender import/source read.
- Objects 1; meshes 1; vertices 322; polygons 493; raw triangles 493; evaluated triangles 493; materials 1 (env_Hallway_GateFrame).
- Mesh objects: env_Hallway_GateFrame_hole (493 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 162.
- Selected roles: Custodian containment frames; Magnetar optional housing. Parts: Static gate/frame pieces; gate has separate left/right/bottom geometry, eye has sensor/body. No embedded model clips or rig.
- Blender kitbash: Moderate rigid hardware extraction; replace ornate/organic source motifs with industrial containment.
- Unity integration: HDRP-to-URP material conversion and new authored pivots/motion required. Package has one .anim but that does not establish actor animation.
- Mobile/visual gap: Small frames can help containment; 10-material gate-frame step requires consolidation. No central reactor or complete boss architecture supplied.

### env_Hallway_GateFrame_step.FBX вЂ” C

- Exact path: `Assets/Polygonautic/SeedHunter/Art Environment/Hallway_GateFrame/env_Hallway_GateFrame_step.FBX`; FBX; SHA-256 `0039db8450fb2a0e9714976e57293530a849d95f2dcc3d2d34dabf6c14127784`. Method: Blender import/source read.
- Objects 1; meshes 1; vertices 2,006; polygons 3,944; raw triangles 3,944; evaluated triangles 3944; materials 10 (Material #231, myMaterial1, myMaterial10, myMaterial2, myMaterial3, myMaterial5, myMaterial6, myMaterial7, myMaterial8, myMaterial9).
- Mesh objects: env_Hallway_GateFrame_step (3,944 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 162.
- Selected roles: Custodian containment frames; Magnetar optional housing. Parts: Static gate/frame pieces; gate has separate left/right/bottom geometry, eye has sensor/body. No embedded model clips or rig.
- Blender kitbash: Moderate rigid hardware extraction; replace ornate/organic source motifs with industrial containment.
- Unity integration: HDRP-to-URP material conversion and new authored pivots/motion required. Package has one .anim but that does not establish actor animation.
- Mobile/visual gap: Small frames can help containment; 10-material gate-frame step requires consolidation. No central reactor or complete boss architecture supplied.

### env_Roots_gate.FBX вЂ” D

- Exact path: `Assets/Polygonautic/SeedHunter/Art Environment/Roots/env_Roots_gate.FBX`; FBX; SHA-256 `d0e9f14c0a5d6d6b10ba2e79a67bed1dbc2e27342e7fc7cc7e2df66520cda69c`. Method: Blender import/source read.
- Objects 1; meshes 1; vertices 57,384; polygons 74,276; raw triangles 74,276; evaluated triangles 74276; materials 2 (Material #25, Material #26).
- Mesh objects: env_Roots_gate (74,276 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (2): Assets/Polygonautic/SeedHunter/Art Environment/Roots/Textures/leaf_BaseColor.tga (1024x1024); Assets/Polygonautic/SeedHunter/Art Environment/Roots/Textures/tile_Roots_BaseColor.png (2048x2048).
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 162.
- Selected roles: no actor mechanics role. Parts: Collision mesh, organic snake/roots sculpture or HUD panels; no mechanical rig or clips.
- Blender kitbash: Not selected for actor anatomy. Serpent is actual ornamental snakes, not an articulated machine.
- Unity integration: No actor import recommended; package remains environment/secondary drone reference.
- Mobile/visual gap: Roots are 74,276/147,404 triangles and Serpent is 28,296; expensive organic forms and wrong anatomy. Collision/HUD meshes are not production body donors.

### env_Roots_serpent.FBX вЂ” D

- Exact path: `Assets/Polygonautic/SeedHunter/Art Environment/Roots/env_Roots_serpent.FBX`; FBX; SHA-256 `71681724ddc7cdde63eff5e3489d9a439384cd9ad80e489794418f4f4afef694`. Method: Blender import/source read.
- Objects 1; meshes 1; vertices 115,752; polygons 147,404; raw triangles 147,404; evaluated triangles 147404; materials 2 (2mat1, 2mat2).
- Mesh objects: env_Roots_serpent (147,404 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (2): Assets/Polygonautic/SeedHunter/Art Environment/Roots/Textures/leaf_BaseColor.tga (1024x1024); Assets/Polygonautic/SeedHunter/Art Environment/Roots/Textures/tile_Roots_BaseColor.png (2048x2048).
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 162.
- Selected roles: no actor mechanics role. Parts: Collision mesh, organic snake/roots sculpture or HUD panels; no mechanical rig or clips.
- Blender kitbash: Not selected for actor anatomy. Serpent is actual ornamental snakes, not an articulated machine.
- Unity integration: No actor import recommended; package remains environment/secondary drone reference.
- Mobile/visual gap: Roots are 74,276/147,404 triangles and Serpent is 28,296; expensive organic forms and wrong anatomy. Collision/HUD meshes are not production body donors.

### coll_Gate.FBX вЂ” D

- Exact path: `Assets/Polygonautic/SeedHunter/Art Props/Gate/coll_Gate.FBX`; FBX; SHA-256 `5043814eee1ab6888d08dbf4ee528302cbc21825765ff6e9927ec1ac4d0d2fb9`. Method: Blender import/source read.
- Objects 4; meshes 3; vertices 26; polygons 28; raw triangles 40; evaluated triangles 40; materials 0 ().
- Mesh objects: coll_bottom (16 tris); coll_left (12 tris); coll_right (12 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 162.
- Selected roles: no actor mechanics role. Parts: Collision mesh, organic snake/roots sculpture or HUD panels; no mechanical rig or clips.
- Blender kitbash: Not selected for actor anatomy. Serpent is actual ornamental snakes, not an articulated machine.
- Unity integration: No actor import recommended; package remains environment/secondary drone reference.
- Mobile/visual gap: Roots are 74,276/147,404 triangles and Serpent is 28,296; expensive organic forms and wrong anatomy. Collision/HUD meshes are not production body donors.

### prop_Gate.FBX вЂ” C

- Exact path: `Assets/Polygonautic/SeedHunter/Art Props/Gate/prop_Gate.FBX`; FBX; SHA-256 `fc1e5d6857ad02ba2186a6eb7f4cacfec7221118d8d290c28fcc4541a13fb9b6`. Method: Blender import/source read.
- Objects 4; meshes 3; vertices 2,237; polygons 3,842; raw triangles 3,842; evaluated triangles 3842; materials 3 (2mat1, 2mat2, prop_Gate).
- Mesh objects: bottom (666 tris); left (1,588 tris); right (1,588 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 162.
- Selected roles: Custodian containment frames; Magnetar optional housing. Parts: Static gate/frame pieces; gate has separate left/right/bottom geometry, eye has sensor/body. No embedded model clips or rig.
- Blender kitbash: Moderate rigid hardware extraction; replace ornate/organic source motifs with industrial containment.
- Unity integration: HDRP-to-URP material conversion and new authored pivots/motion required. Package has one .anim but that does not establish actor animation.
- Mobile/visual gap: Small frames can help containment; 10-material gate-frame step requires consolidation. No central reactor or complete boss architecture supplied.

### prop_GateEye.FBX вЂ” C

- Exact path: `Assets/Polygonautic/SeedHunter/Art Props/Gate/prop_GateEye.FBX`; FBX; SHA-256 `cba1a6627fee698796ead5c8a8c4247cceb046a1f34457b90172e5683ca033d6`. Method: Blender import/source read.
- Objects 2; meshes 2; vertices 520; polygons 988; raw triangles 988; evaluated triangles 988; materials 1 (prop_GateEye).
- Mesh objects: prop_GateEye (808 tris); sensor (180 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 162.
- Selected roles: Custodian containment frames; Magnetar optional housing. Parts: Static gate/frame pieces; gate has separate left/right/bottom geometry, eye has sensor/body. No embedded model clips or rig.
- Blender kitbash: Moderate rigid hardware extraction; replace ornate/organic source motifs with industrial containment.
- Unity integration: HDRP-to-URP material conversion and new authored pivots/motion required. Package has one .anim but that does not establish actor animation.
- Mobile/visual gap: Small frames can help containment; 10-material gate-frame step requires consolidation. No central reactor or complete boss architecture supplied.

### prop_Keydrone.FBX вЂ” B

- Exact path: `Assets/Polygonautic/SeedHunter/Art Props/Keydrone/prop_Keydrone.FBX`; FBX; SHA-256 `5462f8b67bde6c328a866b4898e2a424c489732e9dd3b1091bba2e7851ad3110`. Method: Blender import/source read.
- Objects 1; meshes 1; vertices 2,071; polygons 4,076; raw triangles 4,076; evaluated triangles 4076; materials 1 (prop_Keydrone).
- Mesh objects: prop_Keydrone (4,076 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 162.
- Selected roles: Arc Drone secondary shell. Parts: Angular floating key-drone shell, one mesh. No bones/embedded clips; NOT a rigged flying enemy.
- Blender kitbash: Moderate shell adaptation; use EyeDrone articulation only through a deliberate new skin/rig, not automatic compatibility.
- Unity integration: HDRP donor with four 4K maps. Repack mask channels to URP, downsample selectively; create flight and ranged attack animations.
- Mobile/visual gap: 4,076 triangles/one material. Stronger surface detail than the simple kit, but key/ornate motif and core emitter need redesign.

### prop_Serpent.FBX вЂ” D

- Exact path: `Assets/Polygonautic/SeedHunter/Art Props/Serpent/prop_Serpent.FBX`; FBX; SHA-256 `01c2b1513c97b756dec93350319869910870e7f02c3c47c165c968186978e553`. Method: Blender import/source read.
- Objects 1; meshes 1; vertices 14,230; polygons 28,296; raw triangles 28,296; evaluated triangles 28296; materials 2 (05 - Default, env_chamber_serpent).
- Mesh objects: env_Chamber_serpent (28,296 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 162.
- Selected roles: no actor mechanics role. Parts: Collision mesh, organic snake/roots sculpture or HUD panels; no mechanical rig or clips.
- Blender kitbash: Not selected for actor anatomy. Serpent is actual ornamental snakes, not an articulated machine.
- Unity integration: No actor import recommended; package remains environment/secondary drone reference.
- Mobile/visual gap: Roots are 74,276/147,404 triangles and Serpent is 28,296; expensive organic forms and wrong anatomy. Collision/HUD meshes are not production body donors.

### keydrone_hud_panel.FBX вЂ” D

- Exact path: `Assets/Polygonautic/SeedHunter/Effects/keydrone_hud_panel.FBX`; FBX; SHA-256 `859264b0e11579383be4c451c5c7dd78e7ca04b6f60385a56e6d2bcb0abaace8`. Method: Blender import/source read.
- Objects 6; meshes 6; vertices 182; polygons 150; raw triangles 150; evaluated triangles 150; materials 3 (01 - Default, myMaterial2, myMaterial3).
- Mesh objects: keydrone_hud_panel (22 tris); L_panel (25 tris); L_wing (27 tris); R_panel (25 tris); R_wing (24 tris); triangle (27 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (1): Assets/Polygonautic/SeedHunter/Effects/Textures/hud_Keydrone_lamp.tga (1024x1024).
- Additional local texture candidates (0, not proven shader bindings): none resolved. Whole package readable raster images: 162.
- Selected roles: no actor mechanics role. Parts: Collision mesh, organic snake/roots sculpture or HUD panels; no mechanical rig or clips.
- Blender kitbash: Not selected for actor anatomy. Serpent is actual ornamental snakes, not an articulated machine.
- Unity integration: No actor import recommended; package remains environment/secondary drone reference.
- Mobile/visual gap: Roots are 74,276/147,404 triangles and Serpent is 28,296; expensive organic forms and wrong anatomy. Collision/HUD meshes are not production body donors.

## WarZone Sci-Fi Turret Pack

Canonical archive: `ExternalAssetIntake/Current/warzone-scifi-turret/WarZone Sci-Fi Turret pack.unitypackage`. [Official source](https://assetstore.unity.com/packages/3d/environments/warzone-sci-fi-turret-pack-57540). Licence: Standard Unity Asset Store EULA.

### gun.FBX вЂ” C

- Exact path: `Assets/WarZone Sci-Fi Turret pack/gun.FBX`; FBX; SHA-256 `f7a27047c664a5cc942e3d75c2476ceaf62cf01795259a6a0b2636d18a571a34`. Method: Blender import/source read.
- Objects 1; meshes 1; vertices 920; polygons 1,490; raw triangles 1,490; evaluated triangles 1490; materials 2 (02 - Default, 03 - Default).
- Mesh objects: gun (1,490 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (1, not proven shader bindings): Assets/WarZone Sci-Fi Turret pack/Materials/TurretTexture.png (256x256). Whole package readable raster images: 1.
- Selected roles: Magnetar turret hardware; Custodian articulation; Cutter weapon mounts. Parts: Static rigid weapon assembly; mesh object names below show detachable parts. No armature or clips; multi-mesh assemblies can become pivoted modules.
- Blender kitbash: Easy-to-moderate rigid kitbash; establish pivots and replace generic weapon styling.
- Unity integration: Two materials per candidate, package contains one 256x256 colour texture. No tested moving mechanism/animation supplied.
- Mobile/visual gap: Use selected hardware, bake a consistent PBR atlas and consolidate materials. Whole turrets do not supply elite/boss anatomy.

### gun1.FBX вЂ” C

- Exact path: `Assets/WarZone Sci-Fi Turret pack/gun1.FBX`; FBX; SHA-256 `1d093e7326f9a72d32ec4b43c460b9430ffbe5312a9abe9980e05f53fbfd8289`. Method: Blender import/source read.
- Objects 4; meshes 3; vertices 3,145; polygons 5,084; raw triangles 5,136; evaluated triangles 5136; materials 2 (02 - Default, 03 - Default).
- Mesh objects: gun.001 (2,878 tris); gun.002 (1,690 tris); radar (568 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (1, not proven shader bindings): Assets/WarZone Sci-Fi Turret pack/Materials/TurretTexture.png (256x256). Whole package readable raster images: 1.
- Selected roles: Magnetar turret hardware; Custodian articulation; Cutter weapon mounts. Parts: Static rigid weapon assembly; mesh object names below show detachable parts. No armature or clips; multi-mesh assemblies can become pivoted modules.
- Blender kitbash: Easy-to-moderate rigid kitbash; establish pivots and replace generic weapon styling.
- Unity integration: Two materials per candidate, package contains one 256x256 colour texture. No tested moving mechanism/animation supplied.
- Mobile/visual gap: Use selected hardware, bake a consistent PBR atlas and consolidate materials. Whole turrets do not supply elite/boss anatomy.

### gun2.FBX вЂ” C

- Exact path: `Assets/WarZone Sci-Fi Turret pack/gun2.FBX`; FBX; SHA-256 `38e085a36c23ad7b85616c53195e3f38d2bcf804b11970903c952bedac304a41`. Method: Blender import/source read.
- Objects 6; meshes 5; vertices 4,719; polygons 7,346; raw triangles 7,706; evaluated triangles 7706; materials 2 (02 - Default, 03 - Default).
- Mesh objects: gun (2,980 tris); gun.001 (1,690 tris); gun.002 (1,874 tris); Object031 (840 tris); rocket (322 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (1, not proven shader bindings): Assets/WarZone Sci-Fi Turret pack/Materials/TurretTexture.png (256x256). Whole package readable raster images: 1.
- Selected roles: Magnetar turret hardware; Custodian articulation; Cutter weapon mounts. Parts: Static rigid weapon assembly; mesh object names below show detachable parts. No armature or clips; multi-mesh assemblies can become pivoted modules.
- Blender kitbash: Easy-to-moderate rigid kitbash; establish pivots and replace generic weapon styling.
- Unity integration: Two materials per candidate, package contains one 256x256 colour texture. No tested moving mechanism/animation supplied.
- Mobile/visual gap: Use selected hardware, bake a consistent PBR atlas and consolidate materials. Whole turrets do not supply elite/boss anatomy.

### gun3.FBX вЂ” C

- Exact path: `Assets/WarZone Sci-Fi Turret pack/gun3.FBX`; FBX; SHA-256 `2bdcf7060a4997703238f7143922d0152ab9cca45bbf57aa1d96605cd38bcb67`. Method: Blender import/source read.
- Objects 2; meshes 2; vertices 2,205; polygons 3,564; raw triangles 3,564; evaluated triangles 3564; materials 2 (02 - Default, 03 - Default).
- Mesh objects: gun (1,690 tris); gun.001 (1,874 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (1, not proven shader bindings): Assets/WarZone Sci-Fi Turret pack/Materials/TurretTexture.png (256x256). Whole package readable raster images: 1.
- Selected roles: Magnetar turret hardware; Custodian articulation; Cutter weapon mounts. Parts: Static rigid weapon assembly; mesh object names below show detachable parts. No armature or clips; multi-mesh assemblies can become pivoted modules.
- Blender kitbash: Easy-to-moderate rigid kitbash; establish pivots and replace generic weapon styling.
- Unity integration: Two materials per candidate, package contains one 256x256 colour texture. No tested moving mechanism/animation supplied.
- Mobile/visual gap: Use selected hardware, bake a consistent PBR atlas and consolidate materials. Whole turrets do not supply elite/boss anatomy.

### lazer0.FBX вЂ” C

- Exact path: `Assets/WarZone Sci-Fi Turret pack/lazer0.FBX`; FBX; SHA-256 `51cb2f3004a3c64b05991adb7ab1f21fe96be21e4199d9b31829aaac49350deb`. Method: Blender import/source read.
- Objects 1; meshes 1; vertices 914; polygons 1,464; raw triangles 1,464; evaluated triangles 1464; materials 2 (02 - Default, 03 - Default).
- Mesh objects: lazer (1,464 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (1, not proven shader bindings): Assets/WarZone Sci-Fi Turret pack/Materials/TurretTexture.png (256x256). Whole package readable raster images: 1.
- Selected roles: Magnetar turret hardware; Custodian articulation; Cutter weapon mounts. Parts: Static rigid weapon assembly; mesh object names below show detachable parts. No armature or clips; multi-mesh assemblies can become pivoted modules.
- Blender kitbash: Easy-to-moderate rigid kitbash; establish pivots and replace generic weapon styling.
- Unity integration: Two materials per candidate, package contains one 256x256 colour texture. No tested moving mechanism/animation supplied.
- Mobile/visual gap: Use selected hardware, bake a consistent PBR atlas and consolidate materials. Whole turrets do not supply elite/boss anatomy.

### lazer1.FBX вЂ” C

- Exact path: `Assets/WarZone Sci-Fi Turret pack/lazer1.FBX`; FBX; SHA-256 `ca0a6e4d7249ae362a6a023881aee4d33c9cef87d247e7150c011a57dfaac086`. Method: Blender import/source read.
- Objects 2; meshes 2; vertices 1,955; polygons 3,190; raw triangles 3,190; evaluated triangles 3190; materials 2 (02 - Default, 03 - Default).
- Mesh objects: lazer (1,690 tris); lazer.001 (1,500 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (1, not proven shader bindings): Assets/WarZone Sci-Fi Turret pack/Materials/TurretTexture.png (256x256). Whole package readable raster images: 1.
- Selected roles: Magnetar turret hardware; Custodian articulation; Cutter weapon mounts. Parts: Static rigid weapon assembly; mesh object names below show detachable parts. No armature or clips; multi-mesh assemblies can become pivoted modules.
- Blender kitbash: Easy-to-moderate rigid kitbash; establish pivots and replace generic weapon styling.
- Unity integration: Two materials per candidate, package contains one 256x256 colour texture. No tested moving mechanism/animation supplied.
- Mobile/visual gap: Use selected hardware, bake a consistent PBR atlas and consolidate materials. Whole turrets do not supply elite/boss anatomy.

### machinegun1.FBX вЂ” C

- Exact path: `Assets/WarZone Sci-Fi Turret pack/machinegun1.FBX`; FBX; SHA-256 `124dbf6bad461db03e6854d945231e3de0b59b0aa9a4c4e0e070ece24e88de4c`. Method: Blender import/source read.
- Objects 2; meshes 2; vertices 3,839; polygons 6,092; raw triangles 6,092; evaluated triangles 6092; materials 2 (02 - Default, 03 - Default).
- Mesh objects: machinegun (4,402 tris); machinegun.001 (1,690 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (1, not proven shader bindings): Assets/WarZone Sci-Fi Turret pack/Materials/TurretTexture.png (256x256). Whole package readable raster images: 1.
- Selected roles: Magnetar turret hardware; Custodian articulation; Cutter weapon mounts. Parts: Static rigid weapon assembly; mesh object names below show detachable parts. No armature or clips; multi-mesh assemblies can become pivoted modules.
- Blender kitbash: Easy-to-moderate rigid kitbash; establish pivots and replace generic weapon styling.
- Unity integration: Two materials per candidate, package contains one 256x256 colour texture. No tested moving mechanism/animation supplied.
- Mobile/visual gap: Use selected hardware, bake a consistent PBR atlas and consolidate materials. Whole turrets do not supply elite/boss anatomy.

### machinegun2.FBX вЂ” C

- Exact path: `Assets/WarZone Sci-Fi Turret pack/machinegun2.FBX`; FBX; SHA-256 `af579f9e401199065b9e9ed75836986a34fd0ffa565584ee776600ed795e0a6f`. Method: Blender import/source read.
- Objects 1; meshes 1; vertices 316; polygons 502; raw triangles 502; evaluated triangles 502; materials 2 (02 - Default, 03 - Default).
- Mesh objects: machinegun (502 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (1, not proven shader bindings): Assets/WarZone Sci-Fi Turret pack/Materials/TurretTexture.png (256x256). Whole package readable raster images: 1.
- Selected roles: Magnetar turret hardware; Custodian articulation; Cutter weapon mounts. Parts: Static rigid weapon assembly; mesh object names below show detachable parts. No armature or clips; multi-mesh assemblies can become pivoted modules.
- Blender kitbash: Easy-to-moderate rigid kitbash; establish pivots and replace generic weapon styling.
- Unity integration: Two materials per candidate, package contains one 256x256 colour texture. No tested moving mechanism/animation supplied.
- Mobile/visual gap: Use selected hardware, bake a consistent PBR atlas and consolidate materials. Whole turrets do not supply elite/boss anatomy.

### machinegun3.FBX вЂ” C

- Exact path: `Assets/WarZone Sci-Fi Turret pack/machinegun3.FBX`; FBX; SHA-256 `a6391a49711747bf00f50d60f309e51b39c1aefb06db9e4e002996c540dac2a4`. Method: Blender import/source read.
- Objects 2; meshes 2; vertices 2,293; polygons 3,624; raw triangles 3,624; evaluated triangles 3624; materials 2 (02 - Default, 03 - Default).
- Mesh objects: machinegun (1,934 tris); machinegun.001 (1,690 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (1, not proven shader bindings): Assets/WarZone Sci-Fi Turret pack/Materials/TurretTexture.png (256x256). Whole package readable raster images: 1.
- Selected roles: Magnetar turret hardware; Custodian articulation; Cutter weapon mounts. Parts: Static rigid weapon assembly; mesh object names below show detachable parts. No armature or clips; multi-mesh assemblies can become pivoted modules.
- Blender kitbash: Easy-to-moderate rigid kitbash; establish pivots and replace generic weapon styling.
- Unity integration: Two materials per candidate, package contains one 256x256 colour texture. No tested moving mechanism/animation supplied.
- Mobile/visual gap: Use selected hardware, bake a consistent PBR atlas and consolidate materials. Whole turrets do not supply elite/boss anatomy.

### radar.FBX вЂ” C

- Exact path: `Assets/WarZone Sci-Fi Turret pack/radar.FBX`; FBX; SHA-256 `9e2c559143ffc453dd8feb55684b48f6e4e032b0e5d8c5708a1c31518fc33e51`. Method: Blender import/source read.
- Objects 1; meshes 1; vertices 308; polygons 516; raw triangles 568; evaluated triangles 568; materials 2 (02 - Default, 03 - Default).
- Mesh objects: radar (568 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (1, not proven shader bindings): Assets/WarZone Sci-Fi Turret pack/Materials/TurretTexture.png (256x256). Whole package readable raster images: 1.
- Selected roles: Magnetar turret hardware; Custodian articulation; Cutter weapon mounts. Parts: Static rigid weapon assembly; mesh object names below show detachable parts. No armature or clips; multi-mesh assemblies can become pivoted modules.
- Blender kitbash: Easy-to-moderate rigid kitbash; establish pivots and replace generic weapon styling.
- Unity integration: Two materials per candidate, package contains one 256x256 colour texture. No tested moving mechanism/animation supplied.
- Mobile/visual gap: Use selected hardware, bake a consistent PBR atlas and consolidate materials. Whole turrets do not supply elite/boss anatomy.

### rocket.FBX вЂ” C

- Exact path: `Assets/WarZone Sci-Fi Turret pack/rocket.FBX`; FBX; SHA-256 `fb83c51ad4919be43eea77ce5a63dbe03df3af83e1dffdbe8e0c1418e6e64372`. Method: Blender import/source read.
- Objects 1; meshes 1; vertices 674; polygons 1,162; raw triangles 1,162; evaluated triangles 1162; materials 2 (02 - Default, 03 - Default).
- Mesh objects: rocket (1,162 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (1, not proven shader bindings): Assets/WarZone Sci-Fi Turret pack/Materials/TurretTexture.png (256x256). Whole package readable raster images: 1.
- Selected roles: Magnetar turret hardware; Custodian articulation; Cutter weapon mounts. Parts: Static rigid weapon assembly; mesh object names below show detachable parts. No armature or clips; multi-mesh assemblies can become pivoted modules.
- Blender kitbash: Easy-to-moderate rigid kitbash; establish pivots and replace generic weapon styling.
- Unity integration: Two materials per candidate, package contains one 256x256 colour texture. No tested moving mechanism/animation supplied.
- Mobile/visual gap: Use selected hardware, bake a consistent PBR atlas and consolidate materials. Whole turrets do not supply elite/boss anatomy.

### rocket1.FBX вЂ” C

- Exact path: `Assets/WarZone Sci-Fi Turret pack/rocket1.FBX`; FBX; SHA-256 `e5e785011506f35240ccd3776e853d9536a092af1d362b1acc5f15766f2b8080`. Method: Blender import/source read.
- Objects 4; meshes 4; vertices 3,537; polygons 4,984; raw triangles 5,996; evaluated triangles 5996; materials 2 (02 - Default, 03 - Default).
- Mesh objects: radar (568 tris); rocket (2,112 tris); rocket.001 (1,626 tris); rocket.002 (1,690 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (1, not proven shader bindings): Assets/WarZone Sci-Fi Turret pack/Materials/TurretTexture.png (256x256). Whole package readable raster images: 1.
- Selected roles: Magnetar turret hardware; Custodian articulation; Cutter weapon mounts. Parts: Static rigid weapon assembly; mesh object names below show detachable parts. No armature or clips; multi-mesh assemblies can become pivoted modules.
- Blender kitbash: Easy-to-moderate rigid kitbash; establish pivots and replace generic weapon styling.
- Unity integration: Two materials per candidate, package contains one 256x256 colour texture. No tested moving mechanism/animation supplied.
- Mobile/visual gap: Use selected hardware, bake a consistent PBR atlas and consolidate materials. Whole turrets do not supply elite/boss anatomy.

### rocket2.FBX вЂ” C

- Exact path: `Assets/WarZone Sci-Fi Turret pack/rocket2.FBX`; FBX; SHA-256 `622f17abb18a9146385fd676cae913af88977cbab224c2209d3edb4472ec058a`. Method: Blender import/source read.
- Objects 5; meshes 5; vertices 5,111; polygons 7,606; raw triangles 8,566; evaluated triangles 8566; materials 2 (02 - Default, 03 - Default).
- Mesh objects: rocket (622 tris); rocket.001 (1,690 tris); rocket.002 (2,980 tris); rocket.003 (1,162 tris); rocket.004 (2,112 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (1, not proven shader bindings): Assets/WarZone Sci-Fi Turret pack/Materials/TurretTexture.png (256x256). Whole package readable raster images: 1.
- Selected roles: Magnetar turret hardware; Custodian articulation; Cutter weapon mounts. Parts: Static rigid weapon assembly; mesh object names below show detachable parts. No armature or clips; multi-mesh assemblies can become pivoted modules.
- Blender kitbash: Easy-to-moderate rigid kitbash; establish pivots and replace generic weapon styling.
- Unity integration: Two materials per candidate, package contains one 256x256 colour texture. No tested moving mechanism/animation supplied.
- Mobile/visual gap: Use selected hardware, bake a consistent PBR atlas and consolidate materials. Whole turrets do not supply elite/boss anatomy.

### rocket3.FBX вЂ” C

- Exact path: `Assets/WarZone Sci-Fi Turret pack/rocket3.FBX`; FBX; SHA-256 `b4c79c2b460c99c8a6381ed2056f566850c2a324e50927393f6308eb93e0fd7c`. Method: Blender import/source read.
- Objects 3; meshes 3; vertices 2,597; polygons 3,464; raw triangles 4,424; evaluated triangles 4424; materials 2 (02 - Default, 03 - Default).
- Mesh objects: rocket (2,112 tris); rocket.001 (622 tris); rocket.002 (1,690 tris).
- Rig: none. Imported action records: 0. Normalized take labels: none.
- Exact filename/material-linked image files (0): none resolved.
- Additional local texture candidates (1, not proven shader bindings): Assets/WarZone Sci-Fi Turret pack/Materials/TurretTexture.png (256x256). Whole package readable raster images: 1.
- Selected roles: Magnetar turret hardware; Custodian articulation; Cutter weapon mounts. Parts: Static rigid weapon assembly; mesh object names below show detachable parts. No armature or clips; multi-mesh assemblies can become pivoted modules.
- Blender kitbash: Easy-to-moderate rigid kitbash; establish pivots and replace generic weapon styling.
- Unity integration: Two materials per candidate, package contains one 256x256 colour texture. No tested moving mechanism/animation supplied.
- Mobile/visual gap: Use selected hardware, bake a consistent PBR atlas and consolidate materials. Whole turrets do not supply elite/boss anatomy.
