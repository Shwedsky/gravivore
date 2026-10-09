# Chapter 01 visual blueprint

Status: **CURRENT / AUTHORITATIVE FOR THE NEXT FULL CHAPTER 01 WORLD REBUILD**

Primary blueprint contract:

- `CHAPTER01_VISUAL_BLUEPRINT_V1.md`
- `CHAPTER01_VISUAL_BLUEPRINT_V1.jpg`

The board and text specification are intended to be read together.

The blueprint defines the target scene composition for the next Chapter 01 rebuild. It preserves gameplay semantics while explicitly allowing the old V44–V47 physical layout, exact coordinates, floor grid, facility footprints and collision fingerprint to be replaced.

For implementation also read:

- `../../CURRENT_AUTHORITIES.md`
- `../../CURRENT_VISUAL_TARGET.md`
- `../../ART_DIRECTION.md`
- `../../ASSET_SOURCE_OF_TRUTH.md`
- `../../../specs/S04_ENEMY_FRAMEWORK_SPAWN_SPOTS.md`
- `../../../specs/S08_WORLD_ZONE_GATES.md`

Current owner-local raw packages expected by the blueprint task:

- `ExternalAssetIntake/Current/quaternius-modular-scifi-megakit-standard/Modular_SciFi_MegaKit_Standard.zip`
- `ExternalAssetIntake/Current/molten-maps-scifi/Molten Maps SciFi Asset Pack.zip`

These ZIPs remain git-ignored. The board is committed to Git and is visual authority for the rebuild.

Do not implement the next pass as another presentation layer over the old map. Build the new Chapter world/layout from the blueprint and remap existing gameplay identities into that rebuilt world.