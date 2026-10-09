# Isolated asset zoo — V2

Local project: `ExternalAssetIntake/FreeAssetIntakeV1/_work_v2/unity_zoo/` in the reused V1 worktree. Unity **6000.3.0f1**, URP **17.3.0**, compatible owned render-resource configuration. Open `Assets/Zoo/AssetZoo.unity` in this scratch project only. Production was neither opened by Unity nor modified.

## Art-only import boundary

1171 canonical FBX models imported and measured. Flat/textured and FBX/FBX Unity duplicates were reduced by source/stem, preferring textured exports. Distinct LOD-named exports are retained as independent models. Original exports remain immutable and local; `.blend`, OBJ/GLTF/source archives are not executed or used as secondary independent candidates.

Only FBX, selected material data and preview PNG copies enter `Assets/Intake`. Source `.cs`, DLL/plugins, Editor extensions, prefab/scene/controller dependencies, custom shaders and package dependencies do not. Eight payload `.cs` files were inventoried and quarantined: Wallcoeur2, CreepyCat1, Sickhead5. There were no native/DLL executable payload files in the inspected extracted sources. Source prefabs are audited statically and not instantiated. Engine/URP package code and the project-owned `IntakeZoo.cs` are the only code required for rendering.

Binary Unity materials are copied byte-for-byte; text materials substitute a baseline shader only in scratch. Unity later upgraded two WarZone scratch binary materials to YAML when saving; their original/extracted material bytes remain intact. Original art GUIDs are preserved after collision checks, including binary references. Preview textures are clamped to1024px, with original dimensions in inventory. PSD source images are converted as local previews, not imported as Photoshop executable integration. Every copied art file has original path/hash/normalization recorded in local `reports/normalization_audit.json`.

## Categories and selected evidence

The saved scene contains ENVIRONMENT, MACHINERY_HERO_PROPS, PIPES_SERVICES, MECHS_ENEMIES, WEAPONS_TURRETS, VFX and UI roots, an industrial ground, neutral directional light and overview camera. Only representative models are placed: **120 model samples**, plus selected safe UI preview cards. All four Quaternius mechs, three Essentials enemy candidates, main Striker and Combat Drone are included; redundant Striker fragments are measured but not placed.

VFX root objects explicitly point to static technical review. No fake static effect surrogate is rendered. UI includes native EXE/Kenney graphics on inspection cards; Tiago stays outside Unity and is compared as local native thumbnail evidence only.

Committed flattened, annotated contact sheets:

- Environment [page1](evidence/environment_contact_sheet.png), [page2](evidence/environment_contact_sheet_02.png), [page3](evidence/environment_contact_sheet_03.png): platform/floor/wall/gate/ramp/light alternatives, including representative rejected styles.
- [Machinery](evidence/machinery_contact_sheet.png): generator rings, cryo/centrifuge, containers and cover alternatives.
- [Pipes](evidence/pipes_contact_sheet.png): cable bundles, pipe columns, ducts and the unresolved TechLab assembly.
- [Mechs/enemies](evidence/mechs_contact_sheet.png): every materially distinct robot/enemy donor, no G-0 replacement.
- [Weapons](evidence/weapons_contact_sheet.png): assembled gun groups, turret/structure donors and rejected alternatives.
- [UI](evidence/ui_contact_sheet.png): source-color thumbnails on neutral gray so black frame details remain visible; rights-limited Tiago clearly labelled.

`EVIDENCE_MANIFEST.json` maps every tile to source/original path and exact local render/native-image hash. `INVENTORY_SUMMARY.json` records **2727 rows: 1171 Unity-measured models, 787 static prefab/graph rows, 769 UI image rows**. Every row carries sourceDecision/itemDecision/selectionReason. Item USE is restricted to named selected model families and EXE normal frames; source prefabs remain DONOR references, never unchanged USE behavior. UI counts include states, colors, upscaled versions, previews and source images, not finished sprite/component counts.

## Capture language and interpretation

Elevated camera: position offset(5,8,-6),35° FOV,480×360 individual captures. Dark neutral ground, flat ambient + neutral directional key, no post-processing/bloom. Each model's longest bound dimension is normalized to a **4m inspection plinth**. The manifest records sourceLongestDimension and previewScale; small avatar width versus flat prop width can differ despite the same longest dimension. This establishes comparable detail/silhouette inspection, not actual gameplay placement or approved G-0 scale. V3 must check the chosen assets at the real existing portrait camera and collision footprint.

Source material colors/maps are converted to URP Lit for comparison, not proof of native URP compatibility. Where available, inert source glTF declarations resolve exact model/material/base-color/normal-image associations; glTF itself is never imported. MegaKit's declared trim maps replace the initial generic-atlas fallback. Packed ORM maps are inventoried but not repacked into URP smoothness/metal channels for this neutral preview. The local `material_bindings.json` preserves382 declared associations, and capture materialMode records whether a declared binding was actually used. Other missing embedded FBX references use matching source materials or explicitly limited filename/atlas fallback; these are approximate surfaces. Some Sickhead/RTS/TechLab samples remain geometry fallback; do not judge their original surface quality from grey renders. Molten is a gradient-color atlas and has no verified PBR wear set. Every USE family needs the named material pass.

## Technical inventory limits

Mesh triangles/vertices/submeshes count each renderer mesh instance, with bone/skinned counts and clip names from Unity import. Animation clips were enumerated, not played/retargeted. Imported texture counts precede neutral conversion and do not fully capture missing prefab material remaps. Source-wide image counts/resolutions/PBR filename hints are explicitly separate from per-model bindings.

Binary prefabs remain NOT_MEASURED. YAML component counts inspect direct serialized records; nested prefab references are not expanded. Particle caps are serialized maxima per system and their sum, not observed simultaneous particles. Script GUIDs are references, not executed code. No FPS, draw-call, memory or device claims are made. Alternative OBJ/GLTF/Blender/RAR representations are source variants, not independently measured models.

## Validation and recovery

Initial scratch images had a magenta ground and were rejected; none of those hashes is in committed evidence. A later restricted process could not connect to Unity licensing and was stopped; the authorized scratch-only launch succeeded. Final inspection compiled owned editor code, imported/measured all1171 models, saved the scene and produced120 captures, exit0. Automated magenta screening rejects invalid images before sheets are written; all eight final sheets were visually reviewed.

The log also contains non-fatal Unity package resource import/type errors for AutodeskInteractive.shadergraph and TraceVirtualOffset.urtshader. Selected URP Lit renders work; those unused graph/probe resource paths are not validated. This is not reported as a warning-free Unity project or a production validation pass. See `VALIDATION_REPORT.md` for exact task-scope checks. No APK/device gate was run or required.
