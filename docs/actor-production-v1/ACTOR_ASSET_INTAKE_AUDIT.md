# Chapter 01 actor asset intake audit

Completed 2026-10-10. Branch `chore/actor-asset-intake-v1`, baseline `origin/main` at `38e0ed8fe5c95ff41ab5dbb246f45ed26bdcf9cc`. [Draft PR #72](https://github.com/Shwedsky/gravivore/pull/72); no merge.

## Outcome and authority

Every file in the authorized warehouse was inventoried: **24 payloads, 9 ZIP and 15 UnityPackage**. Nineteen were copied to canonical Current folders with matching SHA-256; five were rejected from promotion and remain in the warehouse. Six existing canonical archives (four UnityFan vehicles and both active world packages) were inspected without replacing their bytes. Seventy-eight model/source candidates were measured: 53 through Blender and 25 static ASCII FBX parts through a read-only geometry parser. The four original mech Blender files and their FBX exports are separate evidence rows, not eight different characters.

This audit selects reusable mechanics, not finished actors. Current G-0 remains bipedal. The owner's current request overrides the old radial Scout target in `docs/visual-production-v2/ENEMY_VISUAL_TARGETS_APPROVED_V1.md`: Scout is a light blade-armed biped; Warden holds separate shields; Carrier is a low streamlined vehicle. Cutter may use quadruped mechanics. Magnetar and Custodian need newly authored heavy architecture, not enlarged ordinary enemies.

Authorities read on this branch: `AGENTS.md`, `docs/CURRENT_AUTHORITIES.md`, `docs/CURRENT_VISUAL_TARGET.md`, `docs/ASSET_SOURCE_OF_TRUTH.md`, `docs/ART_DIRECTION.md`, current enemy target/blueprint text, and `docs/DECISIONS.md`. The textual target supports anatomy and quality judgments; no pixel-level concept-fidelity certification is claimed. Historical warehouse contents are evidence only. No shared source-of-truth document was changed because intake here is local and PR #71 owns the active world work.

## Locations and safe inspection

Owner-local root: `C:\Users\pamak\Documents\ChatGPT\gravivore`.

- Warehouse: `.codex-worktrees/free-asset-intake-v1/98_unclassified`.
- Canonical sources: `ExternalAssetIntake/Current/<source-id>/<original archive filename>` under that owner-local root, **not inside the audit Git worktree**.
- Temporary extracted evidence/renders: `ExternalAssetIntake/Current/_actor-audit-v1`; isolated, ignored, not a production source ID. Local tools/evidence: `.utmp/actor-intake-v1`.
- Git worktree: `C:\Users\pamak\.codex\worktrees\actor-asset-intake-v1\gravivore`.

Archives were listed with traversal/absolute-path/link checks, hashed and selectively extracted locally. Unity GUID/pathname records were decoded; importer `.meta`, material and animation files were inspected as data. Vendor scripts were never executed. Blender 5.2.2 LTS ran with factory startup and auto-execution disabled; `.blend` files opened with scripts disabled. Neutral geometry previews of selected donors were inspected locally; no modified model was saved. Two embedded Creepy Cat Unity packages (458 URP, 119 HDRP member records) and five Striker PSD-source RARs (one PSD each, listing exit code 0) were also inventoried.

`ARCHIVE_MANIFEST.json` is the complete member/format/folder/hash inventory. `COPY_MANIFEST.json` gives exact source and destination filenames. `SOURCE_LICENSES.json` stores publisher/item/license/version/evidence hashes. `MODEL_INVENTORY.md`, `MODEL_METRICS.json` and `TEXTURE_INVENTORY.json` hold measured donor details. `NESTED_ARCHIVES.json` records embedded archives. Raw packages, extracted sources, supplier scripts, PSDs and donor images are not committed.

## Exact warehouse packages and curation

Each entry below is one actual warehouse payload; format counts include variants/exports and are not unique-model counts. Missing formats have zero entries. Exact warehouse relative filenames and every nested member are in the archive manifest.

### Quaternius Animated Mech Pack (March 2021)

- Archive: `Animated Mech Pack - March 2021-20261008T180817Z-1-001.zip`. Publisher: Quaternius; [official source](https://quaternius.com/packs/animatedmech.html).
- Category: actor/mech. Role: G-0; Scout; Warden secondary. Local version: `not declared`.
- Contents: 8 BLEND, 8 FBX, 1 GIF, 8 GLTF, 1 JPG, 8 MTL, 8 OBJ, 20 PNG, 1 TXT.
- Licence: CC0 1.0 in payload and item pages; conflicting current general QAL v1.0 (2026-08-28). Public raw redistribution cleared: **no**. Commercial game use allowed by both notices. Preserve exact CC0 evidence and archive hash. Public editable-source redistribution conservatively NOT cleared in this audit; no retroactive QAL assumption. https://quaternius.com/license.html
- **Copied:** `ExternalAssetIntake/Current/quaternius-animated-mech-pack/Animated Mech Pack - March 2021-20261008T180817Z-1-001.zip`. Original retained; exact archive bytes/hash preserved.

### VFX вЂ“ Impact and Hit вЂ“ Light Version

- Archive: `VFX - Impact and Hit - Light Version.unitypackage`. Publisher: Cartoon VFX by Wallcoeur; [official source](https://assetstore.unity.com/packages/vfx/particles/vfx-impact-and-hit-light-version-335800).
- Category: VFX. Role: impact texture reference; no geometry/rig. Local version: `1.0`.
- Contents: 2 CS, 1 FBX, 82 MAT, 77 PNG, 18 PREFAB, 1 UNITY.
- Licence: Standard Unity Asset Store EULA. Public raw redistribution cleared: **no**. Compiled product use does not license public raw/modified donor-source redistribution.
- **Copied:** `ExternalAssetIntake/Current/vfx-impact-light/VFX - Impact and Hit - Light Version.unitypackage`. Original retained; exact archive bytes/hash preserved.

### 3D Scifi Kit Starter Kit

- Archive: `3D Scifi Kit Starter Kit.unitypackage`. Publisher: Creepy Cat; [official source](https://assetstore.unity.com/packages/3d/environments/3d-scifi-kit-starter-kit-92152).
- Category: environment. Role: environment only; mechanical panel/frame parts. Local version: `3.5`.
- Contents: 2 ASSET, 1 CS, 1 CUBEMAP, 1 EXR, 125 FBX, 39 MAT, 97 PNG, 136 PREFAB, 1 PSD, 1 SHADER, 21 TIF, 1 UNITY, 2 UNITYPACKAGE, 4 WAV.
- Licence: Standard Unity Asset Store EULA. Public raw redistribution cleared: **no**. Compiled product use does not license public raw/modified donor-source redistribution.
- **Copied:** `ExternalAssetIntake/Current/3d-scifi-kit-starter/3D Scifi Kit Starter Kit.unitypackage`. Original retained; exact archive bytes/hash preserved.

### RPG/FPS Industrial Set v2.0

- Archive: `RPGFPS Game Assets for PCMobile Industrial Set v20.unitypackage`. Publisher: Dmitrii Kutsenko; [official source](https://assetstore.unity.com/packages/3d/environments/industrial/rpg-fps-game-assets-for-pc-mobile-industrial-set-v2-0-86679).
- Category: environment. Role: environment only; machinery parts. Local version: `2.0`.
- Contents: 1 ASSET, 2 EXR, 36 FBX, 2 LIGHTING, 36 MAT, 3 PNG, 134 PREFAB, 1 PSD, 33 TGA, 2 UNITY.
- Licence: Standard Unity Asset Store EULA. Public raw redistribution cleared: **no**. Compiled product use does not license public raw/modified donor-source redistribution.
- **Copied:** `ExternalAssetIntake/Current/industrial-set-v20/RPGFPS Game Assets for PCMobile Industrial Set v20.unitypackage`. Original retained; exact archive bytes/hash preserved.

### RTS Sci-Fi Game Assets v1

- Archive: `RTS Sci-Fi Game Assets v1.unitypackage`. Publisher: Dmitrii Kutsenko (local header: Vdr0id); [official source](https://assetstore.unity.com/packages/3d/environments/sci-fi/rts-sci-fi-game-assets-v1-112251).
- Category: vehicle; weapon; environment. Role: Carrier secondary chassis; Magnetar/Custodian pivots. Local version: `1.0`.
- Contents: 9 FBX, 12 MAT, 11 PREFAB, 70 TGA, 1 UNITY, 32 WAV.
- Licence: Standard Unity Asset Store EULA. Public raw redistribution cleared: **no**. Compiled product use does not license public raw/modified donor-source redistribution.
- **Copied:** `ExternalAssetIntake/Current/rts-scifi-assets-v1/RTS Sci-Fi Game Assets v1.unitypackage`. Original retained; exact archive bytes/hash preserved.

### EXE вЂ“ Mini Scifi UI Pack

- Archive: `EXE - Mini Scifi UI Pack.zip`. Publisher: Catherine Laserna / cjlaserna; [official source](https://cjlaserna.itch.io/exe).
- Category: UI. Role: no actor role; UI reference. Local version: `not declared`.
- Contents: 2 JPG, 1 PDF, 18 PNG, 19 TTF, 3 TXT.
- Licence: Artwork CC BY 4.0; bundled Nunito and Zen Dots fonts SIL OFL 1.1. Public raw redistribution cleared: **yes**. Credit Catherine Laserna/cjlaserna and link source + CC BY 4.0; preserve separate font notices.
- **Copied:** `ExternalAssetIntake/Current/exe-mini-scifi-ui/EXE - Mini Scifi UI Pack.zip`. Original retained; exact archive bytes/hash preserved.

### Tower Defence Sci-Fi Turret FREE

- Archive: `Tower Defence Sci-Fi Turret FREE.unitypackage`. Publisher: Firadzo Assets; [official source](https://assetstore.unity.com/packages/3d/environments/sci-fi/tower-defence-sci-fi-turret-free-246331).
- Category: weapon. Role: Cutter paired assemblies; Magnetar/Custodian pivots. Local version: `1.0`.
- Contents: 1 FBX, 10 MAT, 18 PNG, 3 PREFAB, 1 UNITY.
- Licence: Standard Unity Asset Store EULA. Public raw redistribution cleared: **no**. Compiled product use does not license public raw/modified donor-source redistribution.
- **Copied:** `ExternalAssetIntake/Current/tower-defence-turret-free/Tower Defence Sci-Fi Turret FREE.unitypackage`. Original retained; exact archive bytes/hash preserved.

### Fog Particles

- Archive: `Fog Particles.unitypackage`. Publisher: Game Seed Assets; [official source](https://assetstore.unity.com/packages/vfx/particles/fog-particles-351840).
- Category: VFX. Role: environment only; no actor mechanics. Local version: `1.0.0`.
- Contents: 6 MAT, 1 PDF, 3 PNG, 6 PREFAB, 1 UNITY.
- Licence: Standard Unity Asset Store EULA. Public raw redistribution cleared: **no**. Compiled product use does not license public raw/modified donor-source redistribution.
- **Copied:** `ExternalAssetIntake/Current/fog-particles/Fog Particles.unitypackage`. Original retained; exact archive bytes/hash preserved.

### Magic Effects FREE

- Archive: `Magic Effects FREE.unitypackage`. Publisher: Hovl Studio; [official source](https://assetstore.unity.com/packages/vfx/particles/spells/magic-effects-free-247933).
- Category: VFX. Role: rejected fantasy presentation. Local version: `1.6`.
- Contents: 2 ASSET, 3 EXR, 4 FBX, 1 LIGHTING, 34 MAT, 40 PNG, 76 PREFAB, 1 PSD, 1 TXT, 2 UNITY.
- Licence: Standard Unity Asset Store EULA. Public raw redistribution cleared: **no**. Compiled product use does not license public raw/modified donor-source redistribution.
- **Rejected; no new canonical folder/payload:** Fantasy/spell VFX does not fill a selected mechanical actor gap; additional shader/VFX dependency and visual mismatch.

### Sci-Fi Styled Modular Pack

- Archive: `Sci-Fi Styled Modular Pack.unitypackage`. Publisher: karboosx; [official source](https://assetstore.unity.com/packages/3d/environments/sci-fi/sci-fi-styled-modular-pack-82913).
- Category: environment. Role: environment only; moving door parts. Local version: `1.1`.
- Contents: 16 ANIM, 4 CONTROLLER, 202 FBX, 24 MAT, 23 PNG, 152 PREFAB, 1 TXT, 2 UNITY.
- Licence: Standard Unity Asset Store EULA. Public raw redistribution cleared: **no**. Compiled product use does not license public raw/modified donor-source redistribution.
- **Copied:** `ExternalAssetIntake/Current/scifi-styled-modular-pack/Sci-Fi Styled Modular Pack.unitypackage`. Original retained; exact archive bytes/hash preserved.

### Kenney Modular Space Kit 1.0

- Archive: `kenney_modular-space-kit_1.0.zip`. Publisher: Kenney; [official source](https://kenney.nl/assets/modular-space-kit).
- Category: environment. Role: environment only; rejected actor promotion. Local version: `not declared`.
- Contents: 40 FBX, 40 GLB, 1 HTML, 40 MTL, 40 OBJ, 49 PNG, 1 TXT, 3 URL.
- Licence: CC0 1.0 Universal. Public raw redistribution cleared: **yes**. Preserve provenance; no raw bytes committed despite permissive license.
- **Rejected; no new canonical folder/payload:** Legal CC0 environment kit, but duplicates active world coverage and supplies no selected actor mechanics; simple stock forms miss current actor quality.

### Kenney Space Station Kit

- Archive: `kenney_space-station-kit.zip`. Publisher: Kenney; [official source](https://kenney.nl/assets/space-station-kit).
- Category: environment. Role: environment only; rejected actor promotion. Local version: `not declared`.
- Contents: 97 FBX, 97 GLB, 1 HTML, 97 MTL, 97 OBJ, 104 PNG, 1 TXT, 3 URL.
- Licence: CC0 1.0 Universal. Public raw redistribution cleared: **yes**. Preserve provenance; no raw bytes committed despite permissive license.
- **Rejected; no new canonical folder/payload:** Legal CC0 environment kit with no selected actor rig; duplicates environment coverage and does not resolve current donor gaps.

### Kenney UI Pack Sci-Fi 2.0 (archive named space-expansion)

- Archive: `kenney_ui-pack-space-expansion.zip`. Publisher: Kenney; [official source](https://kenney.nl/assets/ui-pack-sci-fi).
- Category: UI. Role: no actor role; UI reference. Local version: `not declared`.
- Contents: 742 PNG, 370 SVG, 2 TTF, 1 TXT, 2 URL.
- Licence: CC0 1.0 Universal. Public raw redistribution cleared: **yes**. Preserve provenance; no raw bytes committed despite permissive license.
- **Copied:** `ExternalAssetIntake/Current/kenney-ui-space-expansion/kenney_ui-pack-space-expansion.zip`. Original retained; exact archive bytes/hash preserved.

### Quaternius Sci-Fi Modular Gun Pack (Nov 2021)

- Archive: `Modular Sci Fi Guns - Nov 2021-20261008T181022Z-1-001.zip`. Publisher: Quaternius; [official source](https://quaternius.com/packs/scifimodularguns.html).
- Category: weapon. Role: G-0 equipment; Cutter mechanisms; Magnetar/Custodian parts. Local version: `not declared`.
- Contents: 78 BLEND, 78 FBX, 1 GLB, 78 GLTF, 1 JPG, 78 MTL, 78 OBJ, 1 TXT.
- Licence: CC0 1.0 in payload and item pages; conflicting current general QAL v1.0 (2026-08-28). Public raw redistribution cleared: **no**. Commercial game use allowed by both notices. Preserve exact CC0 evidence and archive hash. Public editable-source redistribution conservatively NOT cleared in this audit; no retroactive QAL assumption. https://quaternius.com/license.html
- **Copied:** `ExternalAssetIntake/Current/quaternius-scifi-modular-gun-pack/Modular Sci Fi Guns - Nov 2021-20261008T181022Z-1-001.zip`. Original retained; exact archive bytes/hash preserved.

### MSGDI Medium Mech Striker

- Archive: `Medium Mech Striker.unitypackage`. Publisher: MSGDI; [official source](https://assetstore.unity.com/packages/3d/characters/robots/medium-mech-striker-124342).
- Category: actor/mech; weapon. Role: Warden primary; G-0 secondary mechanics. Local version: `1.1`.
- Contents: 1 CONTROLLER, 26 FBX, 9 MAT, 1 PDF, 29 PNG, 51 PREFAB, 5 RAR, 4 TGA, 1 UNITY.
- Licence: Standard Unity Asset Store EULA. Public raw redistribution cleared: **no**. Compiled product use does not license public raw/modified donor-source redistribution.
- **Copied:** `ExternalAssetIntake/Current/msgdi-medium-mech-striker/Medium Mech Striker.unitypackage`. Original retained; exact archive bytes/hash preserved.

### Seed Hunter

- Archive: `Seed Hunter.unitypackage`. Publisher: POLYGONAUTIC; [official source](https://assetstore.unity.com/packages/3d/environments/seed-hunter-143414).
- Category: environment; actor shell. Role: Arc Drone secondary shell; environment only; serpent rejected. Local version: `1.3`.
- Contents: 1 ANIM, 29 ASSET, 7 EXR, 84 FBX, 1 JSON, 83 MAT, 1 PLAYABLE, 134 PNG, 71 PREFAB, 7 PSD, 1 SHADERGRAPH, 11 TGA, 3 TIF, 2 TXT, 2 UNITY, 5 VFX.
- Licence: Standard Unity Asset Store EULA. Public raw redistribution cleared: **no**. Compiled product use does not license public raw/modified donor-source redistribution.
- **Copied:** `ExternalAssetIntake/Current/seed-hunter/Seed Hunter.unitypackage`. Original retained; exact archive bytes/hash preserved.

### Quaternius Sci-Fi Essentials Kit Standard

- Archive: `Sci-Fi Essentials Kit[Standard].zip`. Publisher: Quaternius; [official source](https://quaternius.com/packs/scifiessentialskit.html).
- Category: actor/mech; weapon; environment. Role: Arc Drone; Cutter; Magnetar support mechanics. Local version: `not declared`.
- Contents: 37 BIN, 74 FBX, 37 GLTF, 1 JPG, 37 MTL, 37 OBJ, 68 PNG, 1 TXT.
- Licence: CC0 1.0 in payload and item pages; conflicting current general QAL v1.0 (2026-08-28). Public raw redistribution cleared: **no**. Commercial game use allowed by both notices. Preserve exact CC0 evidence and archive hash. Public editable-source redistribution conservatively NOT cleared in this audit; no retroactive QAL assumption. https://quaternius.com/license.html
- **Copied:** `ExternalAssetIntake/Current/quaternius-scifi-essentials/Sci-Fi Essentials Kit[Standard].zip`. Original retained; exact archive bytes/hash preserved.

### Sci-Fi Construction Kit Modular

- Archive: `Sci-Fi Construction Kit Modular.unitypackage`. Publisher: Sickhead Games; [official source](https://assetstore.unity.com/packages/3d/environments/sci-fi/sci-fi-construction-kit-modular-159280).
- Category: environment. Role: environment only; structural components. Local version: `1.1.0`.
- Contents: 1 ASSET, 5 CS, 1 EXR, 83 FBX, 32 MAT, 1 PDF, 1 PHYSICMATERIAL, 84 PNG, 98 PREFAB, 1 UNITY, 10 WAV.
- Licence: Standard Unity Asset Store EULA. Public raw redistribution cleared: **no**. Compiled product use does not license public raw/modified donor-source redistribution.
- **Copied:** `ExternalAssetIntake/Current/scifi-construction-kit-modular/Sci-Fi Construction Kit Modular.unitypackage`. Original retained; exact archive bytes/hash preserved.

### Free Asset Black Hole Effect

- Archive: `Free Asset Black Hole Effect.unitypackage`. Publisher: SOLODREAM CREATION; [official source](https://assetstore.unity.com/packages/vfx/free-asset-black-hole-effect-356686).
- Category: VFX. Role: G-0/Magnetar core effect reference; no reactor geometry. Local version: `1.0.0`.
- Contents: 1 ASSET, 1 JSON, 4 MAT, 1 PDF, 1 PNG, 2 PREFAB, 3 SHADERGRAPH, 1 TXT, 1 UNITY, 1 VFX.
- Licence: Standard Unity Asset Store EULA. Public raw redistribution cleared: **no**. Compiled product use does not license public raw/modified donor-source redistribution.
- **Copied:** `ExternalAssetIntake/Current/black-hole-effect/Free Asset Black Hole Effect.unitypackage`. Original retained; exact archive bytes/hash preserved.

### TechLab Modular Sci-Fi Pipes

- Archive: `techlab-modular-scifi-pipes.zip`. Publisher: TooManyDemons; [official source](https://sketchfab.com/3d-models/techlab-modular-scifi-pipes-a481a1af0598431da712a10d11b24321).
- Category: environment. Role: quarantine; no actor rig. Local version: `not declared`.
- Contents: 1 FBX, 12 PNG.
- Licence: CC Attribution; exact version unresolved. Public raw redistribution cleared: **no**. No bundled license. Do not infer CC BY version from label; duplicate texture paths have different bytes.
- **Rejected; no new canonical folder/payload:** Exact CC Attribution version was not recoverable from the live official page (403); six texture paths are each duplicated with conflicting bytes in the ZIP. No actor rig. Quarantined; do not resolve duplicate names by silent overwrite.

### FREE Sci-Fi GUI / UI Sci-Fi Files

- Archive: `UI Sci-Fi Files.zip`. Publisher: Tiago PatrГ­cio; [official source](https://thiff.itch.io/free-ui-sci-fi).
- Category: UI. Role: whole pack rejected; no actor role. Local version: `not declared`.
- Contents: 1 PDF, 5 PNG, 2 PSD.
- Licence: Creator custom free personal/commercial permission; Icons8 components unresolved. Public raw redistribution cleared: **no**. Creator frames could be isolated later; no explicit standalone raw redistribution permission. Icons8 icons and Audiowide example text are separate provenance.
- **Rejected; no new canonical folder/payload:** Creator permits free/commercial projects but whole PSD includes Icons8 content with separate obligations. Whole-pack redistribution is unresolved and it supplies no actor mechanics.

### WarZone Sci-Fi Turret Pack

- Archive: `WarZone Sci-Fi Turret pack.unitypackage`. Publisher: VDGames; [official source](https://assetstore.unity.com/packages/3d/environments/warzone-sci-fi-turret-pack-57540).
- Category: weapon. Role: Magnetar/Custodian turret pivots; Cutter mechanics. Local version: `2.0`.
- Contents: 14 FBX, 4 MAT, 1 PNG.
- Licence: Standard Unity Asset Store EULA. Public raw redistribution cleared: **no**. Compiled product use does not license public raw/modified donor-source redistribution.
- **Copied:** `ExternalAssetIntake/Current/warzone-scifi-turret/WarZone Sci-Fi Turret pack.unitypackage`. Original retained; exact archive bytes/hash preserved.

### Free Fire VFX вЂ“ URP

- Archive: `Free Fire VFX - URP.unitypackage`. Publisher: Vefects; [official source](https://assetstore.unity.com/packages/vfx/particles/fire-explosions/free-fire-vfx-urp-266226).
- Category: VFX. Role: damage/heat effects reference; no actor rig. Local version: `1.0.2023.1`.
- Contents: 4 ASSET, 3 FBX, 27 MAT, 2 PDF, 18 PREFAB, 5 SHADER, 32 TGA, 2 UNITY, 3 WAV.
- Licence: Standard Unity Asset Store EULA. Public raw redistribution cleared: **no**. Compiled product use does not license public raw/modified donor-source redistribution.
- **Copied:** `ExternalAssetIntake/Current/free-fire-vfx-urp/Free Fire VFX - URP.unitypackage`. Original retained; exact archive bytes/hash preserved.

### VoodooPlay Low Poly Combat Drone

- Archive: `Low poly combat drone.unitypackage`. Publisher: VoodooPlay; [official source](https://assetstore.unity.com/packages/3d/characters/robots/low-poly-combat-drone-82234).
- Category: actor shell. Role: Arc Drone secondary; static shell only. Local version: `1.0`.
- Contents: 1 ASSET, 1 EXR, 1 FBX, 1 LIGHTING, 1 MAT, 5 PNG, 1 PREFAB, 1 TGA, 1 TXT, 1 UNITY.
- Licence: Standard Unity Asset Store EULA. Public raw redistribution cleared: **no**. Compiled product use does not license public raw/modified donor-source redistribution.
- **Copied:** `ExternalAssetIntake/Current/voodooplay-low-poly-combat-drone/Low poly combat drone.unitypackage`. Original retained; exact archive bytes/hash preserved.

## Existing canonical sources and absent candidates

The following archives were already in Current and were retained unchanged; they were not found in the warehouse or counted among the nineteen new copies.

- **Molten Maps SciFi Asset Pack** вЂ” `ExternalAssetIntake/Current/molten-maps-scifi/Molten Maps SciFi Asset Pack.zip`; CC0 1.0 Universal; [exact official item](https://moltenmaps.itch.io/molten-maps-scifi-pack). environment only; reactor/containment shape donors.
- **Quaternius Modular Sci-Fi MegaKit Standard** вЂ” `ExternalAssetIntake/Current/quaternius-modular-scifi-megakit-standard/Modular_SciFi_MegaKit_Standard.zip`; CC0 1.0 in payload and item pages; conflicting current general QAL v1.0 (2026-08-28); [exact official item](https://quaternius.com/packs/modularscifimegakit.html). environment only; Custodian containment parts.
- **UnityFan Free Sci-Fi Vehicle 011-02** вЂ” `ExternalAssetIntake/Current/unityfan-scifi-vehicle-011-02/free-sci-fi-vehicle-011-02-public-domain-cc0.zip`; Author CC0 public-domain dedication on exact Sketchfab item; platform Free Standard label also displayed; [exact official item](https://sketchfab.com/3d-models/free-sci-fi-vehicle-011-02-public-domain-cc0-3887e33fd0514df795739c24bfc137c8). Carrier secondary shell.
- **UnityFan Free Sci-Fi Vehicle 012** вЂ” `ExternalAssetIntake/Current/unityfan-scifi-vehicle-012/free-sci-fi-vehicle-012-public-domain-cc0.zip`; Author CC0 public-domain dedication on exact Sketchfab item; platform Free Standard label also displayed; [exact official item](https://sketchfab.com/3d-models/free-sci-fi-vehicle-012-public-domain-cc0-d69b22aa84304ecb8098138baa67c8dd). Carrier primary shell.
- **UnityFan Free Sci-Fi Vehicle 012-01** вЂ” `ExternalAssetIntake/Current/unityfan-scifi-vehicle-012-01/free-sci-fi-vehicle-012-01-public-domain-cc0.zip`; Author CC0 public-domain dedication on exact Sketchfab item; platform Free Standard label also displayed; [exact official item](https://sketchfab.com/3d-models/free-sci-fi-vehicle-012-01-public-domain-cc0-be5907569dec439881bcaf91284de555). Carrier alternate shell.
- **UnityFan Free Sci-Fi Vehicle 022** вЂ” `ExternalAssetIntake/Current/unityfan-scifi-vehicle-022/free-sci-fi-vehicle-022-public-domain-cc0.zip`; Author CC0 public-domain dedication on exact Sketchfab item; platform Free Standard label also displayed; [exact official item](https://sketchfab.com/3d-models/free-sci-fi-vehicle-022-public-domain-cc0-eba7965b617d4b97a79dbb0a673934c4). Carrier secondary armour/door forms.

The existing `josue-the-bot` and `arma-free-mecha` folders contained no payloads. They were not created by this task and are not usable donors. Neither those packages nor Modular SciFi Pack 146 Objects or Armored Pipes exist in the inspected warehouse. Meshy is unavailable. No filename inference or empty folder was counted as acquisition.

## Licence decisions and evidence limits

Unity-package item IDs/publishers were cross-checked against their official Asset Store pages; local gzip metadata records the installed package version. They use the [Standard Unity Asset Store EULA](https://unity.com/legal/as-terms): use in a compiled game under the owner's acquisition is distinct from publishing raw or modified source. The public PR therefore contains metadata/docs only. Future editable Unity-donor work must stay in the owner's private workflow; publicly committing a transformed donor mesh is not automatically permitted.

Quaternius payloads include CC0 notices and the exact item pages still state CC0. Its [general licence page](https://quaternius.com/license.html) now presents QAL v1.0 dated 2026-08-28, with a non-retroactivity clause and standalone-redistribution restrictions. Both permit commercial game use; this report preserves the older item/payload CC0 evidence and does not assert that QAL retroactively cancels it. Because the source notices conflict, raw public redistribution is conservatively not cleared here. Keeping all raw files local avoids needing that decision for this audit.

Kenney/Molten exact pages and included licences identify CC0. EXE artwork uses CC BY 4.0 (credit Catherine Laserna/cjlaserna, licence link and modifications); bundled Nunito/Zen Dots fonts have OFL 1.1 notices. EXE is a UI reference only. UnityFan's exact indexed official item descriptions/title explicitly dedicate the models to CC0; a platform Free Standard label is also displayed. Live Sketchfab opens returned 403, so the metadata names the indexed-source limitation and preserves exact item URLs rather than pretending a licence receipt was inside the ZIP.

The TechLab page identifies CC Attribution in indexed official results, but its exact version could not be established. FREE Sci-Fi GUI's own permission is not blanket permission for embedded Icons8 content. Those two are not approved for actor production. This is a provenance decision from the evidence listed, not a legal guarantee for future distribution arrangements.

Local Striker is v1.1 while its current store page is v1.2; local VoodooPlay Drone is v1.0 while current is v1.1; Creepy Cat local is v3.5 while current is v4.0. Recommendations use measured local bytes, not advertised newer features. No refresh download is required to validate this selection.

## Selected actor donors and exact production gaps

The compact selection is in [ACTOR_DONOR_MATRIX.md](ACTOR_DONOR_MATRIX.md). A/B/C/D grade indicates donor utility, **not final-art acceptance**. Full meshes, material counts, bones, every imported action name/frame range and separated-object names are in the model inventory and metrics.

- **G-0:** retain the current project-owned biped. George/Stan/Striker may provide mechanical or locomotion references if later authorized; this audit does not select a replacement G-0 or retarget its production rig. Current silhouette/armour/finish remains the target.
- **Scout:** George is the best light biped rig/legs/torso donor: 7,864 triangles, one material, 47 bones and 20 named actions. Its original `.blend` provides editability and 61 disconnected geometry islands. Preserve leg chains/foot placement; replace arms with long blades/spikes, build the aggressive forward shell and core enclosure, tune Run and author blade sweep/recover/death poses. Leela lacks arm bones; Mike/Stan are secondary heavier bodies. Existing SwordSlash is only a reference, not verified game combat.
- **Cutter:** QuadShell supplies quadruped mechanics (7,494 triangles, two meshes, 28 bones, eight takes). Replace its two guns at Gun.L/R with independently legible cutting assemblies and industrial drive details. Tower Defence's paired barrels/mounts can donate mounting mechanics, not finished cutters. Substantial custom shell/tool geometry and motion remain.
- **Warden:** Striker is the heavy biped donor (6,472 triangles, three meshes, 17 bones). Hands exist as separate 999-triangle FBX meshes each, but the skeleton has no wrist/finger bones. Add visible separate shields with physical grips, lower-arm sockets or new wrists, maintain clear space between shield and shoulder armour, and author brace/raise/strike/break animation. 102 imported actions include duplicates; they are not 102 distinct gameplay clips. Stock shoulder plates cannot satisfy handheld shields. ASCII hand sources need a trusted private conversion before Blender kitbash.
- **Arc Drone:** EyeDrone is the primary aerial articulation donor (3,530 triangles, 14 bones, six takes). Seed Hunter's Keydrone (4,076 triangles, four 4K maps) supplies a secondary angular shell but is static/unrigged. VoodooPlay (1,362 triangles) is another static aerial shell. Author emitter/energy containment, piston/nozzle detail, airborne idle/aim/discharge and destruction; no walking legs. EyeDrone-to-Keydrone skinning is deliberate custom work, not drop-in compatibility.
- **Carrier:** UnityFan 012 is the primary low wedge/hover shell, already editable in Blender. It measures **9,248 evaluated triangles**, seven meshes and five materials; its 485 control-cage triangles are not runtime cost. 012-01/011-02 provide alternative shells; 022 is a more expensive 36,916-triangle secondary source. All four have **zero rigs and zero animation clips**. Separate objects do not prove useful moving pivots. No ready locomotion solution exists: author concealed hover modules/running gear, root/pivot rig, bank/drift/idle/death animation, enclosed assault fittings, loading/attack doors and unified PBR atlas. RTS Vehicle_v1 can donate chassis details but has one combined mesh/no wheel rig; tall Vehicle_v3 is rejected as a complete Carrier. A low sleek vehicle is available, a finished assault transport is not.
- **Magnetar:** author the broad multi-support machine architecture and contained magnetic core. QuadShell support mechanics, Trilobite joints and turret pivot modules are component references; strengthen/rebuild supports rather than enlarging QuadShell. No donor provides the complete central containment, connected power routing or elite stance. New multi-support rig, charge/vent/stagger/death motion and substantial geometry are required.
- **Custodian:** author an arena-defining articulated machine with reactor/core, major structural members and separate phase mechanisms. Tower Defence/WarZone/RTS doors/rotors/pivots and existing world containment forms are selective references only. No inspected actor is a full boss donor; merely scaling Striker/Trilobite is rejected. Architecture, articulated rig, reactor enclosure and multi-phase animation are very substantial custom work.

All newly produced enemies need substantial custom Blender geometry, material unification and animation adaptation. Scout/Warden/Cutter have reusable rig foundations; Arc has reusable aerial articulation; Carrier saves shell work but lacks motion; Magnetar/Custodian need the greatest new architecture. G-0 is retained and was not assigned a new modeling scope by this audit.

## Remaining free-acquisition gaps

**No mandatory additional download was identified.** Available donors already cover the reusable biped/quadruped/aerial mechanics and low vehicle shell; missing shields, blade arms, contained cores and boss architecture are project-specific authored work. Reacquiring generic environment libraries would not make production deterministic.

- [JosuГ© The Bot](https://shyr.itch.io/josue-the-bot): optional free rigged FBX, publisher says no animations. Editable Blender download is paid and excluded. CC0 tag conflicts with a standalone-redistribution prohibition in the description; no reason to replace George/Striker with this unmeasured candidate.
- [ARMA Free Mecha](https://arma-labs.itch.io/free-mecha-not-rigged): free unrigged FBX advertised, no clear published production licence found. It does not close the animation/rig gap and is not recommended until explicit permission exists.
- UnityFan optional high-detail modules download was not present and would increase complexity without closing locomotion or boss-architecture gaps. No paid downloads or Meshy assumptions.

## Deterministic handoff and validation

The next actor-production task can start with Scout: use the exact George source path/hash in MODEL_METRICS, preserve the original rig, author the long blade arms and forward armour in a private editable work copy, then adapt the existing locomotion and validate combat poses. Warden follows the separate-shield recipe above; Carrier uses the named 012 `.blend` and evaluated modifier cost. No existing next spec ID was supplied or created. This audit does not authorize or start those modeling tasks.

Before later promotion: preserve source/item/hash/licence notices; convert ASCII sources privately; freeze/review modifier output, scale/orientation and pivots; bake controls to a Generic-compatible rig; rebuild URP materials/channels; consolidate materials and choose texture/LOD budgets from profiling; then run relevant Unity wiring/build tests and the real-device human visual gate. Triangle counts here are measurements, not a prototype ceiling or performance pass.

Audit verification results are in `VALIDATION_SUMMARY.json`: exact warehouse coverage, source/copy hashes, untouched retained archive hashes, safe archive/nested paths, source/licence/model associations, valid model metrics and ignored raw locations. `VALIDATE_MANIFESTS.py` can rerun metadata checks anywhere and owner-local hash checks with `--local-root`. Git staged scope is limited to twelve files under `docs/actor-production-v1`; whitespace checks passed. The exact file list is recorded in VALIDATION_SUMMARY.json.

Unity compilation, edit/play-mode gameplay tests, project scene validation and Android APK build were **not run**: this is a docs/local-intake-only branch with no imported production assets or runtime change, and launching Unity/builds could disturb the parallel world session. No APK was produced. Unity importer/retargeting, clip semantics, mobile performance and final visual fidelity remain unverified; native Blender import succeeded for 53 rows, while 25 ASCII rows were parsed only. No scene/prefab/runtime/map/minimap/content change and no final modeling occurred. Draft PR #72 remains independent; PR #71 and its branch were not modified.
