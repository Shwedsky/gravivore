# Art and Asset Policy

Initial asset spend: **0**.

## Current ART SPIKE V3 gate

The selected G-0/Cutter pipeline must have no ShareAlike, NC, unclear-license,
paid or recognizable franchise assets. Skip acquisition requiring authentication
or manual user interaction. The current explicit instruction permits original
temporary modular proxy geometry when no suitable autonomous donor is available;
preserve the approved V2 silhouette direction and document its eventual replacement.
V3 uses original proxy modules with CC0 Poly Haven PBR maps; Kenney remains scenery.
No production Chapter integration follows without human visual approval and a
subsequently authorized specification. See ART_ASSET_SHORTLIST.md for verified
candidate terms and ART_SPIKE_REVIEW.md for the pending review.

## Project-owned generated audio

The S14 placeholder combat cues under `Assets/_Game/Content/Audio/S14_*.wav` are original,
deterministically generated mono tones created by `S14PresentationAssetConfigurator`.
They contain no third-party samples, require no attribution, and may be replaced later through
the serialized `S14PresentationDefinition` references.

## Style target

Stylized low-poly 3D:
- strong silhouettes;
- compact textures/material atlases;
- limited material count;
- exaggerated readable proportions;
- bright readable VFX against darker industrial environment.

Do not try to visually imitate Butcher Hero exactly.

## Preferred free sources

Only import after checking the license on the actual downloaded pack.

Good current candidates:

### KayKit
- Character Pack: Adventurers — CC0, rigged/animated, mobile-friendly.
- Character Pack: Skeletons — CC0, free subset.
- Character Animations — CC0, large animation library.
- Prototype Bits — CC0.
- Dungeon Pack — CC0, 200+ free modular props.

Source: https://kaylousberg.itch.io/

### Kenney
Kenney states game assets on asset pages are CC0 and usable commercially without attribution.

Source: https://kenney.nl/

### Quaternius
Multiple packs are CC0 and include rigged/animated low-poly characters/environment pieces.

Source: https://quaternius.com/ and https://quaternius.itch.io/

## Techno-organism problem

Free packs may not provide the exact player fantasy.

For v0.1, solve this by **kitbashing**:
- choose a rigged free humanoid/mechanical base or simple custom primitive rig;
- hide/replace visually human parts;
- mount CC0 geometry/primitive armor modules on stable bones/sockets;
- use emissive core rings and gravity VFX to establish identity;
- use evolution attachments to make the silhouette progressively less generic.

Do not wait for a perfect bespoke character before validating the gameplay.

## Visual evolution implementation

Create attachment sockets on the player rig:
- Back
- LeftShoulder
- RightShoulder
- LeftArm
- RightArm
- Core
- Hips
- RearThruster

Evolution modules are prefabs attached to sockets.

No runtime mesh fusion required.

## License tracking

Create `ThirdPartyNotices.md` with:
- pack name;
- author;
- download URL;
- exact license;
- date checked;
- files/directories imported;
- whether attribution is required.

CC0 attribution is optional, but recording provenance is mandatory.

## Forbidden

- asset ripping;
- importing models/textures/audio from installed commercial games;
- using Dota/Pudge derivatives;
- "free" assets without a license allowing redistribution/commercial game use;
- paid Asset Store dependencies for v0.1.
