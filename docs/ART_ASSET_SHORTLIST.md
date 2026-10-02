# Art spike asset shortlist

Official sources checked on **2026-10-02**. No unofficial mirrors, paid tiers, or engine packages were used. Every pack below was evaluated before donor selection.

## Quaternius Sci-Fi Essentials Kit — rejected for this import

- Author: Quaternius.
- Official pages: <https://quaternius.com/packs/scifiessentialskit.html> and <https://quaternius.itch.io/sci-fi-essentials-kit>.
- License evidence: both pack pages advertise CC0; the author's current [QAL v1.0](https://quaternius.com/license.html), dated 2026-08-28, covers assets from any distribution platform and restricts redistribution of assets as assets. Commercial products, modification, and contractor use are permitted without attribution under QAL. CC0 permits commercial use, modification, and redistribution without attribution. These conflicting official representations do not establish clear permission to retain raw assets in this public source repository.
- Formats: FBX, OBJ, glTF; Blend/engine projects in the paid Source tier.
- Contents: 60+ models in the full offering; the free Standard description identifies 37 models, including animated robot enemies, weapons, and props.
- Useful roles: character bases and detachable weapon/prop donors.
- Limitations: modular separability of individual robots was not inspected because no model was imported. Free and paid contents differ.
- Acquisition: official download and purchase pages reached. Free archive is `Sci-Fi Essentials Kit[Standard].zip` (159 MB), accessed through an interactive itch.io download step. No archive acquired or imported; license conflict takes precedence.
- Decision: rejected for this spike pending clarification of the applicable license. Do not substitute a mirror or buy Pro/Source.

## Quaternius Animated Mech Pack — rejected for this import

- Author: Quaternius.
- Official page: <https://quaternius.com/packs/animatedmech.html>.
- License evidence: pack page labels CC0 and commercial use; the current site-wide QAL conflicts with raw-asset redistribution as described above. Modification and commercial product use are allowed by both representations; redistribution permission remains unresolved. No attribution required by either.
- Formats listed: FBX, OBJ, Blend; the format badge also lists glTF.
- Contents: four mechs, each with an animation set.
- Useful roles: non-human locomotion reference and possible mechanical child donors.
- Limitations: four complete character models are not proven modular. No rig/child mesh inspection was performed.
- Acquisition: official page's download button points to the author's [Google Drive folder](https://drive.google.com/drive/folders/1sueV_4CGMpZC8y30mWfgKK9UaT3mkHBX?usp=sharing). The folder listing was not accessible to the read-only web tool; no direct archive filename was exposed by the official page. No import.
- Decision: rejected pending license resolution; animated donor candidate for a later authorized task.

## Quaternius Modular Sci-Fi MegaKit — rejected for this import

- Author: Quaternius.
- Official pages: <https://quaternius.com/packs/modularscifimegakit.html> and <https://quaternius.itch.io/modular-sci-fi-megakit>.
- License evidence: CC0 on both pack pages versus current site-wide QAL restrictions. Commercial use/modification/no attribution are explicit, but raw-asset redistribution is ambiguous. Not imported.
- Formats: FBX, OBJ, glTF; Blend and engine integration in paid Source.
- Contents: 270+ environment pieces in the full pack; the free Standard tier supplies a subset. Floors, walls, doors, columns, props, and other categories.
- Useful roles: modular industrial environment and machinery framing.
- Limitations: source-engine projects/custom shaders and extra content are paid; no free model hierarchy was inspected.
- Acquisition: official free purchase page reached; `Modular SciFi MegaKit[Standard].zip` (46 MB) requires the interactive free-download step. No archive acquired.
- Decision: rejected for this spike pending license clarification.

## Molten Maps – SciFi Assets Pack — backup, manual asset required

- Author: Moltenbolt.
- Official page: <https://moltenmaps.itch.io/molten-maps-scifi-pack>.
- Verified published license: CC0. Commercial use, modification, and redistribution are permitted by [CC0](https://creativecommons.org/publicdomain/zero/1.0/); no attribution required. No downloaded bundled license was available to inspect, so import would require checking that archive as well.
- Formats advertised: FBX, OBJ, and glTF (the page spells it GLFT).
- Contents: 130+ modular walls/floors/ramps, monitors, generators, cryo installations, and industrial props. The page describes a shared atlas and three material roles.
- Useful roles: reactor, generator, and environment machinery donors.
- Limitations: stylized props need palette replacement; the advertised atlas size is 1048×1048 and remains unverified from files.
- Acquisition: official asset and purchase pages reached. The free download is a browser/session interaction; no direct archive link was exposed. No authentication or captcha bypass was attempted. No files imported.
- Decision: backup; the available Kenney donors were sufficient to continue.

**MANUAL ASSET REQUIRED:** download `Molten Maps SciFi Asset Pack.zip` (102 MB) from the official page using the free-download option. Place it at:

`C:\Users\pamak\Documents\ChatGPT\gravivore\.codex-worktrees\art-spike-kitbash\Builds\ArtSpike\ManualAssets\Molten Maps SciFi Asset Pack.zip`

This is optional for the current review package; do not import it automatically into production.

## Kenney Modular Space Kit 1.0 — selected

- Author: Kenney.
- Official page: <https://kenney.nl/assets/modular-space-kit>.
- License: CC0 on the official page and in the downloaded `License.txt`. Commercial use/modification/redistribution allowed; no attribution required.
- Formats in archive: FBX, OBJ, GLB, with palette textures and alternate color maps.
- Contents: 40 modular pieces, including floor and wall templates, rooms, corridors, gates, and cables.
- Useful roles: quiet floors, low bulkheads, perimeter framing, and a cable bundle.
- Limitations: complete rooms would obstruct top-down gameplay; only templates/low framing retained. Vendor palette and texture variants were omitted.
- Acquisition: official direct ZIP downloaded successfully: `kenney_modular-space-kit_1.0.zip`.
- Retained subset: `template-floor.fbx`, `template-wall-half.fbx`, `template-wall-detail-a.fbx`, `cables.fbx`, bundled license, and generated Unity metadata.
- Decision: selected for supplemental environment structure.

## Kenney Factory Kit 3.0 — selected additional mechanical donor

- Author: Kenney.
- Official page: <https://kenney.nl/assets/factory-kit>.
- License: CC0 on the official page and in the freshly downloaded `License.txt`. Commercial use/modification/redistribution allowed; no attribution required.
- Formats in archive: FBX, OBJ, GLB, with palette textures.
- Contents: 140 industrial models, including machines, articulated pistons, robot arms, cogs, pipes, containers, conveyors, and screens.
- Useful roles: gravity housing, core rings, four mechanical supports, armor panels, cutter saws, reactor plinth, generator, containers, and pipes.
- Limitations: static industrial parts rather than a character rig; nonuniform scaling can expose familiar prop shapes. Piston FBX files each expose three child meshes, increasing submissions.
- Acquisition: official direct ZIP downloaded successfully: `kenney_factory-kit_3.0.zip`. Source bytes in the spike were verified against this archive, independently of S15's historical import.
- Retained subset: 14 FBX files listed in `ThirdPartyNotices.md` and `art-spike/ASSET_MANIFEST.json`; license and generated metadata. No textures or complete robots retained.
- Decision: selected for transform-only modular kitbashing. No S15 assets or production recipes were edited.

The Quaternius license conflict is recorded as a source-selection limitation, not resolved through assumptions or past conversation summaries.
