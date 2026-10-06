# Third Party Notices

## Kenney Factory Kit 3.0

- Asset/pack: Factory Kit 3.0 (selected FBX subset only)
- Author: Kenney
- Source URL: https://kenney.nl/assets/factory-kit
- License: Creative Commons CC0 1.0 Universal
- License URL or bundled license file: `Assets/ThirdParty/KenneyFactoryKit/License.txt`
- Date verified: 2026-09-30
- Imported path(s): `Assets/ThirdParty/KenneyFactoryKit/Models/`
- Attribution required: no
- Notes: 14 low-poly models are redistributed as an audited subset for player, enemy, and Chapter 01 environment presentation. No textures, source project, sample scenes, or unused package content are included.

## Art Spike V3 — Blue Metal Plate

- Asset/author: Blue Metal Plate — Rob Tuytel / Poly Haven.
- Original source: https://polyhaven.com/a/blue_metal_plate
- Asset license: https://polyhaven.com/license
- Exact selected license: **CC0 1.0 Universal**, https://creativecommons.org/publicdomain/zero/1.0/legalcode.en
- Verified: 2026-10-02 from the original asset page, license page and official public download metadata.
- Commercial use, modification, raw/modified public repository redistribution: permitted; attribution optional. Credit is retained voluntarily.
- Original metadata: https://api.polyhaven.com/files/blue_metal_plate
- Retained path: Assets/_Game/ArtSpike/Imported/PolyHaven/BlueMetalPlate/.
- Retained originals: blue_metal_plate_diff_1k.jpg, blue_metal_plate_nor_gl_1k.png, blue_metal_plate_arm_1k.png. All 1024×1024; original CDN bytes unchanged and MD5 verified.
- Local License.txt is a project-authored source/license record, not a claimed bundled notice. SHA-256, MD5 and per-file original URLs: docs/art-spike/ASSET_MANIFEST.json.
- Changes: project-owned URP tint/material instances; derived linear AO map and metallic/smoothness map (R=source ARM.B, A=1-ARM.G) under Assets/_Game/ArtSpike/Proxy/Textures. No donor character geometry, rig, animation, site preview renders, website text or vendor code imported.
- V3 G-0/Cutter geometry is original temporary project proxy geometry. The current selected pipeline and ten regenerated PNGs contain no Julius meshes. Earlier V2 files and their CC BY-SA obligations remain naturally in Git history at 783876869e5506304c4521a9ea9b89d6e37e40ef; they are not relicensed. Current scope: docs/art-spike/ART_LICENSE.md.

## Art Spike — Kenney Factory Kit 3.0 (isolated supporting subset)

- Author: Kenney.
- Official source: https://kenney.nl/assets/factory-kit
- Official archive: https://kenney.nl/media/pages/assets/factory-kit/edaac9d4f6-1777639602/kenney_factory-kit_3.0.zip
- License: Creative Commons CC0 1.0 Universal; official page and bundled `Assets/_Game/ArtSpike/Imported/Kenney/FactoryKit/License.txt`.
- Verified: 2026-10-02.
- Commercial use, modification and raw repository redistribution: permitted. Attribution: not required. CC0 does not grant trademark/patent rights.
- Retained directory: `Assets/_Game/ArtSpike/Imported/Kenney/FactoryKit/Models/`.
- Exact retained FBX: `box-long.fbx`, `machine-fortified.fbx`, `pipe-large-bend.fbx`, `pipe-large-long.fbx`.
- Roles: peripheral containers, generator and conduits only; no character geometry. Ten obsolete V1 character/gear donors removed.
- Original license and Unity metadata retained; no textures/vendor code/sample scenes. Source files match official archive; manifest above. Existing production S15 source assets unchanged.

## Art Spike — Kenney Modular Space Kit 1.0 (isolated supporting subset)

- Author: Kenney.
- Official source: https://kenney.nl/assets/modular-space-kit
- Official archive: https://kenney.nl/media/pages/assets/modular-space-kit/8261428a47-1771146076/kenney_modular-space-kit_1.0.zip
- License: Creative Commons CC0 1.0 Universal; official page and bundled `Assets/_Game/ArtSpike/Imported/Kenney/ModularSpaceKit/License.txt`.
- Verified: 2026-10-02.
- Commercial use, modification and raw repository redistribution: permitted. Attribution: not required. CC0 does not grant trademark/patent rights.
- Retained directory: `Assets/_Game/ArtSpike/Imported/Kenney/ModularSpaceKit/Models/`.
- Exact retained FBX: `cables.fbx`, `template-floor.fbx`, `template-floor-detail-a.fbx`, `template-wall-detail-a.fbx`, `template-wall-half.fbx`.
- Roles: deck, quiet insets and low background framing only. Original license and Unity metadata retained; no textures, complete rooms or sample files. Source files match official archive; manifest above.

