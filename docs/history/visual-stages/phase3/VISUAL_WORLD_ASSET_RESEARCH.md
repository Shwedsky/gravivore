# GRAVIVORE — Phase 3 Visual World Asset Research

Research date: **2026-10-04**

This is a provenance/selection note for `VISUAL_WORLD_BASELINE.md`. No new third-party asset bytes are committed by this research PR.

## Selection principles

The current biggest visual problem is not lack of polygon count; it is lack of a coherent world context. Use a small number of legally clean modular sources, then normalize them through GRAVIVORE materials, scale and composition.

Preferred:
- CC0;
- direct/free acquisition without account dependency;
- FBX/OBJ/glTF;
- mobile-readable geometry;
- modular/mechanical shapes that tolerate recoloring and kitbash.

Reject or defer:
- unclear redistribution terms;
- login-only autonomous acquisition;
- stock characters that look cute, humanoid or recognizable from another game;
- packs whose visual identity overwhelms GRAVIVORE.

## SELECT — Quaternius Modular Sci-Fi MegaKit

Official:
- https://quaternius.com/packs/modularscifimegakit.html
- https://quaternius.itch.io/modular-sci-fi-megakit

Verified public facts:
- CC0;
- 270+ modular environment models in the pack family;
- FBX / OBJ / glTF formats;
- free Standard download exists;
- built for grid-based sci-fi rooms/platforms;
- the paid Source edition is not required for the zero-spend baseline.

Use:
- primary structural environment donor;
- walls, floor modules, columns, doors, platforms, pipes, utility shapes;
- re-material through the project-owned dark industrial palette rather than preserving the pack's full original color language.

Risk:
- stock modular repetition. Mitigate with asymmetry, broken sections, Poly Haven surfaces, authored hero landmarks and mixed scale.

Decision: **PRIMARY ENVIRONMENT SOURCE**.

## SELECT — Kenney Factory Kit

Official:
- https://kenney.nl/assets/factory-kit

Verified:
- CC0;
- 140 files;
- industrial/factory props;
- current repository already contains a small audited subset.

Use:
- conveyors, machines, pipes, crates and utility clutter;
- secondary/supporting dressing only.

Risk:
- clean/simple geometry can read toy-like if it dominates.

Decision: **SECONDARY SUPPORT SOURCE**.

## SELECT — Kenney Modular Space Kit

Official:
- https://kenney.nl/assets/modular-space-kit

Verified:
- CC0;
- 40 files;
- modular sci-fi/space-station pieces;
- current repository already contains a small audited subset.

Use:
- small structural fillers, cable/floor/wall details.

Decision: **SECONDARY SUPPORT SOURCE**.

## SELECT — Poly Haven surface set

License:
- https://polyhaven.com/license
- Poly Haven assets are CC0.

Already selected in repository:
- Blue Metal Plate: https://polyhaven.com/a/blue_metal_plate

Good Phase 3 additions, only if needed:
- Metal Plate: https://polyhaven.com/a/metal_plate
- Metal Plate 02: https://polyhaven.com/a/metal_plate_02
- Concrete: https://polyhaven.com/a/concrete
- Concrete Floor 01: https://polyhaven.com/a/concrete_floor_01
- Concrete Debris: https://polyhaven.com/a/concrete_debris
- Rusty Metal Sheet: https://polyhaven.com/a/rusty_metal_sheet
- Rusty Metal Grid: https://polyhaven.com/a/rusty_metal_grid

Import rule:
- download only the required 1K maps;
- prefer diffuse + GL normal + ARM where available;
- pack to the existing URP metallic/smoothness convention;
- mipmaps on;
- Android ASTC 6x6;
- no CPU readback;
- preserve source/license record and hash.

Decision: **PRIMARY SURFACE SOURCE**.

## AUDITION ONLY — Quaternius Sci-Fi Essentials Kit

Official:
- https://quaternius.com/packs/scifiessentialskit.html
- https://quaternius.itch.io/sci-fi-essentials-kit

Verified:
- CC0;
- 60+ models;
- includes animated robot enemies, guns, props, screens and other sci-fi assets;
- FBX / OBJ / glTF.

Potential use:
- neutral props;
- screens/hologram-like shapes;
- individual mechanical pieces;
- quick animation/rig experiments.

Why not primary actors:
- previewed robot shapes include rounded/compact forms that can easily recreate the exact toy/cute problem the Phase 3 baseline is meant to remove.

Decision: **PARTS/REFERENCE/AUDITION ONLY**.

## AUDITION ONLY — Quaternius Animated Mech Pack

Official:
- https://quaternius.com/packs/animatedmech.html

Verified:
- CC0;
- four animated mechs;
- FBX / OBJ / Blend / glTF.

Potential use:
- inspect animation/rig structure;
- temporary elite/boss motion reference;
- small modular donor parts if separation is clean.

Why not primary:
- the pack is older and the previewed full mechs are humanoid/stylized enough that dropping them in whole would weaken GRAVIVORE's techno-industrial identity.

Decision: **REFERENCE/PARTS, NOT WHOLE-ACTOR DEFAULT**.

## PRIMARY ACTOR STRATEGY — project-authored hard-surface kitbash

Use the existing ArtSpike technique as the autonomous default:
- authored chamfered shells;
- joint cylinders/pistons;
- plates, forks, blades, housings and power cores;
- independent positive-scale pivots;
- shared PBR surface family;
- presentation-only prefabs.

Benefits:
- no license uncertainty;
- exact silhouette control at the real camera;
- avoids recognizable stock characters;
- can reuse the current G-0/Cutter design language;
- faster to iterate with Codex than waiting for a perfect downloadable mech.

Important: do not simply recolor the same body five times. Each enemy family must have a different mass distribution, support topology and dominant feature.

## Proposed source mapping

Environment shell:
- Quaternius Modular Sci-Fi MegaKit + project materials.

Repair hub:
- Quaternius structural pieces + Kenney factory manipulators/pipes + project-authored service arms.

Five zone landmarks:
- mostly authored kitbash using Quaternius/Kenney neutral industrial modules.

Ordinary enemies:
- project-authored hard-surface prefabs; keep Cutter's current accepted asymmetry as one family.

Elite:
- project-authored larger guardian kitbash, optionally borrowing neutral CC0 modules.

Boss:
- project-authored industrial-maintenance mech; no whole stock humanoid donor.

Surface wear:
- Poly Haven 1K CC0 PBR set.

## Provenance before import

For every actually imported source:
1. record original page and author;
2. capture exact license;
3. confirm commercial use, modification and public-repository redistribution;
4. retain license/source notice inside the imported source folder;
5. record imported paths in `ThirdPartyNotices.md`;
6. record modifications/repacking performed by the project;
7. do not copy web preview images into the game repository unless their redistribution rights are separately clear.

A pack being "free" is not enough.
