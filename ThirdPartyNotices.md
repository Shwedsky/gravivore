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

## Visual Production V2 — local intake review and prepared CC0 maps

Inspected 2026-10-06. Raw archives and donor models remain local and uncommitted. Character review renders are project-generated neutral derivatives, solely for source comparison, with the following attribution; no final character is shipped from them. Source checks and exact archive SHA256: `docs/visual-production-v2/modeling/SOURCE_INSPECTION_RESULTS.md`.

- [Robot spider](https://sketchfab.com/3d-models/robot-spider-c9c7188c7f9e4504b8499f1131693b72) by PAndras — Creative Commons Attribution, license recorded in PR #55; [CC BY reference](https://creativecommons.org/licenses/by/4.0/). Changes: import/static neutral clay renders and contact-sheet layout only. Stalenhag source is excluded from production reuse.
- [Spider Robot](https://sketchfab.com/3d-models/spider-robot-7ea58c2e0e7e48f281d7a840f02aa7b1) by Muhammad Hasan Alasady — Creative Commons Attribution, license recorded in PR #55; [CC BY reference](https://creativecommons.org/licenses/by/4.0/). Changes: import/static neutral clay renders and contact-sheet layout only. Stalenhag source is excluded from production reuse.
- [Spider Robot (Rigged)](https://sketchfab.com/3d-models/spider-robot-rigged-419632470d3b4328b8d8ea7ae0462ce7) by Keralt — Creative Commons Attribution, license recorded in PR #55; [CC BY reference](https://creativecommons.org/licenses/by/4.0/). Changes: import/static neutral clay renders and contact-sheet layout only. Stalenhag source is excluded from production reuse.
- [Scorpion mech](https://sketchfab.com/3d-models/scorpion-mech-db3225ec5dcf431d8b3b8dc328a42bbb) by gwim — Creative Commons Attribution, license recorded in PR #55; [CC BY reference](https://creativecommons.org/licenses/by/4.0/). Changes: import/static neutral clay renders and contact-sheet layout only. Stalenhag source is excluded from production reuse.
- [Scorpion Robot](https://sketchfab.com/3d-models/scorpion-robot-e44be9a622af4e85b4b10cf59b25de1a) by Mirandanimator — Creative Commons Attribution, license recorded in PR #55; [CC BY reference](https://creativecommons.org/licenses/by/4.0/). Changes: import/static neutral clay renders and contact-sheet layout only. Stalenhag source is excluded from production reuse.
- [Gunslinger Crab Mech v1.2](https://sketchfab.com/3d-models/gunslinger-crab-mech-v12-5147619f337a45b0974f349eac27b34a) by Vaportrash — Creative Commons Attribution, license recorded in PR #55; [CC BY reference](https://creativecommons.org/licenses/by/4.0/). Changes: import/static neutral clay renders and contact-sheet layout only. Stalenhag source is excluded from production reuse.
- [Mechanical Spider](https://sketchfab.com/3d-models/mechanical-spider-d1f67d2e995d4c81a056242df1b97390) by Preview_Tempest — Creative Commons Attribution, license recorded in PR #55; [CC BY reference](https://creativecommons.org/licenses/by/4.0/). Changes: import/static neutral clay renders and contact-sheet layout only. Stalenhag source is excluded from production reuse.
- [Stalenhag Environment Project: Spider Mech](https://sketchfab.com/3d-models/stalenhag-environment-project-spider-mech-ec5914b53b6a4cde8de4820050bc46c5) by Enrico Labarile — Creative Commons Attribution, license recorded in PR #55; [CC BY reference](https://creativecommons.org/licenses/by/4.0/). Changes: import/static neutral clay renders and contact-sheet layout only. Stalenhag source is excluded from production reuse.

Quaternius [Modular Sci-Fi MegaKit](https://quaternius.com/packs/modularscifimegakit.html), Quaternius, [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/): neutral geometry review renders; no pack FBX redistributed.

- [blue_metal_plate](https://polyhaven.com/a/blue_metal_plate), Rob Tuytel, [Poly Haven CC0](https://polyhaven.com/license): converted diffuse/normal/roughness and metallic when supplied, max 2K. Production copies under `art/visual-production-v2/materials/blue_metal_plate/`; source/output SHA256 in `data/material_prep.json`. Displacement excluded.
- [metal_plate](https://polyhaven.com/a/metal_plate), Rob Tuytel, [Poly Haven CC0](https://polyhaven.com/license): converted diffuse/normal/roughness and metallic when supplied, max 2K. Production copies under `art/visual-production-v2/materials/metal_plate/`; source/output SHA256 in `data/material_prep.json`. Displacement excluded.
- [metal_grate_rusty](https://polyhaven.com/a/metal_grate_rusty), Dimitrios Savva / Rob Tuytel, [Poly Haven CC0](https://polyhaven.com/license): converted diffuse/normal/roughness and metallic when supplied, max 2K. Production copies under `art/visual-production-v2/materials/metal_grate_rusty/`; source/output SHA256 in `data/material_prep.json`. Displacement excluded.
- [rusty_metal_grid](https://polyhaven.com/a/rusty_metal_grid), Amal Kumar, [Poly Haven CC0](https://polyhaven.com/license): converted diffuse/normal/roughness and metallic when supplied, max 2K. Production copies under `art/visual-production-v2/materials/rusty_metal_grid/`; source/output SHA256 in `data/material_prep.json`. Displacement excluded.