## Art Spike sources not imported

Expanded original-source research, licenses and rejection/acquisition reasons are recorded in `docs/ART_ASSET_SHORTLIST.md`. Colorado Stark and Robot Enemy Pack archives were inspected in ignored research storage only. Quaternius, Molten Maps, Ryan/rcorre, Mech Drone, Masterxeon, Markom3D and all other reviewed candidates have no imported files in this spike. Unresolved licensing or download availability is not treated as permission to redistribute.

## Phase 2 combat audio — Kenney

- Packs/authors: Sci-Fi Sounds 1.0 (Kenney, 2020) and Impact Sounds 1.0 (Kenney, 2019).
- Official sources: https://kenney.nl/assets/sci-fi-sounds and https://kenney.nl/assets/impact-sounds
- License: CC0 1.0 Universal; https://creativecommons.org/publicdomain/zero/1.0/
- Verified: 2026-10-03, against both official pack pages and each downloaded archive's License.txt.
- Attribution required: no; credit Kenney voluntarily.
- Imported originals: Assets/ThirdParty/Kenney/CombatAudio — eight OGG files plus SciFi_License.txt and Impact_License.txt.
- Runtime derivatives: Assets/_Game/Content/Audio/PlayerFeel — Step, Charge, Release, Impact, EnemyHit, EnemyDeath, PlayerHit, PlayerDeath WAVs.
- Processing: silence trim, duration cap, mono PCM, peak normalization and short fades; reproducible in PlayerFeelAssetConfigurator.Configure.
- Exact source-to-event mapping and archive checksums: docs/PLAYER_FEEL_COMBAT_PRESENTATION.md.
- No ripped game sounds or purchased dependency; unchanged encounter/progression placeholders are project-generated S14 cues.

## G-0 Bipedal Blockout V2 — Catfish donor mechanisms and comparison

- Asset: **Catfish Mech low-poly (animated)**.
- Author: **Jungle Jim** (jungle_jim), https://sketchfab.com/jungle_jim.
- Source: https://sketchfab.com/3d-models/catfish-mech-low-poly-animated-ad9bc16464744935b1ac9b7768a17474.
- License: **Creative Commons Attribution 4.0 International (CC BY 4.0)**, https://creativecommons.org/licenses/by/4.0/.
- Verified: 2026-10-06 against the original public API, archived in docs/visual-production-v2/g0-bipedal-v2/data/catfish_official_metadata.json. Direct page fetch returned 403; source/license verification did not rely on a third-party mirror.
- User-provided ZIP SHA256: 3c20dd24a1611d8391c4d430d8f920dc67969d641cbb93811514cb627ccf98c6.
- Source FBX entry: source/Mech long legs Army CC export.fbx.
- Adapted components: mesh_rep_0_ori_repair_134 (pelvis), 135–148 plus 152–153 (paired knee/ankle mechanisms); 17 objects / 1499 source triangles. They are transformed, regrouped and assigned project materials inside art/visual-production-v2/g0/G0_Bipedal_Blockout_V2.blend. All outer leg shells, feet and upper-body geometry are original project blockout geometry.
- Comparison evidence also displays the original donor silhouette and source materials, rest pose, equal-height normalization; source textures may be downsampled/packed solely for portable comparison. No original full donor is bound to gameplay.
- Attribution required: yes. Preserve this title/author/source/license and modification notice with adapted geometry and donor comparison renders; no author endorsement is implied.
- Raw archive, extracted source and imported animation remain local ignored inspection inputs. The final authoring file must include an internal attribution text block. Exact per-object mapping and adaptation metrics accompany the review.

## Import template (future additions)

When importing any external asset, append:

- Asset/pack:
- Author:
- Source URL:
- License:
- License URL or bundled license file:
- Date verified:
- Imported path(s):
- Attribution required: yes/no
- Notes:

Only assets compatible with commercial redistribution/use may enter the project. Prefer CC0.
