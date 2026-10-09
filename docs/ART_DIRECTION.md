# GRAVIVORE current art direction — industrial hard-surface sci-fi

Status: **CURRENT / AUTHORITATIVE**

Primary visual contract: `docs/CURRENT_VISUAL_TARGET.md`.

## G-0

Current G-0 is a **bipedal robotic combat mech**.

Required read:
- separate torso/body mass;
- head/visor or equivalent readable sensor mass;
- two mechanical legs with believable joints and load-bearing feet;
- arms/tool/weapon mounting that reads at the portrait gameplay camera;
- central cyan gravity-core identity integrated into the body;
- non-human machine anatomy rather than a soldier in powered armor;
- visible equipment/weapon changes when equipped;
- evolution changes silhouette through added/replaced authored geometry, not only scale or brighter emission.

Historical low four-support/radial/tank-like G-0 directions are superseded and must not be resurrected from old phase docs or concept-board player anatomy.

## Hostile machines

Shared faction language:
- serious modern sci-fi combat machines;
- dark graphite exposed mechanics;
- cool/pale metal armor shells;
- layered armor over visible joints/actuators;
- localized red/orange hostile energy;
- mechanically plausible supports and attack sources;
- role-specific silhouettes readable from the real gameplay camera;
- no toy proportions;
- no low-detail box-mech final forms;
- no color-only role differentiation;
- no generic stock identity after production adaptation.

Scout/Cutter/Magnetar specifics remain in `docs/visual-production-v2/ENEMY_VISUAL_TARGETS_APPROVED_V1.md`.

## Environment

The world is a dense but readable industrial exclusion complex, not a quiet open floor with decoration around the edges.

Target composition:
- layered floor construction, seams, trenches, grates and damaged transitions;
- substantial wall/gate thickness and service framing;
- connected pipes/cables/services with visible endpoints and purpose;
- large machinery/facilities that explain the function of a zone;
- reactors, generators, containment systems, docks, gantries, cranes, storage and wreckage;
- broken/collapsed silhouettes and wear where the setting is damaged;
- strong edge/periphery massing and vertical machinery;
- open encounter centres and protected telegraph lanes.

A gameplay `SpawnSpot` is only spawn authority. Its presentation may be a substantial facility/dock/fabricator or other authored industrial structure if that best communicates the concept.

## Materials

Core palette:
- graphite/near-black structure;
- cool desaturated steel;
- worn painted machinery;
- dirty deck/concrete/industrial surfaces;
- restrained rust/heat/damage;
- cyan/blue service/player energy;
- red/orange hostile/danger energy;
- amber/ochre warning paint where functional.

Use PBR surface depth and within-object variation. Do not let shared-material optimization make unrelated objects look like the same plastic construction toy.

## Lighting

Lighting should reveal mass, recesses and materials while keeping targets/telegraphs readable.

Prefer efficient techniques suitable for mobile: directional/baked/emissive/reflection/fake-light treatment first, bounded local realtime lights only where the gain is real.

## Camera-first acceptance

Judge production art in the actual playable portrait camera on device.

Beauty renders and close material views help diagnose quality but do not replace the gameplay-camera gate. Fine detail that cannot be perceived at gameplay scale must not carry the design; primary and secondary forms must do the work.

## Production quality

Primitive/procedural proxy modules, obvious blockout geometry and untouched stock models are acceptable for technical exploration only.

When the active task is a production/concept-fidelity pass, final acceptance requires authored/refined forms and surfaces that materially close the gap to the owner concept rather than simply making the previous proxy more complex.
