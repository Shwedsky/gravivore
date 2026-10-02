# GRAVIVORE art spike — visual review

**ART SPIKE VISUAL REVIEW: PENDING**

All images below were rendered from the actual Unity comparison scene. You do not need to inspect the Unity hierarchy. Quality and direction approval belong to you.

1. Open [the close evolution comparison](art-spike/images/01_G0_Evolution.png). Left to right: **Tier 0, Tier 1, Tier 2**. Check whether these look like one machine evolving around the same exposed core. The short ruler is one metre with 20 cm ticks. This is a closer art-review view.
2. Open [all tiers at the S20 camera](art-spike/images/07_Evolution_S20Scale.png). Each panel is a full 1080×1920 portrait render at the real S20 offset and FOV. Compare silhouette changes, especially side armor and the Tier 2 forward fork, without relying only on cyan accents. Small details should not be needed to identify the tier.
3. Open [the player alone at gameplay scale](art-spike/images/02_G0_GameplayScale.png). Check whether Tier 1's chassis, core, four supports, and forward direction read clearly on a phone-sized display.
4. Open [player and Cutter together](art-spike/images/03_Enemy_GameplayScale.png). Player is left/near center; Cutter is upper right. Ignore color briefly: the player has four supports and a circular core; the enemy has two runners, a rectangular energy block, and offset saws. Check whether you can tell them apart.
5. Open [the industrial bay overview](art-spike/images/04_Environment_Overview.png). Reactor is at the rear left; generator/container at rear right; low walls/pipes frame an open center. Check whether the setting supports the intended dark industrial mood.
6. Open [the gameplay composition](art-spike/images/05_Gameplay_Mock.png). This is Tier 2, Cutter, and the bay at the S20 portrait camera. Check whether characters remain the first thing you notice. There is no gameplay HUD, combat animation, or balancing simulation in this static mock.
7. Optional: [player/enemy scale comparison](art-spike/images/06_ScaleReference.png) gives a closer same-scale view. It does not change the intended prefab root scales.

## What you are seeing

- Characters: project-owned compositions of separate CC0 Kenney Factory Kit machine casings, pistons, cogs, panels, containers, and cone pieces. No complete stock robot is used.
- Environment: CC0 Kenney Modular Space Kit floor/wall/cable donors plus Factory Kit machinery/pipes.
- Project-owned: all composition hierarchies, module transforms, graphite/armor/secondary materials, cyan/red/amber energy materials, small core/lens/emitter/joint primitives, lighting, review layout, and capture utilities.
- Temporary: all spike prefabs, static posing, simple deck treatment, one-metre reference marks, review lighting, and capture cameras. The production player, enemies, elite, boss, catalog, evolution presentation, camera balance, and Chapter 01 scene were not replaced.

## Weaknesses to judge

Familiar cog and industrial prop shapes remain; some bevels stretch under nonuniform scale. Static joints do not demonstrate a working gait. The floor is simple and the lighting can hide inner mechanics. Tier 2 has 60 renderers and needs batching/performance evaluation before production use. Quaternius donors were excluded because official license statements conflict; the optional Molten Maps archive is not yet available locally.

If useful, open `Assets/_Game/ArtSpike/Scenes/ArtSpike_Comparison.unity` in Unity. Its four review areas correspond to the images above. No manual 3D modeling or Blender work is needed to inspect or reproduce this package.

See [the measured prefab snapshot](art-spike/PERFORMANCE.md), [asset provenance](ART_ASSET_SHORTLIST.md), and [implementation report](ART_SPIKE_REPORT.md) for factual details. Do not start full art replacement until this visual direction and the individual compositions are approved.
